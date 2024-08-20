namespace updateParticipant;

using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;


public class UpdateParticipant
{
    private readonly ILogger<UpdateParticipant> _logger;
    private readonly IHttpRequestService _httpRequestService;

    public UpdateParticipant(ILogger<UpdateParticipant> logger, IHttpRequestService httpRequestService)
    {
        _logger = logger;
        _httpRequestService = httpRequestService;
    }

    [Function("updateParticipant")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)

    {
        Participant participant;
        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = reader.ReadToEnd();
                participant = JsonSerializer.Deserialize<Participant>(postData);
                _logger.LogInformation("PostData: {postData}", postData);
            }

            return req.CreateResponse(HttpStatusCode.OK);

        }
        catch
        {
            _logger.LogError("Could not read participant");

            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}


