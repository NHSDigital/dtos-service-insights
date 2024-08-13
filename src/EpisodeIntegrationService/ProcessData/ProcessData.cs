using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using NHS.ServiceInsights.Common;


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
    [FunctionName("ProcessData")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log,
        ExecutionContext context)
    {
        log.LogInformation("C# HTTP trigger function received a request.");

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
            log.LogError($"Error reading request body: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        log.LogInformation($"Request body: {requestBody}");

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
            log.LogError($"Deserialization error: {ex.Message}");
            return new BadRequestObjectResult("Invalid JSON format.");
        }

        if (data == null)
        {
            log.LogError("Deserialized data is null.");
            return new BadRequestObjectResult("No data received.");
        }

        log.LogInformation($"Deserialized data: {JsonConvert.SerializeObject(data, Formatting.Indented)}");

        // Load configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Read URLs from configuration
        string episodeUrl = config["EpisodeManagementUrl"];
        string participantUrl = config["ParticipantManagementUrl"];

        log.LogInformation($"Episode URL: {episodeUrl}");
        log.LogInformation($"Participant URL: {participantUrl}");

        if (string.IsNullOrEmpty(episodeUrl) || string.IsNullOrEmpty(participantUrl))
        {
            log.LogError("One or both URLs are not configured. Please check your settings.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        if (data.TryGetValue("Episodes", out var episodeData))
        {
            log.LogInformation("Processing episode data.");

            // Assuming episodeData is a list of episodes
            var episodes = JsonConvert.DeserializeObject<List<object>>(episodeData.ToString());
            foreach (var episode in episodes)
            {
                log.LogInformation($"Sending episode: {JsonConvert.SerializeObject(episode, Formatting.Indented)}");
                // await SendToFunction(episodeUrl, episode, log);
                // _httpRequestService.SendPost(episodeUrl, episode, log);
                await _httpRequestService.SendPost(Environment.GetEnvironmentVariable("EpisodeManagementUrl"), episode);

            }
        }
        else
        {
            log.LogInformation("No episode data found.");
        }

        if (data.TryGetValue("Participants", out var participantData))
        {
            log.LogInformation("Processing participant data.");

            // Assuming participantData is a list of participants
            var participants = JsonConvert.DeserializeObject<List<object>>(participantData.ToString());
            foreach (var participant in participants)
            {
                log.LogInformation($"Sending participant: {JsonConvert.SerializeObject(participant, Formatting.Indented)}");
                // await SendToFunction(participantUrl, participant, log);
                await _httpRequestService.SendPost(Environment.GetEnvironmentVariable("ParticipantManagementUrl"), participant);

            }
        }
        else
        {
            log.LogInformation("No participant data found.");
        }

        log.LogInformation("Data processed successfully.");
        return new OkObjectResult("Data processed successfully.");
    }

    // private static async Task SendToFunction(string functionUrl, object data, ILogger log)
    // {
    //     if (string.IsNullOrWhiteSpace(functionUrl))
    //     {
    //         log.LogError("Function URL is not configured.");
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
    //             log.LogInformation($"Data sent to function {functionUrl} successfully.");
    //         }
    //         else
    //         {
    //             log.LogError($"Failed to send data to function {functionUrl}. Status code: {response.StatusCode}");
    //         }
    //     }
    //     catch (HttpRequestException ex)
    //     {
    //         log.LogError($"HTTP request error: {ex.Message}");
    //     }
    // }
}
