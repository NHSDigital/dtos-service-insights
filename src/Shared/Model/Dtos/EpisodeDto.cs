namespace NHS.ServiceInsights.Model;

public class EpisodeDto
{
    public long EpisodeId { get; set; }

    public long? EpisodeIdSystem { get; set; }

    public string ScreeningName { get; set; }

    public long NhsNumber { get; set; }

    public string? EpisodeType { get; set; }

    public DateTime SrcSysProcessedDateTime { get; set; }

    public DateOnly? EpisodeOpenDate { get; set; }

    public short? AppointmentMadeFlag { get; set; }

    public DateOnly? FirstOfferedAppointmentDate { get; set; }

    public DateOnly? ActualScreeningDate { get; set; }

    public DateOnly? EarlyRecallDate { get; set; }

    public string? CallRecallStatusAuthorisedBy { get; set; }

    public string? EndCode { get; set; }

    public DateTime? EndCodeLastUpdated { get; set; }

    public string? OrganisationCode { get; set; }

    public string? BatchId { get; set; }

    public string? EndPoint { get; set; }

    public string? ReasonClosedCode { get; set; }

    public string? FinalActionCode { get; set; }
}
