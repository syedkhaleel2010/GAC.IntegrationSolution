using GAC.Integration.Infrastructure.ApiClients;
using GAC.Integration.Scheduler.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GAC.Integration.Tests
{
    public class FilePollingServiceRetryTests
    {
        private readonly string _testDir = Path.Combine(Path.GetTempPath(), "InboundRetry");
        private readonly string _archiveDir = Path.Combine(Path.GetTempPath(), "ArchiveRetry");

        private readonly Mock<WmsApiClient> _mockWmsApiClient;
        private readonly Mock<ILogger<FilePollingService>> _mockLogger;
        private readonly IConfiguration _config;

        public FilePollingServiceRetryTests()
        {
            Directory.CreateDirectory(_testDir);
            Directory.CreateDirectory(_archiveDir);

            _mockWmsApiClient = new Mock<WmsApiClient>(null, Mock.Of<ILogger<WmsApiClient>>());
            _mockLogger = new Mock<ILogger<FilePollingService>>();

            var inMemorySettings = new Dictionary<string, string>
            {
                {"Scheduler:PollingDirectory", _testDir},
                {"Scheduler:ArchiveDirectory", _archiveDir},
                {"Scheduler:CronExpression", "*/5 * * * *"}
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task PollFilesAsync_RetriesOnFailureAndSucceeds()
        {
            // Arrange: Create test XML file
            var filePath = Path.Combine(_testDir, "retry.xml");
            var xmlContent = @"<PurchaseOrder>
                                  <LegacyOrderId>PO456</LegacyOrderId>
                                  <OrderDate>2025-09-05</OrderDate>
                                  <CustID>CUST02</CustID>
                                  <Item>
                                      <LegacyProductCode>P002</LegacyProductCode>
                                      <Qty>10</Qty>
                                  </Item>
                               </PurchaseOrder>";

            await File.WriteAllTextAsync(filePath, xmlContent);

            // Fail twice, succeed third time
            var callCount = 0;
            //_mockWmsApiClient
            //    .Setup(x => x.PushPurchaseOrderAsync(It.IsAny<object>()))
            //    .ReturnsAsync(() =>
            //    {
            //        callCount++;
            //        return Task.FromResult(new HttpResponseMessage
            //        {
            //            StatusCode = callCount >= 3 ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.InternalServerError
            //        });
            //    });

            var service = new FilePollingService(
                _mockLogger.Object,
                _config,
                _mockWmsApiClient.Object);

            // Act
            var method = typeof(FilePollingService).GetMethod("PollFilesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                await (Task)method.Invoke(service, null);
            }

            // Assert
            var archivedFilePath = Path.Combine(_archiveDir, "retry.xml");
            Assert.True(File.Exists(archivedFilePath));

            // Verify retries happened (3 total attempts)
            _mockWmsApiClient.Verify(x => x.PushPurchaseOrderAsync(It.IsAny<object>()), Times.Exactly(3));
        }
    }
}
