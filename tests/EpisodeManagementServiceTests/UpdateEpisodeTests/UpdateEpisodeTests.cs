using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace UpdateEpisodeTests
{
    [TestClass]
    public class EpisodeManagementTests
    {
        [TestMethod]
        public async Task Run_ShouldReturnOk_WhenValidRequestIsReceived()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockHttpRequest = new Mock<HttpRequest>();
            var episodeData = new { episode_id = "245395" }; // Example episode data
            var json = JsonConvert.SerializeObject(episodeData);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            mockHttpRequest.Setup(req => req.Body).Returns(stream);

            // Act
            var result = await EpisodeManagement.Run(mockHttpRequest.Object, mockLogger.Object) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("Episode data updated successfully.", result.Value);
        }

        [TestMethod]
        public async Task Run_ShouldLogInformation_WhenValidRequestIsReceived()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockHttpRequest = new Mock<HttpRequest>();
            var episodeData = new { episode_id = "245395" }; // Example episode data
            var json = JsonConvert.SerializeObject(episodeData);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            mockHttpRequest.Setup(req => req.Body).Returns(stream);

            // Act
            await EpisodeManagement.Run(mockHttpRequest.Object, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                log => log.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Episode data updated successfully.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once
            );
        }
    }
}
