using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace NHS.ServiceInsights.ParticipantManagementService;

public class GetParticipant
{
    private readonly ILogger<GetParticipant> _logger;

    public GetParticipant(ILogger<GetParticipant> logger)
    {
        _logger = logger;
    }

    [Function("GetParticipant")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Request to retrieve a participant has been processed.");

        string NhsNumber = req.Query["nhs_number"];

        if (string.IsNullOrEmpty(NhsNumber))
        {
            _logger.LogError("Please enter a valid NHS Number.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var participant = ParticipantRepository.GetParticipantByNhsNumber(NhsNumber);

        if (participant == null)
        {
            _logger.LogError($"Participant with NHS Number {NhsNumber} not found.");
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        var json = JsonSerializer.Serialize(participant);
        await response.WriteStringAsync(json);
        return response;
    }
}
