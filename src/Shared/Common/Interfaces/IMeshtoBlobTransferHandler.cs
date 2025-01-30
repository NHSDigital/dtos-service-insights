using NHS.MESH.Client.Models;

namespace NHS.ServiceInsights.Common;

public interface IMeshToBlobTransferHandler
{
    Task<bool> MoveFilesFromMeshToBlob(Func<MessageMetaData, bool> predicate, Func<MessageMetaData, string> fileNameFunction, string mailboxId, string blobConnectionString, string destinationContainer, string poisonContainer, bool executeHandshake = false);
}
