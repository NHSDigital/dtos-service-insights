using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHS.ServiceInsights.Common;
using Azure.Messaging.EventGrid;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IHttpRequestService, HttpRequestService>();

        services.AddSingleton(sp =>
        {
            var credentials = new Azure.AzureKeyCredential(Environment.GetEnvironmentVariable("topicKey"));
            return new EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("topicEndpoint")), credentials);
        });
    })
    .Build();
await host.RunAsync();
