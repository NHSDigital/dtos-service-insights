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

namespace NHS.ServiceInsights.EpisodeDataServiceTests;

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
    public async Task Run_ShouldReturnBadRequest_WhenEpisodeIdIsNotProvidedOrInvalid()
    {
        // Arrange
        var queryParam = new NameValueCollection();

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Episode ID missing or not valid.")),
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

        _mockEpisodeRepository.Setup(repo => repo.GetEpisodeAsync(12345)).ReturnsAsync((Episode)null);

        // Act
        var response = await _function.Run(_mockRequest.Object);

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

        _mockEpisodeRepository.Setup(repo => repo.GetEpisodeAsync(245395)).ReturnsAsync(episode);

        // Act
        var response = await _function.Run(_mockRequest.Object);

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

        _mockEpisodeRepository.Setup(repo => repo.GetEpisodeAsync(12345))
            .Throws(new Exception("Database error"));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to get episode from database.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
