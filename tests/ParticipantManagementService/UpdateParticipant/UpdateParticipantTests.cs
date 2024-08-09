using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace UpdateParticipantTests
{
    [TestClass]
    public class ParticipantManagementTests
    {
        [TestMethod]
        public async Task Run_ShouldReturnOk_WhenValidRequestIsReceived()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockHttpRequest = new Mock<HttpRequest>();
            var participantData = new { Name = "John Doe", Age = 30 }; // Example participant data
            var json = JsonConvert.SerializeObject(participantData);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            mockHttpRequest.Setup(req => req.Body).Returns(stream);

            // Act
            var result = await ParticipantManagement.Run(mockHttpRequest.Object, mockLogger.Object) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("Participant data updated successfully.", result.Value);
        }

        [TestMethod]
        public async Task Run_ShouldLogInformation_WhenValidRequestIsReceived()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockHttpRequest = new Mock<HttpRequest>();
            var participantData = new { Name = "John Doe", Age = 30 }; // Example participant data
            var json = JsonConvert.SerializeObject(participantData);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            mockHttpRequest.Setup(req => req.Body).Returns(stream);

            // Act
            await ParticipantManagement.Run(mockHttpRequest.Object, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                log => log.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Participant data updated successfully.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once
            );
        }
    }
}
