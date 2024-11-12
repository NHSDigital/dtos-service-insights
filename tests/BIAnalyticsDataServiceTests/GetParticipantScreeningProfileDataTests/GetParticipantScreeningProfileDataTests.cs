using Moq;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Specialized;
using NHS.ServiceInsights.TestUtils;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.BIAnalyticsDataService;

namespace NHS.ServiceInsights.BIAnalyticsDataServiceTests;

[TestClass]
public class GetParticipantScreeningProfileDataTests
{
    private readonly Mock<ILogger<GetParticipantScreeningProfileData>> _mockLogger = new();
    private readonly Mock<IParticipantScreeningProfileRepository> _mockParticipantScreeningProfileRepository = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly GetParticipantScreeningProfileData _function;

    private readonly ProfilesDataPage profilesDataPage = new ProfilesDataPage
    {
        TotalResults = 2,
        TotalPages = 1,
        TotalRemainingPages = 0,
        Profiles = new List<ParticipantScreeningProfile>
        {
            new ParticipantScreeningProfile
            {
                Id = 1,
                NhsNumber = 1234567890,
                ScreeningName = "John Doe",
                RecordInsertDatetime = DateTime.Parse("2023-07-05 10:30:00")
            },
            new ParticipantScreeningProfile
            {
                Id = 2,
                NhsNumber = 9876543210,
                ScreeningName = "Jane Smith",
                RecordInsertDatetime = DateTime.Parse("2023-07-05 11:30:00")
            }
        }
    };

    public GetParticipantScreeningProfileDataTests()
    {
        _function = new GetParticipantScreeningProfileData(_mockLogger.Object, _mockParticipantScreeningProfileRepository.Object);
    }

    [TestMethod]
    public async Task Run_Should_Return_OK_With_ProfilesDataPage_Json_Data()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "page", "1" },
            { "pageSize", "2" },
            { "startDate", "2023-07-05 08:30:00" },
            { "endDate", "2023-07-05 08:30:00" }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);
        _mockParticipantScreeningProfileRepository.Setup(r => r.GetParticipantProfile(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>())).Returns(Task.FromResult(profilesDataPage));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Information),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetParticipantScreeningProfileData: Participant profiles found successfully.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        response.Body.Seek(0, SeekOrigin.Begin);
        var profilesDataPageResult = await JsonSerializer.DeserializeAsync<ProfilesDataPage>(response.Body);
        Assert.IsTrue(profilesDataPageResult.Profiles.Count() == 2);
        Assert.IsTrue(profilesDataPageResult.TotalResults == 2);
        Assert.IsTrue(profilesDataPageResult.TotalPages == 1);
        Assert.IsTrue(profilesDataPageResult.TotalRemainingPages == 0);
    }

    [TestMethod]
    public async Task Run_Should_Return_NotFound_When_It_Doesnt_Find_Any_Profiles()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "page", "1" },
            { "pageSize", "2" },
            { "startDate", "2023-07-05 08:30:00" },
            { "endDate", "2023-07-05 08:30:00" }
        };

        var emptyprofilesDataPage = new ProfilesDataPage(){
            TotalResults = 0,
            TotalPages = 0,
            TotalRemainingPages = 0,
            Profiles = new List<ParticipantScreeningProfile>()
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);
        _mockParticipantScreeningProfileRepository.Setup(r => r.GetParticipantProfile(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>())).Returns(Task.FromResult(emptyprofilesDataPage));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Information),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetParticipantScreeningProfileData: Could not find any participant profiles.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Run_Should_Return_InternalServerError_When_Exception_Is_Thrown()
    {
        // Arrange
        var queryParam = new NameValueCollection()
        {
            { "page", "1" },
            { "pageSize", "2" },
            { "startDate", "2023-07-05 08:30:00" },
            { "endDate", "2023-07-05 08:30:00" }
        };

        _mockRequest = _setupRequest.SetupGet(queryParam);
        _mockParticipantScreeningProfileRepository.Setup(r => r.GetParticipantProfile(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>())).Throws(new Exception("Database error"));

        // Act
        var response = await _function.Run(_mockRequest.Object);

        // Assert
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

        _mockLogger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error),
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetParticipantScreeningProfileData: Failed to get participant profiles from the database.")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
    }
}
