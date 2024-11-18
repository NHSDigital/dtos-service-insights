using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class EpisodeTypeLkp
{
    public long EpisodeTypeId { get; set; }

    public string? EpisodeType { get; set; }

    public string? EpisodeDescription { get; set; }

    public virtual ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
