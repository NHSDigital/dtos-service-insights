using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Net;
using System.Text;

namespace NHS.ServiceInsights.EpisodeIntegrationService;
public class ProcessData
{
    private readonly ILogger<ProcessData> _logger;
    private readonly IHttpRequestService _httpRequestService;

    public ProcessData(ILogger<ProcessData> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("ProcessData")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function received a request.");

        string requestBody = await ReadRequestBodyAsync(req);
        if (requestBody == null)
        {
            return CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Error reading request body");
        }

        _logger.LogInformation($"Request body: {requestBody}");

        var data = await DeserializeDataAsync(requestBody);
        if (data == null)
        {
            return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format or no data received");
        }

        _logger.LogInformation($"Deserialized data: {JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}");

        var (episodeUrl, participantUrl) = GetConfigurationUrls();
        if (string.IsNullOrEmpty(episodeUrl) || string.IsNullOrEmpty(participantUrl))
        {
            return CreateErrorResponse(req, HttpStatusCode.InternalServerError, "One or both URLs are not configured");
        }

        // Log out useful debug information
        _logger.LogInformation(participantUrl);
        _logger.LogInformation(episodeUrl);

        // Send to downstream functions
        await ProcessParticipantDataAsync(data.Participants, participantUrl);
        await ProcessEpisodeDataAsync(data.Episodes, episodeUrl);

        _logger.LogInformation("Data processed successfully.");
        return req.CreateResponse(HttpStatusCode.OK);
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequestData req)
    {
        try
        {
            using var reader = new StreamReader(req.Body);
            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading request body: {ex.Message}");
            return null;
        }
    }

    private async Task<DataPayLoad?> DeserializeDataAsync(string requestBody)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return await JsonSerializer.DeserializeAsync<DataPayLoad>(new MemoryStream(Encoding.UTF8.GetBytes(requestBody)), options);
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Deserialization error: {ex.Message}");
            return null;
        }
    }

    private (string episodeUrl, string participantUrl) GetConfigurationUrls()
    {
        return (Environment.GetEnvironmentVariable("EpisodeManagementUrl"), Environment.GetEnvironmentVariable("ParticipantManagementUrl"));
    }

    private HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
    {
        _logger.LogError(message);
        var response = req.CreateResponse(statusCode);
        response.WriteString(message);
        return response;
    }

    private async Task ProcessEpisodeDataAsync(List<Episode> episodes, string episodeUrl)
    {
        if (episodes != null && episodes.Any())
        {
            _logger.LogInformation("Processing episode data.");
            foreach (var episode in episodes)
            {
                // Create a new object with EpisodeId instead of episode_id
                var modifiedEpisode = new
                {
                    EpisodeId = episode.episode_id,
                    ParticipantId = episode.participant_id,
                    ScreeningId = episode.screening_id,
                    NhsNumber = episode.nhs_number,
                    EpisodeTypeId = episode.episode_type_id,
                    EpisodeOpenDate = episode.episode_open_date,
                    AppointmentMadeFlag = episode.appointment_made_flag,
                    FirstOfferedAppointmentDate = episode.first_offered_appointment_date,
                    ActualScreeningDate = episode.actual_screening_date,
                    EarlyRecallDate = episode.early_recall_date,
                    CallRecallStatusAuthorisedBy = episode.call_recall_status_authorised_by,
                    EndCodeId = episode.end_code_id,
                    EndCodeLastUpdated = episode.end_code_last_updated,
                    OrganisationId = episode.organisation_id,
                    BatchId = episode.batch_id,
                    RecordInsertDatetime = episode.record_insert_datetime,
                    RecordUpdateDatetime = episode.record_update_datetime
                };

                string serializedEpisode = JsonSerializer.Serialize(modifiedEpisode, new JsonSerializerOptions { WriteIndented = true });

                // Log the Episode data before sending it
                _logger.LogInformation($"Sending Episode to {episodeUrl}: {serializedEpisode}");

                await _httpRequestService.SendPost(episodeUrl, serializedEpisode);
            }
        }
        else
        {
            _logger.LogInformation("No episode data found.");
        }
    }

    private async Task ProcessParticipantDataAsync(List<Participant> participants, string participantUrl)
    {
        if (participants != null && participants.Any())
        {
            _logger.LogInformation("Processing participant data.");
            foreach (var participant in participants)
            {
                string serializedParticipant = JsonSerializer.Serialize(participant, new JsonSerializerOptions { WriteIndented = true });

                // Log the participant data before sending it
                _logger.LogInformation($"Sending participant to {participantUrl}: {serializedParticipant}");

                await _httpRequestService.SendPost(participantUrl, serializedParticipant);
            }
        }
        else
        {
            _logger.LogInformation("No participant data found.");
        }
    }
}

public class Participant
{
    public string? nhs_number { get; set; }
    public string? next_test_due_date { get; set; }
    public string? gp_practice_id { get; set; }
    public string? subject_status_code { get; set; }
    public string? is_higher_risk { get; set; }
    public string? higher_risk_next_test_due_date { get; set; }
    public string? removal_reason { get; set; }
    public string? removal_date { get; set; }
    public string? bso_organisation_id { get; set; }
    public string? early_recall_date { get; set; }
    public string? latest_invitation_date { get; set; }
    public string? preferred_language { get; set; }
    public string? higher_risk_referral_reason_code { get; set; }
    public string? date_irradiated { get; set; }
    public string? is_higher_risk_active { get; set; }
    public string? gene_code { get; set; }
    public string? ntdd_calculation_method { get; set; }
}

public class Episode
{
    public string? episode_id { get; set; }
    public string? participant_id { get; set; }
    public string? screening_id { get; set; }
    public string? nhs_number { get; set; }
    public string? episode_type_id { get; set; }
    public string? episode_open_date { get; set; }
    public string? appointment_made_flag { get; set; }
    public string? first_offered_appointment_date { get; set; }
    public string? actual_screening_date { get; set; }
    public string? early_recall_date { get; set; }
    public string? call_recall_status_authorised_by { get; set; }
    public string? end_code_id { get; set; }
    public string? end_code_last_updated { get; set; }
    public string? appointment_made { get; set; }
    public string? organisation_id { get; set; }
    public string? batch_id { get; set; }
    public string? record_insert_datetime { get; set; }
    public string? record_update_datetime { get; set; }
}


public class DataPayLoad
{
    public List<Episode> Episodes { get; set; } = new List<Episode>();
    public List<Participant> Participants { get; set; } = new List<Participant>();
}
