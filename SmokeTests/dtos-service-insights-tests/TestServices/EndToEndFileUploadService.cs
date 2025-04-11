using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using dtos_service_insights_tests.Config;
using dtos_service_insights_tests.Helpers;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace dtos_service_insights_tests.TestServices;

public class EndToEndFileUploadService
{
    private readonly ILogger<EndToEndFileUploadService> _logger;
    private readonly AppSettings _appSettings;
    private readonly BlobStorageHelper _blobStorageHelper;
    private readonly string _connectionString;
    //public string LocalFilePath => _appSettings.FilePaths.Local;
    private readonly string _managedIdentityClientId;

    public EndToEndFileUploadService(ILogger<EndToEndFileUploadService> logger, AppSettings appSettings, BlobStorageHelper blobStorageHelper)
    {
        _logger = logger;
        _appSettings = appSettings;
        _blobStorageHelper = blobStorageHelper;
        _connectionString = _appSettings.ConnectionStrings.ServiceInsightsDatabaseConnectionString;
        _managedIdentityClientId = _appSettings.ManagedIdentityClientId;
    }

    public async Task CleanDatabaseAsync(IEnumerable<string> episodeIds)
    {
        _logger.LogInformation("Starting database cleanup.");

        try
        {
            foreach (var episodeId in episodeIds)
            {
                //  parameterized queries to prevent SQL injection
                await DatabaseHelper.ExecuteNonQueryAsync(_sqlConnectionWithAuthentication,
                    "DELETE FROM dbo.EPISODE WHERE Episode_Id = @episodeId",
                    new SqlParameter("@episodeId", episodeId));
            }

            _logger.LogInformation("Database cleanup completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database cleanup.");
            throw; // Re-throw the exception to be handled elsewhere
        }
    }

    public async Task UploadFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
        _logger.LogError("File not found at {FilePath}", filePath);
        throw new FileNotFoundException($"File not found at {filePath}");
        }

        int retryCount = 0;
        const int maxRetries = 5;
        TimeSpan delay = TimeSpan.FromSeconds(1);

        while (retryCount < maxRetries)
        {
            try
            {
                _logger.LogInformation("Uploading file {FilePath} to Blob Storage (Attempt {AttemptNumber}).", filePath, retryCount + 1);
                await _blobStorageHelper.UploadFileToBlobStorageAsync(filePath, _appSettings.BlobContainerName);
                _logger.LogInformation("File uploaded successfully.");
                return; // Exit the loop if successful
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FilePath} to Blob Storage (Attempt {AttemptNumber}).", filePath, retryCount + 1);
                retryCount++;
                await Task.Delay(delay);
                delay = delay * 2; // Exponential backoff
            }
        }

        _logger.LogError("Failed to upload file {FilePath} to Blob Storage after {MaxRetries} retries.", filePath, maxRetries);
        throw new Exception($"Failed to upload file {filePath} to Blob Storage after {maxRetries} retries.");
    }

    public async Task<bool> VerifyRecordCountAsync(string tableName, int originalCount, int expectedIncrement, int retries = 10, int delay = 1000)
    {
        _logger.LogInformation("Verifying record count for table {TableName}.", tableName);

        for (int i = 0; i < retries; i++)
        {
            var newCount = await DatabaseHelper.GetRecordCountAsync(_connectionString, tableName);
            if (newCount == originalCount + expectedIncrement)
            {
                _logger.LogInformation("Record count verified: Expected = {Expected}, Actual = {Actual}.", originalCount + expectedIncrement, newCount);
                return true;
            }

            _logger.LogWarning("Record count not updated for {TableName}. Retry {Retry}/{MaxRetries}.", tableName, i + 1, retries);
            await Task.Delay(delay);
        }

        _logger.LogError("Failed to verify record count for {TableName} after {MaxRetries} retries.", tableName, retries);
        return false;
    }

   public async Task VerifyEpisodeIdsAsync(string tableName, List<string> episodeIds)
    {
        _logger.LogInformation("Validating Episode Ids in table {TableName}.", tableName);
        await DatabaseValidationHelper.VerifyEpisodeIdsAsync(_sqlConnectionWithAuthentication, tableName, episodeIds, _logger,_managedIdentityClientId);
        _logger.LogInformation("Validation of Episode Ids completed successfully.");
    }

    public async Task VerifyCsvDataAsync(string tableName, List<string> episodeIds)
    {
        _logger.LogInformation("Validating csv data in table {TableName}.", tableName);
        await DatabaseValidationHelper.VerifyEpisodeIdsAsync(_sqlConnectionWithAuthentication, tableName, episodeIds, _logger,_managedIdentityClientId);
        _logger.LogInformation("Validation of Episode Ids completed successfully.");
    }

    public async Task VerifyEpisodeIdsCountAsync(string tableName, string episodeId, int expectedCount)
    {
        _logger.LogInformation("Validating Episode Id count in table {TableName}.", tableName);
        Func<Task> act = async () =>
        {
            var nhsNumberCount = await DatabaseValidationHelper.GetEpisodeIdCount(_sqlConnectionWithAuthentication, tableName, episodeId, _logger, _managedIdentityClientId);
            nhsNumberCount.Should().Be(expectedCount);
        };

        await act.Should().NotThrowAfterAsync(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(5));
        _logger.LogInformation("Validation of Episode Id count completed successfully.");
    }

    public async Task VerifyFieldUpdateAsync(string tableName, string episodeId, string fieldName, string expectedValue)
    {
        Func<Task> act = async () =>
        {
            var result = await DatabaseValidationHelper.VerifyFieldUpdateAsync(_sqlConnectionWithAuthentication, tableName, episodeId,fieldName,_managedIdentityClientId, expectedValue, _logger);
            result.Should().BeTrue();
        };

        await act.Should().NotThrowAfterAsync(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(5));

    }
    public async Task VerifyFullDatabaseRecordAsync(string tableName, string nhsNumber, string csvFilePath)
    {
        Func<Task> act = async () =>
        {
            var result= await DatabaseValidationHelper.VerifyCsvWithDatabaseAsync(_connectionString,tableName,nhsNumber,csvFilePath,_logger,_managedIdentityClientId);
            result.Should().BeTrue();
        };

        await act.Should().NotThrowAfterAsync(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(5));

    }


}
