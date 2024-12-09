using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.EpisodeDataService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using Azure.Messaging.EventGrid;

namespace NHS.ServiceInsights.EpisodeDataServiceTests;

[TestClass]
public class CreateEpisodeTests
{
    private readonly Mock<ILogger<CreateEpisode>> _mockLogger = new();
    private readonly Mock<IEpisodeRepository> _mockEpisodeRepository = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly CreateEpisode _function;
    private readonly Mock<IEndCodeLkpRepository> _mockEndCodeLkpRepository = new();
    private readonly Mock<IEpisodeTypeLkpRepository> _mockEpisodeTypeLkpRepository = new();
    private readonly Mock<IReasonClosedCodeLkpRepository> _mockReasonClosedCodeLkpRepository = new();
    private readonly Mock<IFinalActionCodeLkpRepository> _mockFinalActionCodeLkpRepository = new();
    private readonly Mock<EventGridPublisherClient> _mockEventGridPublisherClient  = new();

    public CreateEpisodeTests()
    {
        _function = new CreateEpisode(_mockLogger.Object, _mockEpisodeRepository.Object, _mockEndCodeLkpRepository.Object, _mockEpisodeTypeLkpRepository.Object, _mockFinalActionCodeLkpRepository.Object, _mockReasonClosedCodeLkpRepository.Object,  _mockEventGridPublisherClient.Object);
    }

    [TestMethod]
    public async Task Run_Should_Return_BadRequest_When_Episode_Is_Invalid()
    {
        // Arrange
        var json = JsonSerializer.Serialize("Invalid episode");
        _mockRequest = _setupRequest.Setup(json);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_OK_When_Repository_Creates_Episode()
    {
        // Arrange
        var episode = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(22222);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(333333);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(444444);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServiceError_When_Repository_Throw_Exception()
    {
        // Arrange
        var episode = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(22222);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(333333);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(444444);

        _mockEpisodeRepository.Setup(repo => repo.CreateEpisode(It.IsAny<Episode>())).Throws<Exception>();

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_EpisodeType_Not_Found()
    {
        // Arrange
        var episode = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "InvalidType",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(22222);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(33333);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(44444);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("InvalidType")).ReturnsAsync((int?)null);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_Ok_When_EpisodeType_Is_Empty_Or_Null()
    {
        // Arrange
        var episode = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("")).ReturnsAsync((int?)null);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(22222);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(333333);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(444444);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_EndCode_Not_Found()
    {
        // Arrange
        var episode = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "InvalidType",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(22222);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(33333);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("InvalidType")).ReturnsAsync((int?)null);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_Ok_When_EndCode_Is_Empty_Or_Null()
    {
        // Arrange
        var episode = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = " ",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync(" ")).ReturnsAsync((int?)null);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(333333);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(444444);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_ReasonClosedCode_Not_Found()
    {
        // Arrange
        var episode = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "InvalidType",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(22222);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(44444);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("InvalidType")).ReturnsAsync((int?)null);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_Ok_When_ReasonClosedCode_Is_Empty_Or_Null()
    {
        // Arrange
        var episode = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(22222);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("")).ReturnsAsync((int?)null);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(444444);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_FinalActionCode_Not_Found()
    {
        // Arrange
        var episode = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "InvalidType",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(33333);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(44444);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("InvalidType")).ReturnsAsync((int?)null);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_Ok_When_FinalActionCode_Is_Empty_Or_Null()
    {
        // Arrange
        var episode = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = " ",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(22222);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(333333);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync(" ")).ReturnsAsync((int?)null);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

}
