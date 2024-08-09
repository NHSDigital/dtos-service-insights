namespace NHS.ServiceInsights.CreateUpdateEpisodeTests;

using Moq;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.CohortManager.Tests.TestUtils;
using NHS.ServiceInsights.EpisodeManagementService;
using Common;

[TestClass]
public class CreateUpdateEpisodeTests
{
    private readonly Mock<ILogger<CreateUpdateEpisode>> _logger = new();
    private readonly Mock<ICallFunction> _callFunction = new();
    private Mock<HttpRequestData> _request;
    private readonly SetupRequest _setupRequest = new();
    private readonly CreateUpdateEpisode _sut;

    public CreateUpdateEpisodeTests()
    {
        _sut = new CreateUpdateEpisode(_logger.Object, _callFunction.Object);
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

        // Act
        var result = await _sut.Run(_request.Object);

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

        // Act
        var result = await _sut.Run(_request.Object);

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
