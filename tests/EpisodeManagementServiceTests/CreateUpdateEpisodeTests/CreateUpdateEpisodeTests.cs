using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.EpisodeManagementService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Common;
using Azure.Messaging.EventGrid;
using Azure;

namespace NHS.ServiceInsights.EpisodeManagementServiceTests;

[TestClass]
public class CreateUpdateEpisodeTests
{
    private readonly Mock<ILogger<CreateUpdateEpisode>> _mockLogger = new();
    private readonly Mock<IHttpRequestService> _mockHttpRequestService = new();

    private readonly Mock<EventGridPublisherClient> _mockEventGridPublisherClient  = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly CreateUpdateEpisode _function;

    public CreateUpdateEpisodeTests()
    {
        Environment.SetEnvironmentVariable("CreateEpisodeUrl", "CreateEpisodeUrl");
        Environment.SetEnvironmentVariable("UpdateEpisodeUrl", "UpdateEpisodeUrl");
        Environment.SetEnvironmentVariable("GetEpisodeUrl", "GetEpisodeUrl");
        Environment.SetEnvironmentVariable( "topicEndpoint", "topicEndpoint");
        Environment.SetEnvironmentVariable("topicKey", "topicKey");

        _function = new CreateUpdateEpisode(_mockLogger.Object, _mockHttpRequestService.Object, _mockEventGridPublisherClient.Object);
    }

    [TestMethod]
    public async Task Run_Returns_OK_And_Creates_Episode_When_Episode_Does_Not_Exist()
    {
        // Arrange
        var episode = new Episode
        {
            EpisodeId = 123456
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockHttpRequestService.Setup(x => x.SendGet(It.IsAny<string>())).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        Mock<Response> responseMock = new Mock<Response>();
        responseMock.SetupGet(r => r.Status).Returns(200);
        Response response = responseMock.Object;

        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendGet(It.IsAny<string>()), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost("CreateEpisodeUrl", It.IsAny<string>()), Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_OK_When_Episode_Already_Exists()
    {
        // Arrange
        var episode = new Episode
        {
            EpisodeId = 234567
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        _mockHttpRequestService.Setup(x => x.SendGet(It.IsAny<string>())).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendGet(It.IsAny<string>()), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPut("UpdateEpisodeUrl", It.IsAny<string>()), Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_Processing_Episode_Fails()
    {
        // Arrange
        var episode = new Episode
        {
            EpisodeId = 1234567890
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        _mockHttpRequestService.Setup(x => x.SendGet(It.IsAny<string>())).Throws<Exception>();

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendGet(It.IsAny<string>()), Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_BadRequest_When_Episode_Data_Is_Invalid()
    {
        // Arrange
        var json = JsonSerializer.Serialize("Invalid");
        _mockRequest = _setupRequest.Setup(json);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().StartsWith("Could not read episode data.")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_GetEpisodeResponse_Is_Not_OK_Nor_NotFound()
    {
        // Arrange
        var episode = new Episode
        {
            EpisodeId = 123456
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        _mockHttpRequestService.Setup(x => x.SendGet(It.IsAny<string>())).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict));

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendGet(It.IsAny<string>()), Times.Once);
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().StartsWith("Error occurred while checking episode existence.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }


}
