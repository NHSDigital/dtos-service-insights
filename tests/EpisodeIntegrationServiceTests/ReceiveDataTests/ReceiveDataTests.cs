using Moq;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Data;
using System.Text;

namespace NHS.ServiceInsights.EpisodeIntegrationServiceTests;

[TestClass]
public class ReceiveDataTests
{
    private readonly Mock<IHttpRequestService> _mockHttpRequestService = new();
    private readonly Mock<ILogger<EpisodeIntegrationService.ReceiveData>> _mockLogger = new();
    private readonly EpisodeIntegrationService.ReceiveData _function;
    private readonly Mock<IEndCodeLkpRepository> _mockEndCodeLkpRepository = new();
    private readonly Mock<IEpisodeTypeLkpRepository> _mockEpisodeTypeLkpRepository = new();

    public ReceiveDataTests()
    {
        Environment.SetEnvironmentVariable("EpisodeManagementUrl", "EpisodeManagementUrl");
        Environment.SetEnvironmentVariable("ParticipantManagementUrl", "ParticipantManagementUrl");

        _function = new EpisodeIntegrationService.ReceiveData(_mockLogger.Object, _mockHttpRequestService.Object, _mockEndCodeLkpRepository.Object, _mockEpisodeTypeLkpRepository.Object);
    }

    [TestMethod]
    public async Task ReceiveData_ShouldSendEpisodeDataToDownstreamFunctions()
    {
        // Arrange
        string data = "\"nhs_number\",\"episode_id\",\"episode_type\",\"change_db_date_time\",\"episode_date\",\"appointment_made\",\"date_of_foa\",\"date_of_as\",\"early_recall_date\",\"call_recall_status_authorised_by\",\"end_code\",\"end_code_last_updated\",\"bso_organisation_code\",\"bso_batch_id\",\"reason_closed_code\",\"end_point\",\"final_action_code\"\n" +
        "\"9999999999\",1000,\"C\",\"2022-08-17 13:02:17.110314+01\",\"2022-08-17\",,,,,,,,\"AGA\",\"AGA000000A\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-09-02 14:30:54.121779+01\",\"2022-09-02\",,,,,,,,\"ANE\",\"ANE000000A\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-10-13 22:52:34.825602+01\",\"2022-09-02\",\"True\",\"2022-09-27\",,,\"SCREENING_OFFICE\",\"DNA\",\"2022-10-13 00:00:00+01\",\"ANE\",\"ANE000000A\",,,\n" +
        "\"9999999999\",1000,\"C\",\"2022-11-08 22:32:23.326676+00\",\"2022-08-17\",\"True\",\"2022-09-18\",\"2022-11-05\",,\"SCREENING_OFFICE\",\"SC\",\"2022-11-08 00:00:00+00\",\"AGA\",\"AGA000000A\",,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(4));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));

    }

    [TestMethod]
    public async Task ReceiveData_ShouldSendParticipantDataToDownstreamFunctions()
    {
        // Arrange
        string data = "\"change_db_date_time\",\"nhs_number\",\"superseded_nhs_number\",\"gp_practice_code\",\"bso_organisation_code\",\"next_test_due_date\",\"subject_status_code\",\"early_recall_date\",\"latest_invitation_date\",\"removal_reason\",\"removal_date\",\"reason_for_ceasing_code\",\"is_higher_risk\",\"higher_risk_next_test_due_date\",\"hr_recall_due_date\",\"higher_risk_referral_reason_code\",\"date_irradiated\",\"is_higher_risk_active\",\"gene_code\",\"ntdd_calculation_method\",\"preferred_language\"\n" +
        "\"2022-08-16 14:21:48.330694+01\",\"9999999998\",,\"A81001\",\"ANE\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"2022-08-16 14:21:48.651537+01\",\"9999999999\",,\"A81002\",\"AGA\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"2022-09-15 22:50:02.516313+01\",\"9999999999\",,\"A81002\",\"AGA\",\"2025-09-16\",\"NORMAL\",,\"2023-07-22\",,,,False,,,,,False,,\"ROUTINE\",\"EN\"\n" +
        "\"2022-10-13 22:52:34.825602+01\",\"9999999998\",,\"A81001\",\"ANE\",\"2025-09-29\",\"NORMAL\",,\"2023-02-07\",,,,False,,,,,False,,\"ROUTINE\",\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_subjects_test_data_20240930");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(4));
    }

    [TestMethod]
    public async Task ReceiveData_ShouldLogErrorOnFindingABadRowInEpisodesCsvFile()
    {
        // Arrange
        string data = "\"nhs_number\",\"episode_id\",\"episode_type\",\"change_db_date_time\",\"episode_date\",\"appointment_made\",\"date_of_foa\",\"date_of_as\",\"early_recall_date\",\"call_recall_status_authorised_by\",\"end_code\",\"end_code_last_updated\",\"bso_organisation_code\",\"bso_batch_id\",\"reason_closed_code\",\"end_point\",\"final_action_code\"\n" +
        "\"9999999999\",1000,\"C\",\"2022-08-17 13:02:17.110314+01\",\"2022-08-17\",,,,,,,,\"AGA\",\"AGA000000A\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-09-02 14:30:54.121779+01\",\"2022-09-02\",,,,,,,,\"ANE\",\"ANE000000A\",,,\n" +
        "\"BadRow\",,,\n" +
        "\"BadRow\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-09-02 14:30:54.121779+01\",\"2022-09-02\",,,,,,,,\"ANE\",\"ANE000000A\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-09-02 14:30:54.121779+01\",\"2022-09-02\",,,,,,,,\"ANE\",\"ANE000000A\",,,\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("Error in ProcessEpisodeDataAsync: ")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Exactly(2));

        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(4));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));
    }

    [TestMethod]
    public async Task ReceiveData_ShouldLogErrorOnFindingABadRowInSubjectsCsvFile()
    {
        // Arrange
        string data = "\"change_db_date_time\",\"nhs_number\",\"superseded_nhs_number\",\"gp_practice_code\",\"bso_organisation_code\",\"next_test_due_date\",\"subject_status_code\",\"early_recall_date\",\"latest_invitation_date\",\"removal_reason\",\"removal_date\",\"reason_for_ceasing_code\",\"is_higher_risk\",\"higher_risk_next_test_due_date\",\"hr_recall_due_date\",\"higher_risk_referral_reason_code\",\"date_irradiated\",\"is_higher_risk_active\",\"gene_code\",\"ntdd_calculation_method\",\"preferred_language\"\n" +
        "\"2022-08-16 14:21:48.330694+01\",\"9999999998\",,\"A81001\",\"ANE\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"2022-08-16 14:21:48.651537+01\",\"9999999999\",,\"A81002\",\"AGA\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"BadRow\",,,\n" +
        "\"BadRow\",,,\n" +
        "\"2022-09-15 22:50:02.516313+01\",\"9999999999\",,\"A81002\",\"AGA\",\"2025-09-16\",\"NORMAL\",,\"2023-07-22\",,,,False,,,,,False,,\"ROUTINE\",\"EN\"\n" +
        "\"2022-10-13 22:52:34.825602+01\",\"9999999998\",,\"A81001\",\"ANE\",\"2025-09-29\",\"NORMAL\",,\"2023-02-07\",,,,False,,,,,False,,\"ROUTINE\",\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_subjects_test_data_20240930");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("Error in ProcessParticipantDataAsync: ")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Exactly(2));

        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(4));
    }

    [TestMethod]
    public async Task ReceiveData_ShouldReturnBadRequestAndLogErrorIfFileNameIsInvalid()
    {
        // Arrange
        string data = "\"change_db_date_time\",\"nhs_number\",\"superseded_nhs_number\",\"gp_practice_code\",\"bso_organisation_code\",\"next_test_due_date\",\"subject_status_code\",\"early_recall_date\",\"latest_invitation_date\",\"removal_reason\",\"removal_date\",\"reason_for_ceasing_code\",\"is_higher_risk\",\"higher_risk_next_test_due_date\",\"hr_recall_due_date\",\"higher_risk_referral_reason_code\",\"date_irradiated\",\"is_higher_risk_active\",\"gene_code\",\"ntdd_calculation_method\",\"preferred_language\"\n" +
        "\"2022-08-16 14:21:48.330694+01\",\"9999999998\",,\"A81001\",\"ANE\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"2022-08-16 14:21:48.651537+01\",\"9999999999\",,\"A81002\",\"AGA\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"2022-09-15 22:50:02.516313+01\",\"9999999999\",,\"A81002\",\"AGA\",\"2025-09-16\",\"NORMAL\",,\"2023-07-22\",,,,False,,,,,False,,\"ROUTINE\",\"EN\"\n" +
        "\"2022-10-13 22:52:34.825602+01\",\"9999999998\",,\"A81001\",\"ANE\",\"2025-09-29\",\"NORMAL\",,\"2023-02-07\",,,,False,,,,,False,,\"ROUTINE\",\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "invalid_file_name");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("fileName is invalid. file name: invalid_file_name")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ReceiveData_ShouldReturnErrorIfEpisodesFileHeadersAreNotValid()
    {
        // Arrange
        string data = "\"INVALID\",\"episode_id\",\"episode_type\",\"change_db_date_time\",\"episode_date\",\"date_of_foa\",\"date_of_as\",\"early_recall_date\",\"call_recall_status_authorised_by\",\"end_code\",\"end_code_last_updated\",\"bso_organisation_code\",\"bso_batch_id\",\"reason_closed_code\",\"end_point\",\"final_action_code\"\n" +
        "\"9999999999\",1000,\"C\",\"2022-08-17 13:02:17.110314+01\",\"2022-08-17\",,,,,,,,\"AGA\",\"AGA000000A\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-09-02 14:30:54.121779+01\",\"2022-09-02\",,,,,,,,\"ANE\",\"ANE000000A\",,,\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("Episodes CSV file headers are invalid.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ReceiveData_ShouldReturnErrorIfSubjectsFileHeadersAreNotValid()
    {
        // Arrange
        string data = "\"INVALID\",\"nhs_number\",\"superseded_nhs_number\",\"gp_practice_code\",\"bso_organisation_code\",\"next_test_due_date\",\"subject_status_code\",\"early_recall_date\",\"latest_invitation_date\",\"removal_reason\",\"removal_date\",\"reason_for_ceasing_code\",\"is_higher_risk\",\"higher_risk_next_test_due_date\",\"hr_recall_due_date\",\"higher_risk_referral_reason_code\",\"date_irradiated\",\"is_higher_risk_active\",\"gene_code\",\"ntdd_calculation_method\",\"preferred_language\"\n" +
        "\"2022-08-16 14:21:48.330694+01\",\"9999999998\",,\"A81001\",\"ANE\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"2022-08-16 14:21:48.651537+01\",\"9999999999\",,\"A81002\",\"AGA\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"2022-09-15 22:50:02.516313+01\",\"9999999999\",,\"A81002\",\"AGA\",\"2025-09-16\",\"NORMAL\",,\"2023-07-22\",,,,False,,,,,False,,\"ROUTINE\",\"EN\"\n" +
        "\"2022-10-13 22:52:34.825602+01\",\"9999999998\",,\"A81001\",\"ANE\",\"2025-09-29\",\"NORMAL\",,\"2023-02-07\",,,,False,,,,,False,,\"ROUTINE\",\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_subjects_test_data_20240930");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("Subjects CSV file headers are invalid.")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ReceiveData_ShouldSkipEpisodesRowIfSendPostThrowsException()
    {
        // Arrange
        string data = "\"nhs_number\",\"episode_id\",\"episode_type\",\"change_db_date_time\",\"episode_date\",\"appointment_made\",\"date_of_foa\",\"date_of_as\",\"early_recall_date\",\"call_recall_status_authorised_by\",\"end_code\",\"end_code_last_updated\",\"bso_organisation_code\",\"bso_batch_id\",\"reason_closed_code\",\"end_point\",\"final_action_code\"\n" +
        "\"9999999999\",1000,\"C\",\"2022-08-17 13:02:17.110314+01\",\"2022-08-17\",,,,,,,,\"AGA\",\"AGA000000A\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-09-02 14:30:54.121779+01\",\"2022-09-02\",,,,,,,,\"ANE\",\"ANE000000A\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-10-13 22:52:34.825602+01\",\"2022-09-02\",\"True\",\"2022-09-27\",,,\"SCREENING_OFFICE\",\"DNA\",\"2022-10-13 00:00:00+01\",\"ANE\",\"ANE000000A\",,,\n" +
        "\"9999999999\",1000,\"C\",\"2022-11-08 22:32:23.326676+00\",\"2022-08-17\",\"True\",\"2022-09-18\",\"2022-11-05\",,\"SCREENING_OFFICE\",\"SC\",\"2022-11-08 00:00:00+00\",\"AGA\",\"AGA000000A\",,,";

        _mockHttpRequestService.SetupSequence(r => r.SendPost(It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>().Returns(Task.FromResult(new HttpResponseMessage())).Returns(Task.FromResult(new HttpResponseMessage())).Returns(Task.FromResult(new HttpResponseMessage()));
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(4));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));
    }

    [TestMethod]
    public async Task ReceiveData_ShouldSkipSubjectsRowIfSendPostThrowsException()
    {
        // Arrange
        string data = "\"change_db_date_time\",\"nhs_number\",\"superseded_nhs_number\",\"gp_practice_code\",\"bso_organisation_code\",\"next_test_due_date\",\"subject_status_code\",\"early_recall_date\",\"latest_invitation_date\",\"removal_reason\",\"removal_date\",\"reason_for_ceasing_code\",\"is_higher_risk\",\"higher_risk_next_test_due_date\",\"hr_recall_due_date\",\"higher_risk_referral_reason_code\",\"date_irradiated\",\"is_higher_risk_active\",\"gene_code\",\"ntdd_calculation_method\",\"preferred_language\"\n" +
        "\"2022-08-16 14:21:48.330694+01\",\"9999999998\",,\"A81001\",\"ANE\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"2022-08-16 14:21:48.651537+01\",\"9999999999\",,\"A81002\",\"AGA\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"2022-09-15 22:50:02.516313+01\",\"9999999999\",,\"A81002\",\"AGA\",\"2025-09-16\",\"NORMAL\",,\"2023-07-22\",,,,False,,,,,False,,\"ROUTINE\",\"EN\"\n" +
        "\"2022-10-13 22:52:34.825602+01\",\"9999999998\",,\"A81001\",\"ANE\",\"2025-09-29\",\"NORMAL\",,\"2023-02-07\",,,,False,,,,,False,,\"ROUTINE\",\n";

        _mockHttpRequestService.SetupSequence(r => r.SendPost(It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>().Returns(Task.FromResult(new HttpResponseMessage())).Returns(Task.FromResult(new HttpResponseMessage())).Returns(Task.FromResult(new HttpResponseMessage()));

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_subjects_test_data_20240930");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(4));
    }
}
