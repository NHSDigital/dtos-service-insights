using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHS.ServiceInsights.Data;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.EventGrid;
using Azure.Identity;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<IEpisodeRepository, EpisodeRepository>();
        services.AddScoped<IEndCodeLkpRepository, EndCodeLkpRepository>();
        services.AddScoped<IEpisodeTypeLkpRepository, EpisodeTypeLkpRepository>();
        services.AddScoped<IReasonClosedCodeLkpRepository, ReasonClosedCodeLkpRepository>();
        services.AddScoped<IFinalActionCodeLkpRepository, FinalActionCodeLkpRepository>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<ServiceInsightsDbContext>(
            options => options.UseSqlServer(Environment.GetEnvironmentVariable("ServiceInsightsDbConnectionString")));

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
