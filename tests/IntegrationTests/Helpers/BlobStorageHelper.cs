using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
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

        public async Task<bool> ReadFileFromBlobStorageAsync(string filePath, string blobContainerName){
            try
            {
                _logger.LogInformation("Checking file exists in blob storage.");

                int idx = filePath.LastIndexOf('/');
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
                var blobClient = blobContainerClient.GetBlobClient(filePath.Substring(idx+1));

                if (await blobClient.ExistsAsync())
                {
                    _logger.LogInformation("File was found in blob storage");
                    return true;
                }
                _logger.LogInformation("File was not found in blob storage");
                return false;

            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while searching for file in blob storage: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteFileFromBlobStorageAsync(string filePath, string blobContainerName)
        {
            int idx = filePath.LastIndexOf('/');
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
            var blobClient = blobContainerClient.GetBlobClient(Path.GetFileName(filePath));
            var azureResponse = await blobClient.DeleteAsync(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots);
            if (azureResponse.IsError) {
                _logger.LogInformation("Failed to clean blob storage");
                return false;
            }
            return true;
        }
    }
}
