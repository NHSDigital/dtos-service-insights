using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class ParticipantScreeningEpisode
{
    public string? EpisodeId { get; set; }

    public string? ScreeningName { get; set; }

    public string? NhsNumber { get; set; }

    public string? EpisodeType { get; set; }

    public string? EpisodeTypeDescription { get; set; }

    public string? EpisodeOpenDate { get; set; }

    public string? AppointmentMadeFlag { get; set; }

    public string? FirstOfferedAppointmentDate { get; set; }

    public string? ActualScreeningDate { get; set; }

    public string? EarlyRecallDate { get; set; }

    public string? CallRecallStatusAuthorisedBy { get; set; }

    public string? EndCode { get; set; }

    public string? EndCodeDescription { get; set; }

    public string? EndCodeLastUpdated { get; set; }

    public string? OrganisationCode { get; set; }

    public string? OrganisationName { get; set; }

    public string? BatchId { get; set; }

    public string? RecordInsertDatetime { get; set; }
}
