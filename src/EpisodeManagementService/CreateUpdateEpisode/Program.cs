using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHS.ServiceInsights.Common;
using Azure.Messaging.EventGrid;
using Azure.Identity;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IHttpRequestService, HttpRequestService>();

        services.AddSingleton(sp =>
        {
            if(HostEnvironmentEnvExtensions.IsDevelopment(context.HostingEnvironment))
            {
                var credentials = new Azure.AzureKeyCredential(Environment.GetEnvironmentVariable("topicKey"));
                return new EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("topicEndpoint")), credentials);
            }

            return new EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("topicEndpoint")), new DefaultAzureCredential());
        });
    })
    .Build();
await host.RunAsync();
