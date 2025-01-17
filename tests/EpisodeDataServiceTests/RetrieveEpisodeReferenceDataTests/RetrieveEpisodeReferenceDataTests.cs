using Moq;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.TestUtils;
using System.Collections.Specialized;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.EpisodeDataService;

namespace NHS.ServiceInsights.EpisodeDataServiceTests;

[TestClass]
public class RetrieveEpisodeReferenceDataTests
{
    private Mock<ILogger<RetrieveEpisodeReferenceData>> _mockLogger = new();
    private Mock<IEndCodeLkpRepository> _mockEndCodeLkpRepository = new();
    private Mock<IEpisodeTypeLkpRepository> _mockEpisodeTypeLkpRepository = new();
    private Mock<IFinalActionCodeLkpRepository> _mockFinalActionCodeLkpRepository = new();
    private Mock<IReasonClosedCodeLkpRepository> _mockReasonClosedCodeLkpRepository = new();
    private RetrieveEpisodeReferenceData _function;
    private Mock<HttpRequestData> _mockRequest = new();
    private SetupRequest _setupRequest = new();

    public RetrieveEpisodeReferenceDataTests()
    {
        _function = new RetrieveEpisodeReferenceData(_mockLogger.Object, _mockEndCodeLkpRepository.Object, _mockEpisodeTypeLkpRepository.Object, _mockFinalActionCodeLkpRepository.Object, _mockReasonClosedCodeLkpRepository.Object);
    }

    [TestMethod]
    public async Task Run_Logs_Information_Message_When_Getting_Episode_Reference_Data()
    {
        // Arrange
        var queryParam = new NameValueCollection();

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Retrieving Episode Reference Data...")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ), Times.Once);
    }




    [TestMethod]
    public async Task Run_Returns_Ok_Response_When_Data_Is_Retrieved_Successfully()
    {
        // Arrange
        var queryParam = new NameValueCollection();
        var endCodes = new List<EndCodeLkp> { new EndCodeLkp { EndCode = "Code1", EndCodeDescription = "Description1" } };
        var episodeTypes = new List<EpisodeTypeLkp> { new EpisodeTypeLkp { EpisodeType = "Type1", EpisodeDescription = "Description1" } };
        var finalActionCodes = new List<FinalActionCodeLkp> { new FinalActionCodeLkp { FinalActionCode = "Code1", FinalActionCodeDescription = "Description1" } };
        var reasonClosedCodes = new List<ReasonClosedCodeLkp> { new ReasonClosedCodeLkp { ReasonClosedCode = "Code1", ReasonClosedCodeDescription = "Description1" } };

        _mockEndCodeLkpRepository.Setup(repo => repo.GetAllEndCodesAsync()).ReturnsAsync(endCodes);
        _mockEpisodeTypeLkpRepository.Setup(repo => repo.GetAllEpisodeTypesAsync()).ReturnsAsync(episodeTypes);
        _mockFinalActionCodeLkpRepository.Setup(repo => repo.GetAllFinalActionCodesAsync()).ReturnsAsync(finalActionCodes);
        _mockReasonClosedCodeLkpRepository.Setup(repo => repo.GetAllReasonClosedCodesAsync()).ReturnsAsync(reasonClosedCodes);

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }


    [TestMethod]
    public async Task Run_Returns_InternalServerError_Response_When_Exception_Is_Thrown()
    {
        // Arrange
        var queryParam = new NameValueCollection();

        _mockRequest = _setupRequest.SetupGet(queryParam);

        _mockEndCodeLkpRepository.Setup(repo => repo.GetAllEndCodesAsync()).Throws<Exception>();
        _mockEpisodeTypeLkpRepository.Setup(repo => repo.GetAllEpisodeTypesAsync()).Throws<Exception>();
        _mockFinalActionCodeLkpRepository.Setup(repo => repo.GetAllFinalActionCodesAsync()).Throws<Exception>();
        _mockReasonClosedCodeLkpRepository.Setup(repo => repo.GetAllReasonClosedCodesAsync()).Throws<Exception>();

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }


    [TestMethod]

    public async Task Run_Logs_Error_When_Exception_Is_Thrown()
    {
        // Arrange
        var queryParam = new NameValueCollection();

        _mockRequest = _setupRequest.SetupGet(queryParam);

        _mockEndCodeLkpRepository.Setup(repo => repo.GetAllEndCodesAsync()).Throws<Exception>();
        _mockEpisodeTypeLkpRepository.Setup(repo => repo.GetAllEpisodeTypesAsync()).Throws<Exception>();
        _mockFinalActionCodeLkpRepository.Setup(repo => repo.GetAllFinalActionCodesAsync()).Throws<Exception>();
        _mockReasonClosedCodeLkpRepository.Setup(repo => repo.GetAllReasonClosedCodesAsync()).Throws<Exception>();

        // Act
        await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Failed to retrieve data from the db.\nException:")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ), Times.Once);
    }
}

