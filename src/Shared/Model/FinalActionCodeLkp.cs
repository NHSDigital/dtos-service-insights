using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class FinalActionCodeLkp
{
    public long FinalActionCodeId { get; set; }

    public string FinalActionCode { get; set; } = null!;

    public string? FinalActionCodeDescription { get; set; }

    public virtual ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
