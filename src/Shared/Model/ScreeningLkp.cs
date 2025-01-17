using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class ScreeningLkp
{
    public long ScreeningId { get; set; }

    public string ScreeningName { get; set; } = null!;

    public string? ScreeningType { get; set; }

    public string? ScreeningAcronym { get; set; }

    public string? ScreeningWorkflowId { get; set; }
}
