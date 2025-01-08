using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IEpisodeTypeLkpRepository
{
    Task<long?> GetEpisodeTypeIdAsync(string episodeType);

    Task<EpisodeTypeLkp?> GetEpisodeTypeLkp(string episodeType);

    Task<IEnumerable<EpisodeTypeLkp>> GetAllEpisodeTypesAsync();
}
