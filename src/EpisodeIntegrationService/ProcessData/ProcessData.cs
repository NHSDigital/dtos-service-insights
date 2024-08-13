using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NHS.ServiceInsights.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Net;

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

        string requestBody;
        try
        {
            using (var reader = new StreamReader(req.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading request body: {ex.Message}");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        _logger.LogInformation($"Request body: {requestBody}");

        Dictionary<string, object> data;
        try
        {
            // Check if data is wrapped in a "Data" field
            var wrapper = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);
            if (wrapper != null && wrapper.TryGetValue("Data", out var dataJson))
            {
                data = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson);
            }
            else
            {
                data = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Deserialization error: {ex.Message}");
            return new BadRequestObjectResult("Invalid JSON format.");
        }

        if (data == null)
        {
            _logger.LogError("Deserialized data is null.");
            return new BadRequestObjectResult("No data received.");
        }

        _logger.LogInformation($"Deserialized data: {JsonConvert.SerializeObject(data, Formatting.Indented)}");

        // Load configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Read URLs from configuration
        string episodeUrl = config["EpisodeManagementUrl"];
        string participantUrl = config["ParticipantManagementUrl"];

        _logger.LogInformation($"Episode URL: {episodeUrl}");
        _logger.LogInformation($"Participant URL: {participantUrl}");

        if (string.IsNullOrEmpty(episodeUrl) || string.IsNullOrEmpty(participantUrl))
        {
            _logger.LogError("One or both URLs are not configured. Please check your settings.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        if (data.TryGetValue("Episodes", out var episodeData))
        {
            _logger.LogInformation("Processing episode data.");

            // Assuming episodeData is a list of episodes
            var episodes = JsonConvert.DeserializeObject<List<object>>(episodeData.ToString());
            foreach (var episode in episodes)
            {
                _logger.LogInformation($"Sending episode: {JsonConvert.SerializeObject(episode, Formatting.Indented)}");
                // await SendToFunction(episodeUrl, episode, log);
                // _httpRequestService.SendPost(episodeUrl, episode, log);
                await _httpRequestService.SendPost(Environment.GetEnvironmentVariable("EpisodeManagementUrl"), episode);

            }
        }
        else
        {
            _logger.LogInformation("No episode data found.");
        }

        if (data.TryGetValue("Participants", out var participantData))
        {
            _logger.LogInformation("Processing participant data.");

            // Assuming participantData is a list of participants
            var participants = JsonConvert.DeserializeObject<List<object>>(participantData.ToString());
            foreach (var participant in participants)
            {
                _logger.LogInformation($"Sending participant: {JsonConvert.SerializeObject(participant, Formatting.Indented)}");
                // await SendToFunction(participantUrl, participant, log);
                await _httpRequestService.SendPost(Environment.GetEnvironmentVariable("ParticipantManagementUrl"), participant);

            }
        }
        else
        {
            _logger.LogInformation("No participant data found.");
        }

        _logger.LogInformation("Data processed successfully.");
        return new OkObjectResult("Data processed successfully.");
    }

    // private static async Task SendToFunction(string functionUrl, object data, ILogger log)
    // {
    //     if (string.IsNullOrWhiteSpace(functionUrl))
    //     {
    //         _logger.LogError("Function URL is not configured.");
    //         return;
    //     }

    //     try
    //     {
    //         // Prepare content with correct JSON structure
    //         var content = new StringContent(JsonConvert.SerializeObject(data));
    //         content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    //         var response = await client.PostAsync(functionUrl, content);

    //         if (response.IsSuccessStatusCode)
    //         {
    //             _logger.LogInformation($"Data sent to function {functionUrl} successfully.");
    //         }
    //         else
    //         {
    //             _logger.LogError($"Failed to send data to function {functionUrl}. Status code: {response.StatusCode}");
    //         }
    //     }
    //     catch (HttpRequestException ex)
    //     {
    //         _logger.LogError($"HTTP request error: {ex.Message}");
    //     }
    // }
}
