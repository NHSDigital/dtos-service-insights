using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

public static class BlobJsonTrigger
{
  [FunctionName("receiveData")]
  public static async Task Run(
      [BlobTrigger("sample-container/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob,
      string name,
      ILogger log)
  {
    log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

    // Validate JSON
    using (var reader = new StreamReader(myBlob))
    {
      var jsonData = reader.ReadToEnd();
      if (IsValidJson(jsonData))
      {
        log.LogInformation("JSON is valid.");
        await SendToProcessDataFunction(jsonData, log);
      }
      else
      {
        log.LogError("JSON is invalid.");
      }
    }
  }

  private static bool IsValidJson(string jsonData)
  {
    try
    {
      var obj = JsonConvert.DeserializeObject<object>(jsonData);
      return obj != null;
    }
    catch (JsonException)
    {
      return false;
    }
  }

  private static async Task SendToProcessDataFunction(string jsonData, ILogger log)
  {
    var functionUrl = "http://localhost:7171/api/ProcessData";
    using (var client = new HttpClient())
    {
      var content = new StringContent(JsonConvert.SerializeObject(new { Data = jsonData }));
      content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
      var response = await client.PostAsync(functionUrl, content);
      if (response.IsSuccessStatusCode)
      {
        log.LogInformation("Data sent to ProcessData function successfully.");
      }
      else
      {
        log.LogError("Failed to send data to ProcessData function.");
      }
    }
  }
}
