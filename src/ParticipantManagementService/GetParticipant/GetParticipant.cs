using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace GetParticipantFunction
{
    public static class GetParticipant
    {
        [FunctionName("GetParticipant")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Read the JSON data from the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Log the received JSON data
            log.LogInformation($"Received Participant Data: {requestBody}");

            return new OkResult();
        }
    }
}
