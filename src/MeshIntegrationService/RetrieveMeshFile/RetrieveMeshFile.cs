using System.Text.Json;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHS.MESH.Client.Models;
using System.Globalization;

namespace NHS.ServiceInsights.MeshIntegrationService;

public class RetrieveMeshFile
{
    private readonly ILogger<RetrieveMeshFile> _logger;
    private readonly IMeshToBlobTransferHandler _meshToBlobTransferHandler;
    private readonly string _mailboxId;
    private readonly string _blobConnectionString;
    private readonly string _destinationContainer;
    private readonly string _poisonContainer;
    private readonly IBlobStorageHelper _blobStorageHelper;
    private const string NextHandShakeTimeConfigKey = "NextHandShakeTime";
    private const string ConfigFileName = "MeshState.json";

    public RetrieveMeshFile(ILogger<RetrieveMeshFile> logger, IMeshToBlobTransferHandler meshToBlobTransferHandler, IBlobStorageHelper blobStorageHelper, IOptions<RetrieveMeshFileConfig> options)
    {
        _logger = logger;
        _meshToBlobTransferHandler = meshToBlobTransferHandler;
        _blobStorageHelper = blobStorageHelper;
        _mailboxId = options.Value.BSSMailBox;
        _blobConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        _destinationContainer = Environment.GetEnvironmentVariable("BSSContainerName");
        _poisonContainer = Environment.GetEnvironmentVariable("PoisonContainerName");

        // Check for required environment variables
        if (string.IsNullOrEmpty(_mailboxId))
        {
            throw new InvalidOperationException("Environment variable 'BSSMailBox' is missing.");
        }

        if (string.IsNullOrEmpty(_blobConnectionString))
        {
            throw new InvalidOperationException("Environment variable 'AzureWebJobsStorage' is missing.");
        }

        if (string.IsNullOrEmpty(_destinationContainer))
        {
            throw new InvalidOperationException("Environment variable 'BSSContainerName' is missing.");
        }

        if (string.IsNullOrEmpty(_poisonContainer))
        {
            throw new InvalidOperationException("Environment variable 'PoisonContainerName' is missing.");
        }
    }

    /// <summary>
    /// This function polls the MESH Mailbox every 5 minutes, if there is a file posted to the mailbox.
    /// If there is a file in there will move the file to the Service Insights Blob Storage where it will be picked up by the ReceiveData Function.
    /// Invalid files will be moved to the Blob Storage Poison Container.
    /// </summary>
    [Function("RetrieveMeshFile")]
    public async Task RunAsync([TimerTrigger("%TimerExpression%")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {DateTime}", DateTime.Now);

        static bool messageFilter(MessageMetaData i) =>
            (i.FileName.StartsWith("bss_subjects") || i.FileName.StartsWith("bss_episodes")) &&
            (i.FileName.EndsWith(".csv") || i.FileName.EndsWith(".gz"));

        static string fileNameFunction(MessageMetaData i) => i.FileName;

        try
        {
            var shouldExecuteHandShake = await ShouldExecuteHandShake();

            // Process files
            var result = await _meshToBlobTransferHandler.MoveFilesFromMeshToBlob(
                messageFilter,
                fileNameFunction,
                _mailboxId,
                _blobConnectionString,
                _destinationContainer,
                _poisonContainer,
                shouldExecuteHandShake
            );

            if (!result)
            {
                _logger.LogError("An error encountered while moving files from Mesh to Blob Storage");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error encountered while moving files from Mesh to Blob Storage");
        }

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {NextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }

    private async Task<bool> ShouldExecuteHandShake()
    {
        Dictionary<string, string> configValues;
        TimeSpan handShakeInterval = new TimeSpan(0, 23, 54, 0);
        var meshState = await _blobStorageHelper.GetFileFromBlobStorage(_blobConnectionString, "config", ConfigFileName);
        if (meshState == null)
        {

            _logger.LogInformation("MeshState File did not exist, Creating new MeshState File in Blob Storage");
            configValues = new Dictionary<string, string>
            {
                { NextHandShakeTimeConfigKey, DateTime.UtcNow.Add(handShakeInterval).ToString() }
            };
            await SetConfigState(configValues);

            return true;

        }
        using (StreamReader reader = new StreamReader(meshState.Data))
        {
            meshState.Data.Seek(0, SeekOrigin.Begin);
            string jsonData = await reader.ReadToEndAsync();
            configValues = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);
        }

        string nextHandShakeDateString;
        //config value does not exist
        if (!configValues.TryGetValue(NextHandShakeTimeConfigKey, out nextHandShakeDateString))
        {
            _logger.LogInformation("NextHandShakeTime config item does not exist, creating new config item");
            configValues.Add(NextHandShakeTimeConfigKey, DateTime.UtcNow.Add(handShakeInterval).ToString());
            await SetConfigState(configValues);
            return true;


        }
        DateTime nextHandShakeDateTime;
        //date cannot be parsed
        if (!DateTime.TryParse(nextHandShakeDateString, CultureInfo.InvariantCulture, out nextHandShakeDateTime))
        {
            _logger.LogInformation("Unable to Parse NextHandShakeTime, Updating config value");
            configValues[NextHandShakeTimeConfigKey] = DateTime.UtcNow.Add(handShakeInterval).ToString();
            SetConfigState(configValues);
            return true;
        }

        if (DateTime.Compare(nextHandShakeDateTime, DateTime.UtcNow) <= 0)
        {
            _logger.LogInformation("Next HandShakeTime was in the past, will execute handshake");
            var NextHandShakeTimeConfig = DateTime.UtcNow.Add(handShakeInterval).ToString();

            configValues[NextHandShakeTimeConfigKey] = NextHandShakeTimeConfig;
            _logger.LogInformation("Next Handshake scheduled for {NextHandShakeTimeConfig}", nextHandShakeDateTime);

            return true;

        }
        _logger.LogInformation("Next handshake scheduled for {nextHandShakeDateTime}", nextHandShakeDateTime);
        return false;
    }

    private async Task<bool> SetConfigState(Dictionary<string, string> state)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(state);
            using (var stream = GenerateStreamFromString(jsonString))
            {
                var blobFile = new BlobFile(stream, ConfigFileName);
                // Upload blob but do not overwrite an existing blob
                var result = await _blobStorageHelper.UploadFileToBlobStorage(_blobConnectionString, "config", blobFile, false);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable To set Config State");
            return false;
        }
    }

    public static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
