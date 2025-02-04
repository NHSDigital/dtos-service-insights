using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.EpisodeDataService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Common;
using Azure.Messaging.EventGrid;
using Azure;
using System.Text;

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
    private readonly Mock<IHttpRequestService> _mockHttpRequestService = new();

    public CreateEpisodeTests()
    {
        _function = new CreateEpisode(_mockLogger.Object, _mockEpisodeRepository.Object, _mockEndCodeLkpRepository.Object, _mockEpisodeTypeLkpRepository.Object, _mockFinalActionCodeLkpRepository.Object, _mockReasonClosedCodeLkpRepository.Object, _mockEventGridPublisherClient.Object, _mockHttpRequestService.Object);
        Environment.SetEnvironmentVariable("GetOrganisationIdByCodeUrl", "GetOrganisationIdByCodeUrl");
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
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            OrganisationCode="LAV",
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        var mockEventGridResponce = new Mock<Response>();
        mockEventGridResponce.Setup(m => m.Status).Returns(200);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockEventGridResponce.Object));

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }



    [TestMethod]
    public async Task Run_Should_Return_InternalServiceError_When_Repository_Throw_Exception()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            OrganisationCode="LAV",
            EpisodeType = "C",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        _mockEpisodeRepository.Setup(repo => repo.CreateEpisode(It.IsAny<Episode>())).Throws<Exception>();

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServiceError_When_OrganisationCode_Not_Found()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="InvalidOrganisationCode",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=InvalidOrganisationCode"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp(episode.EpisodeType)).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp(episode.EndCode)).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp(episode.ReasonClosedCode)).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp(episode.FinalActionCode)).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_EpisodeType_Not_Found()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "InvalidType",
            OrganisationCode="LAV",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });

        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});
        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("InvalidType")).ReturnsAsync((EpisodeTypeLkp?)null);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_Ok_When_EpisodeType_Is_Empty_Or_Null()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "",
            OrganisationCode="LAV",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp(It.IsAny<string>())).ReturnsAsync((EpisodeTypeLkp?)null);
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        var mockEventGridResponce = new Mock<Response>();
        mockEventGridResponce.Setup(m => m.Status).Returns(200);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockEventGridResponce.Object));

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }


    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_EndCode_Not_Found()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
            EndCode = "InvalidType",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("InvalidType")).ReturnsAsync((EndCodeLkp?)null);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_Ok_When_EndCode_Is_Empty_Or_Null()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
            EndCode = "",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp(It.IsAny<string>())).ReturnsAsync((EndCodeLkp?)null);
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        var mockEventGridResponce = new Mock<Response>();
        mockEventGridResponce.Setup(m => m.Status).Returns(200);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockEventGridResponce.Object));

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }


    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_ReasonClosedCode_Not_Found()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
            EndCode = "SC",
            ReasonClosedCode = "InvalidType",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("InvalidType")).ReturnsAsync((ReasonClosedCodeLkp?)null);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_Ok_When_ReasonClosedCode_Is_Empty_Or_Null()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
            EndCode = "SC",
            ReasonClosedCode = "",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp(It.IsAny<string>())).ReturnsAsync((ReasonClosedCodeLkp?)null);
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        var mockEventGridResponce = new Mock<Response>();
        mockEventGridResponce.Setup(m => m.Status).Returns(200);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockEventGridResponce.Object));

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }


    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_FinalActionCode_Not_Found()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "InvalidType",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("InvalidType")).ReturnsAsync((FinalActionCodeLkp?)null);

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_Ok_When_FinalActionCode_Is_Empty_Or_Null()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp(It.IsAny<string>())).ReturnsAsync((FinalActionCodeLkp?)null);

        var mockEventGridResponce = new Mock<Response>();
        mockEventGridResponce.Setup(m => m.Status).Returns(200);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockEventGridResponce.Object));

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }


    [TestMethod]
    public async Task Run_Return_InternalServerError_When_Exception_Is_Thrown_By_Call_To_Event_Grid()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Throws(new Exception("Error sending event"));

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_Return_InternalServerError_When_Call_To_Event_Grid_Is_Not_200_OK()
    {
        // Arrange
        var episode = new InitialEpisodeDto
        {
            EpisodeId = 245395,
            EpisodeType = "C",
            OrganisationCode="LAV",
            EndCode = "SC",
            ReasonClosedCode = "TEST",
            FinalActionCode = "MT",
        };

        var json = JsonSerializer.Serialize(episode);
        _mockRequest = _setupRequest.Setup(json);

        var organisationDataJson = "{\"OrganisationId\":1,\"OrganisationCode\":\"LAV\"}";
        _mockHttpRequestService
            .Setup(service => service.SendGet($"GetOrganisationIdByCodeUrl?organisation_code=LAV"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationDataJson, Encoding.UTF8, "application/json")
            });

        _mockEpisodeTypeLkpRepository.Setup(x => x.GetEpisodeTypeLkp("C")).ReturnsAsync(new EpisodeTypeLkp { EpisodeTypeId = 1, EpisodeType = "C", EpisodeDescription = "C's description"});
        _mockEndCodeLkpRepository.Setup(x => x.GetEndCodeLkp("SC")).ReturnsAsync(new EndCodeLkp { EndCodeId = 1, EndCode = "SC", EndCodeDescription = "SC's description"});
        _mockReasonClosedCodeLkpRepository.Setup(x => x.GetReasonClosedLkp("TEST")).ReturnsAsync(new ReasonClosedCodeLkp { ReasonClosedCodeId = 1, ReasonClosedCode = "TEST", ReasonClosedCodeDescription = "TEST's description"});
        _mockFinalActionCodeLkpRepository.Setup(x => x.GetFinalActionCodeLkp("MT")).ReturnsAsync(new FinalActionCodeLkp { FinalActionCodeId = 1, FinalActionCode = "MT", FinalActionCodeDescription = "MT's description"});

        var mockEventGridResponce = new Mock<Response>();
        mockEventGridResponce.Setup(m => m.Status).Returns(404);
        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockEventGridResponce.Object));

        // Act
        var result = await _function.RunAsync(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }
}
