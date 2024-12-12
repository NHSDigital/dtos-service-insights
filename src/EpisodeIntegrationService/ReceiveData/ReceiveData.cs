using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;
using CsvHelper;
using System.Globalization;

namespace NHS.ServiceInsights.EpisodeIntegrationService;

public class ReceiveData
{
    private readonly ILogger<ReceiveData> _logger;
    private readonly IHttpRequestService _httpRequestService;
    private readonly string[] episodesExpectedHeaders = new[] { "nhs_number", "episode_id", "episode_type", "change_db_date_time", "episode_date", "appointment_made", "date_of_foa", "date_of_as", "early_recall_date", "call_recall_status_authorised_by", "end_code", "end_code_last_updated", "bso_organisation_code", "bso_batch_id", "reason_closed_code", "end_point", "final_action_code" };
    private readonly string[] subjectsExpectedHeaders = new[] { "change_db_date_time", "nhs_number", "superseded_nhs_number", "gp_practice_code", "bso_organisation_code", "next_test_due_date", "subject_status_code", "early_recall_date", "latest_invitation_date", "removal_reason", "removal_date", "reason_for_ceasing_code", "is_higher_risk", "higher_risk_next_test_due_date", "hr_recall_due_date", "higher_risk_referral_reason_code", "date_irradiated", "is_higher_risk_active", "gene_code", "ntdd_calculation_method", "preferred_language" };

    private int participantSuccessCount = 0;
    private int participantFailureCount = 0;
    private int participantRowIndex = 0;

    private int episodeSuccessCount = 0;
    private int episodeFailureCount = 0;
    private int episodeRowIndex = 0;

    public ReceiveData(ILogger<ReceiveData> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("ReceiveData")]
    public async Task Run([BlobTrigger("sample-container/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob, string name)
    {

        try
        {
            DateTime processingStart = DateTime.UtcNow;

            _logger.LogInformation("C# HTTP trigger function ReceiveData received a request.");

            var (episodeUrl, participantUrl) = GetConfigurationUrls();
            if (string.IsNullOrEmpty(episodeUrl) || string.IsNullOrEmpty(participantUrl))
            {
                _logger.LogError("One or both URLs are not configured");
                return;
            }

            if (name.StartsWith("bss_episodes"))
            {
                if (!CheckCsvFileHeaders(myBlob, FileType.Episodes))
                {
                    _logger.LogError("Episodes CSV file headers are invalid.");
                    return;
                }

                using (var reader = new StreamReader(myBlob))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var episodesEnumerator = csv.GetRecords<BssEpisode>();

                    await ProcessEpisodeDataAsync(name,episodesEnumerator, episodeUrl);
                }

                DateTime processingEnd = DateTime.UtcNow;

                _logger.LogInformation("===============================================================================\n"
                                +"Episode Data: File {name} processed successfully.\n"
                                +"Start Time: {processingStart}, End Time: {processingEnd}.\n"
                                +"Rows Processed: {episodeRowIndex}, Success: {episodesuccessCount}, Failures: {episodefailureCount}"
                                ,name,processingStart,processingEnd,episodeRowIndex,episodeSuccessCount, episodeFailureCount );

            }

            else if (name.StartsWith("bss_subjects"))
            {
                if (!CheckCsvFileHeaders(myBlob, FileType.Subjects))
                {
                    _logger.LogError("Subjects CSV file headers are invalid.");
                    return;
                }

                using (var reader = new StreamReader(myBlob))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var participantsEnumerator = csv.GetRecords<BssSubject>();

                    await ProcessParticipantDataAsync(name,participantsEnumerator, participantUrl);
                }

                DateTime processingEnd = DateTime.UtcNow;

                _logger.LogInformation("==================================================================\n"
                                +"Participant Data: File {name} processed successfully.\n"
                                +"Start Time: {processingStart}, End Time: {processingEnd}.\n"
                                +"Rows Processed: {participantRowIndex}, Success: {participantSuccessCount}, Failures: {participantFailureCount}"
                                ,name,processingStart,processingEnd,participantRowIndex,participantSuccessCount, participantFailureCount );
            }
            else
            {
                _logger.LogError("fileName is invalid. file name: {Name}", name);
                return;
            }

        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReceiveData: {Message} \n StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
        }
    }

    private static (string episodeUrl, string participantUrl) GetConfigurationUrls()
    {
        return (Environment.GetEnvironmentVariable("EpisodeManagementUrl"), Environment.GetEnvironmentVariable("ParticipantManagementUrl"));
    }

    private bool CheckCsvFileHeaders(Stream requestBody, FileType fileType)
    {
        string[] expectedHeaders = { "" };
        if (fileType == FileType.Episodes)
        {
            expectedHeaders = episodesExpectedHeaders;
        }
        else if (fileType == FileType.Subjects)
        {
            expectedHeaders = subjectsExpectedHeaders;
        }

        using (var reader = new StreamReader(requestBody, leaveOpen: true))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();

            var actualHeaders = csv.Context.Reader.HeaderRecord;

            if (!actualHeaders.SequenceEqual(expectedHeaders))
            {
                return false;
            }

            requestBody.Position = 0;
            return true;
        }
    }


    private async Task ProcessEpisodeDataAsync(string name,IEnumerable<BssEpisode> episodes, string episodeUrl)
    {

        try
        {
            _logger.LogInformation("Processing episode data.");
            foreach (var episode in episodes)
            {
                var modifiedEpisode = MapEpisodeToEpisodeDto(episode);
                string serializedEpisode = JsonSerializer.Serialize(modifiedEpisode, new JsonSerializerOptions { WriteIndented = true });

                _logger.LogInformation("Sending Episode to {Url}: {Request}", episodeUrl, serializedEpisode);

                await _httpRequestService.SendPost(episodeUrl, serializedEpisode);

                episodeSuccessCount++;
                episodeRowIndex++;
                _logger.LogInformation("Row No.{rowIndex} processed successfully",episodeRowIndex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessEpisodeDataAsync: {Message}", ex.Message);

            episodeFailureCount++;
            episodeRowIndex++;
            _logger.LogInformation("Row No.{rowIndex} processed unsuccessfully",episodeRowIndex);
            await ProcessEpisodeDataAsync(name,episodes, episodeUrl);
        }
    }


    private static readonly string[] AllowedDateFormats = ["dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "yyyy/MM/dd"];

    private EpisodeDto MapEpisodeToEpisodeDto(BssEpisode episode)
    {
        return new EpisodeDto
        {
            EpisodeId = episode.episode_id,
            EpisodeType = episode.episode_type,
            ScreeningName = "Breast Screening",
            NhsNumber = episode.nhs_number,
            EpisodeOpenDate = ParseNullableDate(episode.episode_date),
            AppointmentMadeFlag = ParseBooleanStringToShort(episode.appointment_made),
            FirstOfferedAppointmentDate = ParseNullableDate(episode.date_of_foa),
            ActualScreeningDate = ParseNullableDate(episode.date_of_as),
            EarlyRecallDate = ParseNullableDate(episode.early_recall_date),
            CallRecallStatusAuthorisedBy = episode.call_recall_status_authorised_by,
            EndCode = episode.end_code,
            EndCodeLastUpdated = ParseNullableDateTime(episode.end_code_last_updated, "yyyy-MM-dd HH:mm:ssz"),
            OrganisationCode = episode.bso_organisation_code,
            BatchId = episode.bso_batch_id,
            EndPoint = episode.end_point,
            ReasonClosedCode = episode.reason_closed_code,
            FinalActionCode = episode.final_action_code
        };
    }

    private async Task ProcessParticipantDataAsync(string name,IEnumerable<BssSubject> subjects, string participantUrl)
    {

        try
        {
            _logger.LogInformation("Processing participant data.");

            foreach (var subject in subjects)
            {
                var modifiedParticipant = MapParticipantToParticipantDto(subject);
                string serializedParticipant = JsonSerializer.Serialize(modifiedParticipant, new JsonSerializerOptions { WriteIndented = true });

                _logger.LogInformation("Sending participant to {Url}: {Request}", participantUrl, serializedParticipant);

                await _httpRequestService.SendPost(participantUrl, serializedParticipant);

                participantSuccessCount++;
                participantRowIndex++;
                _logger.LogInformation("Row No.{rowIndex} processed successfully",participantRowIndex);
            }
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessParticipantDataAsync: {Message}", ex.Message);

            participantFailureCount++;
            participantRowIndex++;
            _logger.LogInformation("Row No.{rowIndex} processed unsuccessfully",participantRowIndex);
            await ProcessParticipantDataAsync(name,subjects, participantUrl);
        }
    }
    private ParticipantDto MapParticipantToParticipantDto(BssSubject subject)
    {
        return new ParticipantDto
        {
            NhsNumber = subject.nhs_number,
            ScreeningName = "Breast Screening",
            NextTestDueDate = ParseNullableDate(subject.next_test_due_date),
            NextTestDueDateCalculationMethod = subject.ntdd_calculation_method,
            ParticipantScreeningStatus = subject.subject_status_code,
            ScreeningCeasedReason = subject.reason_for_ceasing_code,
            IsHigherRisk = ParseBooleanStringToShort(subject.is_higher_risk),
            IsHigherRiskActive = ParseBooleanStringToShort(subject.is_higher_risk_active),
            HigherRiskNextTestDueDate = ParseNullableDate(subject.higher_risk_next_test_due_date),
            HigherRiskReferralReasonCode = subject.higher_risk_referral_reason_code,
            DateIrradiated = ParseNullableDate(subject.date_irradiated),
            GeneCode = subject.gene_code
        };
    }

    private static short? ParseBooleanStringToShort(string booleanString)
    {
        if (booleanString.ToUpper() == "TRUE")
        {
            return (short)1;
        }
        else if (booleanString.ToUpper() == "FALSE")
        {
            return (short)0;
        }
        else
        {
            return null;
        }
    }

    private static DateOnly? ParseNullableDate(string? date)
    {
        if (string.IsNullOrEmpty(date)) return null;

        var dateTime = DateTime.ParseExact(date, AllowedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
        return DateOnly.FromDateTime(dateTime);
    }


    private static DateTime? ParseNullableDateTime(string? dateTime, string format)
    {
        if (string.IsNullOrEmpty(dateTime)) return null;

        return DateTime.ParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }
}

