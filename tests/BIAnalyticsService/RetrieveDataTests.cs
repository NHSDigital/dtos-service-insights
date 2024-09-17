using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NHS.ServiceInsights.BIAnalyticsService;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.TestUtils;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.Tests
{
    [TestClass]
    public class RetrieveDataTests
    {
        private Mock<ILogger<RetrieveData>> _mockLogger = new();
        private Mock<IHttpRequestService> _httpRequestService = new();
        private RetrieveData _function;
        private Mock<HttpRequestData> _mockRequest = new();
        private SetupRequest _setupRequest = new();

        public RetrieveDataTests()
        {

            Environment.SetEnvironmentVariable("GetEpisodeUrl", "http://localhost:6060/api/GetEpisode");
            _function = new RetrieveData(_mockLogger.Object, _httpRequestService.Object);
        }

        [TestMethod]
        public async Task Run_ShouldReturnBadRequest_WhenEpisodeIdIsNotProvided()
        {
            // Arrange
            var queryParam = new NameValueCollection
            {
                { "EpisodeId", null }
            };

            _mockRequest = _setupRequest.SetupGet(queryParam);

            // Act
            var response = await _function.Run(_mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockLogger.Verify(log =>
                log.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((state, type) => state.ToString() == "Please enter a valid Episode ID."),
                null,
                (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Run_ShouldReturnNotFound_WhenEpisodeIsNotFound()
        {
            // Arrange
            var queryParam = new NameValueCollection
            {
                { "EpisodeId", "123456" }
            };

            _mockRequest = _setupRequest.SetupGet(queryParam);

            var url = "http://localhost:6060/api/GetEpisode?EpisodeId=123456";

            _httpRequestService
                .Setup(service => service.SendGet(url))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            // Act
            var response = await _function.Run(_mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            _httpRequestService.Verify(service =>
                service.SendGet(url), Times.Once());
        }

        [TestMethod]
        public async Task Run_ShouldReturnOk_WhenEpisodeIsFound()
        {
            // Arrange
            var queryParam = new NameValueCollection
            {
                { "EpisodeId", "745396" }
            };

            _mockRequest = _setupRequest.SetupGet(queryParam);

            var url = "http://localhost:6009/api/GetEpisode?EpisodeId=745396";

            var jsonResponse = "{\"EpisodeId\": \"745396\", \"Status\": \"Active\"}";

            _httpRequestService
                .Setup(service => service.SendGet(url))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            // Act
            var response = await _function.Run(_mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            response.Body.Seek(0, SeekOrigin.Begin);
            var episode = await JsonSerializer.DeserializeAsync<JsonElement>(response.Body);
            Assert.AreEqual("745396", episode.GetProperty("EpisodeId").GetString());

            _httpRequestService.Verify(service =>
                service.SendGet(url), Times.Once());
        }

        [TestMethod]
        public async Task Run_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var queryParam = new NameValueCollection
            {
                { "episodeId", "745396" }
            };
            _mockRequest = _setupRequest.SetupGet(queryParam);

            var url = "http://localhost:6060/api/GetEpisode?EpisodeId=745396";

            _httpRequestService
                .Setup(service => service.SendGet(url))
                .ThrowsAsync(new HttpRequestException("Exception: System.Net.Http.HttpRequestException:"));

            // Act
            var response = await _function.Run(_mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            _mockLogger.Verify(log => log.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Failed to call the GetEpisode Data Service.") &&
                                                        state.ToString().Contains("Exception: System.Net.Http.HttpRequestException:")),
                null,
                (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Run_ShouldReturnBadRequest_WhenNhsNumberIsNotProvided()
        {
            // Arrange
            var queryParam = new NameValueCollection
            {
                {
                    "nhsNumber", null
                }
            };

            _mockRequest = _setupRequest.SetupGet(queryParam);

            // Act
            var response = await _function.Run(_mockRequest.Object);

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
                    "nhsNumber", "9999999999"
                }
            };

            _mockRequest = _setupRequest.SetupGet(queryParam);

            // Act
            var response = await _function.Run(_mockRequest.Object);

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
                    "nhs_number", "1111111112"
                }
            };

            _mockRequest = _setupRequest.SetupGet(queryParam);

            // Act
            var response = await _function.Run(_mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Participant participant;
            using (StreamReader reader = new StreamReader(response.Body, Encoding.UTF8))
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                var responseBody = reader.ReadToEnd();
                participant = JsonSerializer.Deserialize<Participant>(responseBody);
            }
            Assert.AreEqual("1111111112", participant.nhs_number);
        }


    }
}
