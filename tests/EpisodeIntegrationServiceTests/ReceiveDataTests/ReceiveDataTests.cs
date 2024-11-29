using Moq;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using System.Text;

namespace NHS.ServiceInsights.EpisodeIntegrationServiceTests;

[TestClass]
public class ReceiveDataTests
{
    private readonly Mock<IHttpRequestService> _mockHttpRequestService = new();
    private readonly Mock<ILogger<EpisodeIntegrationService.ReceiveData>> _mockLogger = new();
    private readonly EpisodeIntegrationService.ReceiveData _function;

    public ReceiveDataTests()
    {
        Environment.SetEnvironmentVariable("EpisodeManagementUrl", "EpisodeManagementUrl");
        Environment.SetEnvironmentVariable("ParticipantManagementUrl", "ParticipantManagementUrl");

        _function = new EpisodeIntegrationService.ReceiveData(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task ReceiveData_ShouldSendEpisodeDataToDownstreamFunctions()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,05/09/2016,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007997,569965,R,2020-03-31 13:06:30.814448+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007702,574222,R,2020-03-31 13:10:21.420187+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000014174,568703,C,2020-03-31 13:21:37.94545+01,30/12/2016,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV172471J,,,";


        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(6));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));

    }

    [TestMethod]
    public async Task ReceiveData_ShouldValidateEpisodeDateWhenDelimitedByDash()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,11-01-2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,05-09-2016,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,11-01-2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007997,569965,R,2020-03-31 13:06:30.814448+01,11-01-2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007702,574222,R,2020-03-31 13:10:21.420187+01,11-01-2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000014174,568703,C,2020-03-31 13:21:37.94545+01,30-12-2016,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV172471J,,,";


        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(6));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));

    }

    public async Task ReceiveData_ShouldValidateEpisodeDateWhenDelimitedBySlash()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020/03/31 12:11:47.339148+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020/03/31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020/03/31 12:49:47.513821+01,05/09/2016,True,,,,SCREENING_OFFICE,SC,2020/03/31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020/03/31 12:52:13.463901+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020/03/31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007997,569965,R,2020/03/31 13:06:30.814448+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020/03/31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007702,574222,R,2020/03/31 13:10:21.420187+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020/03/31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000014174,568703,C,2020/03/31 13:21:37.94545+01,30/12/2016,True,,,,SCREENING_OFFICE,SC,2020/03/31 00:00:00+01,LAV,LAV172471J,,,";


        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(6));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));

    }

    [TestMethod]
    public async Task ReceiveData_ShouldLogErrorOnFindingInvalidDatesInEpisodesCsvFile()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,11-INVALID-20555,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,32/01/2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,2017-02-30,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("contained an invalid date")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Exactly(3)); // Expecting three invalid dates to be logged
    }

    [TestMethod]
    public async Task ReceiveData_ShouldSendParticipantDataToDownstreamFunctions()
    {
        // Arrange
        string data = "change_db_date_time,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                "2020-03-31 12:11:47.339148+01,9000007053,,A00014,LAV,11/01/2020,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 12:49:47.513821+01,9000009808,,A00009,LAV,05/05/2019,NORMAL,,2016-09-05,,,,False,,,,,,,,\n" +
                "2020-03-31 12:52:13.463901+01,9000006316,,A00017,LAV,11/01/202,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 13:06:30.814448+01,9000007997,,A00018,LAV,11/01/202,NORMAL,,2017-01-11,,,,False,,,,,,,,";

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
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,05/09/2016,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "BadRow,,,,\n" +
                    "BadRow,,,,\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("Error in ReceiveData: ")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once());

        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(2));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));
    }

    [TestMethod]
    public async Task ReceiveData_ShouldLogErrorOnFindingABadRowInSubjectsCsvFile()
    {
        // Arrange
        string data = "change_db_date_time,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                "2020-03-31 12:11:47.339148+01,9000007053,,A00014,LAV,11/01/2020,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 12:49:47.513821+01,9000009808,,A00009,LAV,05/05/2019,NORMAL,,2016-09-05,,,,False,,,,,,,,\n" +
                "BadRow,,,,\n" +
                "BadRow,,,,\n" +
                "2020-03-31 12:52:13.463901+01,9000006316,,A00017,LAV,11/01/2020,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 13:06:30.814448+01,9000007997,,A00018,LAV,11/01/2020,NORMAL,,2017-01-11,,,,False,,,,,,,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_subjects_test_data_20240930");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("Error in ProcessParticipantDataAsync: ")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Exactly(2));

        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(4));
    }

    [TestMethod]
    public async Task ReceiveData_ShouldReturnBadRequestAndLogErrorIfFileNameIsInvalid()
    {
        // Arrange
        string data = "change_db_date_time,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                "2020-03-31 12:11:47.339148+01,9000007053,,A00014,LAV,11/01/2020,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 12:49:47.513821+01,9000009808,,A00009,LAV,05/09/2019,NORMAL,,2016-09-05,,,,False,,,,,,,,\n" +
                "2020-03-31 12:52:13.463901+01,9000006316,,A00017,LAV,11/01/2020,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 13:06:30.814448+01,9000007997,,A00018,LAV,11/01/2020,NORMAL,,2017-01-11,,,,False,,,,,,,,";

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
        string data = "INVALID,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,05/06/2016,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,";

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
        string data = "INVALID,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                "2020-03-31 12:11:47.339148+01,9000007053,,A00014,LAV,11/01/2020,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 12:49:47.513821+01,9000009808,,A00009,LAV,05/09/2019,NORMAL,,2016-09-05,,,,False,,,,,,,,\n" +
                "2020-03-31 12:52:13.463901+01,9000006316,,A00017,LAV,11/01/2020,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 13:06:30.814448+01,9000007997,,A00018,LAV,11/01/2020,NORMAL,,2017-01-11,,,,False,,,,,,,,";

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
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,11/01/2020,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,05/09/2016,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,11/01/2020,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007997,569965,R,2020-03-31 13:06:30.814448+01,11/01/2020,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007702,574222,R,2020-03-31 13:10:21.420187+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000014174,568703,C,2020-03-31 13:21:37.94545+01,30/12/2016,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV172471J,,,";

        _mockHttpRequestService.SetupSequence(r => r.SendPost(It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>().Returns(Task.FromResult(new HttpResponseMessage())).Returns(Task.FromResult(new HttpResponseMessage())).Returns(Task.FromResult(new HttpResponseMessage()));
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(6));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));
    }

    [TestMethod]
    public async Task ReceiveData_ShouldSkipSubjectsRowIfSendPostThrowsException()
    {
        // Arrange
        string data = "\"change_db_date_time\",\"nhs_number\",\"superseded_nhs_number\",\"gp_practice_code\",\"bso_organisation_code\",\"next_test_due_date\",\"subject_status_code\",\"early_recall_date\",\"latest_invitation_date\",\"removal_reason\",\"removal_date\",\"reason_for_ceasing_code\",\"is_higher_risk\",\"higher_risk_next_test_due_date\",\"hr_recall_due_date\",\"higher_risk_referral_reason_code\",\"date_irradiated\",\"is_higher_risk_active\",\"gene_code\",\"ntdd_calculation_method\",\"preferred_language\"\n" +
        "\"16/08/2022 14:21:48.330694+01\",\"9999999998\",,\"A81001\",\"ANE\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"16/08/2022 14:21:48.651537+01\",\"9999999999\",,\"A81002\",\"AGA\",,\"NORMAL\",,,,,,False,,,,,False,,\"ROUTINE\",\n" +
        "\"15/08/2022 22:50:02.516313+01\",\"9999999999\",,\"A81002\",\"AGA\",\"16/09/2025\",\"NORMAL\",,\"22/07/2023\",,,,False,,,,,False,,\"ROUTINE\",\"EN\"\n" +
        "\"13/08/2022 22:52:34.825602+01\",\"9999999998\",,\"A81001\",\"ANE\",\"29/09/2025\",\"NORMAL\",,\"07/02/2023\",,,,False,,,,,False,,\"ROUTINE\",\n";

        _mockHttpRequestService.SetupSequence(r => r.SendPost(It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>().Returns(Task.FromResult(new HttpResponseMessage())).Returns(Task.FromResult(new HttpResponseMessage())).Returns(Task.FromResult(new HttpResponseMessage()));

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_subjects_test_data_20240930");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(4));
    }
}

