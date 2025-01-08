using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHS.ServiceInsights.Data;
using Microsoft.Azure.Functions.Worker;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<IEndCodeLkpRepository, EndCodeLkpRepository>();
        services.AddScoped<IEpisodeTypeLkpRepository, EpisodeTypeLkpRepository>();
        services.AddScoped<IReasonClosedCodeLkpRepository, ReasonClosedCodeLkpRepository>();
        services.AddScoped<IFinalActionCodeLkpRepository, FinalActionCodeLkpRepository>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<ServiceInsightsDbContext>(
            options => options.UseSqlServer(Environment.GetEnvironmentVariable("ServiceInsightsDbConnectionString")));
    })
    .Build();
await host.RunAsync();
