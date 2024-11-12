using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class ParticipantScreeningEpisode
{
    public long Id { get; set; }

    public long EpisodeId { get; set; }

    public long NhsNumber { get; set; }

    public string? ScreeningName { get; set; }

    public string? EpisodeType { get; set; }

    public string? EpisodeTypeDescription { get; set; }

    public DateOnly? EpisodeOpenDate { get; set; }

    public short? AppointmentMadeFlag { get; set; }

    public DateOnly? FirstOfferedAppointmentDate { get; set; }

    public DateOnly? ActualScreeningDate { get; set; }

    public DateOnly? EarlyRecallDate { get; set; }

    public string? CallRecallStatusAuthorisedBy { get; set; }

    public string? EndCode { get; set; }

    public string? EndCodeDescription { get; set; }

    public DateOnly? EndCodeLastUpdated { get; set; }

    public string? ReasonClosedCode { get; set; }

    public string? ReasonClosedCodeDescription { get; set; }

    public string? FinalActionCode { get; set; }

    public string? FinalActionCodeDescription { get; set; }

    public string? EndPoint { get; set; }

    public string? OrganisationCode { get; set; }

    public string? OrganisationName { get; set; }

    public string? BatchId { get; set; }

    public DateTime? RecordInsertDatetime { get; set; }
}
