using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHS.ServiceInsights.Common;
using Microsoft.EntityFrameworkCore;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IHttpRequestService, HttpRequestService>();
    })
    .Build();
host.Run();
