using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using GAC.Integration.Scheduler.Services;
using GAC.Integration.Infrastructure.ApiClients;
using System.Collections.Generic;
using System.Threading;

namespace GAC.Integration.Tests
{
    public class FilePollingServiceTests
    {
        private readonly string _testDir = Path.Combine(Path.GetTempPath(), "Inbound");
        private readonly string _archiveDir = Path.Combine(Path.GetTempPath(), "Archive");

        private readonly Mock<WmsApiClient> _mockWmsApiClient;
        private readonly Mock<ILogger<FilePollingService>> _mockLogger;
        private readonly IConfiguration _config;

        public FilePollingServiceTests()
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
        public async Task PollFilesAsync_ProcessesFileAndMovesToArchive()
        {
            // Arrange: Create a dummy XML file
            var filePath = Path.Combine(_testDir, "test.xml");
            var xmlContent = @"<PurchaseOrder>
                                  <LegacyOrderId>PO123</LegacyOrderId>
                                  <OrderDate>2025-09-05</OrderDate>
                                  <CustID>CUST01</CustID>
                                  <Item>
                                      <LegacyProductCode>P001</LegacyProductCode>
                                      <Qty>5</Qty>
                                  </Item>
                               </PurchaseOrder>";

            await File.WriteAllTextAsync(filePath, xmlContent);

            _mockWmsApiClient
                .Setup(x => x.PushPurchaseOrderAsync(It.IsAny<object>()))
                .ReturnsAsync(true);

            var service = new FilePollingService(
                _mockLogger.Object,
                _config,
                _mockWmsApiClient.Object);

            // Act: Call internal method (simulate scheduler trigger)
            var method = typeof(FilePollingService).GetMethod("PollFilesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method.Invoke(service, null);

            // Assert: File should be moved to archive
            var archivedFilePath = Path.Combine(_archiveDir, "test.xml");
            Assert.True(File.Exists(archivedFilePath));

            // Verify API client was called once
            _mockWmsApiClient.Verify(x => x.PushPurchaseOrderAsync(It.IsAny<object>()), Times.Once);
        }
    }
}
