namespace NHS.ServiceInsights.Data;

public interface IEpisodeLkpRepository
{
    public IEndCodeLkpRepository EndCodeLkpRepository { get; }
    public IEpisodeTypeLkpRepository EpisodeTypeLkpRepository { get; }
    public IFinalActionCodeLkpRepository FinalActionCodeLkpRepository { get; }
    public IReasonClosedCodeLkpRepository ReasonClosedCodeLkpRepository { get; }
}
