using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class Episode
{
    public long EpisodeId { get; set; }
    public string episode_type { get; set; } = null!;
}
