
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public interface IEpisodeRepository
{
    void CreateEpisode(Episode episode);
    Episode GetEpisode(string episodeId);
    void UpdateEpisode(Episode episode);

}
