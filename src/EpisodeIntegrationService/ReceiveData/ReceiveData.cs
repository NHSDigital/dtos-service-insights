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
                    var participantsEnumerator = csv.GetRecords<Participant>();

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
        var validCount = 0;
        var invalidCount = 0;
        var totalCount = 0;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Processing episode data.");

            foreach (var episode in episodes)
            {
                totalCount++;

                try
                {
                    var modifiedEpisode = MapEpisodeToEpisodeDto(episode);
                    string serializedEpisode = JsonSerializer.Serialize(modifiedEpisode, new JsonSerializerOptions { WriteIndented = true });

                    _logger.LogInformation("Sending Episode {episode.episode_id} to {Url}: {Request}", episode.episode_id, episodeUrl, serializedEpisode);
                    await _httpRequestService.SendPost(episodeUrl, serializedEpisode);
                    validCount++;
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning($"Episode {episode.episode_id} contained an invalid episode_date of {episode.episode_date}, the whole row will be skipped.");
                    invalidCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error when processing episode: {Episode}. Skipping processing for this episode.", episode);
                    invalidCount++;
                }
            }

            var elapsedTime = DateTime.UtcNow - startTime;

            _logger.LogWarning("Processed {Total} episodes in {ElapsedTime} seconds. {ValidCount} valid, {InvalidCount} invalid.",
                totalCount, elapsedTime.TotalSeconds, validCount, invalidCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessEpisodeDataAsync: {Message}", ex.Message);
            throw;
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
            AppointmentMadeFlag = GetAppointmentMadeFlag(episode.appointment_made),
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

    private DateOnly? ParseNullableDate(string? date)
    {
        if (string.IsNullOrEmpty(date)) return null;

        if (DateTime.TryParseExact(date, AllowedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            return DateOnly.FromDateTime(dateTime);
        }

        throw new FormatException($"Invalid date format: {date}. Expected formats: {string.Join(", ", AllowedDateFormats)}");
    }


    private DateTime? ParseNullableDateTime(string? dateTime, string format)
    {
        if (string.IsNullOrEmpty(dateTime))
            return null;

        if (DateTime.TryParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid date-time format: {dateTime}. Expected format: {format}");
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

    private async Task ProcessParticipantDataAsync(IEnumerable<Participant> participants, string participantUrl)
    {
        try
        {
            _logger.LogInformation("Processing participant data.");
            foreach (var participant in participants)
            {
                string serializedParticipant = JsonSerializer.Serialize(participant, new JsonSerializerOptions { WriteIndented = true });

                _logger.LogInformation("Sending participant to {Url}: {Request}", participantUrl, serializedParticipant);

                await _httpRequestService.SendPost(participantUrl, serializedParticipant);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessParticipantDataAsync: {Message}", ex.Message);
            await ProcessParticipantDataAsync(participants, participantUrl);
        }
    }
}

public class BssEpisode
{
    public long episode_id { get; set; }
    public long nhs_number { get; set; }
    public string? episode_type { get; set; }
    public DateTime change_db_date_time { get; set; }
    public string? episode_date { get; set; }
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

enum FileType
{
    Episodes = 0,
    Subjects = 1,
}
