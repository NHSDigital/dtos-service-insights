using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class EndCodeLkp
{
    public long EndCodeId { get; set; }

    public string? LegacyEndCode { get; set; }

    public string? EndCode { get; set; }

    public string? EndCodeDescription { get; set; }

    public virtual ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
