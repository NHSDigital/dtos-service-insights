using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.EventGrid;
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
                return topicName =>
                {
                    if (topicName == "episode")
                    {
                        if (HostEnvironmentEnvExtensions.IsDevelopment(context.HostingEnvironment))
                        {
                            var credentials = new Azure.AzureKeyCredential(Environment.GetEnvironmentVariable("topicKey1"));
                            return new EventGridPublisherClientEpisode(new EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("topicEndpoint1")), credentials));
                        }

                        return new EventGridPublisherClientEpisode(new EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("topicEndpoint1")), new DefaultAzureCredential()));
                    }
                    else if (topicName == "participant")
                    {
                        if (HostEnvironmentEnvExtensions.IsDevelopment(context.HostingEnvironment))
                        {
                            var credentials = new Azure.AzureKeyCredential(Environment.GetEnvironmentVariable("topicKey2"));
                            return new EventGridPublisherClientParticipant(new EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("topicEndpoint2")), credentials));
                        }

                        return new EventGridPublisherClientParticipant(new EventGridPublisherClient(new Uri(Environment.GetEnvironmentVariable("topicEndpoint2")), new DefaultAzureCredential()));
                    }
                    else
                    {
                        throw new ArgumentException("Invalid topic name", nameof(topicName));
                    }
                };
            });
        });
    }
}
