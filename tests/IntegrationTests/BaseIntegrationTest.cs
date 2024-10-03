using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using NHS.ServiceInsights.Data;

namespace IntegrationTests.Helpers
{
    public abstract class BaseIntegrationTest
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected ILogger<BaseIntegrationTest> Logger { get; private set; }
        protected AppSettings AppSettings { get; private set; }
        protected HttpClient HttpClient { get; private set; }

        [TestInitialize]
        public void Setup()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();
            Logger = ServiceProvider.GetService<ILogger<BaseIntegrationTest>>();
            AppSettings = ServiceProvider.GetService<IOptions<AppSettings>>()?.Value;
            HttpClient = new HttpClient();

            AssertAllConfigurations();
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Bind the configuration to AppSettings
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

            // Register Blob Storage dependencies
            services.AddSingleton(sp =>
            {
                var azureStorageConnectionString = configuration["AppSettings:AzureWebJobsStorage"];
                return new BlobServiceClient(azureStorageConnectionString);
            });

            services.AddSingleton<BlobStorageHelper>();

            // Register DbContext
            services.AddDbContext<ServiceInsightsDbContext>(options =>
                options.UseSqlServer(configuration.GetSection("AppSettings:ConnectionStrings")["ServiceInsightsDbConnectionString"]));

            // Register DatabaseHelper
            services.AddScoped<DatabaseHelper>();

            // Add logging
            services.AddLogging(logging =>
            {
                logging.AddConsole();
            });
        }

        protected virtual void AssertAllConfigurations()
        {
            // Assert configs
            if (AppSettings == null)
            {
                Assert.Fail("AppSettings configuration is not set.");
            }

            if (string.IsNullOrEmpty(AppSettings.ConnectionStrings?.ServiceInsightsDbConnectionString))
            {
                Assert.Fail("Database connection string is not set in AppSettings.");
            }
        }
    }
}
