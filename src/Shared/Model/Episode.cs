﻿using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class Episode
{
    public string EpisodeId { get; set; } = null!;

    public string? ParticipantId { get; set; }

    public string? ScreeningId { get; set; }

    public string? NhsNumber { get; set; }

    public string? EpisodeTypeId { get; set; }

    public string? EpisodeOpenDate { get; set; }

    public string? AppointmentMadeFlag { get; set; }

    public string? FirstOfferedAppointmentDate { get; set; }

    public string? ActualScreeningDate { get; set; }

    public string? EarlyRecallDate { get; set; }

    public string? CallRecallStatusAuthorisedBy { get; set; }

    public string? EndCodeId { get; set; }

    public string? EndCodeLastUpdated { get; set; }

    public string? OrganisationId { get; set; }

    public string? BatchId { get; set; }

    public string? RecordInsertDatetime { get; set; }

    public string? RecordUpdateDatetime { get; set; }
}
