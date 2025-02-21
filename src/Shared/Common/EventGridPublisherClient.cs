using Azure.Messaging.EventGrid;

namespace NHS.ServiceInsights.Common;

public interface IEventGridPublisherClient
{
    Task SendEventAsync(EventGridEvent @event);
}

public class EventGridPublisherClient : IEventGridPublisherClient
{
    private readonly Azure.Messaging.EventGrid.EventGridPublisherClient _eventGridPublisherClient;

    public EventGridPublisherClient(Azure.Messaging.EventGrid.EventGridPublisherClient eventGridPublisherClient)
    {
        _eventGridPublisherClient = eventGridPublisherClient;
    }

    public async Task SendEventAsync(EventGridEvent @event)
    {
        await _eventGridPublisherClient.SendEventAsync(@event);
    }
}

public class EventGridPublisherClientEpisode : EventGridPublisherClient
{
    public EventGridPublisherClientEpisode(Azure.Messaging.EventGrid.EventGridPublisherClient eventGridPublisherClient)
        : base(eventGridPublisherClient)
    {
    }
}

public class EventGridPublisherClientParticipant : EventGridPublisherClient
{
    public EventGridPublisherClientParticipant(Azure.Messaging.EventGrid.EventGridPublisherClient eventGridPublisherClient)
        : base(eventGridPublisherClient)
    {
    }
}
