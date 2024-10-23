using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;
using CsvHelper;
using System.Globalization;

namespace NHS.ServiceInsights.EpisodeIntegrationService;

public class ReceiveData
{
    private readonly ILogger<ReceiveData> _logger;
    private readonly IHttpRequestService _httpRequestService;
    private readonly IEndCodeLkpRepository _endCodeLkpRepository;
    private readonly IEpisodeTypeLkpRepository _episodeTypeLkpRepository;
    private readonly IOrganisationLkpRepository _organisationLkpRepository;
    private readonly string[] episodesExpectedHeaders = new[] { "nhs_number", "episode_id", "episode_type", "change_db_date_time", "episode_date", "appointment_made", "date_of_foa", "date_of_as", "early_recall_date", "call_recall_status_authorised_by", "end_code", "end_code_last_updated", "bso_organisation_code", "bso_batch_id", "reason_closed_code", "end_point", "final_action_code" };
    private readonly string[] subjectsExpectedHeaders = new[] { "change_db_date_time", "nhs_number", "superseded_nhs_number", "gp_practice_code", "bso_organisation_code", "next_test_due_date", "subject_status_code", "early_recall_date", "latest_invitation_date", "removal_reason", "removal_date", "reason_for_ceasing_code", "is_higher_risk", "higher_risk_next_test_due_date", "hr_recall_due_date", "higher_risk_referral_reason_code", "date_irradiated", "is_higher_risk_active", "gene_code", "ntdd_calculation_method", "preferred_language" };

    public ReceiveData(ILogger<ReceiveData> logger, IHttpRequestService httpRequestService, ServiceInsightsDbContext dbContext)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
        _endCodeLkpRepository = new EndCodeLkpRepository(dbContext);
        _episodeTypeLkpRepository = new EpisodeTypeLkpRepository(dbContext);
        _organisationLkpRepository = new OrganisationLkpRepository(dbContext);
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

            if (name.StartsWith("episodes"))
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
            else if (name.StartsWith("subjects"))
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
                _logger.LogError("fileName is invalid. file name: " + name);
                return;
            }

            _logger.LogInformation("Data processed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in ReceiveData: {ex.Message} \n StackTrace: {ex.StackTrace}");
            return;
        }
    }

    private (string episodeUrl, string participantUrl) GetConfigurationUrls()
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
                var modifiedEpisode = new Episode
                {
                    EpisodeId = episode.episode_id,
                    EpisodeTypeId = _episodeTypeLkpRepository.GetEpisodeTypeId(episode.episode_type),
                    EpisodeOpenDate = DateOnly.ParseExact(episode.episode_date, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    AppointmentMadeFlag = episode.appointment_made,
                    FirstOfferedAppointmentDate = string.IsNullOrEmpty(episode.date_of_foa) ? null : DateOnly.ParseExact(episode.date_of_foa, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    ActualScreeningDate = episode.date_of_as,
                    EarlyRecallDate = string.IsNullOrEmpty(episode.early_recall_date) ? null : DateOnly.ParseExact(episode.early_recall_date, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    CallRecallStatusAuthorisedBy = episode.call_recall_status_authorised_by,
                    EndCodeId = _endCodeLkpRepository.GetEndCodeId(episode.end_code),
                    EndCodeLastUpdated = episode.end_code_last_updated,
                    OrganisationId = _organisationLkpRepository.GetOrganisationId(episode.bso_organisation_code),
                    BatchId = episode.bso_batch_id
                };

                string serializedEpisode = JsonSerializer.Serialize(modifiedEpisode, new JsonSerializerOptions { WriteIndented = true });

                _logger.LogInformation($"Sending Episode to {episodeUrl}: {serializedEpisode}");

                await _httpRequestService.SendPost(episodeUrl, serializedEpisode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in ProcessEpisodeDataAsync: " + ex.Message);
            await ProcessEpisodeDataAsync(episodes, episodeUrl);
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

                _logger.LogInformation($"Sending participant to {participantUrl}: {serializedParticipant}");

                await _httpRequestService.SendPost(participantUrl, serializedParticipant);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in ProcessParticipantDataAsync: " + ex.Message);
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
    public DateOnly? date_of_as { get; set; }
    public string? early_recall_date { get; set; }
    public string? call_recall_status_authorised_by { get; set; }
    public string? end_code { get; set; }
    public DateTime? end_code_last_updated { get; set; }
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
