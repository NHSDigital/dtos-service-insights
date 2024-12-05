using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHS.ServiceInsights.Common;
using Microsoft.Azure.Functions.Worker;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IHttpRequestService, HttpRequestService>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

await host.RunAsync();
