using System.Text.Json;
using Microsoft.Extensions.Logging;


namespace NHS.ServiceInsights.Common;

public static class GetReferenceDataHelper
{
    public static async Task<T?> GetCodeObject<T>(string code, string codeName, Func<string, Task<T?>> getObjectMethod, ILogger logger) where T : class?
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var codeObject = await getObjectMethod(code);
        if (codeObject == null)
        {
            logger.LogError("{codeName} '{code}' not found in lookup table.", codeName, code);
            throw new InvalidOperationException($"{codeName} '{code}' not found in lookup table.");
        }
        return codeObject;
    }

    public static async Task<long?> GetOrganisationId(string organisationCode, IHttpRequestService httpRequestService, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(organisationCode))
        {
            logger.LogInformation("Organisation code is null");
            return null;
        }

        var url = $"{Environment.GetEnvironmentVariable("GetOrganisationIdByCodeUrl")}?organisation_code={organisationCode}";
        var response = await httpRequestService.SendGet(url);
        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Organisation ID with code '{organisationCode}' found successfully.", organisationCode);
            return await JsonSerializer.DeserializeAsync<long>(await response.Content.ReadAsStreamAsync());
        }
        else
        {
            logger.LogError("Organisation ID with code '{organisationCode}' not found in lookup table.", organisationCode);
            throw new InvalidOperationException($"Organisation with code '{organisationCode}' not found in lookup table.");
        }
    }
}
