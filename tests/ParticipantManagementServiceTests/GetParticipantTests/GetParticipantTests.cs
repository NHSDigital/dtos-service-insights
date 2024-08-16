using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NHS.ServiceInsights.ParticipantManagementService;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Tests
{
    [TestClass]
    public class GetParticipantTests
    {
        private Mock<ILogger<GetParticipant>> _mockLogger;
        private GetParticipant _function;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<GetParticipant>>();
            _function = new GetParticipant(_mockLogger.Object);
        }

        [TestMethod]
        public async Task Run_ShouldReturnBadRequest_WhenNhsNumberIsNotProvided()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict);
            mockRequest.Setup(req => req.Query["nhs_number"]).Returns((string)null);
            mockRequest.Setup(req => req.CreateResponse(HttpStatusCode.BadRequest)).Returns(new MockHttpResponseData(HttpStatusCode.BadRequest));

            // Act
            var response = await _function.Run(mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            _mockLogger.Verify(log => log.LogError("Please enter a valid NHS Number."), Times.Once);
        }

        [TestMethod]
        public async Task Run_ShouldReturnNotFound_WhenParticipantIsNotFound()
        {
            // Arrange
            var nhs_number = "1234567890";
            var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict);
            mockRequest.Setup(req => req.Query["nhs_number"]).Returns(nhs_number);
            mockRequest.Setup(req => req.CreateResponse(HttpStatusCode.NotFound)).Returns(new MockHttpResponseData(HttpStatusCode.NotFound));

            // Act
            var response = await _function.Run(mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            _mockLogger.Verify(log => log.LogError($"Participant with NHS Number {nhs_number} not found."), Times.Once);
        }

        [TestMethod]
        public async Task Run_ShouldReturnOk_WhenParticipantIsFound()
        {
            // Arrange
            var nhsNumber = "1111111110";
            var participant = new Participant { nhs_number = nhsNumber };
            var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict);
            mockRequest.Setup(req => req.Query["nhs_number"]).Returns(nhsNumber);
            mockRequest.Setup(req => req.CreateResponse(HttpStatusCode.OK)).Returns(new MockHttpResponseData(HttpStatusCode.OK));

            // Act
            var response = await _function.Run(mockRequest.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            _mockLogger.Verify(log => log.LogInformation("Request to retrieve a participant has been processed."), Times.Once);
        }

        // Mock HttpResponseData class
        public class MockHttpResponseData : HttpResponseData
        {
            public MockHttpResponseData(HttpStatusCode statusCode) : base(new Mock<FunctionContext>().Object)
            {
                StatusCode = statusCode;
                Headers = new HttpHeadersCollection();
            }

            public override HttpStatusCode StatusCode { get; set; }
            public override HttpHeadersCollection Headers { get; set; }
            public override Stream Body { get; set; } = new MemoryStream();
            public override HttpCookies Cookies { get; } = new Mock<HttpCookies>().Object;
        }
    }
}
