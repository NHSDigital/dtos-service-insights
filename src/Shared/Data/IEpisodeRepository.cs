using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IEpisodeRepository
{
    void CreateEpisode(Episode episode);
    Task<Episode?> GetEpisodeAsync(long episodeId);
    Task UpdateEpisode(Episode episode);
}
