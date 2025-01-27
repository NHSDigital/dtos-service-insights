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
using Azure.Messaging.EventGrid;
using Azure;

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
    private readonly Mock<EventGridPublisherClient> _mockEventGridPublisherClient  = new();
    private readonly Mock<IOrganisationLkpRepository> _mockOrganisationLkpRepository = new();

    public UpdateEpisodeTests()
    {
        _mockRequest = _setupRequest.Setup("");
        _function = new UpdateEpisode(_mockLogger.Object, _mockEpisodeRepository.Object, _mockEndCodeLkpRepository.Object, _mockEpisodeTypeLkpRepository.Object, _mockFinalActionCodeLkpRepository.Object, _mockReasonClosedCodeLkpRepository.Object, _mockOrganisationLkpRepository.Object, _mockEventGridPublisherClient.Object);
    }

    [TestMethod]
    public async Task Run_Return_OK_When_Episode_Updated_Successfully()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
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
        _mockOrganisationLkpRepository.Setup(repo => repo.GetOrganisationByCodeAsync("LAV")).ReturnsAsync(new OrganisationLkp { OrganisationId = 1 });
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        var mockEventGridResponce = new Mock<Response>();
        mockEventGridResponce.Setup(m => m.Status).Returns(200);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockEventGridResponce.Object));

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
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
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
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp(episodeDto.EpisodeType)).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockOrganisationLkpRepository.Setup(x => x.GetOrganisationByCodeAsync(episodeDto.OrganisationCode)).ReturnsAsync(new OrganisationLkp { OrganisationId = 1});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp(episodeDto.EndCode)).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp(episodeDto.ReasonClosedCode)).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp(episodeDto.FinalActionCode)).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        _mockEpisodeRepository.Setup(x => x.UpdateEpisode(It.IsAny<Episode>())).Throws(new DbUpdateException());

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
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
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_OrganisationCode_Not_Found()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode = "InvalidOrganisationCode",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(new Episode { EpisodeId = 245395 });
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description" });
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description" });
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description" });
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description" });
        _mockOrganisationLkpRepository.Setup(x => x.GetOrganisationByCodeAsync("InvalidOrganisationCode")).ReturnsAsync((OrganisationLkp)null);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_EpisodeType_Not_Found()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "InvalidType",
            OrganisationCode="LAV",
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
         _mockOrganisationLkpRepository.Setup(repo => repo.GetOrganisationByCodeAsync("LAV")).ReturnsAsync(new OrganisationLkp { OrganisationId = 1 });
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("InvalidType")).ReturnsAsync((EpisodeTypeLkp?)null);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }


    [TestMethod]
    public async Task Run_Return_InternalServerError_When_EndCode_Not_Found()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            EndCode = "InvalidCode",
            OrganisationCode="LAV",
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
         _mockOrganisationLkpRepository.Setup(repo => repo.GetOrganisationByCodeAsync("LAV")).ReturnsAsync(new OrganisationLkp { OrganisationId = 1 });
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("InvalidType")).ReturnsAsync((EndCodeLkp?)null);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_ReasonClosedCode_Not_Found()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
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
         _mockOrganisationLkpRepository.Setup(repo => repo.GetOrganisationByCodeAsync("LAV")).ReturnsAsync(new OrganisationLkp { OrganisationId = 1 });
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("InvalidType")).ReturnsAsync((ReasonClosedCodeLkp?)null);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }


    [TestMethod]
    public async Task Run_Return_InternalServerError_When_FinalActionCode_Not_Found()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
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
        _mockOrganisationLkpRepository.Setup(repo => repo.GetOrganisationByCodeAsync("LAV")).ReturnsAsync(new OrganisationLkp { OrganisationId = 1 });
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("InvalidType")).ReturnsAsync((FinalActionCodeLkp?)null);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_Exception_Is_Thrown_In_TryBlock()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
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
        _mockEpisodeRepository.Setup(repo => repo.GetEpisodeAsync(245395)).Throws(new Exception("Error updating episode {episodeId}."));

        // Act
        var result = await _function.Run(_mockRequest.Object);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_Exception_Is_Thrown_By_Call_To_Event_Grid()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
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
        _mockOrganisationLkpRepository.Setup(repo => repo.GetOrganisationByCodeAsync("LAV")).ReturnsAsync(new OrganisationLkp { OrganisationId = 1 });
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Throws(new Exception("Error sending event"));

        // Act
        var result = await _function.Run(_mockRequest.Object);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_Call_To_Event_Grid_Is_Not_200_OK()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
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
         _mockOrganisationLkpRepository.Setup(repo => repo.GetOrganisationByCodeAsync("LAV")).ReturnsAsync(new OrganisationLkp { OrganisationId = 1 });

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        var mockEventGridResponce = new Mock<Response>();
        mockEventGridResponce.Setup(m => m.Status).Returns(404);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockEventGridResponce.Object));

        // Act
        var result = await _function.Run(_mockRequest.Object);
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_ShouldUpdate_When_DtoHasNewerProcessedDateTime()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto { SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1) };
        var episode = new Episode { SrcSysProcessedDatetime = DateTime.UtcNow };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);

        // Act
        await _function.Run(_mockRequest.Object);

        // Assert
        _mockEpisodeRepository.Verify(x => x.UpdateEpisode(It.IsAny<Episode>()), Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldNotUpdate_When_DtoHasOlderProcessedDateTime()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto { SrcSysProcessedDateTime = DateTime.UtcNow };
        var episode = new Episode { SrcSysProcessedDatetime = DateTime.UtcNow.AddDays(1) };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);

        // Act
        await _function.Run(_mockRequest.Object);

        // Assert
        _mockEpisodeRepository.Verify(x => x.UpdateEpisode(It.IsAny<Episode>()), Times.Never);
    }
}
