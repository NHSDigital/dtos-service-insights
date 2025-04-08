using System;

namespace dtos_service_insights_tests.Config;

public class AppSettings
{
    public ConnectionStrings ConnectionStrings { get; set; }
    public FilePaths FilePaths { get; set; }
    public string BlobContainerName { get; set; }
    public string AzureWebJobsStorage { get; set; }
    public string ManagedIdentityClientId { get; set; }
    public string SubscriptionId { get; set; }

    public AzureSettings AzureSettings { get; set; }

    public EndPoints EndPoints { get; set; }
    public string IsCloudEnvironment { get; set; }
}



