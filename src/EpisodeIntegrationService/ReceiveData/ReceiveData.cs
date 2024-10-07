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

        using (var reader = new StreamReader(myBlob))
        {
            var csvData = reader.ReadToEnd();

            _logger.LogInformation("Sending CSV data to the ProcessData function");
            await _httpRequestService.SendPost(Environment.GetEnvironmentVariable("ProcessDataURL") + $"?FileName={name}", csvData);
        }
    }
}
