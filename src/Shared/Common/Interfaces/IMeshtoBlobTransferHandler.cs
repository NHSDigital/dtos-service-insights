using NHS.MESH.Client.Models;

namespace NHS.ServiceInsights.Common;

public interface IMeshToBlobTransferHandler
{
    Task<bool> MoveFilesFromMeshToBlob(Func<MessageMetaData, bool> predicate, string mailboxId, string blobConnectionString, string destinationContainer);
}
