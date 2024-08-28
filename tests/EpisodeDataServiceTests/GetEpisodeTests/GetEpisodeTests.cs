using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.EpisodeDataService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using System.Collections.Specialized;

namespace NHS.ServiceInsights.EpisodeDataServiceTests;

[TestClass]
public class GetEpisodeTests
{
    private readonly Mock<ILogger<GetEpisode>> _mockLogger = new();
    private readonly Mock<IEpisodeRepository> _mockEpisodeRepository = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly GetEpisode _function;

    public GetEpisodeTests()
    {
        _function = new GetEpisode(_mockLogger.Object, _mockEpisodeRepository.Object);
    }

    [TestMethod]
    public async Task Run_Should_Return_BadRequest_When_Episode_Is_Not_Provided()
    {
        // Arrange
        var queryParam = new NameValueCollection
            {
                { "EpisodeId", null }
            };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var result = _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
                It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Getting Episode ID: (null)") &&
                                                     state.ToString().Contains("Episode not found.")),
                null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);

    }

    // [TestMethod]
    // public async Task Run_Should_Return_OK_When_Repository_Gets_Episode()
    // {
    //     // Arrange
    //     var episode = new Episode
    //     {
    //         EpisodeId = "1234567890"
    //     };

    //     var json = JsonSerializer.Serialize(episode);
    //     _mockRequest = _setupRequest.Setup(json);

    //     // Act
    //     var result = _function.Run(_mockRequest.Object);

    //     // Assert
    //     Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    // }

    // [TestMethod]
    // public async Task Run_Should_Return_InternalServiceError_When_Repository_Throw_Exception()
    // {
    //     // Arrange
    //     var episode = new Episode
    //     {
    //         EpisodeId = "1234567890"
    //     };

    //     var json = JsonSerializer.Serialize(episode);
    //     _mockRequest = _setupRequest.Setup(json);

    //     _mockEpisodeRepository.Setup(repo => repo.GetEpisode(It.IsAny<Episode>())).Throws<Exception>();

    //     // Act
    //     var result = _function.Run(_mockRequest.Object);

    //     // Assert
    //     Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    // }
}
