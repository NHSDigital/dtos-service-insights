using Azure.Messaging.EventGrid;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Common;

public interface IEventGridPublisherClient
{
    Task SendEventAsync(EventGridEvent @event);
}

public class EventGridPublisherClient<T> : IEventGridPublisherClient
{
    private readonly EventGridPublisherClient _eventGridPublisherClient;

    public EventGridPublisherClient(EventGridPublisherClient eventGridPublisherClient)
    {
        _eventGridPublisherClient = eventGridPublisherClient;
    }

    public async Task SendEventAsync(EventGridEvent @event)
    {
        await _eventGridPublisherClient.SendEventAsync(@event);
    }
}

public class EventGridPublisherClientEpisode : EventGridPublisherClient<Episode>
{
    public EventGridPublisherClientEpisode(EventGridPublisherClient eventGridPublisherClient)
        : base(eventGridPublisherClient)
    {
    }
}

public class EventGridPublisherClientParticipant : EventGridPublisherClient<InitialParticipantDto>
{
    public EventGridPublisherClientParticipant(EventGridPublisherClient eventGridPublisherClient)
        : base(eventGridPublisherClient)
    {
    }
}
