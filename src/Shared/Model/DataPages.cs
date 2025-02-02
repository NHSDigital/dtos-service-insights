using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class ProfilesDataPage
{
    public List<ParticipantScreeningProfile>? Profiles { get; set; } = null!;

    public int TotalResults { get; set; }

    public int TotalPages { get; set; }

    public int TotalRemainingPages { get; set; }
}

public partial class EpisodesDataPage
{
    public List<ParticipantScreeningEpisode> episodes { get; set; } = null;

    public int TotalResults { get; set; }

    public int TotalPages { get; set; }

    public int TotalRemainingPages { get; set; }
}
