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
using System.Text;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.EpisodeDataServiceTests;

[TestClass]
public class UpdateEpisodeTests
{
    private readonly Mock<ILogger<UpdateEpisode>> _mockLogger = new();
    private readonly Mock<IEpisodeRepository> _mockEpisodeRepository = new();
    private Mock<HttpRequestData> _mockRequest = new();
    private readonly SetupRequest _setupRequest = new();
    private readonly UpdateEpisode _function;
    private readonly Mock<IEndCodeLkpRepository> _mockEndCodeLkpRepository = new();
    private readonly Mock<IEpisodeTypeLkpRepository> _mockEpisodeTypeLkpRepository = new();
    private readonly Mock<IFinalActionCodeLkpRepository> _mockFinalActionCodeLkpRepository = new();
    private readonly Mock<IReasonClosedCodeLkpRepository> _mockReasonClosedCodeLkpRepository = new();
    private readonly Mock<EventGridPublisherClient> _mockEventGridPublisherClient = new();
    private readonly Mock<Response> _mockEventGridResponse = new();
    private readonly Mock<IHttpRequestService> _mockHttpRequestService = new();



    public UpdateEpisodeTests()
    {
        var episodeTypeLkpRepository = new EpisodeLkpRepository(_mockEndCodeLkpRepository.Object, _mockEpisodeTypeLkpRepository.Object, _mockFinalActionCodeLkpRepository.Object, _mockReasonClosedCodeLkpRepository.Object);

        _function = new UpdateEpisode(_mockLogger.Object, _mockEpisodeRepository.Object, episodeTypeLkpRepository, _mockEventGridPublisherClient.Object, _mockHttpRequestService.Object);

        Environment.SetEnvironmentVariable("CheckParticipantExistsUrl", "CheckParticipantExistsUrl");

        // _mockRequest = _setupRequest.Setup("");
        // _function = new UpdateEpisode(_mockLogger.Object, _mockEpisodeRepository.Object, _mockEndCodeLkpRepository.Object, _mockEpisodeTypeLkpRepository.Object, _mockFinalActionCodeLkpRepository.Object, _mockReasonClosedCodeLkpRepository.Object,  _mockEventGridPublisherClient.Object, _mockHttpRequestService.Object);
        Environment.SetEnvironmentVariable("GetOrganisationIdByCodeUrl", "GetOrganisationIdByCodeUrl");

    }

    [TestMethod]
    public async Task Run_Return_OK_When_Episode_Updated_Successfully()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            NhsNumber = 9990000000,
            EpisodeType = "C",
            OrganisationCode="LAV",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
            SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1)
        };
        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        var organisationId = 1;
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});
        _mockHttpRequestService.Setup(x => x.SendGet($"CheckParticipantExistsUrl?NhsNumber={episodeDto.NhsNumber}&ScreeningId=1")).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        _mockEventGridResponse.Setup(m => m.Status).Returns(200);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_mockEventGridResponse.Object));
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
            SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1)
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp(episodeDto.EpisodeType)).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
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
            SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1)
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(new Episode { EpisodeId = 245395 });
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description" });
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description" });
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description" });
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description" });

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
            SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1)
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
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
            SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1)
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("InvalidCode")).ReturnsAsync((EndCodeLkp?)null);
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
            SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1)
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("InvalidCode")).ReturnsAsync((ReasonClosedCodeLkp?)null);
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
            SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1)
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("InvalidCode")).ReturnsAsync((FinalActionCodeLkp?)null);
        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);

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
            SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1)
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);
        _mockEpisodeRepository.Setup(repo => repo.GetEpisodeAsync(245395)).Throws(new Exception("Error updating episode {episodeId}."));

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
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
            SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1)
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        _mockHttpRequestService.Setup(x => x.SendGet($"CheckParticipantExistsUrl?NhsNumber={episode.NhsNumber}&ScreeningId=1")).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Throws(new Exception("Error sending event"));
        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
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
            SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1)
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);
        var episode = new Episode
        {
            EpisodeId = 245395
        };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        _mockHttpRequestService.Setup(x => x.SendGet($"CheckParticipantExistsUrl?NhsNumber={episode.NhsNumber}&ScreeningId=1")).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        _mockEventGridResponse.Setup(m => m.Status).Returns(404);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_mockEventGridResponse.Object));

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }


    [TestMethod]
    public async Task Run_ShouldUpdate_When_DtoHasNewerProcessedDateTime()
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            OrganisationCode="LAV",
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
            SrcSysProcessedDateTime = DateTime.UtcNow.AddDays(1)
        };
        var episode = new Episode
        {
            EpisodeId = 245395,
            OrganisationId = 1,
            EpisodeTypeId = 1,
            EndCodeId = 1,
            ReasonClosedCodeId = 1,
            FinalActionCodeId = 1,
            SrcSysProcessedDatetime = DateTime.UtcNow,
        };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});


        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        _mockHttpRequestService.Setup(x => x.SendGet($"CheckParticipantExistsUrl?NhsNumber={episode.NhsNumber}&ScreeningId=1")).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

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

        _mockHttpRequestService.Setup(x => x.SendGet($"CheckParticipantExistsUrl?NhsNumber={episode.NhsNumber}&ScreeningId=1")).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _function.Run(_mockRequest.Object);

        // Assert
        _mockEpisodeRepository.Verify(x => x.UpdateEpisode(It.IsAny<Episode>()), Times.Never);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task Run_Should_Flag_Exception_When_Participant_Check_Fails(HttpStatusCode statusCode)
    {
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            NhsNumber = 9990000000,
            EpisodeType = "C",
            OrganisationCode="LAV",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
            SrcSysProcessedDateTime = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);

        var episode = new Episode
        {
            EpisodeId = 245395,
            NhsNumber = 9990000000,
            SrcSysProcessedDatetime = DateTime.UtcNow.AddDays(-1)
        };

        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);


        _mockHttpRequestService.Setup(x => x.SendGet($"CheckParticipantExistsUrl?NhsNumber={episode.NhsNumber}&ScreeningId=1")).ReturnsAsync(new HttpResponseMessage(statusCode));

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});


        _mockEventGridResponse.Setup(m => m.Status).Returns(200);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_mockEventGridResponse.Object));

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockEpisodeRepository.Verify(x => x.UpdateEpisode(It.Is<Episode>(e => e.ExceptionFlag == 1)), Times.Once);
        _mockEventGridPublisherClient.Verify(x => x.SendEventAsync(It.Is<EventGridEvent>(e => e.Data.ToObjectFromJson<FinalizedEpisodeDto>(null).ExceptionFlag == 1), It.IsAny<CancellationToken>()), Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Not_Flag_Exception_When_Participant_Does_Exist()
{
        // Arrange
        var episodeDto = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            NhsNumber = 9990000000,
            EpisodeType = "C",
            OrganisationCode="LAV",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
            SrcSysProcessedDateTime = DateTime.UtcNow
        };
        var json = JsonSerializer.Serialize(episodeDto);
        _mockRequest = _setupRequest.Setup(json);

        var episode = new Episode
        {
            EpisodeId = 245395,
            NhsNumber = 9990000000,
            SrcSysProcessedDatetime = DateTime.UtcNow.AddDays(-1)
        };
        _mockEpisodeRepository.Setup(x => x.GetEpisodeAsync(It.IsAny<long>())).ReturnsAsync(episode);

        _mockHttpRequestService.Setup(x => x.SendGet($"CheckParticipantExistsUrl?NhsNumber={episode.NhsNumber}&ScreeningId=1")).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var organisationId = 1;
        _mockHttpRequestService.Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code={episodeDto.OrganisationCode}"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK){Content = new StringContent(organisationId.ToString(), Encoding.UTF8, "application/json")});

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        _mockEventGridResponse.Setup(m => m.Status).Returns(200);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(_mockEventGridResponse.Object));

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockEpisodeRepository.Verify(x => x.UpdateEpisode(It.Is<Episode>(e => e.ExceptionFlag == 0)), Times.Once);
        _mockEventGridPublisherClient.Verify(x => x.SendEventAsync(It.Is<EventGridEvent>(e => e.Data.ToObjectFromJson<FinalizedEpisodeDto>(null).ExceptionFlag == 0), It.IsAny<CancellationToken>()), Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

}
