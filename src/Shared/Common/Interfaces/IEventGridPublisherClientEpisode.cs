using Azure.Messaging.EventGrid;

namespace NHS.ServiceInsights.Common;

public interface IEventGridPublisherClientEpisode
{
    Task SendEventAsync(EventGridEvent @event);
}
