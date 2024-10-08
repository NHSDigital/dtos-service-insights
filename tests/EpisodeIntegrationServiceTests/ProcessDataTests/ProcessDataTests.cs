using Moq;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.EpisodeIntegrationService;
using Microsoft.Azure.Functions.Worker.Http;
using NHS.ServiceInsights.TestUtils;
using System.Text.Json;
using System.Collections.Specialized;
using System.Net;

namespace NHS.ServiceInsights.EpisodeIntegrationServiceTests;

[TestClass]
public class ProcessDataTests
{
    private readonly Mock<IHttpRequestService> _mockHttpRequestService = new();
    private readonly Mock<ILogger<ProcessData>> _mockLogger = new();
    private Mock<HttpRequestData> _mockRequest;
    private readonly SetupRequest _setupRequest = new();
    private readonly ProcessData _function;

    public ProcessDataTests()
    {
        Environment.SetEnvironmentVariable("EpisodeManagementUrl", "EpisodeManagementUrl");
        Environment.SetEnvironmentVariable("ParticipantManagementUrl", "ParticipantManagementUrl");

        _function = new ProcessData(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task ProcessData_ShouldSendEpisodeDataToDownstreamFunctions()
    {
        // Arrange
        string data = "\"nhs_number\",\"episode_id\",\"episode_type\",\"change_db_date_time\",\"episode_date\",\"appointment_made\",\"date_of_foa\",\"date_of_as\",\"early_recall_date\",\"call_recall_status_authorised_by\",\"end_code\",\"end_code_last_updated\",\"bso_organisation_code\",\"bso_batch_id\",\"reason_closed_code\",\"end_point\",\"final_action_code\"\n" +
        "\"9999999999\",1000,\"C\",\"2022-08-17 13:02:17.110314+01\",\"2022-08-17\",,,,,,,,\"AGA\",\"AGA000000A\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-09-02 14:30:54.121779+01\",\"2022-09-02\",,,,,,,,\"ANE\",\"ANE000000A\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-10-13 22:52:34.825602+01\",\"2022-09-02\",\"True\",\"2022-09-27\",,,\"SCREENING_OFFICE\",\"DNA\",\"2022-10-13 00:00:00+01\",\"ANE\",\"ANE000000A\",,,\n" +
        "\"9999999999\",1000,\"C\",\"2022-11-08 22:32:23.326676+00\",\"2022-08-17\",\"True\",\"2022-09-18\",\"2022-11-05\",,\"SCREENING_OFFICE\",\"SC\",\"2022-11-08 00:00:00+00\",\"AGA\",\"AGA000000A\",,,";

        _mockRequest = _setupRequest.Setup(data);
        _mockRequest.Setup(r => r.Query).Returns(new NameValueCollection
        {
            { "FileName", "episodes_test_data_20240930" }
        });

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(4));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));
    }

    [TestMethod]
    public async Task ProcessData_ShouldSendParticipantDataToDownstreamFunctions()
    {
        // Arrange
        string data = "\"change_db_date_time\",\"nhs_number\",\"superseded_nhs_number\",\"gp_practice_code\",\"bso_organisation_code\",\"next_test_due_date\",\"subject_status_code\",\"early_recall_date\",\"latest_invitation_date\",\"removal_reason\",\"removal_date\",\"reason_for_ceasing_code\",\"is_higher_risk\",\"higher_risk_next_test_due_date\",\"hr_recall_due_date\",\"higher_risk_referral_reason_code\",\"date_irradiated\",\"is_higher_risk_active\",\"gene_code\",\"ntdd_calculation_method\",\"preferred_language\"\n" +
        "\"2022-08-16 14:21:48.330694+01\",\"9999999998\",,\"A81001\",\"ANE\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"2022-08-16 14:21:48.651537+01\",\"9999999999\",,\"A81002\",\"AGA\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"2022-09-15 22:50:02.516313+01\",\"9999999999\",,\"A81002\",\"AGA\",\"2025-09-16\",\"NORMAL\",,\"2023-07-22\",,,,False,,,,,False,,\"ROUTINE\",\"EN\"\n" +
        "\"2022-10-13 22:52:34.825602+01\",\"9999999998\",,\"A81001\",\"ANE\",\"2025-09-29\",\"NORMAL\",,\"2023-02-07\",,,,False,,,,,False,,\"ROUTINE\",\n";

        _mockRequest = _setupRequest.Setup(data);
        _mockRequest.Setup(r => r.Query).Returns(new NameValueCollection
        {
            { "FileName", "subjects_test_data_20240930" }
        });

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(4));
    }

    [TestMethod]
    public async Task ProcessData_ShouldReturnBadRequestAndLogErrorIfFileNameIsInvalid()
    {
        // Arrange
        _mockRequest = _setupRequest.Setup("");
        _mockRequest.Setup(r => r.Query).Returns(new NameValueCollection
        {
            { "FileName", "invalid_file_name" }
        });

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("fileName is invalid. file name: invalid_file_name")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);

        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
    }

        [TestMethod]
    public async Task ProcessData_ShouldReturnInternalServerErrorAndLogErrorWhenCsvIsInvalid()
    {
        // Arrange
        string data = "\"change_db_date_time\",\"nhs_number\",\"superseded_nhs_number\",\"gp_practice_code\",\"bso_organisation_code\",\"next_test_due_date\",\"subject_status_code\",\"early_recall_date\",\"latest_invitation_date\",\"removal_reason\",\"removal_date\",\"reason_for_ceasing_code\",\"is_higher_risk\",\"higher_risk_next_test_due_date\",\"hr_recall_due_date\",\"higher_risk_referral_reason_code\",\"date_irradiated\",\"is_higher_risk_active\",\"gene_code\",\"ntdd_calculation_method\",\"preferred_language\"\n" +
        "\"INVALID_DATA\",\n";

        _mockRequest = _setupRequest.Setup(data);
        _mockRequest.Setup(r => r.Query).Returns(new NameValueCollection
        {
            { "FileName", "subjects_test_data_20240930" }
        });

        // Act
        var result = await _function.Run(_mockRequest.Object);

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("Error in ProcessData:")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);

        Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
    }
}
