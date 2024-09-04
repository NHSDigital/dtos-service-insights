using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using NHS.ServiceInsights.Data;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.AnalyticsDataService;

public class SaveTransformedData
{
    private readonly ILogger<SaveTransformedData> _logger;
    private readonly IAnalyticsRepository _analyticsRepository;

    public SaveTransformedData(ILogger<SaveTransformedData> logger, IAnalyticsRepository analyticsRepository)
    {
        _logger = logger;
        _analyticsRepository = analyticsRepository;
    }

    [Function("SaveTransformedData")]
    public  HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        Analytic Data = new Analytic();

        try
        {
            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                var postData = reader.ReadToEnd();
                Data = JsonSerializer.Deserialize<Analytic>(postData);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError("SaveTransformedData: Could not read Json data.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            bool successful = _analyticsRepository.SaveData(Data);
            if (!successful)
            {
                _logger.LogError("SaveTransformedData: Could not save analytics data. Data: " + Data);
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            _logger.LogInformation("SaveTransformedData: Analytics data saved successfully.");

            var response = req.CreateResponse(HttpStatusCode.OK);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("SaveTransformedData: Failed to save analytics data to the database.\nException: {ex}", ex);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}