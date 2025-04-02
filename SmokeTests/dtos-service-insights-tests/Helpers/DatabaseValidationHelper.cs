using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Azure.Identity;
using Azure.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Linq;
using Ddtos_service_insights_tests.Helpers;
using System.Collections;
using AllOverIt.Assertion;

namespace dtos_service_insights_tests.Helpers;

public class DatabaseValidationHelper
{
    private static readonly HashSet<string> AllowedTables = new HashSet<string>
    {
        "EPISODE"
    };

    private static readonly HashSet<string> AllowedFields =
    [
        "NHS_NUMBER",
        "EPISODE_OPEN_DATE"
        // Add other allowed fields here
    ];

    private static readonly Hashtable CsvTableFieldMap = new Hashtable()
    {
        {"nhs_number","NHS_NUMBER"},
        {"episode_id","EPISODE_ID"},
        {"episode_date","EPISODE_OPEN_DATE"},
        {"date_of_foa","FIRST_OFFERED_APPOINTMENT_DATE"},
        {"date_of_as","ACTUAL_SCREENING_DATE"},
        {"early_recall_date","EARLY_RECALL_DATE"},
        {"call_recall_status_authorised_by","CALL_RECALL_STATUS_AUTHORISED_BY"},
        {"bso_batch_id","BATCH_ID"},
        {"reason_closed_code","REASON_CLOSED_CODE_ID"},
        {"end_point","END_POINT"}
    };


    private static void ValidateTableName(string tableName)
    {
        if (!AllowedTables.Contains(tableName.ToUpper()))
        {
            throw new ArgumentException($"Table '{tableName}' is not in the list of allowed tables.");
        }
    }

    private static void ValidateFieldName(string fieldName)
    {
        if (!AllowedFields.Contains(fieldName.ToUpper()))
        {
            throw new ArgumentException($"Field '{fieldName}' is not in the list of allowed fields.");
        }
    }

    public static async Task VerifyNhsNumbersAsync(string connectionString, string tableName, List<string> nhsNumbers, ILogger logger, string managedIdentityClientId)
    {
        ValidateTableName(tableName);

        var credential = new DefaultAzureCredential(
        new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = managedIdentityClientId
        });

        using (var connection = new SqlConnection(connectionString))
        {
        connection.AccessToken = (await credential.GetTokenAsync(new TokenRequestContext(new[] { "https://database.windows.net/.default" }))).Token;
        await connection.OpenAsync();

        foreach (var nhsNumber in nhsNumbers)
        {
            var isVerified = await VerifyNhsNumberAsync(connection, tableName, nhsNumber, logger);
            if (!isVerified)
            {
                logger.LogError($"Verification failed: NHS number {nhsNumber} not found in {tableName} table.");
                Assert.Fail($"NHS number {nhsNumber} not found in {tableName} table.");
            }
        }
        }
    }

    public static async Task<bool> VerifyFieldUpdateAsync(string connectionString, string tableName, string nhsNumber, string fieldName,string managedIdentityClientId, string expectedValue, ILogger logger)
    {
        List<string> fieldValues  = new List<string>();
        ValidateTableName(tableName);
        ValidateFieldName(fieldName);

        //Comment lines 96 to 97 for executing tests against local environment
        var credential = new DefaultAzureCredential(
        new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = managedIdentityClientId
        });
        using (var connection = new SqlConnection(connectionString))
        {
            connection.AccessToken = (await credential.GetTokenAsync(new TokenRequestContext(new[] { "https://database.windows.net/.default" }))).Token;
            await connection.OpenAsync();
            var query = $"SELECT {fieldName} FROM {tableName} WHERE [NHS_NUMBER] = @NhsNumber";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NhsNumber", nhsNumber);
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var value = reader.IsDBNull(0) ? null : reader.GetValue(0);
                        if (value != null)
                        {
                            // Handle conversion based on the actual type of the column
                            if (value is int intValue)
                                fieldValues.Add(intValue.ToString());
                            else if(value is DateTime dateTimeValue)
                            {
                                DateTime date = System.Convert.ToDateTime(reader.GetValue(0));
                                fieldValues.Add(date.ToString("dd/MM/yyyy"));
                            }
                            else
                                fieldValues.Add(value.ToString()!);
                        }
                    }
                }

                if (fieldValues.Count == 0)
                {
                    logger.LogError($"Field {fieldName} is null for NHS number {nhsNumber} in {tableName} table.");
                    return false;
                }


                if (!fieldValues.Contains(expectedValue))
                {
                    logger.LogError($"Field {fieldName} for NHS number {nhsNumber} does not match the expected value. Expected: {expectedValue}, Actual: {expectedValue}");
                    return false;
                }

                logger.LogInformation($"Field {fieldName} for NHS number {nhsNumber} successfully updated to {expectedValue}.");
                return true;
            }
        }
    }

    public static async Task<bool> VerifyRecordCountAsync(string connectionString, string tableName, int expectedCount, ILogger logger, int maxRetries = 10, int delay = 1000)
    {
        ValidateTableName(tableName);

        for (int i = 0; i < maxRetries; i++)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = $"SELECT COUNT(*) FROM {tableName}";
                using (var command = new SqlCommand(query, connection))
                {
                    var count = (int)await command.ExecuteScalarAsync();
                    if (count == expectedCount)
                    {
                        logger.LogInformation($"Database record count verified for {tableName}: {count}");
                        return true;
                    }
                    logger.LogInformation($"Database record count not yet updated for {tableName}, retrying... ({i + 1}/{maxRetries})");
                    await Task.Delay(delay);
                }
            }
        }
        logger.LogError($"Failed to verify record count for {tableName} after {maxRetries} retries.");
        return false;
    }

    private static async Task<bool> VerifyNhsNumberAsync(SqlConnection connection, string tableName, string nhsNumber, ILogger logger)
    {
        ValidateTableName(tableName);

        int retryCount = 0;
        const int maxRetries = 8;
        TimeSpan delay = TimeSpan.FromSeconds(3); // Initial delay

        while (retryCount < maxRetries)
        {
            try
            {
                using (var command = new SqlCommand($"SELECT 1 FROM {tableName} WHERE NHS_Number = @nhsNumber", connection))
                {
                    command.Parameters.AddWithValue("@nhsNumber", nhsNumber);
                    var result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        return true; // NHS number found
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error verifying NHS number {nhsNumber} in table {tableName} (Attempt {retryCount + 1})");
            }

            retryCount++;
            await Task.Delay(delay);
            delay = delay * 2; // Exponential backoff (double the delay on each retry)
        }

        logger.LogError($"Verification failed after {maxRetries} retries for NHS number {nhsNumber} in table {tableName}");
        return false; // NHS number not found after retries
    }

    public static async Task<bool> VerifyFieldsMatchCsvAsync(string connectionString, string tableName, string nhsNumber, string csvFilePath, ILogger logger)
    {
        ValidateTableName(tableName);

        var csvRecords = CsvHelperService.ReadCsv(csvFilePath);
        var expectedRecord = csvRecords.FirstOrDefault(record => record["NHS Number"] == nhsNumber);

        if (expectedRecord == null)
        {
            logger.LogError($"NHS number {nhsNumber} not found in the CSV file.");
            return false;
        }

        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            var query = $"SELECT * FROM {tableName} WHERE [NHS_NUMBER] = @NhsNumber";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NhsNumber", nhsNumber);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                    {
                        logger.LogError($"No record found in {tableName} for NHS number {nhsNumber}.");
                        return false;
                    }

                    while (await reader.ReadAsync())
                    {
                        foreach (var key in expectedRecord.Keys)
                        {
                            var expectedValue = expectedRecord[key];
                            var actualValue = reader[key]?.ToString();

                            if (expectedValue != actualValue)
                            {
                                logger.LogError($"Mismatch in {key} for NHS number {nhsNumber}: expected '{expectedValue}', found '{actualValue}'.");
                                return false;
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    public static async Task<bool> VerifyCsvWithDatabaseAsync(string connectionString, string tableName, string nhsNumber, string csvFilePath, ILogger logger,string managedIdentityClientId)
    {
        ValidateTableName(tableName);

        var csvRecords = CsvHelperService.ReadCsv(csvFilePath);
        var expectedRecord = csvRecords.FirstOrDefault(record => record["nhs_number"] == nhsNumber);

        if (expectedRecord == null)
        {
            logger.LogError($"NHS number {nhsNumber} not found in the CSV file.");
            return false;
        }

        var credential = new DefaultAzureCredential(
        new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = managedIdentityClientId
        });

        using (var connection = new SqlConnection(connectionString))
        {
            connection.AccessToken = (await credential.GetTokenAsync(new TokenRequestContext(new[] { "https://database.windows.net/.default" }))).Token;
            await connection.OpenAsync();
            var query = $"SELECT * FROM {tableName} WHERE [NHS_NUMBER] = @NhsNumber";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NhsNumber", nhsNumber);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                    {
                        logger.LogError($"No record found in {tableName} for NHS number {nhsNumber}.");
                        return false;
                    }

                    while (await reader.ReadAsync())
                    {
                        foreach(DictionaryEntry e in CsvTableFieldMap)
                        {
                            var expectedValue = expectedRecord[e.Key.ToString()];
                            var actualValue = reader[e.Value.ToString()];

                            if (actualValue is DateTime dateTimeValue)
                            {
                                DateTime date = System.Convert.ToDateTime(actualValue);
                                actualValue=date.ToString("dd/MM/yyyy");
                            }

                            if(!String.IsNullOrEmpty(expectedValue) )
                            {
                            if (expectedValue != actualValue.ToString())
                            {
                                logger.LogError($"Inside Mismatch in {e.Key.ToString()} for NHS number {nhsNumber}: expected '{expectedValue}', found '{actualValue}'.");
                                return false;
                            }
                            }
                            else if ( !String.IsNullOrEmpty(actualValue.ToString()))
                            {
                                logger.LogError($"Mismatch in {e.Key.ToString()} for NHS number {nhsNumber}: expected '{expectedValue}', found '{actualValue}'.");
                                return false;
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    public static async Task<int> GetNhsNumberCount(string connectionString, string tableName, string nhsNumber, ILogger logger, string managedIdentityClientId)
    {
    var nhsNumberCount = 0;

        var credential = new DefaultAzureCredential(
        new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = managedIdentityClientId
        });

    using (var connection = new SqlConnection(connectionString))
    {
        connection.AccessToken = (await credential.GetTokenAsync(new TokenRequestContext(new[] { "https://database.windows.net/.default" }))).Token;
        await connection.OpenAsync();

        var query = $"SELECT COUNT(*) FROM {tableName} WHERE [NHS_NUMBER] = @NhsNumber";
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@NhsNumber", nhsNumber);
            nhsNumberCount = (int)(await command.ExecuteScalarAsync() ?? 0);
        }
    }
    return nhsNumberCount;
    }

}
