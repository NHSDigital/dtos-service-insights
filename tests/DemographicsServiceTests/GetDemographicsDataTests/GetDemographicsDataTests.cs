using Moq;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.DemographicsService;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Common;
using System.Collections.Specialized;

namespace NHS.ServiceInsights.DemographicsServiceTests;

[TestClass]
public class GetDemographicsDataTests
{
    private Mock<ILogger<GetDemographicsData>> _mockLogger = new();
    private Mock<IHttpRequestService> _mockHttpRequestService = new();
    private GetDemographicsData _function;
    private Mock<HttpRequestData> _mockRequest = new();
    private SetupRequest _setupRequest = new();


    public GetDemographicsDataTests()
    {
        _function = new GetDemographicsData(_mockLogger.Object);
    }

    [TestMethod]
    public async Task Run_ShouldReturnBadRequest_WhenNhsNumberIsNotProvided()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            {
                "nhs_number", null
            }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((state, type) => state.ToString() == "Please enter a valid NHS Number."),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }


    [TestMethod]
    public async Task Run_ShouldReturnOk_WhenNhsNumberIsProvided()
    {
        // Arrange
        var queryParam = new NameValueCollection
        {
            {
                "nhs_number", "1111111112"
            }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
