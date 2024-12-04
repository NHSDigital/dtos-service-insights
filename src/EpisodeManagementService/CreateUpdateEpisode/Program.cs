using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHS.ServiceInsights.Common;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IHttpRequestService, HttpRequestService>();
    })
    .Build();
await host.RunAsync();
