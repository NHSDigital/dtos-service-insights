using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IntegrationTests.Helpers;
using System.Net;

namespace Tests.Integration.EndToEndTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class E2E_FileUploadAndCreateEpisodeTest : BaseIntegrationTest
    {
        private BlobStorageHelper _blobStorageHelper;
        private DatabaseHelper _databaseHelper;
        private List<string> _episodeIds;
        private string _blobContainerName;
        private ILogger<E2E_FileUploadAndCreateEpisodeTest> _logger;

        [TestInitialize]
        public async Task TestInitialize()
        {
            _blobStorageHelper = ServiceProvider.GetService<BlobStorageHelper>();
            _databaseHelper = ServiceProvider.GetService<DatabaseHelper>();
            _blobContainerName = AppSettings.BlobContainerName;
            _logger = ServiceProvider.GetService<ILogger<E2E_FileUploadAndCreateEpisodeTest>>();

            // Ensure all config is set
            AssertAllConfigurations();

            // Clean the database before tests
            await _databaseHelper.CleanDatabaseAsync();

            // Extract all Episode IDs from the JSON file
            var localFilePath = Path.Combine(AppContext.BaseDirectory, AppSettings.FilePaths.LocalEpisodesCSVFile);
            _episodeIds = EpisodeCsvHelper.ExtractEpisodeIds(localFilePath);
            Assert.IsTrue(_episodeIds.Count > 0, "No Episode IDs found in the JSON file.");
        }

        [TestMethod]
        public async Task EndToEnd_FileUploadAndCreateEpisodeTest()
        {
            await UploadFileToBlobStorageAsync();

            foreach (var episodeId in _episodeIds)
            {
                bool episodeCreated = await RetryGetEpisodeFromFunctionAsync(episodeId);
                Assert.IsTrue(episodeCreated, "Episode was not created as expected.");
            }
        }

        private async Task UploadFileToBlobStorageAsync()
        {
            var localFilePath = Path.Combine(AppContext.BaseDirectory, AppSettings.FilePaths.LocalEpisodesCSVFile);
            Assert.IsTrue(File.Exists(localFilePath), $"File not found at {localFilePath}");

            bool success = await _blobStorageHelper.UploadFileToBlobStorageAsync(localFilePath, _blobContainerName);
            Assert.IsTrue(success, "Failed to upload file to blob storage.");
        }

        private async Task<bool> RetryGetEpisodeFromFunctionAsync(string episodeId)
        {
            var retryHelper = new AzureFunctionRetryHelper(ServiceProvider.GetService<ILogger<AzureFunctionRetryHelper>>());
            return await retryHelper.RetryAsync(() => GetEpisodeFromFunctionAsync(episodeId), "Episode");
        }

        private async Task<bool> GetEpisodeFromFunctionAsync(string episodeId)
        {
            var response = await HttpClient.GetAsync($"{AppSettings.Endpoints.GetEpisode}?EpisodeId={episodeId}");
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}
