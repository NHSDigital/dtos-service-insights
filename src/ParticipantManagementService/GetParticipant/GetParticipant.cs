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

    [Function("CheckParticipantExists")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("CheckParticipantExists stub returning 200");
        return req.CreateResponse(HttpStatusCode.OK);
    }
}
