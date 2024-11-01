using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public class EpisodeDto
{
    public long EpisodeId { get; set; }

    public long? EpisodeIdSystem { get; set; }

    public long ScreeningId { get; set; }

    public long NhsNumber { get; set; }

    public string? EpisodeType { get; set; }

    public DateOnly? EpisodeOpenDate { get; set; }

    public string? AppointmentMadeFlag { get; set; }

    public DateOnly? FirstOfferedAppointmentDate { get; set; }

    public DateOnly? ActualScreeningDate { get; set; }

    public DateOnly? EarlyRecallDate { get; set; }

    public string? CallRecallStatusAuthorisedBy { get; set; }

    public string? EndCode { get; set; }

    public DateTime? EndCodeLastUpdated { get; set; }

    public string? OrganisationCode { get; set; }

    public string? BatchId { get; set; }




}
