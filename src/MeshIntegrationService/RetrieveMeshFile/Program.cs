using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NHS.MESH.Client;
using NHS.ServiceInsights.Common;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        //services.AddApplicationInsightsTelemetryWorkerService();
        //services.ConfigureFunctionsApplicationInsights();
        services
            .AddMeshClient(_ => _.MeshApiBaseUrl = Environment.GetEnvironmentVariable("MeshApiBaseUrl"))
            .AddMailbox(Environment.GetEnvironmentVariable("BSSMailBox")!, new NHS.MESH.Client.Configuration.MailboxConfiguration
            {
                Password = Environment.GetEnvironmentVariable("MeshPassword"),
                SharedKey = Environment.GetEnvironmentVariable("MeshSharedKey"),
                //Cert = new X509Certificate2(Environment.GetEnvironmentVariable("MeshKeyName"),Environment.GetEnvironmentVariable("MeshKeyPassphrase")) //THIS WILL NEED CHANGING TO PULL FROM A KEYSTORE OR BLOB
            })
            .Build();
        services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>();
        services.AddTransient<IMeshToBlobTransferHandler, MeshToBlobTransferHandler>();
    })
    //.AddExceptionHandler()
    .Build();

host.Run();
