using Moq;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.EpisodeManagementService;
using NHS.ServiceInsights.TestUtils;
using System.Collections.Specialized;

namespace NHS.ServiceInsights.EpisodeManagementServiceTests;

[TestClass]
public class GetEpisodeReferenceDataTests
{
    private Mock<ILogger<GetEpisodeReferenceData>> _mockLogger;
    private Mock<IHttpRequestService> _mockHttpRequestService;
    private Mock<HttpRequestData> _mockRequest;
    private SetupRequest _setupRequest;
    private GetEpisodeReferenceData _function;

    public GetEpisodeReferenceDataTests()
    {
        _mockLogger = new Mock<ILogger<GetEpisodeReferenceData>>();
        _mockHttpRequestService = new Mock<IHttpRequestService>();
        _mockRequest = new Mock<HttpRequestData>();
        _setupRequest = new SetupRequest();
        _function = new GetEpisodeReferenceData(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task Run_Logs_Information_Message_When_Request_Is_Processed()
    {
        // Arrange
        var queryParam = new NameValueCollection();

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(log => log.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Request to retrieve reference data has been processed.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ), Times.Once);
    }


    [TestMethod]
    public async Task Run_Returns_OK_Response_When_Reference_Data_Is_Retrieved_Successfully()
    {
        // Arrange
        var queryParam = new NameValueCollection();
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"key\":\"value\"}") };

        _mockRequest = _setupRequest.SetupGet(queryParam);
        _mockHttpRequestService.Setup(s => s.SendGet(It.IsAny<string>())).ReturnsAsync(response);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_ReturnsErrorResponse_When_Reference_Data_Is_Not_Retrieved_Successfully()
    {
        // Arrange
        var queryParam = new NameValueCollection();
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("{\"key\":\"value\"}") };

        _mockRequest = _setupRequest.SetupGet(queryParam);
        _mockHttpRequestService.Setup(s => s.SendGet(It.IsAny<string>())).ReturnsAsync(response);

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [TestMethod]
    public async Task Run_LogsError_When_Exception_Is_Thrown()
    {
        // Arrange
        var queryParam = new NameValueCollection();
        // var exception = new Exception("Test exception");

        _mockRequest = _setupRequest.SetupGet(queryParam);
        _mockHttpRequestService.Setup(s => s.SendGet(It.IsAny<string>())).Throws<Exception>();

        // Act
        await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(log => log.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Failed to call the Retrieve Episode Reference Data Service")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ), Times.Once);
    }


    [TestMethod]
    public async Task Run_ReturnsInternalServerErrorResponse_When_Exception_Is_Thrown()
    {
        // Arrange
        var queryParam = new NameValueCollection();
        // var exception = new Exception("Test exception");

        _mockRequest = _setupRequest.SetupGet(queryParam);
        _mockHttpRequestService.Setup(s => s.SendGet(It.IsAny<string>())).Throws<Exception>();

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }
}
