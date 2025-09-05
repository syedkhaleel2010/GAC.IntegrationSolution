using GAC.Integration.FileProcessor.Parsers;
using GAC.Integration.FileProcessor.Transformers;
using GAC.Integration.Infrastructure.ApiClients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Cronos;
using Microsoft.Extensions.Hosting;

namespace GAC.Integration.Scheduler.Services
{
    public class FilePollingService : BackgroundService
    {
        private readonly ILogger<FilePollingService> _logger;
        private readonly IConfiguration _config;
        private readonly WmsApiClient _wmsApiClient;
        private readonly string _pollingDirectory;
        private readonly string _archiveDirectory;
        private readonly CronExpression _cronExpression;
        private DateTime _nextRun;

        public FilePollingService(
            ILogger<FilePollingService> logger,
            IConfiguration config,
            WmsApiClient wmsApiClient)
        {
            _logger = logger;
            _config = config;
            _wmsApiClient = wmsApiClient;

            _pollingDirectory = _config["Scheduler:PollingDirectory"];
            _archiveDirectory = _config["Scheduler:ArchiveDirectory"];

            var cronExpr = _config["Scheduler:CronExpression"];
            _cronExpression = CronExpression.Parse(cronExpr);
            _nextRun = _cronExpression.GetNextOccurrence(DateTime.UtcNow) ?? DateTime.UtcNow.AddMinutes(5);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                if (now >= _nextRun)
                {
                    await PollFilesAsync();
                    _nextRun = _cronExpression.GetNextOccurrence(DateTime.UtcNow) ?? now.AddMinutes(5);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task PollFilesAsync()
        {
            var files = Directory.GetFiles(_pollingDirectory, "*.xml");

            foreach (var file in files)
            {
                try
                {
                    var content = await File.ReadAllTextAsync(file);

                    // 1. Parse XML → Domain Object
                    var po = XmlParser.ParsePurchaseOrder(content);

                    // 2. Transform → WMS Schema
                    var poDto = LegacyToWmsMapper.MapPurchaseOrderToWms(po);

                    // 3. Push to WMS API
                    var success = await _wmsApiClient.PushPurchaseOrderAsync(poDto);

                    if (success)
                    {
                        var archivePath = Path.Combine(_archiveDirectory, Path.GetFileName(file));
                        Directory.CreateDirectory(_archiveDirectory);
                        File.Move(file, archivePath, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process file {file}", file);
                }
            }
        }
    }
}
