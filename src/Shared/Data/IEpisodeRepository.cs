using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IEpisodeRepository
{
    void CreateEpisode(Episode episode);
    Task<Episode?> GetEpisodeAsync(string episodeId);
    Task UpdateEpisode(Episode episode);
}
