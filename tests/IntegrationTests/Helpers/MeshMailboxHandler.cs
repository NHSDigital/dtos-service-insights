using Microsoft.Extensions.Logging;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Net.Http.Headers;

namespace IntegrationTests.Helpers
{
    public class MeshMailboxHelper
    {
        private readonly string _meshURL;
        private readonly AppSettings _appSettings;
        private readonly HttpClient _httpClient;

        public MeshMailboxHelper(string meshURL, AppSettings appSettings, HttpClient httpClient)
        {
            _meshURL = meshURL;
            _appSettings = appSettings;
            _httpClient = httpClient;
        }

        public async Task<bool> UploadFileToMeshMailboxAsync(string filePath, string fileName)
        {
            Assert.IsTrue(File.Exists(filePath), $"File not found at {filePath}");


            string content = await File.ReadAllTextAsync(filePath);
            HttpContent fileContent = new StringContent(content);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            //auth isn't handled conventionally with Mesh, doesn't require a bearer token
            _httpClient.DefaultRequestHeaders.Add(
                "authorization",
                _appSettings.MeshSettings.authorization
            );
            //filename that shows up in blob storage is set here
            _httpClient.DefaultRequestHeaders.Add("mex-filename", fileName);
            _httpClient.DefaultRequestHeaders.Add("mex-from", _appSettings.MeshSettings.meshID);
            _httpClient.DefaultRequestHeaders.Add("mex-to", _appSettings.MeshSettings.meshID);
            _httpClient.DefaultRequestHeaders.Add("mex-workflowid", "API-DOCS-TEST");

            var success = await _httpClient.PostAsync($"{_appSettings.Endpoints.MeshSandboxOutput}X26ABC1/outbox", fileContent);
            return success.IsSuccessStatusCode;
        }
    }
}
