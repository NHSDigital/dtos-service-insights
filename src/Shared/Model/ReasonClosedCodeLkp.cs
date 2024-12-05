using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class ReasonClosedCodeLkp
{
    public long ReasonClosedCodeId { get; set; }

    public string ReasonClosedCode { get; set; } = null!;

    public string? ReasonClosedCodeDescription { get; set; }

    public virtual ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
