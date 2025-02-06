namespace NHS.ServiceInsights.Data;

public class EpisodeLkpRepository : IEpisodeLkpRepository
{
    public IEndCodeLkpRepository EndCodeLkpRepository { get; }
    public IEpisodeTypeLkpRepository EpisodeTypeLkpRepository { get; }
    public IFinalActionCodeLkpRepository FinalActionCodeLkpRepository { get; }
    public IReasonClosedCodeLkpRepository ReasonClosedCodeLkpRepository { get; }

    public EpisodeLkpRepository(IEndCodeLkpRepository endCodeLkpRepository, IEpisodeTypeLkpRepository episodeTypeLkpRepository, IFinalActionCodeLkpRepository finalActionCodeLkpRepository, IReasonClosedCodeLkpRepository reasonClosedCodeLkpRepository)
    {
        EndCodeLkpRepository = endCodeLkpRepository;
        EpisodeTypeLkpRepository = episodeTypeLkpRepository;
        FinalActionCodeLkpRepository = finalActionCodeLkpRepository;
        ReasonClosedCodeLkpRepository = reasonClosedCodeLkpRepository;
    }
}
