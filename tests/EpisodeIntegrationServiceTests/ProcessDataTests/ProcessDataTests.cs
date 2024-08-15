using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace NHS.EpisodeIntegrationServiceTests
{

    [TestClass]
    public class ProcessDataTests
    {
        private Mock<HttpMessageHandler> httpMessageHandlerMock = null!;
        private Mock<ILogger> loggerMock = null!;
        private HttpClient httpClient = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpClient = new HttpClient(httpMessageHandlerMock.Object);
            loggerMock = new Mock<ILogger>();

            typeof(ProcessData).GetField("client", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(null, httpClient);
        }

        [TestMethod]
        public async Task ProcessData_ShouldSendJsonToDownstreamFunctions()
        {
            // Arrange
            var jsonInput = @"{
            ""Participants"": [
                { ""nhs_number"": ""1111111112"" },
                { ""nhs_number"": ""1111111110"" }
            ],
            ""Episodes"": [
                { ""episode_id"": 245395 },
                { ""episode_id"": 245396 }
            ]
        }";

            var request = new DefaultHttpContext().Request;
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonInput));

            var context = new Mock<Microsoft.Azure.WebJobs.ExecutionContext>();
            context.Setup(c => c.FunctionAppDirectory).Returns(Directory.GetCurrentDirectory());

            // Setup the response to be returned by the mock HttpClient
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var result = await ProcessData.Run(request, loggerMock.Object, context.Object) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Data processed successfully.", result!.Value);

            // Verify the correct data was sent to the EpisodeManagementUrl
            httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2), // 2 episodes
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString() == "https://example.com/episode" &&
                    req.Content.Headers.ContentType!.MediaType == "application/json" &&
                    JsonConvert.SerializeObject(JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(req.Content.ReadAsStringAsync().Result))
                    == JsonConvert.SerializeObject(new List<Dictionary<string, object>> {
                    new Dictionary<string, object> { { "episode_id", 245395 } },
                    new Dictionary<string, object> { { "episode_id", 245396 } }
                    })
                ),
                ItExpr.IsAny<CancellationToken>()
            );

            // Verify the correct data was sent to the ParticipantManagementUrl
            httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2), // 2 participants
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString() == "https://example.com/participant" &&
                    req.Content.Headers.ContentType!.MediaType == "application/json" &&
                    JsonConvert.SerializeObject(JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(req.Content.ReadAsStringAsync().Result))
                    == JsonConvert.SerializeObject(new List<Dictionary<string, object>> {
                    new Dictionary<string, object> { { "nhs_number", "1111111112" } },
                    new Dictionary<string, object> { { "nhs_number", "1111111110" } }
                    })
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
