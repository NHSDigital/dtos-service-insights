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
        if (topicName == "episode")
        {
            return CreateEpisodeClient(context);
        }
        else if (topicName == "participant")
        {
            return CreateParticipantClient(context);
        }
        else
        {
            throw new ArgumentException("Invalid topic name", nameof(topicName));
        }
    }

    private static IEventGridPublisherClient CreateEpisodeClient(HostBuilderContext context)
    {
        if (HostEnvironmentEnvExtensions.IsDevelopment(context.HostingEnvironment))
        {
            var credentials = new Azure.AzureKeyCredential(Environment.GetEnvironmentVariable("topicKey1"));
            return new EventGridPublisherClientEpisode(new Azure.Messaging.EventGrid.EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("topicEndpoint1")), credentials));
        }

        return new EventGridPublisherClientEpisode(new Azure.Messaging.EventGrid.EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("topicEndpoint1")), new DefaultAzureCredential()));
    }

    private static IEventGridPublisherClient CreateParticipantClient(HostBuilderContext context)
    {
        if (HostEnvironmentEnvExtensions.IsDevelopment(context.HostingEnvironment))
        {
            var credentials = new Azure.AzureKeyCredential(Environment.GetEnvironmentVariable("topicKey2"));
            return new EventGridPublisherClientParticipant(new Azure.Messaging.EventGrid.EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("topicEndpoint2")), credentials));
        }

        return new EventGridPublisherClientParticipant(new Azure.Messaging.EventGrid.EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("topicEndpoint2")), new DefaultAzureCredential()));
    }
}
