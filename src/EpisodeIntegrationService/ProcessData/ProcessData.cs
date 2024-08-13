using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;

    public ProcessData(ILogger<ProcessData> logger, IHttpRequestService httpRequestService, IConfiguration configuration)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
        _configuration = configuration;
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

        _logger.LogDebug($"Request body: {requestBody}");

        var data = await DeserializeDataAsync(requestBody);
        if (data == null)
        {
            return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format or no data received");
        }

        _logger.LogDebug($"Deserialized data: {JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}");

        var (episodeUrl, participantUrl) = GetConfigurationUrls();
        if (string.IsNullOrEmpty(episodeUrl) || string.IsNullOrEmpty(participantUrl))
        {
            return CreateErrorResponse(req, HttpStatusCode.InternalServerError, "One or both URLs are not configured");
        }


        await ProcessEpisodeDataAsync(data.Episodes, episodeUrl);
        await ProcessParticipantDataAsync(data.Participants, participantUrl);

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

    private async Task<DataPayload> DeserializeDataAsync(string requestBody)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return await JsonSerializer.DeserializeAsync<DataPayload>(new MemoryStream(Encoding.UTF8.GetBytes(requestBody)), options);
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Deserialization error: {ex.Message}");
            return null;
        }
    }

    private (string episodeUrl, string participantUrl) GetConfigurationUrls()
    {
        return (_configuration["EpisodeManagementUrl"], _configuration["ParticipantManagementUrl"]);
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
                _logger.LogDebug($"Sending episode: {JsonSerializer.Serialize(episode, new JsonSerializerOptions { WriteIndented = true })}");
                await _httpRequestService.SendPost(episodeUrl, JsonSerializer.Serialize(episode));
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
                _logger.LogDebug($"Sending participant: {JsonSerializer.Serialize(participant, new JsonSerializerOptions { WriteIndented = true })}");
                await _httpRequestService.SendPost(participantUrl, JsonSerializer.Serialize(participant));
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
    public string? Id { get; set; }
    public string? Name { get; set; }
}

public class Episode
{
    public string? Id { get; set; }
    public string? Description { get; set; }
}

public class DataPayload
{
    public List<Episode> Episodes { get; set; } = new List<Episode>();
    public List<Participant> Participants { get; set; } = new List<Participant>();
}
