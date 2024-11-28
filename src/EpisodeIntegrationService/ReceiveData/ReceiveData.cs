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

                    await ProcessEpisodeDataAsync(episodesEnumerator, episodeUrl);
                }
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

                    await ProcessParticipantDataAsync(participantsEnumerator, participantUrl);
                }
            }
            else
            {
                _logger.LogError("fileName is invalid. file name: {Name}", name);
                return;
            }

            _logger.LogInformation("Data processed successfully.");
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

    private async Task ProcessEpisodeDataAsync(IEnumerable<BssEpisode> episodes, string episodeUrl)
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
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessEpisodeDataAsync: {Message}", ex.Message);
            await ProcessEpisodeDataAsync(episodes, episodeUrl);
        }
    }

    private const string DateFormat = "dd/MM/yyyy";
    private EpisodeDto MapEpisodeToEpisodeDto(BssEpisode episode)
    {
        return new EpisodeDto
        {
            EpisodeId = episode.episode_id,
            EpisodeType = episode.episode_type,
            ScreeningName = "Breast Screening",
            NhsNumber = episode.nhs_number,
            EpisodeOpenDate = episode.episode_date,
            AppointmentMadeFlag = GetAppointmentMadeFlag(episode.appointment_made),
            FirstOfferedAppointmentDate = string.IsNullOrEmpty(episode.date_of_foa) ? null : DateOnly.FromDateTime(DateTime.ParseExact(episode.date_of_foa, DateFormat, CultureInfo.InvariantCulture)),
            ActualScreeningDate = string.IsNullOrEmpty(episode.date_of_as) ? null : DateOnly.FromDateTime(DateTime.ParseExact(episode.date_of_as, DateFormat, CultureInfo.InvariantCulture)),
            EarlyRecallDate = string.IsNullOrEmpty(episode.early_recall_date) ? null : DateOnly.FromDateTime(DateTime.ParseExact(episode.early_recall_date, DateFormat, CultureInfo.InvariantCulture)),
            CallRecallStatusAuthorisedBy = episode.call_recall_status_authorised_by,
            EndCode = episode.end_code,
            EndCodeLastUpdated = string.IsNullOrEmpty(episode.end_code_last_updated) ? null : DateTime.ParseExact(episode.end_code_last_updated, "yyyy-MM-dd HH:mm:ssz", CultureInfo.InvariantCulture),
            OrganisationCode = episode.bso_organisation_code,
            BatchId = episode.bso_batch_id,
            EndPoint = episode.end_point,
            ReasonClosedCode = episode.reason_closed_code,
            FinalActionCode = episode.final_action_code
        };
    }

    private static short? GetAppointmentMadeFlag(string appointmentMade)
    {
        if (appointmentMade.ToUpper() == "TRUE")
        {
            return (short)1;
        }
        else if (appointmentMade.ToUpper() == "FALSE")
        {
            return (short)0;
        }
        else
        {
            return null;
        }
    }

    private async Task ProcessParticipantDataAsync(IEnumerable<BssSubject> subjects, string participantUrl)
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
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessParticipantDataAsync: {Message}", ex.Message);
            await ProcessParticipantDataAsync(subjects, participantUrl);
        }
    }
        private ParticipantDto MapParticipantToParticipantDto(BssSubject subject)
        {
            return new ParticipantDto
            {
                NhsNumber = subject.nhs_number,
                ScreeningName = "Breast Screening",
                NextTestDueDate = subject.next_test_due_date,
                NextTestDueDateCalculationMethod = subject.ntdd_calculation_method,
                ParticipantScreeningStatus = subject.subject_status_code,
                ScreeningCeasedReason = subject.reason_for_ceasing_code,
                IsHigherRisk = GetIsHigherRisk(subject.is_higher_risk),
                IsHigherRiskActive = GetIsHigherRiskActive(subject.is_higher_risk_active),
                HigherRiskNextTestDueDate = subject.higher_risk_next_test_due_date,
                HigherRiskReferralReasonCode = subject.higher_risk_referral_reason_code,
                DateIrradiated = subject.date_irradiated,
                GeneCode = subject.gene_code
            };
        }

        private static short? GetIsHigherRisk(string isHigherRisk)
        {
            if (isHigherRisk.ToUpper() == "TRUE")
            {
                return (short)1;
            }
            else if (isHigherRisk.ToUpper() == "FALSE")
            {
                return (short)0;
            }
            else
            {
                return null;
            }
        }

        private static short? GetIsHigherRiskActive(string isHigherRiskActive)
        {
            if (isHigherRiskActive.ToUpper() == "TRUE")
            {
                return (short)1;
            }
            else if (isHigherRiskActive.ToUpper() == "FALSE")
            {
                return (short)0;
            }
            else
            {
                return null;
            }
        }
}


public class BssEpisode
{
    public long episode_id { get; set; }
    public long nhs_number { get; set; }
    public string? episode_type { get; set; }
    public DateTime change_db_date_time { get; set; }
    public DateOnly? episode_date { get; set; }
    public string? appointment_made { get; set; }
    public string? date_of_foa { get; set; }
    public string? date_of_as { get; set; }
    public string? early_recall_date { get; set; }
    public string? call_recall_status_authorised_by { get; set; }
    public string? end_code { get; set; }
    public string? end_code_last_updated { get; set; }
    public string? bso_organisation_code { get; set; }
    public string? bso_batch_id { get; set; }
    public string? reason_closed_code { get; set; }
    public string? end_point { get; set; }
    public string? final_action_code { get; set; }
}

public class BssSubject
{
    public long nhs_number { get; set; }
    public DateTime change_db_date_time { get; set; }
    public long? superseded_nhs_number { get; set; }
    public string? gp_practice_code { get; set; }
    public string? bso_organisation_code { get; set; }
    public DateOnly? next_test_due_date { get; set; }
    public string? subject_status_code { get; set; }
    public DateOnly? early_recall_date { get; set; }
    public DateOnly? latest_invitation_date { get; set; }
    public string? removal_reason { get; set; }
    public DateOnly? removal_date { get; set; }
    public string? reason_for_ceasing_code { get; set; }
    public string? is_higher_risk { get; set; }
    public DateOnly? higher_risk_next_test_due_date { get; set; }
    public DateOnly? hr_recall_due_date { get; set; }
    public string? higher_risk_referral_reason_code { get; set; }
    public DateOnly? date_irradiated { get; set; }
    public string? is_higher_risk_active { get; set; }
    public string? gene_code { get; set; }
    public string? ntdd_calculation_method { get; set; }
    public string? preferred_language { get; set; }

}

enum FileType
{
    Episodes = 0,
    Subjects = 1,
}
