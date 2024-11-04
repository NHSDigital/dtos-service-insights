using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NHS.ServiceInsights.Model;

namespace NHS.ServiceInsights.Data;

public partial class ServiceInsightsDbContext : DbContext
{
    public ServiceInsightsDbContext(DbContextOptions<ServiceInsightsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Analytic> Analytics { get; set; }

    public virtual DbSet<EndCodeLkp> EndCodeLkps { get; set; }

    public virtual DbSet<Episode> Episodes { get; set; }

    public virtual DbSet<EpisodeTypeLkp> EpisodeTypeLkps { get; set; }

    public virtual DbSet<ParticipantScreeningEpisode> ParticipantScreeningEpisodes { get; set; }

    public virtual DbSet<ParticipantScreeningProfile> ParticipantScreeningProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Analytic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ID");

            entity.ToTable("ANALYTICS");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AppointmentMade)
                .HasMaxLength(50)
                .HasColumnName("APPOINTMENT_MADE");
            entity.Property(e => e.BsoBatchId)
                .HasMaxLength(50)
                .HasColumnName("BSO_BATCH_ID");
            entity.Property(e => e.BsoOrganisationCode)
                .HasMaxLength(50)
                .HasColumnName("BSO_ORGANISATION_CODE");
            entity.Property(e => e.BsoOrganisationId)
                .HasMaxLength(50)
                .HasColumnName("BSO_ORGANISATION_ID");
            entity.Property(e => e.CallRecallStatusAuthorisedBy)
                .HasMaxLength(50)
                .HasColumnName("CALL_RECALL_STATUS_AUTHORISED_BY");
            entity.Property(e => e.CeasedReason)
                .HasMaxLength(50)
                .HasColumnName("CEASED_REASON");
            entity.Property(e => e.DateIrradiated)
                .HasMaxLength(50)
                .HasColumnName("DATE_IRRADIATED");
            entity.Property(e => e.DateOfAs)
                .HasMaxLength(50)
                .HasColumnName("DATE_OF_AS");
            entity.Property(e => e.DateOfFoa)
                .HasMaxLength(50)
                .HasColumnName("DATE_OF_FOA");
            entity.Property(e => e.EarlyRecallDate)
                .HasMaxLength(50)
                .HasColumnName("EARLY_RECALL_DATE");
            entity.Property(e => e.EndCode)
                .HasMaxLength(50)
                .HasColumnName("END_CODE");
            entity.Property(e => e.EndCodeLastUpdated)
                .HasMaxLength(50)
                .HasColumnName("END_CODE_LAST_UPDATED");
            entity.Property(e => e.EpisodeDate)
                .HasMaxLength(50)
                .HasColumnName("EPISODE_DATE");
            entity.Property(e => e.EpisodeId)
                .HasMaxLength(50)
                .HasColumnName("EPISODE_ID");
            entity.Property(e => e.EpisodeType)
                .HasMaxLength(50)
                .HasColumnName("EPISODE_TYPE");
            entity.Property(e => e.GeneCode)
                .HasMaxLength(50)
                .HasColumnName("GENE_CODE");
            entity.Property(e => e.GpPracticeId)
                .HasMaxLength(50)
                .HasColumnName("GP_PRACTICE_ID");
            entity.Property(e => e.HigherRiskNextTestDueDate)
                .HasMaxLength(50)
                .HasColumnName("HIGHER_RISK_NEXT_TEST_DUE_DATE");
            entity.Property(e => e.HigherRiskReferralReasonCode)
                .HasMaxLength(50)
                .HasColumnName("HIGHER_RISK_REFERRAL_REASON_CODE");
            entity.Property(e => e.IsHigherRisk)
                .HasMaxLength(50)
                .HasColumnName("IS_HIGHER_RISK");
            entity.Property(e => e.IsHigherRiskActive)
                .HasMaxLength(50)
                .HasColumnName("IS_HIGHER_RISK_ACTIVE");
            entity.Property(e => e.LatestInvitationDate)
                .HasMaxLength(50)
                .HasColumnName("LATEST_INVITATION_DATE");
            entity.Property(e => e.NextTestDueDate)
                .HasMaxLength(50)
                .HasColumnName("NEXT_TEST_DUE_DATE");
            entity.Property(e => e.NhsNumber)
                .HasMaxLength(50)
                .HasColumnName("NHS_NUMBER");
            entity.Property(e => e.NtddCalculationMethod)
                .HasMaxLength(50)
                .HasColumnName("NTDD_CALCULATION_METHOD");
            entity.Property(e => e.PreferredLanguage)
                .HasMaxLength(50)
                .HasColumnName("PREFERRED_LANGUAGE");
            entity.Property(e => e.ReasonDeducted)
                .HasMaxLength(50)
                .HasColumnName("REASON_DEDUCTED");
            entity.Property(e => e.ReasonForCeasedCode)
                .HasMaxLength(50)
                .HasColumnName("REASON_FOR_CEASED_CODE");
            entity.Property(e => e.RemovalDate)
                .HasMaxLength(50)
                .HasColumnName("REMOVAL_DATE");
            entity.Property(e => e.RemovalReason)
                .HasMaxLength(50)
                .HasColumnName("REMOVAL_REASON");
            entity.Property(e => e.SubjectStatusCode)
                .HasMaxLength(50)
                .HasColumnName("SUBJECT_STATUS_CODE");
        });

        modelBuilder.Entity<EndCodeLkp>(entity =>
        {
            entity.HasKey(e => e.EndCodeId);

            entity.ToTable("END_CODE_LKP");

            entity.Property(e => e.EndCodeId)
                .ValueGeneratedNever()
                .HasColumnName("END_CODE_ID");
            entity.Property(e => e.EndCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("END_CODE");
            entity.Property(e => e.EndCodeDescription)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("END_CODE_DESCRIPTION");
            entity.Property(e => e.LegacyEndCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("LEGACY_END_CODE");
        });

        modelBuilder.Entity<Episode>(entity =>
        {
            entity.ToTable("EPISODE");

            entity.Property(e => e.EpisodeId)
                .ValueGeneratedNever()
                .HasColumnName("EPISODE_ID");
            entity.Property(e => e.ActualScreeningDate).HasColumnName("ACTUAL_SCREENING_DATE");
            entity.Property(e => e.AppointmentMadeFlag)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("APPOINTMENT_MADE_FLAG");
            entity.Property(e => e.BatchId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BATCH_ID");
            entity.Property(e => e.CallRecallStatusAuthorisedBy)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("CALL_RECALL_STATUS_AUTHORISED_BY");
            entity.Property(e => e.EarlyRecallDate).HasColumnName("EARLY_RECALL_DATE");
            entity.Property(e => e.EndCodeId).HasColumnName("END_CODE_ID");
            entity.Property(e => e.EndCodeLastUpdated)
                .HasColumnType("datetime")
                .HasColumnName("END_CODE_LAST_UPDATED");
            entity.Property(e => e.EpisodeIdSystem).HasColumnName("EPISODE_ID_SYSTEM");
            entity.Property(e => e.EpisodeOpenDate).HasColumnName("EPISODE_OPEN_DATE");
            entity.Property(e => e.EpisodeTypeId).HasColumnName("EPISODE_TYPE_ID");
            entity.Property(e => e.FirstOfferedAppointmentDate).HasColumnName("FIRST_OFFERED_APPOINTMENT_DATE");
            entity.Property(e => e.NhsNumber).HasColumnName("NHS_NUMBER");
            entity.Property(e => e.OrganisationId).HasColumnName("ORGANISATION_ID");
            entity.Property(e => e.RecordInsertDatetime)
                .HasColumnType("datetime")
                .HasColumnName("RECORD_INSERT_DATETIME");
            entity.Property(e => e.RecordUpdateDatetime)
                .HasColumnType("datetime")
                .HasColumnName("RECORD_UPDATE_DATETIME");
            entity.Property(e => e.ScreeningId).HasColumnName("SCREENING_ID");

            entity.HasOne(d => d.EndCode).WithMany(p => p.Episodes)
                .HasForeignKey(d => d.EndCodeId)
                .HasConstraintName("FK_EPISODE_STATUS_OF_END_CODE");

            entity.HasOne(d => d.EpisodeType).WithMany(p => p.Episodes)
                .HasForeignKey(d => d.EpisodeTypeId)
                .HasConstraintName("FK_EPISODE_TYPE_OF_E_EPISODE_");
        });

        modelBuilder.Entity<EpisodeTypeLkp>(entity =>
        {
            entity.HasKey(e => e.EpisodeTypeId);

            entity.ToTable("EPISODE_TYPE_LKP");

            entity.Property(e => e.EpisodeTypeId)
                .ValueGeneratedNever()
                .HasColumnName("EPISODE_TYPE_ID");
            entity.Property(e => e.EpisodeDescription)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("EPISODE_DESCRIPTION");
            entity.Property(e => e.EpisodeType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("EPISODE_TYPE");
        });

        modelBuilder.Entity<ParticipantScreeningEpisode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PARTICIP__3214EC279E6BE266");

            entity.ToTable("PARTICIPANT_SCREENING_EPISODE");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ActualScreeningDate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ACTUAL_SCREENING_DATE");
            entity.Property(e => e.AppointmentMadeFlag)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("APPOINTMENT_MADE_FLAG");
            entity.Property(e => e.BatchId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BATCH_ID");
            entity.Property(e => e.CallRecallStatusAuthorisedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CALL_RECALL_STATUS_AUTHORISED_BY");
            entity.Property(e => e.EarlyRecallDate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EARLY_RECALL_DATE");
            entity.Property(e => e.EndCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("END_CODE");
            entity.Property(e => e.EndCodeDescription)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("END_CODE_DESCRIPTION");
            entity.Property(e => e.EndCodeLastUpdated)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("END_CODE_LAST_UPDATED");
            entity.Property(e => e.EpisodeId).HasColumnName("EPISODE_ID");
            entity.Property(e => e.EpisodeOpenDate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EPISODE_OPEN_DATE");
            entity.Property(e => e.EpisodeType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EPISODE_TYPE");
            entity.Property(e => e.EpisodeTypeDescription)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EPISODE_TYPE_DESCRIPTION");
            entity.Property(e => e.FirstOfferedAppointmentDate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("FIRST_OFFERED_APPOINTMENT_DATE");
            entity.Property(e => e.NhsNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NHS_NUMBER");
            entity.Property(e => e.OrganisationCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ORGANISATION_CODE");
            entity.Property(e => e.OrganisationName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ORGANISATION_NAME");
            entity.Property(e => e.RecordInsertDatetime)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RECORD_INSERT_DATETIME");
            entity.Property(e => e.ScreeningName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SCREENING_NAME");
        });

        modelBuilder.Entity<ParticipantScreeningProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PARTICIP__3214EC2701417CA0");

            entity.ToTable("PARTICIPANT_SCREENING_PROFILE");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DateIrradiated)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DATE_IRRADIATED");
            entity.Property(e => e.GeneCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GENE_CODE");
            entity.Property(e => e.GeneCodeDescription)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GENE_CODE_DESCRIPTION");
            entity.Property(e => e.HigherRiskNextTestDueDate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HIGHER_RISK_NEXT_TEST_DUE_DATE");
            entity.Property(e => e.HigherRiskReferralReasonCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HIGHER_RISK_REFERRAL_REASON_CODE");
            entity.Property(e => e.HrReasonCodeDescription)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HR_REASON_CODE_DESCRIPTION");
            entity.Property(e => e.IsHigherRisk)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("IS_HIGHER_RISK");
            entity.Property(e => e.IsHigherRiskActive)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("IS_HIGHER_RISK_ACTIVE");
            entity.Property(e => e.NextTestDueDate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NEXT_TEST_DUE_DATE");
            entity.Property(e => e.NextTestDueDateCalculationMethod)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NEXT_TEST_DUE_DATE_CALCULATION_METHOD");
            entity.Property(e => e.NhsNumber)
                .HasMaxLength(50)
                .HasColumnName("NHS_NUMBER");
            entity.Property(e => e.ParticipantScreeningStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PARTICIPANT_SCREENING_STATUS");
            entity.Property(e => e.PreferredLanguage)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PREFERRED_LANGUAGE");
            entity.Property(e => e.PrimaryCareProvider)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PRIMARY_CARE_PROVIDER");
            entity.Property(e => e.ReasonForRemoval)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REASON_FOR_REMOVAL");
            entity.Property(e => e.ReasonForRemovalDt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REASON_FOR_REMOVAL_DT");
            entity.Property(e => e.RecordInsertDatetime)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RECORD_INSERT_DATETIME");
            entity.Property(e => e.ScreeningCeasedReason)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SCREENING_CEASED_REASON");
            entity.Property(e => e.ScreeningName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SCREENING_NAME");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
