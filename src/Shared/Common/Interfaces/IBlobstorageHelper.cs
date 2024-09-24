using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Common;

public interface IBlobStorageHelper
{
    Task<bool> CopyFileAsync(string connectionString, string fileName, string containerName);

    Task<bool> UploadFileToBlobStorage(string connectionString, string containerName, BlobFile blobFile);
}
