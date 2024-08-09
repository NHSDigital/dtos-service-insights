using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class ParticipantManagement
{
    [FunctionName("UpdateParticipant")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function received a request for Participant Management.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<object>(requestBody);  // Adjust type as needed

        // Log the received Participant Data in JSON format
        string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
        log.LogInformation($"Received Participant Data: {jsonData}");

        // Add your participant management logic here

        log.LogInformation("Participant data updated successfully.");
        return new OkObjectResult("Participant data updated successfully.");
    }
}
