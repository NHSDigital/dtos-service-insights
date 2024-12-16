namespace NHS.ServiceInsights.Model;

public class FinalizedEpisodeDto
{
    public long EpisodeId { get; set; }

    public long NhsNumber { get; set; }

    public long ScreeningId { get; set; }

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

    public DateTime? EndCodeLastUpdated { get; set; }

    public string? FinalActionCode { get; set; }

    public string? FinalActionCodeDescription { get; set; }

    public string? ReasonClosedCode { get; set; }

    public string? ReasonClosedCodeDescription { get; set; }

    public string? EndPoint { get; set; }

    public long? OrganisationId { get; set; }

    public string? BatchId { get; set; }

    public DateTime SrcSysProcessedDatetime { get; set; }

    public DateTime? RecordInsertDatetime { get; set; }

    public DateTime? RecordUpdateDatetime { get; set; }

    public static explicit operator FinalizedEpisodeDto(Episode episode)
    {
        return new FinalizedEpisodeDto
        {
            EpisodeId = episode.EpisodeId,
            NhsNumber = episode.NhsNumber,
            ScreeningId = episode.ScreeningId,
            EpisodeOpenDate = episode.EpisodeOpenDate,
            AppointmentMadeFlag = episode.AppointmentMadeFlag,
            FirstOfferedAppointmentDate = episode.FirstOfferedAppointmentDate,
            ActualScreeningDate = episode.ActualScreeningDate,
            EarlyRecallDate = episode.EarlyRecallDate,
            CallRecallStatusAuthorisedBy = episode.CallRecallStatusAuthorisedBy,
            EndCodeLastUpdated = episode.EndCodeLastUpdated,
            EndPoint = episode.EndPoint,
            OrganisationId = episode.OrganisationId,
            BatchId = episode.BatchId,
            RecordInsertDatetime = episode.RecordInsertDatetime,
            RecordUpdateDatetime = episode.RecordUpdateDatetime
        };
    }
}
