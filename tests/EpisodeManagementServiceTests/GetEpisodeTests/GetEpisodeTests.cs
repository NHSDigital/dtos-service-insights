using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NHS.ServiceInsights.EpisodeManagementService;
using NHS.ServiceInsights.TestUtils;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json;

namespace NHS.ServiceInsights.Tests
{
    [TestClass]
    public class GetEpisodeTests
    {
        private Mock<ILogger<GetEpisode>> _mockLogger;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private GetEpisode _function;
        private Mock<HttpRequestData> _mockRequest = new();
        private SetupRequest _setupRequest = new();

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<GetEpisode>>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _function = new GetEpisode(_mockLogger.Object, httpClient);
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
                { "EpisodeId", "12345" }
            };

            _mockRequest = _setupRequest.SetupGet(queryParam);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound))
                .Verifiable();

            // Act
            var response = await _function.Run(_mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            _mockLogger.Verify(log =>
                log.Log(
                    LogLevel.Error,
                    0,
                    It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Failed to retrieve episode with Episode ID 12345") &&
                                                          state.ToString().Contains("Status Code: NotFound")),
                    null,
                    (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Once);

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri("http://localhost:6070/api/GetEpisode?EpisodeId=12345")),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task Run_ShouldReturnOk_WhenEpisodeIsFound()
        {
            // Arrange
            var queryParam = new NameValueCollection
            {
                { "EpisodeId", "245395" }
            };

            _mockRequest = _setupRequest.SetupGet(queryParam);

            var jsonResponse = "{\"EpisodeId\": \"245395\", \"Status\": \"Active\"}";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                })
                .Verifiable();

            // Act
            var response = await _function.Run(_mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            response.Body.Seek(0, SeekOrigin.Begin);
            var episode = await JsonSerializer.DeserializeAsync<JsonElement>(response.Body);
            Assert.AreEqual("245395", episode.GetProperty("EpisodeId").GetString());

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri("http://localhost:6070/api/GetEpisode?EpisodeId=245395")),
                ItExpr.IsAny<CancellationToken>());
        }

    }
}
