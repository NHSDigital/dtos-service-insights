using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class Episode
{
    public long EpisodeId { get; set; }

    public string? EpisodeType { get; set; }

    public string? BsoOrganisationCode { get; set; }

    public string? BsoBatchId { get; set; }

    public string? EpisodeDate { get; set; }

    public string? EndCode { get; set; }

    public string? DateOfFoa { get; set; }

    public string? DateOfAs { get; set; }

    public string? AppointmentMade { get; set; }

    public string? CallRecallStatusAuthorisedBy { get; set; }

    public string? EarlyRecallDate { get; set; }

    public string? EndCodeLastUpdated { get; set; }
}
