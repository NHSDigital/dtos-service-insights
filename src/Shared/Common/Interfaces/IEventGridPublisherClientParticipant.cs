using Azure.Messaging.EventGrid;

namespace NHS.ServiceInsights.Common;

public interface IEventGridPublisherClientParticipant
{
    Task SendEventAsync(EventGridEvent @event);
}
