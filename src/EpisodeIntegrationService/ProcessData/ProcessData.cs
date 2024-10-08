using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Net;
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
        try
        {
            _logger.LogInformation("C# HTTP trigger function ProcessData received a request.");

            var (episodeUrl, participantUrl) = GetConfigurationUrls();
            if (string.IsNullOrEmpty(episodeUrl) || string.IsNullOrEmpty(participantUrl))
            {
                return CreateErrorResponse(req, HttpStatusCode.InternalServerError, "One or both URLs are not configured");
            }

            string? fileName = req.Query["FileName"];

            if (string.IsNullOrEmpty(fileName))
            {
                return CreateErrorResponse(req, HttpStatusCode.BadRequest, "no file name provided");
            }

            if(fileName.StartsWith("episodes"))
            {
                using (var reader = new StreamReader(req.Body))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var episodesEnumerator = csv.GetRecords<BssEpisode>();

                    await ProcessEpisodeDataAsync(episodesEnumerator, episodeUrl);
                }
            }
            else if(fileName.StartsWith("subjects"))
            {
                using (var reader = new StreamReader(req.Body))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var participantsEnumerator = csv.GetRecords<Participant>();

                    await ProcessParticipantDataAsync(participantsEnumerator, participantUrl);
                }
            }
            else
            {
                return CreateErrorResponse(req, HttpStatusCode.BadRequest, "fileName is invalid. file name: " + fileName);
            }

            _logger.LogInformation("Data processed successfully.");
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(req, HttpStatusCode.InternalServerError, $"Error in ProcessData: {ex.Message} \n StackTrace: {ex.StackTrace}");
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

    private async Task ProcessEpisodeDataAsync(IEnumerable<BssEpisode> episodes, string episodeUrl)
    {
        _logger.LogInformation("Processing episode data.");
        foreach (var episode in episodes)
        {
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

            _logger.LogInformation($"Sending Episode to {episodeUrl}: {serializedEpisode}");

            await _httpRequestService.SendPost(episodeUrl, serializedEpisode);
        }
    }

    private async Task ProcessParticipantDataAsync(IEnumerable<Participant> participants, string participantUrl)
    {
        _logger.LogInformation("Processing participant data.");
        foreach (var participant in participants)
        {
            string serializedParticipant = JsonSerializer.Serialize(participant, new JsonSerializerOptions { WriteIndented = true });

            _logger.LogInformation($"Sending participant to {participantUrl}: {serializedParticipant}");

            await _httpRequestService.SendPost(participantUrl, serializedParticipant);
        }
    }
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
