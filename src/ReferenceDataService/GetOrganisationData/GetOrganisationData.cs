using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;

namespace NHS.ServiceInsights.ReferenceDataService;

public class GetOrganisationData
{
    private readonly ILogger<GetOrganisationData> _logger;

    private readonly IOrganisationLkpRepository _organisationLkpRepository;

    public GetOrganisationData(ILogger<GetOrganisationData> logger, IOrganisationLkpRepository organisationLkpRepository)
    {
        _logger = logger;
        _organisationLkpRepository = organisationLkpRepository;
    }

    [Function("GetOrganisationData")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Request to retrieve a participant's demographic information has been processed.");

        string organisationId = req.Query["organisation_id"];

        if (string.IsNullOrEmpty(organisationId))
        {
            _logger.LogError("Missing organisation ID.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            OrganisationLkp? organisationLkp = await _organisationLkpRepository.GetOrganisationAsync(organisationId);
            if (organisationLkp == null)
            {
                _logger.LogError("organisation not found.");
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
            _logger.LogInformation("organisation found successfully.");

            string jsonResponse = JsonSerializer.Serialize(organisationLkp);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            response.WriteString(jsonResponse);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("GetOrganisationData: Failed to get organisation from the db.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
