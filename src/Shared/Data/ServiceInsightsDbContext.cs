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

    public virtual DbSet<Episode> Episodes { get; set; }

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

        modelBuilder.Entity<Episode>(entity =>
        {
            entity.ToTable("EPISODE");

            entity.Property(e => e.EpisodeId)
                .HasMaxLength(50)
                .HasColumnName("EPISODE_ID");
            entity.Property(e => e.ActualScreeningDate)
                .HasMaxLength(50)
                .HasColumnName("ACTUAL_SCREENING_DATE");
            entity.Property(e => e.AppointmentMadeFlag)
                .HasMaxLength(50)
                .HasColumnName("APPOINTMENT_MADE_FLAG");
            entity.Property(e => e.BatchId)
                .HasMaxLength(50)
                .HasColumnName("BATCH_ID");
            entity.Property(e => e.CallRecallStatusAuthorisedBy)
                .HasMaxLength(50)
                .HasColumnName("CALL_RECALL_STATUS_AUTHORISED_BY");
            entity.Property(e => e.EarlyRecallDate)
                .HasMaxLength(50)
                .HasColumnName("EARLY_RECALL_DATE");
            entity.Property(e => e.EndCodeId)
                .HasMaxLength(50)
                .HasColumnName("END_CODE_ID");
            entity.Property(e => e.EndCodeLastUpdated)
                .HasMaxLength(50)
                .HasColumnName("END_CODE_LAST_UPDATED");
            entity.Property(e => e.EpisodeOpenDate)
                .HasMaxLength(50)
                .HasColumnName("EPISODE_OPEN_DATE");
            entity.Property(e => e.EpisodeTypeId)
                .HasMaxLength(50)
                .HasColumnName("EPISODE_TYPE_ID");
            entity.Property(e => e.FirstOfferedAppointmentDate)
                .HasMaxLength(50)
                .HasColumnName("FIRST_OFFERED_APPOINTMENT_DATE");
            entity.Property(e => e.OrganisationId)
                .HasMaxLength(50)
                .HasColumnName("ORGANISATION_ID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
