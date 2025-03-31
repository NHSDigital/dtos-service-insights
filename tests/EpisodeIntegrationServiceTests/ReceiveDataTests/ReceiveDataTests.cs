using Moq;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using System.Text;
using NHS.ServiceInsights.Model;
using System.Text.Json;
using Azure.Messaging.EventGrid;
using Azure;
using System.Net;
using NHS.ServiceInsights.EpisodeIntegrationService;
using System.Globalization;

namespace NHS.ServiceInsights.EpisodeIntegrationServiceTests;

[TestClass]
public class ReceiveDataTests
{
    private readonly Mock<IHttpRequestService> _mockHttpRequestService = new();
    private readonly Mock<ILogger<EpisodeIntegrationService.ReceiveData>> _mockLogger = new();
    private readonly EpisodeIntegrationService.ReceiveData _function;
    private readonly Mock<Func<string, IEventGridPublisherClient>> _mockEventGridPublisherClientFactory = new();
    private readonly Mock<IEventGridPublisherClient> _mockEventGridPublisherClient = new();

    public ReceiveDataTests()
    {
        Environment.SetEnvironmentVariable("EpisodeManagementUrl", "EpisodeManagementUrl");
        Environment.SetEnvironmentVariable("ParticipantManagementUrl", "ParticipantManagementUrl");
        Environment.SetEnvironmentVariable("GetAllOrganisationReferenceDataUrl", "GetAllOrganisationReferenceDataUrl");
        Environment.SetEnvironmentVariable("GetEpisodeReferenceDataServiceUrl", "GetEpisodeReferenceDataServiceUrl");
        Environment.SetEnvironmentVariable("FileNameRegexPattern", "^(?<prefix>bss)_(?<type>episodes|subjects)(?:_(?<scope>latest|historic))?(?:_.+)?\\.csv$");

        _function = new EpisodeIntegrationService.ReceiveData(_mockLogger.Object, _mockHttpRequestService.Object, _mockEventGridPublisherClientFactory.Object);
    }

    [TestMethod]
    public async Task ReceiveData_ShouldSendEpisodeDataToDownstreamFunctions()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,latest_invitation_date,hr_recall_due_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,2016-09-05,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007997,569965,R,2020-03-31 13:06:30.814448+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007702,574222,R,2020-03-31 13:10:21.420187+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000014174,568703,C,2020-03-31 13:21:37.94545+01,2016-12-30,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV172471J,,,";


        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.csv");

        // Assert -- verify the counters of Rows
        var expectedLogMessages = new List<string>
        {
            "Row No.1 processed successfully",
            "Row No.2 processed successfully",
            "Row No.3 processed successfully",
            "Row No.4 processed successfully",
            "Row No.5 processed successfully",
            "Row No.6 processed successfully",
            "Rows Processed: 6, Success: 6, Failures: 0"
        };

        foreach (var expectedMessage in expectedLogMessages)
        {
            _mockLogger.Verify(log =>
                log.Log(
                    LogLevel.Information,
                    0,
                    It.Is<object>(state => state.ToString().Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Exactly(1)); // Verifies each log message exactly once
        }

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(6));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));

    }

    [TestMethod]
    public async Task ReceiveData_Should_Map_Episode_To_EpisodeDto()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,latest_invitation_date,hr_recall_due_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,2017-01-11,True,2017-03-14,2017-03-14,2018-03-14,2018-03-14,2019-03-14,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,BS,S,RR\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        var expectedEpisodeDto = new InitialEpisodeDto
        {
            EpisodeId = 571645,
            EpisodeType = "R",
            ScreeningName = "Breast Screening",
            NhsNumber = 9000007053,
            SrcSysProcessedDateTime = DateTime.Parse("2020-03-31 12:11:47.339148+01").ToUniversalTime(),
            EpisodeOpenDate = DateOnly.Parse("2017-01-11"),
            AppointmentMadeFlag = 1,
            FirstOfferedAppointmentDate = DateOnly.Parse("2017-03-14"),
            ActualScreeningDate = DateOnly.Parse("2017-03-14"),
            EarlyRecallDate = DateOnly.Parse("2018-03-14"),
            CallRecallStatusAuthorisedBy = "SCREENING_OFFICE",
            EndCode = "SC",
            EndCodeLastUpdated = DateTime.Parse("2020-03-31 00:00:00+01").ToUniversalTime(),
            OrganisationCode = "LAV",
            BatchId = "LAV121798J",
            EndPoint = "S",
            ReasonClosedCode = "BS",
            FinalActionCode = "RR"
        };
        var expectedJson = JsonSerializer.Serialize(expectedEpisodeDto);

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.csv");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.Is<string>(x => x == expectedJson)), Times.Once);
    }

    [TestMethod]
    public async Task ReceiveData_ShouldValidateEpisodeDate_YYYY_MM_DD_Format()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,latest_invitation_date,hr_recall_due_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,2016-09-05,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007997,569965,R,2020-03-31 13:06:30.814448+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007702,574222,R,2020-03-31 13:10:21.420187+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000014174,568703,C,2020-03-31 13:21:37.94545+01,2016-12-30,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV172471J,,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.csv");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(6));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));

    }

    [TestMethod]
    public async Task ReceiveData_ShouldValidateEpisodeDate_DD_MM_YYYY_Format()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,latest_invitation_date,hr_recall_due_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,11-01-2017,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,05-09-2016,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,11-01-2017,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007997,569965,R,2020-03-31 13:06:30.814448+01,11-01-2017,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007702,574222,R,2020-03-31 13:10:21.420187+01,11-01-2017,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000014174,568703,C,2020-03-31 13:21:37.94545+01,30-12-2016,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV172471J,,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.csv");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(6));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));

    }

    [TestMethod]
    public async Task ReceiveData_ShouldValidateEpisodeDateWhenDelimitedByDash()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,latest_invitation_date,hr_recall_due_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,2016-09-05,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007997,569965,R,2020-03-31 13:06:30.814448+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007702,574222,R,2020-03-31 13:10:21.420187+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000014174,568703,C,2020-03-31 13:21:37.94545+01,2016-12-30,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV172471J,,,";


        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.csv");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(6));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));

    }

    [TestMethod]
    public async Task ReceiveData_ShouldValidateEpisodeDateWhenDelimitedBySlash()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,latest_invitation_date,hr_recall_due_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,2017/01/11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,2016/09/05,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,2017/01/11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007997,569965,R,2020-03-31 13:06:30.814448+01,2017/01/11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007702,574222,R,2020-03-31 13:10:21.420187+01,2017/01/11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000014174,568703,C,2020-03-31 13:21:37.94545+01,2016/12/30,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV172471J,,,";


        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.csv");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(6));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));

    }

    [TestMethod]
    public async Task ReceiveData_ShouldLogErrorOnFindingInvalidDatesInEpisodesCsvFile()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,latest_invitation_date,hr_recall_due_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,11-INVALID-20555,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,2017-01-33,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,201-02-30,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.csv");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("processed unsuccessfully")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Exactly(3));
    }

    [TestMethod]
    public async Task ReceiveData_ShouldSendParticipantDataToDownstreamFunctions()
    {
        // Arrange
        string data = "change_db_date_time,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                "2020-03-31 12:11:47.339148+01,9000007053,,A00014,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 12:49:47.513821+01,9000009808,,A00009,LAV,2019-09-05,NORMAL,,2016-09-05,,,,False,,,,,,,,\n" +
                "2020-03-31 12:52:13.463901+01,9000006316,,A00017,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 13:06:30.814448+01,9000007997,,A00018,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_subjects_test_data_20240930.csv");

        // Assert -- verify the counters of Rows
        var expectedLogMessages = new List<string>
        {
            "Row No.1 processed successfully",
            "Row No.2 processed successfully",
            "Row No.3 processed successfully",
            "Row No.4 processed successfully",
            "Rows Processed: 4, Success: 4, Failures: 0"
        };

        foreach (var expectedMessage in expectedLogMessages)
        {
            _mockLogger.Verify(log =>
                log.Log(
                    LogLevel.Information,
                    0,
                    It.Is<object>(state => state.ToString().Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Exactly(1));
        }

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(4));
    }

    [TestMethod]
    public async Task ReceiveData_LogWarningSendbeforeSendingParticipantDataToDownstreamFunctions()
    {

        // Arrange
        string data = "change_db_date_time,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                "2020-03-31 12:11:47.339148+01,9000007053,,A00014,LAV,2020-01-99,NORMAL,,2017-99-99,,,,False,,,,,,,,\n" +
                "2020-03-31 12:49:47.513821+01,9000009808,,A00009,LAV,2019-09-05,NORMAL,,2016-09-05,,,,False,,,,,,,,\n" +
                "2020-03-31 12:52:13.463901+01,9000006316,,A00017,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 13:06:30.814448+01,9000007997,,A00018,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_subjects_test_data_20240930.csv");

        // Assert -- verify the counters of Rows
        var expectedLogInfoMessages = new List<string>
        {
            "Row No.2 processed successfully",
            "Row No.3 processed successfully",
            "Row No.4 processed successfully",
            "Rows Processed: 4, Success: 3, Failures: 1"
        };

        var expectedLogErrorMessages = new List<string>
        {
            "Row No.1 processed unsuccessfully",
        };

        foreach (var expectedMessage in expectedLogErrorMessages)
        {
            _mockLogger.Verify(log =>
                log.Log(
                    LogLevel.Error,
                    0,
                    It.Is<object>(state => state.ToString().Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Exactly(1));
        }

        foreach (var expectedMessage in expectedLogInfoMessages)
        {
            _mockLogger.Verify(log =>
                log.Log(
                    LogLevel.Information,
                    0,
                    It.Is<object>(state => state.ToString().Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Exactly(1));
        }


        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(3));
    }

    [TestMethod]
    public async Task ReceiveData_Should_Map_Participant_To_ParticipantDto()
    {

        // Arrange
        string data = "change_db_date_time,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                "2020-03-31 12:49:47.513821+01,9000009808,,A00009,LAV,2019-09-05,NORMAL,,2016-09-05,,,INFORMED_SUBJECT_CHOICE,True,2019-09-05,,BRCA_RISK,2021-09-05,True,BRCA1,ROUTINE,\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        var expectedParticipantDto = new InitialParticipantDto
        {
            NhsNumber = 9000009808,
            SrcSysProcessedDateTime = DateTime.Parse("2020-03-31 12:49:47.513821+01"),
            ScreeningName = "Breast Screening",
            NextTestDueDate = DateOnly.Parse("2019-09-05"),
            NextTestDueDateCalculationMethod = "ROUTINE",
            ParticipantScreeningStatus = "NORMAL",
            ScreeningCeasedReason = "INFORMED_SUBJECT_CHOICE",
            IsHigherRisk = 1,
            IsHigherRiskActive = 1,
            HigherRiskNextTestDueDate = DateOnly.Parse("2019-09-05"),
            HigherRiskReferralReasonCode = "BRCA_RISK",
            DateIrradiated = DateOnly.Parse("2021-09-05"),
            GeneCode = "BRCA1"
        };
        var expectedJson = JsonSerializer.Serialize(expectedParticipantDto);

        // Act
        await _function.Run(stream, "bss_subjects_test_data_20240930.csv");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.Is<string>(x => x == expectedJson)), Times.Once);
    }

    [TestMethod]

    public async Task ReceiveData_ShouldLogErrorOnFindingABadRowInEpisodesCsvFile()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,latest_invitation_date,hr_recall_due_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,11/01/2017,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "BadRow,,,,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,05/09/2016,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "BadRow,,,,,,\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.csv");

        // Assert -- verify the counters of Rows
        var expectedLogInfoMessages = new List<string>
        {
            "Row No.1 processed successfully",
            "Row No.3 processed successfully"
        };

        var expectedLogErrorMessages = new List<string>
        {
            "Row No.2 processed unsuccessfully",
            "Row No.4 processed unsuccessfully"
        };

        foreach (var expectedMessage in expectedLogInfoMessages)
        {
            _mockLogger.Verify(log =>
                log.Log(
                    LogLevel.Information,
                    0,
                    It.Is<object>(state => state.ToString().Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Exactly(1));
        }

        foreach (var expectedMessage in expectedLogErrorMessages)
        {
            _mockLogger.Verify(log =>
                log.Log(
                    LogLevel.Error,
                    0,
                    It.Is<object>(state => state.ToString().Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Exactly(1));
        }

        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(2));
    }


    [TestMethod]
    public async Task ReceiveData_ShouldLogErrorOnFindingABadRowInSubjectsCsvFile()
    {
        // Arrange
        string data = "change_db_date_time,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                    "2020-03-31 12:11:47.339148+01,9000007053,,A00014,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                    "2020-03-31 12:49:47.513821+01,9000009808,,A00009,LAV,2019-09-05,NORMAL,,2016-09-05,,,,False,,,,,,,,\n" +
                    "BadRow,,,,,\n" +
                    "BadRow,,,,,\n" +
                    "2020-03-31 12:52:13.463901+01,9000006316,,A00017,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                    "2020-03-31 13:06:30.814448+01,9000007997,,A00018,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_subjects_test_data_20240930.csv");

        // Assert

        var expectedLogInfoMessages = new List<string>
        {
            "Row No.1 processed successfully",
            "Row No.2 processed successfully",
            "Row No.5 processed successfully",
            "Row No.6 processed successfully"
        };

        foreach (var expectedMessage in expectedLogInfoMessages)
        {
            _mockLogger.Verify(log =>
                log.Log(
                    LogLevel.Information,
                    0,
                    It.Is<object>(state => state.ToString().Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Exactly(1));
        }

        var expectedLogErrorMessages = new List<string>
        {
            "Row No.3 processed unsuccessfully",
            "Row No.4 processed unsuccessfully"
        };

        foreach (var expectedMessage in expectedLogErrorMessages)
        {
            _mockLogger.Verify(log =>
                log.Log(
                    LogLevel.Error,
                    0,
                    It.Is<object>(state => state.ToString().Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Exactly(1));
        }

        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(4));
    }


    [TestMethod]
    public async Task ReceiveData_ShouldReturnBadRequestAndLogErrorIfFileNameIsInvalid()
    {
        // Arrange
        string data = "change_db_date_time,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                "2020-03-31 12:11:47.339148+01,9000007053,,A00014,LAV,2020-01-11,NORMAL,,2017-02-11,,,,False,,,,,,,,\n" +
                "2020-03-31 12:49:47.513821+01,9000009808,,A00009,LAV,2019-09-05,NORMAL,,2016-09-05,,,,False,,,,,,,,\n" +
                "2020-03-31 12:52:13.463901+01,9000006316,,A00017,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 13:06:30.814448+01,9000007997,,A00018,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "invalid_file_name.csv");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("File name does not match the expected pattern. file name: invalid_file_name.csv")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ReceiveData_ShouldReturnErrorIfEpisodesFileHeadersAreNotValid()
    {
        // Arrange
        string data = "INVALID,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,2017-01-11,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,2016-09-05,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,2017-01-11,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.csv");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString() == "Episodes CSV file headers are invalid. file name: bss_episodes_test_data_20240930.csv"),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ReceiveData_ShouldReturnErrorIfSubjectsFileHeadersAreNotValid()
    {
        // Arrange
        string data = "INVALID,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                "2020-03-31 12:11:47.339148+01,9000007053,,A00014,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 12:49:47.513821+01,9000009808,,A00009,LAV,2019-09-05,NORMAL,,2016-09-05,,,,False,,,,,,,,\n" +
                "2020-03-31 12:52:13.463901+01,9000006316,,A00017,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,\n" +
                "2020-03-31 13:06:30.814448+01,9000007997,,A00018,LAV,2020-01-11,NORMAL,,2017-01-11,,,,False,,,,,,,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_subjects_test_data_20240930.csv");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString() == "Subjects CSV file headers are invalid. file name: bss_subjects_test_data_20240930.csv"),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);




    }

    [TestMethod]
    public async Task ReceiveData_ShouldSkipEpisodesRowIfSendPostThrowsException()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,latest_invitation_date,hr_recall_due_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,2016-09-05,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n" +
                    "9000006316,570294,R,2020-03-31 12:52:13.463901+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007997,569965,R,2020-03-31 13:06:30.814448+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000007702,574222,R,2020-03-31 13:10:21.420187+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000014174,568703,C,2020-03-31 13:21:37.94545+01,2016-12-30,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV172471J,,,";

        _mockHttpRequestService.SetupSequence(r => r.SendPost(It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>().Returns(Task.FromResult(new HttpResponseMessage())).Returns(Task.FromResult(new HttpResponseMessage())).Returns(Task.FromResult(new HttpResponseMessage()));
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.csv");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(6));
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
        await _function.Run(stream, "bss_subjects_test_data_20240930.csv");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost("EpisodeManagementUrl", It.IsAny<string>()), Times.Exactly(0));
        _mockHttpRequestService.Verify(x => x.SendPost("ParticipantManagementUrl", It.IsAny<string>()), Times.Exactly(4));
    }

    [TestMethod]
    public async Task ReceiveData_ShouldLogErrorAndReturnIfUrlsAreNotConfigured()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,11/01/2017,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,05/09/2016,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        Environment.SetEnvironmentVariable("EpisodeManagementUrl", "");
        Environment.SetEnvironmentVariable("ParticipantManagementUrl", "");

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.csv");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("One or both URLs are not configured")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }


    [TestMethod]
    public async Task ReceiveData_ShouldLogErrorAndReturnIfFileExtensionIsNotCsv()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,2017-01-11,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,2016-09-05,True,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,LAV,LAV000001A,,,\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "bss_episodes_test_data_20240930.txt");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Contains("Invalid file extension. Only .csv files are supported.")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Exactly(1));

        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]

    public async Task Run_Should_Map_Historical_Episode_To_FinalizedEpisodeDto()

    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,latest_invitation_date,hr_recall_due_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,2016-09-05,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,AGA,LAV000001A,,,\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        var referenceDataJson = "{\"EndCodeDescriptions\":{\"SC\":\"Screening complete\",\"DNR\":\"Did not respond\"},\"EpisodeTypeDescriptions\":{\"C\":\"Call\",\"R\":\"Recall\"},\"FinalActionCodeDescriptions\":{\"EC\":\"Short term recall (early clinic)\",\"MT\":\"Medical treatment\"},\"ReasonClosedCodeDescriptions\":{\"BS\":\"Being screened\",\"CP\":\"Under care permanently\"}}";

        _mockHttpRequestService
            .Setup(service => service.SendGet("GetEpisodeReferenceDataServiceUrl"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(referenceDataJson, Encoding.UTF8, "application/json")
            });

        var organisationReferenceDataJson = "{\"OrganisationCodeToIdLookup\":{\"AGA\":1,\"ANE\":2,\"ANT\":3,\"AWC\":4,\"BHL\":5,\"BHU\":6,\"BLE\":7,\"BYO\":8,\"CBA\":9,\"CDN\":10}}";

        _mockHttpRequestService
            .Setup(service => service.SendGet("GetAllOrganisationReferenceDataUrl"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationReferenceDataJson, Encoding.UTF8, "application/json")
            });

        _mockEventGridPublisherClientFactory.Setup(f => f(It.IsAny<string>())).Returns(_mockEventGridPublisherClient.Object);

        // Act
        await _function.Run(stream, "bss_episodes_historic_test_data_20240930.csv");

        // Assert

        _mockHttpRequestService.Verify(x => x.SendGet("GetEpisodeReferenceDataServiceUrl"), Times.Once());
        _mockHttpRequestService.Verify(x => x.SendGet("GetAllOrganisationReferenceDataUrl"), Times.Once());
        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockEventGridPublisherClient.Verify(x => x.SendEventAsync(It.IsAny<EventGridEvent>()), Times.Exactly(2));
    }


    [TestMethod]
    public async Task ProcessHistoricalEpisodeDataAsync_ShouldLogErrorAndIncrementFailureCount_WhenExceptionOccurs()
    {
        // Arrange
        string data = "nhs_number,episode_id,episode_type,change_db_date_time,episode_date,appointment_made,date_of_foa,date_of_as,early_recall_date,latest_invitation_date,hr_recall_due_date,call_recall_status_authorised_by,end_code,end_code_last_updated,bso_organisation_code,bso_batch_id,reason_closed_code,end_point,final_action_code\n" +
                    "9000007053,571645,R,2020-03-31 12:11:47.339148+01,2017-01-11,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,,LAV121798J,,,\n" +
                    "9000009808,333330,R,2020-03-31 12:49:47.513821+01,2016-09-05,True,,,,,,SCREENING_OFFICE,SC,2020-03-31 00:00:00+01,XXX,LAV000001A,,,\n";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        var referenceDataJson = "{\"EndCodeDescriptions\":{\"SC\":\"Screening complete\",\"DNR\":\"Did not respond\"},\"EpisodeTypeDescriptions\":{\"C\":\"Call\",\"R\":\"Recall\"},\"FinalActionCodeDescriptions\":{\"EC\":\"Short term recall (early clinic)\",\"MT\":\"Medical treatment\"},\"ReasonClosedCodeDescriptions\":{\"BS\":\"Being screened\",\"CP\":\"Under care permanently\"}}";

        _mockHttpRequestService
            .Setup(service => service.SendGet("GetEpisodeReferenceDataServiceUrl"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(referenceDataJson, Encoding.UTF8, "application/json")
            });

        var organisationReferenceDataJson = "{\"OrganisationCodeToIdLookup\":{\"AGA\":1,\"ANE\":2,\"ANT\":3,\"AWC\":4,\"BHL\":5,\"BHU\":6,\"BLE\":7,\"BYO\":8,\"CBA\":9,\"CDN\":10}}";

        _mockHttpRequestService
            .Setup(service => service.SendGet("GetAllOrganisationReferenceDataUrl"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(organisationReferenceDataJson, Encoding.UTF8, "application/json")
            });

        // Act
        await _function.Run(stream, "bss_episodes_historic_test_data_20240930.csv");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Equals("Row No.1 processed unsuccessfully")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Exactly(1));
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Error,
            0,
            It.Is<object>(state => state.ToString().Equals("Row No.2 processed unsuccessfully")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Exactly(1));

        _mockLogger.Verify(log =>
        log.Log(
            LogLevel.Information,
            0,
            It.Is<object>(state => state.ToString().Contains("Rows Processed: 2, Success: 0, Failures: 2")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
        Times.Exactly(1));
    }


    [TestMethod]
    public async Task Run_Should_Map_Historical_Participant_To_FinalizedParticipantDto()
    {
        // Arrange
        string data = "change_db_date_time,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                    "2021-11-05 13:33:46.261052+00,9000279152,,N00013,PMA,,NORMAL,,,,,,TRUE,09/06/2022,,BRCA_RISK,,FALSE,BRCA2,ROUTINE,\n" +
                    "2023-03-10 17:35:15.630487+00,9000049841,,A00017,LAV,18/02/2022,NORMAL,,03/02/2019,,,INFORMED_SUBJECT_CHOICE,TRUE,,,BRCA_TESTED,,TRUE,BRCA1,ROUTINE,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        _mockEventGridPublisherClientFactory.Setup(f => f("participant")).Returns(_mockEventGridPublisherClient.Object);

        // Act
        await _function.Run(stream, "bss_subjects_historic_test_data_20240930.csv");

        // Assert
        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockEventGridPublisherClient.Verify(x => x.SendEventAsync(It.IsAny<EventGridEvent>()), Times.Exactly(2));
    }


    [TestMethod]
    public async Task ProcessHistoricalParticipantDataAsync_ShouldLogErrorAndIncrementFailureCount_WhenExceptionOccurs()
    {
        // Arrange
        string data = "change_db_date_time,nhs_number,superseded_nhs_number,gp_practice_code,bso_organisation_code,next_test_due_date,subject_status_code,early_recall_date,latest_invitation_date,removal_reason,removal_date,reason_for_ceasing_code,is_higher_risk,higher_risk_next_test_due_date,hr_recall_due_date,higher_risk_referral_reason_code,date_irradiated,is_higher_risk_active,gene_code,ntdd_calculation_method,preferred_language\n" +
                    "2021-11-05 13:33:46.261052+00,9000279152,,N00013,PMA,,NORMAL,,,,,,TRUE,09/06/2022,,BRCA_RISK,,FALSE,BRCA2,ROUTINE,\n" +
                    "2023-03-10 17:35:15.630487+00,9000049841,,A00017,LAV,18/02/2022,NORMAL,,03/02/2019,,,INFORMED_SUBJECT_CHOICE,TRUE,,,BRCA_TESTED,,TRUE,BRCA1,ROUTINE,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));


        _mockEventGridPublisherClient.Setup(x => x.SendEventAsync(It.IsAny<EventGridEvent>())).Throws<Exception>();
        _mockEventGridPublisherClientFactory.Setup(f => f(It.IsAny<string>())).Returns(_mockEventGridPublisherClient.Object);

        // Act
        await _function.Run(stream, "bss_subjects_historic_test_data_20240930.csv");

        // Assert
        var expectedLogMessages = new List<string>
        {
            "Row No.1 processed unsuccessfully",
            "Row No.2 processed unsuccessfully",
        };

        foreach (var expectedMessage in expectedLogMessages)
        {
            _mockLogger.Verify(log =>
                log.Log(
                    LogLevel.Error,
                    0,
                    It.Is<object>(state => state.ToString().Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Exactly(1));
        }

        _mockLogger.Verify(log =>
        log.Log(
            LogLevel.Information,
            0,
            It.Is<object>(state => state.ToString().Contains("Rows Processed: 2, Success: 0, Failures: 2")),
            It.IsAny<Exception>(),
            (Func<object, Exception, string>)It.IsAny<object>()),
        Times.Exactly(1));

        _mockHttpRequestService.Verify(x => x.SendPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Mock.Get(_mockEventGridPublisherClientFactory.Object("participant")).Verify(x => x.SendEventAsync(It.IsAny<EventGridEvent>()), Times.Exactly(2));
    }

    [TestMethod]
    [DataRow("", null, DisplayName = "ParseNullableDateTime_ShouldReturnNull_WhenGivenNullOrEmptyString")]
    [DataRow("2024-02-26 14:30:00+01", "2024-02-26 13:30:00", DisplayName = "ParseNullableDateTime_ShouldParseCorrectly_WhenGivenTimezone")]
    [DataRow("2024-02-26 14:30:00", "2024-02-26 14:30:00", DisplayName = "ParseNullableDateTime_ShouldParseCorrectly_WhenGivenNoTimezone")]
    [DataRow("2024-02-26 14:30:00 BadFormat", null, DisplayName = "ParseNullableDateTime_ShouldReturnNullForBadFormat")]
    public async Task ParseNullableDateTime_ShouldParseDatesCorrectly(string inputData, string expectedOutput)
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(inputData));
        string[] formats = new[] { "yyyy-MM-dd HH:mm:ssz", "yyyy-MM-dd HH:mm:ss" };
        string dateTimeString;

        using (var reader = new StreamReader(stream))
        {
            dateTimeString = await reader.ReadToEndAsync();
        }

        // Act
        var result = await Task.Run(() => Utils.ParseNullableDateTime(dateTimeString, formats));

        // Assert
        if (expectedOutput == null)
        {
            Assert.IsNull(result);
        }
        else
        {
            Assert.IsNotNull(result);
            DateTime expectedDateTime = DateTime.ParseExact(expectedOutput, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            Assert.AreEqual(expectedDateTime, result);
        }
    }
}

