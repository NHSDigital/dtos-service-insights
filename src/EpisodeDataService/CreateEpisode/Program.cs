using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHS.ServiceInsights.Data;
using Microsoft.Azure.Functions.Worker;
using NHS.ServiceInsights.Common;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IHttpRequestService, HttpRequestService>();
        services.AddScoped<IEpisodeRepository, EpisodeRepository>();
        services.AddScoped<IEpisodeLkpRepository, EpisodeLkpRepository>();
        services.AddScoped<IEndCodeLkpRepository, EndCodeLkpRepository>();
        services.AddScoped<IEpisodeTypeLkpRepository, EpisodeTypeLkpRepository>();
        services.AddScoped<IReasonClosedCodeLkpRepository, ReasonClosedCodeLkpRepository>();
        services.AddScoped<IFinalActionCodeLkpRepository, FinalActionCodeLkpRepository>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<ServiceInsightsDbContext>(
        options => options.UseSqlServer(Environment.GetEnvironmentVariable("ServiceInsightsDbConnectionString")));
        services.AddSingleton<IHttpRequestService, HttpRequestService>();
    }).AddEventGridClient()
    .Build();
await host.RunAsync();
