using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NHS.ServiceInsights.ParticipantManagementService;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.TestUtils;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json;

namespace NHS.ServiceInsights.Tests
{
    [TestClass]
    public class GetParticipantTests
    {
        private Mock<ILogger<GetParticipant>> _mockLogger;
        private GetParticipant _function;
        private Mock<HttpRequestData> _MockRequest = new();
        private SetupRequest _SetupRequest = new();

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<GetParticipant>>();
            _function = new GetParticipant(_mockLogger.Object);
        }

        [TestMethod]
        public async Task Run_ShouldReturnBadRequest_WhenNhsNumberIsNotProvided()
        {
            // Arrange
            var queryParam = new NameValueCollection
            {
                {
                    "nhs_number", null
                }
            };

            _MockRequest = _SetupRequest.SetupGet(queryParam);

            // Act
            var response = await _function.Run(_MockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockLogger.Verify(log =>
                log.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((state, type) => state.ToString() == "Please enter a valid NHS Number."),
                null,
                (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Run_ShouldReturnNotFound_WhenParticipantIsNotFound()
        {
            // Arrange
            var queryParam = new NameValueCollection
            {
                {
                    "nhs_number", "9999999999"
                }
            };

            _MockRequest = _SetupRequest.SetupGet(queryParam);

            // Act
            var response = await _function.Run(_MockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            _mockLogger.Verify(log =>
                log.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((state, type) => state.ToString() == $"Participant with NHS Number 9999999999 not found."),
                null,
                (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Run_ShouldReturnOk_WhenParticipantIsFound()
        {
            // Arrange
            var queryParam = new NameValueCollection
            {
                {
                    "nhs_number", "1111111110"
                }
            };

            _MockRequest = _SetupRequest.SetupGet(queryParam);

            // Act
            var response = await _function.Run(_MockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Participant participant;
            using (StreamReader reader = new StreamReader(response.Body, Encoding.UTF8))
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                var responseBody = reader.ReadToEnd();
                participant = JsonSerializer.Deserialize<Participant>(responseBody);
            }
            Assert.AreEqual("1111111110", participant.nhs_number);
        }

    }
}
