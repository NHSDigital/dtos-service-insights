using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHS.ServiceInsights.Common;
using NHS.ServiceInsights.Data;
using Microsoft.EntityFrameworkCore;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddScoped<IParticipantScreeningEpisodeRepository, ParticipantScreeningEpisodeRepository>();
        services.AddSingleton<IHttpRequestService, HttpRequestService>();
        services.AddDbContext<ServiceInsightsDbContext>(
            options => options.UseSqlServer(Environment.GetEnvironmentVariable("ServiceInsightsDbConnectionString")));
    })
    .Build();
host.Run();
