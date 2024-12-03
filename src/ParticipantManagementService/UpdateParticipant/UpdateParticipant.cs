using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.ParticipantManagementService;

public class UpdateParticipant
{
    private readonly ILogger<UpdateParticipant> _logger;

    public UpdateParticipant(ILogger<UpdateParticipant> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
    }

    [Function("updateParticipant")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)

    {
        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = await reader.ReadToEndAsync();
                JsonSerializer.Deserialize<ParticipantDto>(postData);
                _logger.LogInformation("PostData: {postData}", postData);
            }

            return req.CreateResponse(HttpStatusCode.OK);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not read participant");

            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
