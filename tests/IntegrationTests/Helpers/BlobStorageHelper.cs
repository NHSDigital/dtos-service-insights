using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace IntegrationTests.Helpers
{
    public class BlobStorageHelper
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<BlobStorageHelper> _logger;

        public BlobStorageHelper(BlobServiceClient blobServiceClient, ILogger<BlobStorageHelper> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task<bool> UploadFileToBlobStorageAsync(string filePath, string blobContainerName)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("File not found.");
                return false;
            }

            _logger.LogInformation("Uploading file to blob storage.");

            try
            {
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
                await blobContainerClient.CreateIfNotExistsAsync();

                var blobClient = blobContainerClient.GetBlobClient(Path.GetFileName(filePath));
                await blobClient.UploadAsync(File.OpenRead(filePath), overwrite: true);

                _logger.LogInformation("File uploaded successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while uploading file: {Message}", ex.Message);
                return false;
            }
        }
    }
}
