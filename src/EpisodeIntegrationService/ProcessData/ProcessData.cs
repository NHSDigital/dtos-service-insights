using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Net;
using System.Text;
using System.Data.Common;
using CsvHelper;
using System.Globalization;

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

        var (episodeUrl, participantUrl) = GetConfigurationUrls();
        if (string.IsNullOrEmpty(episodeUrl) || string.IsNullOrEmpty(participantUrl))
        {
            return CreateErrorResponse(req, HttpStatusCode.InternalServerError, "One or both URLs are not configured");
        }
         
        string fileName = req.Query["FileName"]; 
        
        if(fileName.StartsWith("episodes"))
        {
            var records = await DeserializeDataAsync<BssEpisode>(req.Body);
            _logger.LogInformation($"Request body: {records}");

            if (records == null)
            {
                return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid csv file or no data received");
            }

            await ProcessEpisodeDataAsync(records, participantUrl);
        }
        else if(fileName.StartsWith("subjects"))
        {
            var records = await DeserializeDataAsync<Participant>(req.Body);
            _logger.LogInformation($"Request body: {records}");

            if (records == null)
            {
                return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid csv file or no data received");
            }

            await ProcessParticipantDataAsync(records, participantUrl);
        }
        else
        {
            _logger.LogInformation("fileName is invalid. file name: " + fileName);
        }

        _logger.LogInformation("Data processed successfully.");
        return req.CreateResponse(HttpStatusCode.OK);
    }

    private async Task<List<T>> DeserializeDataAsync<T>(Stream requestBody)
    {
        try
        {
            using (var reader = new StreamReader(requestBody))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<T>().ToList();

                return records;
            }
        }
        catch (Exception ex)
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

    private async Task ProcessEpisodeDataAsync(List<BssEpisode> episodes, string episodeUrl)
    {
        if (episodes != null && episodes.Any())
        {
            _logger.LogInformation("Processing episode data.");
            foreach (var episode in episodes)
            {
                // Create a new object with EpisodeId instead of episode_id
                var modifiedEpisode = new Episode
                {
                    EpisodeId = episode.episode_id,
                    EpisodeTypeId = episode.episode_type,
                    EpisodeOpenDate = episode.episode_date,
                    AppointmentMadeFlag = episode.appointment_made,
                    FirstOfferedAppointmentDate = episode.date_of_foa,
                    ActualScreeningDate = episode.date_of_as,
                    EarlyRecallDate = episode.early_recall_date,
                    CallRecallStatusAuthorisedBy = episode.call_recall_status_authorised_by,
                    EndCodeId = episode.end_code,
                    EndCodeLastUpdated = episode.end_code_last_updated,
                    OrganisationId = episode.bso_organisation_code,
                    BatchId = episode.bso_batch_id
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
    public string? superseded_nhs_number { get; set; }
    public string? next_test_due_date { get; set; }
    public string? gp_practice_code { get; set; }
    public string? subject_status_code { get; set; }
    public string? is_higher_risk { get; set; }
    public string? higher_risk_next_test_due_date { get; set; }
    public string? hr_recall_due_date { get; set; }
    public string? removal_reason { get; set; }
    public string? removal_date { get; set; }
    public string? bso_organisation_code { get; set; }
    public string? early_recall_date { get; set; }
    public string? latest_invitation_date { get; set; }
    public string? preferred_language { get; set; }
    public string? higher_risk_referral_reason_code { get; set; }
    public string? date_irradiated { get; set; }
    public string? is_higher_risk_active { get; set; }
    public string? gene_code { get; set; }
    public string? ntdd_calculation_method { get; set; }
    public string? reason_for_ceasing_code { get; set; }
}

public class BssEpisode
{
    public string episode_id { get; set; } = null!;
    public string nhs_number { get; set; }
    public string? episode_type { get; set; }
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
