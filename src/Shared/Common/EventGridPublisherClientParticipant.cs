using Azure.Messaging.EventGrid;

namespace NHS.ServiceInsights.Common;

public class EventGridPublisherClientParticipant : IEventGridPublisherClientParticipant
{
    private readonly EventGridPublisherClient _eventGridPublisherClient;

    public EventGridPublisherClientParticipant(EventGridPublisherClient eventGridPublisherClient)
    {
        _eventGridPublisherClient = eventGridPublisherClient;
    }

    public async Task SendEventAsync(EventGridEvent @event)
    {
        await _eventGridPublisherClient.SendEventAsync(@event);
    }
}
