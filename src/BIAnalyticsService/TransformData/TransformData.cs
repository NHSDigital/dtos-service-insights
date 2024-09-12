using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Net;
using System.Text;

namespace NHS.ServiceInsights.BIAnalyticsService;
public class TransformData
{
    private readonly ILogger<TransformData> _logger;
    private readonly IHttpRequestService _httpRequestService;

    public TransformData(ILogger<TransformData> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("TransformData")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function received a request.");

        string requestBody = await ReadRequestBodyAsync(req);
        if (requestBody == null)
        {
            return CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Error reading request body");
        }

        _logger.LogInformation($"Request body: {requestBody}");

        var data = JsonSerializer.Deserialize<RetrievedData>(requestBody);
        if (data == null)
        {
            return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid data format or no data received");
        }

        _logger.LogInformation($"Deserialized data: {JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}");
        var Response = req.CreateResponse(HttpStatusCode.OK);
        Response.Headers.Add("Content-Type", "application/json");
        return Response;

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
       private HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
    {
        _logger.LogError(message);
        var response = req.CreateResponse(statusCode);
        response.WriteString(message);
        return response;
    }

    public class DataPayLoad{}

}
public class RetrievedData
{
    public Episode episode { get; set; }
    public Participant participant { get; set; }
}
