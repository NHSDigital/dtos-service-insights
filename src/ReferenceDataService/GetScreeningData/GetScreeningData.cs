using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NHS.ServiceInsights.Model;
using NHS.ServiceInsights.Data;

namespace NHS.ServiceInsights.ReferenceDataService;

public class GetScreeningData
{
    private readonly ILogger<GetScreeningData> _logger;

    private readonly IScreeningLkpRepository _screeningLkpRepository;

    public GetScreeningData(ILogger<GetScreeningData> logger, IScreeningLkpRepository screeningLkpRepository)
    {
        _logger = logger;
        _screeningLkpRepository = screeningLkpRepository;
    }

    [Function("GetScreeningData")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("GetScreeningData: start");

        long screeningId;

        if (!long.TryParse(req.Query["screening_id"], out screeningId))
        {
            _logger.LogError("Missing or invalid screening ID.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            ScreeningLkp? screeningLkp = await _screeningLkpRepository.GetScreeningAsync(screeningId);
            if (screeningLkp == null)
            {
                _logger.LogError("screening not found.");
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
            _logger.LogInformation("screening found successfully.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await JsonSerializer.SerializeAsync(response.Body, screeningLkp);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetScreeningData: Failed to get screening from the db.\nException: {Message}", ex.Message);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
