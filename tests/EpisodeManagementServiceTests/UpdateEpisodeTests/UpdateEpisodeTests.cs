namespace NHS.UpdateEpisodeTests;

using Moq;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using UpdateEpisode;
using NHS.CohortManager.Tests.TestUtils;

[TestClass]
public class UpdateEpisodeTests
{
    private readonly Mock<ILogger<UpdateEpisode>> _logger = new();
    private Mock<HttpRequestData> _request;
    private readonly SetupRequest _setupRequest = new();

    public UpdateEpisodeTests()
    {

    }

    [TestMethod]
    public async Task Run_Should_Log_EpisodeId()
    {
        // Arrange
        var episode = new Episode
        {
            EpisodeId = "1234567890"
        };

        var json = JsonSerializer.Serialize(episode);
        _request = _setupRequest.Setup(json);

        var sut = new UpdateEpisode(_logger.Object);

        // Act
        var result = sut.Run(_request.Object);

        // Assert
        _logger.Verify(log =>
            log.Log(
            LogLevel.Information,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString() == episode.EpisodeId.ToString()),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Log_Error_When_Episode_Is_Invalid()
    {
        // Arrange
        var json = JsonSerializer.Serialize("Invalid");
        _request = _setupRequest.Setup(json);

        var sut = new UpdateEpisode(_logger.Object);

        // Act
        var result = sut.Run(_request.Object);

        // Assert
        _logger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "Could not read episode data."),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }
}
