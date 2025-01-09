using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;

namespace NHS.ServiceInsights.EpisodeManagementService;

public class GetEpisodeReferenceData
{
    private readonly ILogger<GetEpisodeReferenceData> _logger;
    private readonly IHttpRequestService _httpRequestService;

    public GetEpisodeReferenceData(ILogger<GetEpisodeReferenceData> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("GetEpisodeReferenceData")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Request to retrieve reference data has been processed.");

        var url = Environment.GetEnvironmentVariable("RetrieveEpisodeReferenceDataServiceUrl");

        try
        {
            var response = await _httpRequestService.SendGet(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve episode reference data. Status Code: {StatusCode}", response.StatusCode);

                var errorResponse = req.CreateResponse(response.StatusCode);
                return errorResponse;
            }

            var referenceDataJson = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Episode reference data retrieved");

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            successResponse.Headers.Add("Content-Type", "application/json");
            await successResponse.WriteStringAsync(referenceDataJson);

            return successResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call the Retrieve Episode Reference Data Service. Url: {url}", url);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
