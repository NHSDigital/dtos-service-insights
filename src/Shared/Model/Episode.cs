using System;
using System.Collections.Generic;

namespace NHS.ServiceInsights.Model;

public partial class Episode
{
    public long EpisodeId { get; set; }

    public long? EpisodeIdSystem { get; set; }

    public long ScreeningId { get; set; }

    public long NhsNumber { get; set; }

    public long? EpisodeTypeId { get; set; }

    public DateOnly? EpisodeOpenDate { get; set; }

    public short? AppointmentMadeFlag { get; set; }

    public DateOnly? FirstOfferedAppointmentDate { get; set; }

    public DateOnly? ActualScreeningDate { get; set; }

    public DateOnly? EarlyRecallDate { get; set; }

    public string? CallRecallStatusAuthorisedBy { get; set; }

    public long? EndCodeId { get; set; }

    public DateTime? EndCodeLastUpdated { get; set; }

    public long? FinalActionCodeId { get; set; }

    public long? ReasonClosedCodeId { get; set; }

    public string? EndPoint { get; set; }

    public long? OrganisationId { get; set; }

    public string? BatchId { get; set; }

    public DateTime? RecordInsertDatetime { get; set; }

    public DateTime? RecordUpdateDatetime { get; set; }

    public virtual EndCodeLkp? EndCode { get; set; }

    public virtual EpisodeTypeLkp? EpisodeType { get; set; }

    public virtual FinalActionCodeLkp? FinalActionCode { get; set; }

    public virtual ReasonClosedCodeLkp? ReasonClosedCode { get; set; }
}
