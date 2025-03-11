using System;
using dtos_service_insights_tests.TestServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
//using SolidToken.SpecFlow.DependencyInjection;
using dtos_service_insights_tests.Config;
using dtos_service_insights_tests.Helpers;
using dtos_service_insights_tests.Contexts;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace dtos_service_insights_tests;

internal static class Startup
{
[ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        return services;

    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Load configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("/Users/nageswarundapalli/Documents/GitHub/DTOS_app_insights_reqn/Config/appsettings.json", optional: false, reloadOnChange: true)
            .Build();
            
        

        // Bind AppSettings section to POCO
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        // Add logging
        services=services.AddLogging(configure => configure.AddConsole());

        // Register Azure Blob Storage helper
        services.AddSingleton(sp =>
        {
            var connectionString = configuration["AppSettings:caasFileStorage"];
            return new Azure.Storage.Blobs.BlobServiceClient(connectionString);
        });
        services.AddSingleton<BlobStorageHelper>();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);
        services.AddTransient<EndToEndFileUploadService>();

        services.AddScoped(_ => new SmokeTestsContexts());
    }
}
