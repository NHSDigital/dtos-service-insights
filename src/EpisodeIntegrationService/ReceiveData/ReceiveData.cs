using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.EpisodeManagementService;

public class ReceiveData
{

    private readonly ILogger<ReceiveData> _logger;
    private readonly IHttpRequestService _httpRequestService;

    public ReceiveData(ILogger<ReceiveData> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }
    [Function("ReceiveData")]
    public async Task Run(
        [BlobTrigger("sample-container/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob,
        string name)
    {
        _logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

        // Validate JSON
        using (var reader = new StreamReader(myBlob))
        {
            var jsonData = reader.ReadToEnd();
            if (IsValidJson(jsonData))
            {
                _logger.LogInformation("JSON is valid.");
                await _httpRequestService.SendPost(Environment.GetEnvironmentVariable("ProcessDataURL"), jsonData);
            }
            else
            {
                _logger.LogError("JSON is invalid.");
            }
        }
    }

    private bool IsValidJson(string jsonData)
    {
        try
        {
            _logger.LogInformation("JSON is valid:{jsonData}", jsonData);
            var obj = JsonSerializer.Deserialize<object>(jsonData);
            return obj != null;
        }
        catch (JsonException)
        {
            _logger.LogError("Could not validate JSON");
            return false;
        }
    }
}
