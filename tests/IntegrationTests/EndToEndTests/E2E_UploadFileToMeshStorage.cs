using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;

namespace Tests.Integration.EndToEndTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class E2E_UploadFileToMeshStorage : BaseIntegrationTest
    {
        private BlobStorageHelper _blobStorageHelper;
        private string _blobContainerName;
        private string _episodeFilePath;
        private string _episodeFileName;
        private string _participantFilePath;
        private string _participantFileName;
        private ILogger<E2E_UploadFileToMeshStorage> _logger;
        private MeshMailboxHelper _meshMailboxHelper;

        [TestInitialize]
        public async Task TestInitialize()
        {
            _blobStorageHelper = ServiceProvider.GetService<BlobStorageHelper>();
            _blobContainerName = AppSettings.BlobContainerName;
            _logger = ServiceProvider.GetService<ILogger<E2E_UploadFileToMeshStorage>>();
            _meshMailboxHelper = new MeshMailboxHelper(AppSettings.Endpoints.MeshSandboxOutput, AppSettings, HttpClient);

            _episodeFilePath = AppSettings.FilePaths.LocalEpisodesCSVFile;
            _participantFilePath = AppSettings.FilePaths.LocalSubjectsCSVFile;

            _episodeFileName = Path.GetFileName(_episodeFilePath);
            _participantFileName = Path.GetFileName(_participantFilePath);

            // Ensure all config is set
            AssertAllConfigurations();

            // Get the direct filepath to the csv test file and ensure it exists
            var localFilePath = Path.Combine(AppContext.BaseDirectory, AppSettings.FilePaths.LocalEpisodesCSVFile);
            Assert.IsTrue(localFilePath.Length > 0);
        }

        [TestMethod]
        public async Task EndToEnd_UploadEpisodeFileToMeshAndWaitForIngestAndCheckItHasReachedBlobStorage()
        {
            var localFilePath = Path.Combine(AppContext.BaseDirectory, _episodeFilePath);
            await UploadFileToMeshMailboxAsync(_episodeFileName, localFilePath);
            //In an integrated environment, the default mesh interval value is 5m, which is why this wait period is so long
            Thread.Sleep(AppSettings.MeshSettings.intervalInMs);
            Assert.IsTrue(await GetFileFromBlobAsync(_episodeFilePath));
        }

        [TestMethod]
        public async Task EndToEnd_UploadParticipantFileToMeshAndWaitForIngestAndCheckItHasReachedBlobStorage()
        {
            var localFilePath = Path.Combine(AppContext.BaseDirectory, _participantFilePath);
            await UploadFileToMeshMailboxAsync(_participantFileName, localFilePath);
            //In an integrated environment, the default mesh interval value is 5m, which is why this wait period is so long
            Thread.Sleep(AppSettings.MeshSettings.intervalInMs);
            Assert.IsTrue(await GetFileFromBlobAsync(_participantFilePath));
        }

        private async Task UploadFileToMeshMailboxAsync(string fileName, string filePath)
        {
            Assert.IsTrue(
                await _meshMailboxHelper.UploadFileToMeshMailboxAsync(filePath, fileName),
                "Failed to upload file to mesh mailbox."
            );
        }

        private async Task<bool> GetFileFromBlobAsync(string filePath)
        {
            return await _blobStorageHelper.ReadFileFromBlobStorageAsync
            (
                filePath,
                _blobContainerName
            );
        }
    }
}

