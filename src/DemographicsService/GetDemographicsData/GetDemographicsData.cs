using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.DemographicsService;

public class GetDemographicsData
{
    private readonly ILogger<GetDemographicsData> _logger;

    public GetDemographicsData(ILogger<GetDemographicsData> logger)
    {
        _logger = logger;
    }

    [Function("GetDemographicsData")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Request to retrieve a participant's demographic information has been processed.");

        string nhsNumber = req.Query["nhs_number"];

        if (string.IsNullOrEmpty(nhsNumber))
        {
            _logger.LogError("Missing NHS Number.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        DemographicsData demographicsData = new DemographicsData
        {
            PrimaryCareProvider = "A81002",
            PreferredLanguage = "EN"
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        var json = JsonSerializer.Serialize(demographicsData);
        await response.WriteStringAsync(json);
        return response;
    }
}
