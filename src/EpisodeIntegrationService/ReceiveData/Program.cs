using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Data;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IHttpRequestService, HttpRequestService>();
        services.AddTransient<IEndCodeLkpRepository, EndCodeLkpRepository>();
        services.AddTransient<IEpisodeTypeLkpRepository, EpisodeTypeLkpRepository>();
        services.AddTransient<IOrganisationLkpRepository, OrganisationLkpRepository>();
        services.AddDbContext<ServiceInsightsDbContext>(
            options => options.UseSqlServer(Environment.GetEnvironmentVariable("ServiceInsightsDbConnectionString")));
    })
    .Build();
host.Run();
