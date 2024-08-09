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
    private readonly Mock<IHttpRequestService> _httpRequestService = new();
    private Mock<HttpRequestData> _request;
    private readonly SetupRequest _setupRequest = new();
    private readonly CreateUpdateEpisode _sut;

    public CreateUpdateEpisodeTests()
    {
        Environment.SetEnvironmentVariable("CreateEpisodeUrl", "CreateEpisodeUrl");

        _sut = new CreateUpdateEpisode(_logger.Object, _httpRequestService.Object);
    }

    [TestMethod]
    public async Task Run_Return_OK_When_Create_Episode_Succeeds()
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
        _httpRequestService.Verify(x => x.SendPost("CreateEpisodeUrl", It.IsAny<string>()), Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_Create_Episode_Fails()
    {
        // Arrange
        var episode = new Episode
        {
            EpisodeId = "1234567890"
        };

        var json = JsonSerializer.Serialize(episode);
        _request = _setupRequest.Setup(json);

        _httpRequestService.Setup(x => x.SendPost("CreateEpisodeUrl", It.IsAny<string>())).Throws<Exception>();

        // Act
        var result = await _sut.Run(_request.Object);

        // Assert
        _httpRequestService.Verify(x => x.SendPost("CreateEpisodeUrl", It.IsAny<string>()), Times.Once);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_BadRequest_When_Episode_Is_Invalid()
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
        _httpRequestService.Verify(x => x.SendPost("CreateEpisodeUrl", It.IsAny<string>()), Times.Never);
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }
}
