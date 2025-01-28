using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NHS.ServiceInsights.ReferenceDataService;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using NHS.ServiceInsights.TestUtils;

namespace NHS.ServiceInsights.ReferenceDataServiceTests;

[TestClass]
public class GetReferenceDataTests
{
    private Mock<ILogger<GetReferenceData>> _mockLogger = new();
    private Mock<IOrganisationLkpRepository> _mockOrganisationLkpRepository = new();
    private GetReferenceData _function;
    private Mock<HttpRequestData> _mockRequest = new();
    private SetupRequest _setupRequest = new();

    public GetReferenceDataTests()
    {
        _function = new GetReferenceData(_mockLogger.Object, _mockOrganisationLkpRepository.Object);
    }

    [TestMethod]
    public async Task Run_ShouldReturnBadRequest_WhenOrganisationCodeIsNotProvided()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "organisation_code", null }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.GetOrganisationIdByCode(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Missing or invalid organisation Code.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldReturnBadRequest_WhenOrganisationIdIsNotProvided()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "organisation_id", null }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Missing or invalid organisation ID.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldReturnNotFound_WhenOrganisationIsNotFound()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "organisation_id", "12345" }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        _mockOrganisationLkpRepository.Setup(repo => repo.GetOrganisationAsync(12345)).ReturnsAsync((OrganisationLkp)null);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("organisation not found.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldReturnOk_WhenEpisodeIsFound()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "organisation_id", "245395" }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        var organisationLkp = new OrganisationLkp
        {
            OrganisationId = 12345,
            ScreeningName = "",
            OrganisationCode = "",
            OrganisationName = "",
            OrganisationType = "",
            IsActive = ""
        };

        _mockOrganisationLkpRepository.Setup(repo => repo.GetOrganisationAsync(245395)).ReturnsAsync(organisationLkp);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        response.Body.Seek(0, SeekOrigin.Begin);
        var organisationResponse = await JsonSerializer.DeserializeAsync<OrganisationLkp>(response.Body);
        Assert.AreEqual<long>(12345, organisationResponse.OrganisationId);

        _mockLogger.Verify(log => log.Log(
            LogLevel.Information,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("organisation found successfully.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "organisation_id", "245395" }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        _mockOrganisationLkpRepository.Setup(repo => repo.GetOrganisationAsync(245395))
            .Throws(new Exception("Database error"));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Failed to get organisation from the db") &&
                                                    state.ToString().Contains("Exception: Database error")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Run_Logs_Information_Message_When_Getting_Organisation_Reference_Data()
    {
        // Arrange
        var queryParam = new NameValueCollection();

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        await _function.Run2(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Retrieving Organisation Reference Data... ")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ), Times.Once);
    }

    [TestMethod]
    public async Task Run_Returns_Ok_Response_When_Data_Is_Retrieved_Successfully()
    {
        // Arrange
        var queryParam = new NameValueCollection();
        var organisationData = new List<OrganisationLkp> { new OrganisationLkp { OrganisationCode = "LAV", OrganisationId = 1 } };


        _mockOrganisationLkpRepository.Setup(repo => repo.GetAllOrganisationsAsync()).ReturnsAsync(organisationData);

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run2(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task Run_Returns_InternalServerError_Response_When_Exception_Is_Thrown()
    {
        // Arrange
        var queryParam = new NameValueCollection();

        _mockRequest = _setupRequest.SetupGet(queryParam);

        _mockOrganisationLkpRepository.Setup(repo => repo.GetAllOrganisationsAsync()).Throws<Exception>();


        // Act
        var response = await _function.Run2(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]

    public async Task Run_Logs_Error_When_Exception_Is_Thrown()
    {
        // Arrange
        var queryParam = new NameValueCollection();

        _mockRequest = _setupRequest.SetupGet(queryParam);

        _mockOrganisationLkpRepository.Setup(repo => repo.GetAllOrganisationsAsync()).Throws<Exception>();

        // Act
        await _function.Run2(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Failed to retrieve all organisation reference data.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ), Times.Once);
    }
}
