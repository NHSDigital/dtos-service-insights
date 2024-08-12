using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace NHS.EpisodeIntegrationServiceTests
{
    [TestClass]
    public class ReceiveDataTests
    {
        private Mock<ILogger> _mockLogger;

        [TestInitialize]
        public void Initialize()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [TestMethod]
        public async Task Run_ValidJson_LogsValidMessage()
        {
            // Arrange
            var validJson = @"{
                ""Participants"": [
                    {
                        ""nhs_number"": ""1111111112"",
                        ""subject_status_code"": ""NORMAL""
                    }
                ],
                ""Episodes"": [
                    {
                        ""episode_id"": 245395,
                        ""episode_type"": ""C""
                    }
                ]
            }";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(validJson));
            var name = "test.json";

            // Act
            await BlobJsonTrigger.Run(stream, name, _mockLogger.Object);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("JSON is valid")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        }


        [TestMethod]
        public async Task Run_InvalidJson_LogsErrorMessage()
        {
            // Arrange
            var invalidJson = "This is not valid JSON";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidJson));
            var name = "invalid.json";

            // Act
            await BlobJsonTrigger.Run(stream, name, _mockLogger.Object);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("JSON is invalid")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        }
    }
}
