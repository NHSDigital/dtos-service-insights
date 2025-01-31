using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace NHS.ServiceInsights.ParticipantManagementService;

public class GetParticipant
{
    private readonly ILogger<GetParticipant> _logger;

    public GetParticipant(ILogger<GetParticipant> logger)
    {
        _logger = logger;
    }

    // Stub function that will be replaced
    // Checks that the participant exists
    [Function("GetParticipant")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        long nhsNumber, screeningId;
        try
        {
            nhsNumber = long.Parse(req.Query["NhsNumber"]);
            screeningId = long.Parse(req.Query["ScreeningId"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request parameters invalid");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        if (nhsNumber == 9999999999 && screeningId == 1)
        {
            _logger.LogInformation("Participant does not exist");
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        return req.CreateResponse(HttpStatusCode.OK);
    }
}
