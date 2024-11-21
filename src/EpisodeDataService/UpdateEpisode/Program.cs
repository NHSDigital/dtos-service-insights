using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHS.ServiceInsights.Data;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddScoped<IEpisodeRepository, EpisodeRepository>();
        services.AddScoped<IEndCodeLkpRepository, EndCodeLkpRepository>();
        services.AddScoped<IEpisodeTypeLkpRepository, EpisodeTypeLkpRepository>();
        services.AddScoped<IReasonClosedCodeLkpRepository, ReasonClosedCodeLkpRepository>();
        services.AddScoped<IFinalActionCodeLkpRepository, FinalActionCodeLkpRepository>();
        services.AddDbContext<ServiceInsightsDbContext>(
            options => options.UseSqlServer(Environment.GetEnvironmentVariable("ServiceInsightsDbConnectionString")));
    })
    .Build();
await host.RunAsync();
