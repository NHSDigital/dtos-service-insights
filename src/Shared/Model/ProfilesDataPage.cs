using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class ProfilesDataPage
{
    public List<ParticipantScreeningProfile> profiles { get; set; } = null;

    public int page { get; set; }

    public int pageSize { get; set; }

    public bool hasMoreData { get; set; }
}

public partial class EpisodesDataPage
{
    public List<ParticipantScreeningEpisode> episodes { get; set; } = null;

    public int TotalResults {get; set; }

    public int TotalPages { get; set; }

    public int TotalRemainingPages { get; set; }
}
