namespace updateParticipant;

using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;


public class UpdateParticipant
{
    private readonly ILogger<UpdateParticipant> _logger;


    public UpdateParticipant(ILogger<UpdateParticipant> logger)
    {
        _logger = logger;

    }

    [Function("updateParticipant")]
    public  HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        Participant  participant;
try
{
      using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
              {
                  var postData = reader.ReadToEnd();
                  participant = JsonSerializer.Deserialize<Participant>(postData);
              }

              _logger.LogInformation(participant.NhsNumber);

              return req.CreateResponse(HttpStatusCode.OK);

}
catch
{
      _logger.LogError("Could not read participant");

      return req.CreateResponse(HttpStatusCode.BadRequest);
}

    }

    }

public class Participant
  {
    public string NhsNumber { get; set; }
  }

