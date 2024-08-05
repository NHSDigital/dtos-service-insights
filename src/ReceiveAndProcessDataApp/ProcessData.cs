using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class ProcessData
{
    [FunctionName("ProcessData")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function received a request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);

        if (data != null && data.ContainsKey("Data"))
        {
            log.LogInformation($"Received Data: {data["Data"]}");
            return new OkObjectResult("Data processed successfully.");
        }
        else
        {
            log.LogError("Invalid data received.");
            return new BadRequestObjectResult("Invalid data.");
        }
    }
}

