using NHS.ServiceInsights.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NHS.MESH.Client.Models;

namespace NHS.Screening.RetrieveMeshFile;
public class RetrieveMeshFile
{
    private readonly ILogger _logger;

    private readonly IMeshToBlobTransferHandler _meshToBlobTransferHandler;
    private readonly string _mailboxId;
    private readonly string _blobConnectionString;

    private readonly string _destinationContainer;

    public RetrieveMeshFile(ILogger<RetrieveMeshFile> logger, IMeshToBlobTransferHandler meshToBlobTransferHandler)
    {
        _logger = logger;
        _meshToBlobTransferHandler = meshToBlobTransferHandler;


        _mailboxId = Environment.GetEnvironmentVariable("BSSMailBox");
        _blobConnectionString = Environment.GetEnvironmentVariable("bssfolder_STORAGE");
        _destinationContainer = Environment.GetEnvironmentVariable("bsscontainer_NAME");
    }
    /// <summary>
    /// This function polls the MESH Mailbox every 5 minutes, if there is a file posted to the mailbox.
    /// If there is a file in there will move the file to the Cohort Manager Blob Storage where it will be picked up by the ReceiveCaasFile Function.
    /// </summary>
    [Function("RetrieveMeshFile")]
    public async Task RunAsync([TimerTrigger("*/10 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        static bool messageFilter(MessageMetaData i) => true; // No current filter defined there might be business rules here

        try
        {
            var result = await _meshToBlobTransferHandler.MoveFilesFromMeshToBlob(messageFilter, _mailboxId, _blobConnectionString, _destinationContainer);

            if (!result)
            {
                _logger.LogError("An error was encountered while moving files from Mesh to Blob");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error encountered while moving files from Mesh to Blob");
        }

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }
    }
}
