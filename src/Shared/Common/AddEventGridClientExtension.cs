using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Identity;

namespace NHS.ServiceInsights.Common;

public static class AddEventGridClientExtension
{
    public static IHostBuilder AddEventGridClient(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<Func<string, IEventGridPublisherClient>>(sp =>
            {
                return topicName => CreateEventGridPublisherClient(context, topicName);
            });
        });
    }

    private static IEventGridPublisherClient CreateEventGridPublisherClient(HostBuilderContext context, string topicName)
    {
        string topicKey;
        string topicEndpoint;

        if (topicName == "episode")
        {
            topicKey = Environment.GetEnvironmentVariable("topicKey1");
            topicEndpoint = Environment.GetEnvironmentVariable("topicEndpoint1");
        }
        else if (topicName == "participant")
        {
            topicKey = Environment.GetEnvironmentVariable("topicKey2");
            topicEndpoint = Environment.GetEnvironmentVariable("topicEndpoint2");
        }
        else
        {
            throw new ArgumentException("Invalid topic name", nameof(topicName));
        }

        return CreateClient(context, topicKey, topicEndpoint, topicName);
    }

    private static IEventGridPublisherClient CreateClient(HostBuilderContext context, string topicKey, string topicEndpoint, string topicName)
    {
        var eventGridClient = HostEnvironmentEnvExtensions.IsDevelopment(context.HostingEnvironment)
            ? new Azure.Messaging.EventGrid.EventGridPublisherClient(new Uri(topicEndpoint), new Azure.AzureKeyCredential(topicKey))
            : new Azure.Messaging.EventGrid.EventGridPublisherClient(new Uri(topicEndpoint), new DefaultAzureCredential());

        return topicName switch
        {
            "episode" => new EventGridPublisherClientEpisode(eventGridClient),
            "participant" => new EventGridPublisherClientParticipant(eventGridClient),
            _ => throw new ArgumentException("Invalid topic name", nameof(topicName))
        };
    }
}
