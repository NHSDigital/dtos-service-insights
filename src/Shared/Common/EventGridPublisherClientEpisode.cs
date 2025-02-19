using Azure.Messaging.EventGrid;

namespace NHS.ServiceInsights.Common;

public class EventGridPublisherClientEpisode : IEventGridPublisherClientEpisode
{
    private readonly EventGridPublisherClient _eventGridPublisherClient;

    public EventGridPublisherClientEpisode(EventGridPublisherClient eventGridPublisherClient)
    {
        _eventGridPublisherClient = eventGridPublisherClient;
    }

    public async Task SendEventAsync(EventGridEvent @event)
    {
        await _eventGridPublisherClient.SendEventAsync(@event);
    }
}
