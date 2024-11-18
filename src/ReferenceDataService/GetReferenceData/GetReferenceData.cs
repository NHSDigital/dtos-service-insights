using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;
using System.Text;

namespace NHS.ServiceInsights.ReferenceDataService;

public class GetReferenceData
{
    private readonly ILogger<GetReferenceData> _logger;

    private readonly IOrganisationLkpRepository _organisationLkpRepository;

    public GetReferenceData(ILogger<GetReferenceData> logger, IOrganisationLkpRepository organisationLkpRepository)
    {
        _logger = logger;
        _organisationLkpRepository = organisationLkpRepository;
    }

    [Function("GetReferenceData")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("GetReferenceData: start");
        
        long organisationId;

        if (!long.TryParse(req.Query["organisation_id"], out organisationId))
        {
            _logger.LogError("Missing or invalid organisation ID.");
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

            var response = req.CreateResponse(HttpStatusCode.OK);
            await JsonSerializer.SerializeAsync(response.Body, organisationLkp);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("GetReferenceData: Failed to get organisation from the db.\nException: " + ex.Message);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
