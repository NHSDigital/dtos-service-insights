using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.EpisodeDataService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Data;
using Microsoft.EntityFrameworkCore;

namespace NHS.ServiceInsights.EpisodeDataServiceTests;

[TestClass]
public class UpdateEpisodeTests
{
    private readonly Mock<ILogger<UpdateEpisode>> _mockLogger = new();
    private readonly Mock<IEpisodeRepository> _mockEpisodeRepository = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly UpdateEpisode _function;
    private readonly Mock<IEndCodeLkpRepository> _mockEndCodeLkpRepository = new();
    private readonly Mock<IEpisodeTypeLkpRepository> _mockEpisodeTypeLkpRepository = new();
    private readonly Mock<IReasonClosedCodeLkpRepository> _mockReasonClosedCodeLkpRepository = new();
    private readonly Mock<IFinalActionCodeLkpRepository> _mockFinalActionCodeLkpRepository = new();


    public UpdateEpisodeTests()
    {
        _mockRequest = _setupRequest.Setup("");
        _function = new UpdateEpisode(_mockLogger.Object, _mockEpisodeRepository.Object, _mockEndCodeLkpRepository.Object, _mockEpisodeTypeLkpRepository.Object, _mockFinalActionCodeLkpRepository.Object, _mockReasonClosedCodeLkpRepository.Object);
    }

    [TestMethod]
    public async Task Run_Return_OK_When_Episode_Updated_Successfully()
    {
        // Arrange
        var episodeDto = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(22222);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(333333);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(444444);


        // Act
        var result = await _function.Run(_mockRequest.Object);

        //Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_NotFound_When_Episode_Not_Found()
    {
        // Arrange
        var episode = new Episode
        {
            EpisodeId = 000000
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync((Episode)null);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_DbUpdateException_Occurs()
    {
        // Arrange
        var episodeDto = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(22222);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(333333);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(444444);
        _mockEpisodeRepository.Setup(x => x.UpdateEpisode(It.IsAny<Episode>())).Throws(new DbUpdateException());

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_Episode_Data_Is_Invalid()
    {
        // Arrange
        var json = JsonSerializer.Serialize("Invalid");
        _mockRequest = _setupRequest.Setup(json);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_EpisodeType_Not_Found()
    {
        // Arrange
        var episodeDto = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "InvalidType",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(22222);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(333333);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(444444);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("InvalidType")).ReturnsAsync((int?)null);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }


    [TestMethod]
    public async Task Run_Return_InternalServerError_When_EndCode_Not_Found()
    {
        // Arrange
        var episodeDto = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "InvalidCode",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(333333);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(444444);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("InvalidCode")).ReturnsAsync((int?)null);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_ReasonClosedCode_Not_Found()
    {
        // Arrange
        var episodeDto = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "InvalidCode",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(22222);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("MT")).ReturnsAsync(444444);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("InvalidCode")).ReturnsAsync((int?)null);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }


    [TestMethod]
    public async Task Run_Return_InternalServerError_When_FinalActionCode_Not_Found()
    {
        // Arrange
        var episodeDto = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "InvalidCode",
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeIdAsync("C")).ReturnsAsync(11111);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeIdAsync("SC")).ReturnsAsync(22222);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedCodeIdAsync("TEST")).ReturnsAsync(333333);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeIdAsync("InvalidCode")).ReturnsAsync((int?)null);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_Exception_Is_Thrown_In_TryBlock()
    {
        // Arrange
        var episodeDto = new EpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEpisodeRepository.Setup(repo => repo.GetEpisodeAsync(245395))
            .Throws(new Exception("Error updating episode {episodeId}."));

        // Act
        var result = await _function.Run(_mockRequest.Object);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

}
