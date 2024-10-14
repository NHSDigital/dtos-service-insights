namespace IntegrationTests.Helpers
{
    public class AppSettings
    {
        public Endpoints Endpoints { get; set; }
        public FilePaths FilePaths { get; set; }
        public string BlobContainerName { get; set; }
        public string AzureWebJobsStorage { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
    }
}
