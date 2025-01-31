using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;


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

    [Function("GetOrganisationIdByCode")]
    public async Task<HttpResponseData> Run3([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("GetReferenceData: start");

        //string organisationCode ;
        string organisationCode = req.Query["organisation_code"];
        if (string.IsNullOrWhiteSpace(organisationCode))
        {
            _logger.LogError("Missing or invalid organisation code.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

            try
            {
                OrganisationLkp? organisationLkp = await _organisationLkpRepository.GetOrganisationByCodeAsync(organisationCode);
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
                _logger.LogError(ex, "GetReferenceData: Failed to get organisation from the db.\nException: {Message}", ex.Message);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

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
            _logger.LogError(ex, "GetReferenceData: Failed to get organisation from the db.\nException: {Message}", ex.Message);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("GetAllOrganisationReferenceData")]
    public async Task<HttpResponseData> Run2([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Retrieving Organisation Reference Data... ");

        try
        {
            var organisationIds = await _organisationLkpRepository.GetAllOrganisationsAsync();


            var response = req.CreateResponse(HttpStatusCode.OK);
            await JsonSerializer.SerializeAsync(response.Body, new OrganisationReferenceData
            {
                OrganisationCodeToIdLookup = organisationIds.ToDictionary(oi => oi.OrganisationCode , oi => oi.OrganisationId ),

            });
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all organisation reference data.");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
