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
public class GetScreeningDataTests
{
    private Mock<ILogger<GetScreeningData>> _mockLogger = new();
    private Mock<IScreeningLkpRepository> _mockScreeningLkpRepository = new();
    private GetScreeningData _function;
    private Mock<HttpRequestData> _mockRequest = new();
    private SetupRequest _setupRequest = new();

    public GetScreeningDataTests()
    {
        _function = new GetScreeningData(_mockLogger.Object, _mockScreeningLkpRepository.Object);
    }

    [TestMethod]
    public async Task Run_ShouldReturnBadRequest_WhenScreeningIdIsNotProvided()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "screening_id", null }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Missing or invalid screening ID.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldReturnNotFound_WhenScreeningIsNotFound()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "screening_id", "12345" }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        _mockScreeningLkpRepository.Setup(repo => repo.GetScreeningAsync(12345)).ReturnsAsync((ScreeningLkp)null);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("screening not found.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Run_ShouldReturnOk_WhenScreeningIsFound()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            { "screening_id", "245395" }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        var screeningLkp = new ScreeningLkp
        {
            ScreeningId = 12345,
            ScreeningName = "",
            ScreeningType = "",
            ScreeningAcronym = "",
            ScreeningWorkflowId = ""
        };

        _mockScreeningLkpRepository.Setup(repo => repo.GetScreeningAsync(245395)).ReturnsAsync(screeningLkp);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        response.Body.Seek(0, SeekOrigin.Begin);
        var screeningResponse = await JsonSerializer.DeserializeAsync<ScreeningLkp>(response.Body);
        Assert.AreEqual<long>(12345, screeningResponse.ScreeningId);

        _mockLogger.Verify(log => log.Log(
            LogLevel.Information,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("screening found successfully.")),
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
            { "screening_id", "245395" }
        };
        _mockRequest = _setupRequest.SetupGet(queryParam);

        _mockScreeningLkpRepository.Setup(repo => repo.GetScreeningAsync(245395))
            .Throws(new Exception("Database error"));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Failed to get screening from the db") &&
                                                    state.ToString().Contains("Exception: Database error")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
