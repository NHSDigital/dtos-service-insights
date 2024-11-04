namespace NHS.ServiceInsights.Data;

public interface IEpisodeTypeLkpRepository
{
    Task<long> GetEpisodeTypeIdAsync(string episodeType);
}
