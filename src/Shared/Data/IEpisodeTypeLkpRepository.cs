namespace NHS.ServiceInsights.Data;

public interface IEpisodeTypeLkpRepository
{
    long GetEpisodeTypeId(string episodeType);
}
