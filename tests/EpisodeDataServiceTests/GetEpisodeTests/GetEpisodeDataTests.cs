using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NHS.ServiceInsights.EpisodeDataService;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using NHS.ServiceInsights.TestUtils;

namespace NHS.ServiceInsights.Tests
{
    [TestClass]
    public class GetEpisodeTests
    {
        private Mock<ILogger<GetEpisode>> _mockLogger = new();
        private Mock<IEpisodeRepository> _mockEpisodeRepository = new();
        private GetEpisode _function;
        private Mock<HttpRequestData> _mockRequest = new();
        private SetupRequest _setupRequest = new();

        public GetEpisodeTests()
        {
            _function = new GetEpisode(_mockLogger.Object, _mockEpisodeRepository.Object);
        }

        [TestMethod]
        public async Task Run_ShouldReturnBadRequest_WhenEpisodeIdIsNotProvided()
        {
            // Arrange
            var queryParam = new NameValueCollection()
            {
                { "EpisodeId", null }
            };
            _mockRequest = _setupRequest.SetupGet(queryParam);

            // Act
            var response = _function.Run(_mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockLogger.Verify(log => log.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Episode ID is not provided.")),
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
                { "episodeId", "12345" }
            };
            _mockRequest = _setupRequest.SetupGet(queryParam);

            _mockEpisodeRepository.Setup(repo => repo.GetEpisode("12345")).Returns((Episode)null);

            // Act
            var response = _function.Run(_mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            _mockLogger.Verify(log => log.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Episode not found.")),
                null,
                (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Run_ShouldReturnOk_WhenEpisodeIsFound()
        {
            // Arrange
            var queryParam = new NameValueCollection
            {
                { "episodeId", "245395" }
            };
            _mockRequest = _setupRequest.SetupGet(queryParam);

            var episode = new Episode
            {
                EpisodeId = 245395
            };

            _mockEpisodeRepository.Setup(repo => repo.GetEpisode("245395")).Returns(episode);

            // Act
            var response = _function.Run(_mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            response.Body.Seek(0, SeekOrigin.Begin);
            var episodeResponse = await JsonSerializer.DeserializeAsync<Episode>(response.Body);
            Assert.AreEqual<long>(245395, episodeResponse.EpisodeId);

            _mockLogger.Verify(log => log.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Episode found successfully.")),
                null,
                (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Run_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var queryParam = new NameValueCollection
            {
                { "episodeId", "12345" }
            };
            _mockRequest = _setupRequest.SetupGet(queryParam);

            _mockEpisodeRepository.Setup(repo => repo.GetEpisode("12345"))
                .Throws(new Exception("Database error"));

            // Act
            var response = _function.Run(_mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            _mockLogger.Verify(log => log.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Failed to get episode from database.") &&
                                                        state.ToString().Contains("Exception: System.Exception: Database error")),
                null,
                (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}