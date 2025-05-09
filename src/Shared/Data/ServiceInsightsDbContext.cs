﻿using System;
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

    public virtual DbSet<EndCodeLkp> EndCodeLkps { get; set; }

    public virtual DbSet<Episode> Episodes { get; set; }

    public virtual DbSet<EpisodeTypeLkp> EpisodeTypeLkps { get; set; }

    public virtual DbSet<FinalActionCodeLkp> FinalActionCodeLkps { get; set; }

    public virtual DbSet<OrganisationLkp> OrganisationLkps { get; set; }

    public virtual DbSet<ParticipantScreeningEpisode> ParticipantScreeningEpisodes { get; set; }

    public virtual DbSet<ParticipantScreeningProfile> ParticipantScreeningProfiles { get; set; }

    public virtual DbSet<ReasonClosedCodeLkp> ReasonClosedCodeLkps { get; set; }

    public virtual DbSet<ScreeningLkp> ScreeningLkps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            entity.Property(e => e.AppointmentMadeFlag).HasColumnName("APPOINTMENT_MADE_FLAG");
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
            entity.Property(e => e.EndPoint)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("END_POINT");
            entity.Property(e => e.EpisodeOpenDate).HasColumnName("EPISODE_OPEN_DATE");
            entity.Property(e => e.EpisodeTypeId).HasColumnName("EPISODE_TYPE_ID");
            entity.Property(e => e.ExceptionFlag).HasColumnName("EXCEPTION_FLAG");
            entity.Property(e => e.FinalActionCodeId).HasColumnName("FINAL_ACTION_CODE_ID");
            entity.Property(e => e.FirstOfferedAppointmentDate).HasColumnName("FIRST_OFFERED_APPOINTMENT_DATE");
            entity.Property(e => e.NhsNumber).HasColumnName("NHS_NUMBER");
            entity.Property(e => e.OrganisationId).HasColumnName("ORGANISATION_ID");
            entity.Property(e => e.ReasonClosedCodeId).HasColumnName("REASON_CLOSED_CODE_ID");
            entity.Property(e => e.RecordInsertDatetime)
                .HasColumnType("datetime")
                .HasColumnName("RECORD_INSERT_DATETIME");
            entity.Property(e => e.RecordUpdateDatetime)
                .HasColumnType("datetime")
                .HasColumnName("RECORD_UPDATE_DATETIME");
            entity.Property(e => e.ScreeningId).HasColumnName("SCREENING_ID");
            entity.Property(e => e.SrcSysProcessedDatetime)
                .HasColumnType("datetime")
                .HasColumnName("SRC_SYS_PROCESSED_DATETIME");

            entity.HasOne(d => d.EndCode).WithMany(p => p.Episodes)
                .HasForeignKey(d => d.EndCodeId)
                .HasConstraintName("FK_EPISODE_END_CODE_LKP");

            entity.HasOne(d => d.EpisodeType).WithMany(p => p.Episodes)
                .HasForeignKey(d => d.EpisodeTypeId)
                .HasConstraintName("FK_EPISODE_EPISODE_TYPE_LKP");

            entity.HasOne(d => d.FinalActionCode).WithMany(p => p.Episodes)
                .HasForeignKey(d => d.FinalActionCodeId)
                .HasConstraintName("FK_EPISODE_FINAL_ACTION_CODE_LKP");

            entity.HasOne(d => d.ReasonClosedCode).WithMany(p => p.Episodes)
                .HasForeignKey(d => d.ReasonClosedCodeId)
                .HasConstraintName("FK_EPISODE_REASON_CLOSED_CODE_LKP");
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

        modelBuilder.Entity<FinalActionCodeLkp>(entity =>
        {
            entity.HasKey(e => e.FinalActionCodeId);

            entity.ToTable("FINAL_ACTION_CODE_LKP");

            entity.Property(e => e.FinalActionCodeId)
                .ValueGeneratedNever()
                .HasColumnName("FINAL_ACTION_CODE_ID");
            entity.Property(e => e.FinalActionCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("FINAL_ACTION_CODE");
            entity.Property(e => e.FinalActionCodeDescription)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("FINAL_ACTION_CODE_DESCRIPTION");
        });

        modelBuilder.Entity<OrganisationLkp>(entity =>
        {
            entity.HasKey(e => e.OrganisationId).HasName("PK_ORGANISATION_ID");

            entity.ToTable("ORGANISATION_LKP");

            entity.Property(e => e.OrganisationId)
                .ValueGeneratedNever()
                .HasColumnName("ORGANISATION_ID");
            entity.Property(e => e.IsActive)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("IS_ACTIVE");
            entity.Property(e => e.OrganisationCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ORGANISATION_CODE");
            entity.Property(e => e.OrganisationName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ORGANISATION_NAME");
            entity.Property(e => e.OrganisationType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ORGANISATION_TYPE");
            entity.Property(e => e.ScreeningName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SCREENING_NAME");
        });

        modelBuilder.Entity<ParticipantScreeningEpisode>(entity =>
        {
            entity.HasKey(e => new { e.EpisodeId, e.SrcSysProcessedDatetime }).HasName("PK__PARTICIP__6BB7DFCB4125B665");

            entity.ToTable("PARTICIPANT_SCREENING_EPISODE");

            entity.Property(e => e.EpisodeId).HasColumnName("EPISODE_ID");
            entity.Property(e => e.SrcSysProcessedDatetime)
                .HasColumnType("datetime")
                .HasColumnName("SRC_SYS_PROCESSED_DATETIME");
            entity.Property(e => e.ActualScreeningDate).HasColumnName("ACTUAL_SCREENING_DATE");
            entity.Property(e => e.AppointmentMadeFlag).HasColumnName("APPOINTMENT_MADE_FLAG");
            entity.Property(e => e.BatchId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BATCH_ID");
            entity.Property(e => e.CallRecallStatusAuthorisedBy)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("CALL_RECALL_STATUS_AUTHORISED_BY");
            entity.Property(e => e.EarlyRecallDate).HasColumnName("EARLY_RECALL_DATE");
            entity.Property(e => e.EndCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("END_CODE");
            entity.Property(e => e.EndCodeDescription)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("END_CODE_DESCRIPTION");
            entity.Property(e => e.EndCodeLastUpdated)
                .HasColumnType("datetime")
                .HasColumnName("END_CODE_LAST_UPDATED");
            entity.Property(e => e.EndPoint)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("END_POINT");
            entity.Property(e => e.EpisodeOpenDate).HasColumnName("EPISODE_OPEN_DATE");
            entity.Property(e => e.EpisodeType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EPISODE_TYPE");
            entity.Property(e => e.EpisodeTypeDescription)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("EPISODE_TYPE_DESCRIPTION");
            entity.Property(e => e.ExceptionFlag).HasColumnName("EXCEPTION_FLAG");
            entity.Property(e => e.FinalActionCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("FINAL_ACTION_CODE");
            entity.Property(e => e.FinalActionCodeDescription)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("FINAL_ACTION_CODE_DESCRIPTION");
            entity.Property(e => e.FirstOfferedAppointmentDate).HasColumnName("FIRST_OFFERED_APPOINTMENT_DATE");
            entity.Property(e => e.NhsNumber).HasColumnName("NHS_NUMBER");
            entity.Property(e => e.OrganisationCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ORGANISATION_CODE");
            entity.Property(e => e.OrganisationName)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("ORGANISATION_NAME");
            entity.Property(e => e.ReasonClosedCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REASON_CLOSED_CODE");
            entity.Property(e => e.ReasonClosedCodeDescription)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("REASON_CLOSED_CODE_DESCRIPTION");
            entity.Property(e => e.RecordInsertDatetime)
                .HasColumnType("datetime")
                .HasColumnName("RECORD_INSERT_DATETIME");
            entity.Property(e => e.RecordUpdateDatetime)
                .HasColumnType("datetime")
                .HasColumnName("RECORD_UPDATE_DATETIME");
            entity.Property(e => e.ScreeningName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("SCREENING_NAME");
        });

        modelBuilder.Entity<ParticipantScreeningProfile>(entity =>
        {
            entity.HasKey(e => new { e.NhsNumber, e.SrcSysProcessedDatetime }).HasName("PK__PARTICIP__D62BD6BC1641FE83");

            entity.ToTable("PARTICIPANT_SCREENING_PROFILE");

            entity.Property(e => e.NhsNumber).HasColumnName("NHS_NUMBER");
            entity.Property(e => e.SrcSysProcessedDatetime)
                .HasColumnType("datetime")
                .HasColumnName("SRC_SYS_PROCESSED_DATETIME");
            entity.Property(e => e.DateIrradiated).HasColumnName("DATE_IRRADIATED");
            entity.Property(e => e.GeneCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("GENE_CODE");
            entity.Property(e => e.GeneCodeDescription)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("GENE_CODE_DESCRIPTION");
            entity.Property(e => e.HigherRiskNextTestDueDate).HasColumnName("HIGHER_RISK_NEXT_TEST_DUE_DATE");
            entity.Property(e => e.HigherRiskReferralReasonCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("HIGHER_RISK_REFERRAL_REASON_CODE");
            entity.Property(e => e.HrReasonCodeDescription)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("HR_REASON_CODE_DESCRIPTION");
            entity.Property(e => e.IsHigherRisk).HasColumnName("IS_HIGHER_RISK");
            entity.Property(e => e.IsHigherRiskActive).HasColumnName("IS_HIGHER_RISK_ACTIVE");
            entity.Property(e => e.NextTestDueDate).HasColumnName("NEXT_TEST_DUE_DATE");
            entity.Property(e => e.NextTestDueDateCalcMethod)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NEXT_TEST_DUE_DATE_CALC_METHOD");
            entity.Property(e => e.ParticipantScreeningStatus)
                .HasMaxLength(100)
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
            entity.Property(e => e.ReasonForRemovalDt).HasColumnName("REASON_FOR_REMOVAL_DT");
            entity.Property(e => e.RecordInsertDatetime)
                .HasColumnType("datetime")
                .HasColumnName("RECORD_INSERT_DATETIME");
            entity.Property(e => e.RecordUpdateDatetime)
                .HasColumnType("datetime")
                .HasColumnName("RECORD_UPDATE_DATETIME");
            entity.Property(e => e.ScreeningCeasedReason)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SCREENING_CEASED_REASON");
            entity.Property(e => e.ScreeningName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("SCREENING_NAME");
        });

        modelBuilder.Entity<ReasonClosedCodeLkp>(entity =>
        {
            entity.HasKey(e => e.ReasonClosedCodeId);

            entity.ToTable("REASON_CLOSED_CODE_LKP");

            entity.Property(e => e.ReasonClosedCodeId)
                .ValueGeneratedNever()
                .HasColumnName("REASON_CLOSED_CODE_ID");
            entity.Property(e => e.ReasonClosedCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REASON_CLOSED_CODE");
            entity.Property(e => e.ReasonClosedCodeDescription)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("REASON_CLOSED_CODE_DESCRIPTION");
        });

        modelBuilder.Entity<ScreeningLkp>(entity =>
        {
            entity.HasKey(e => e.ScreeningId).HasName("PK_SCREENING_ID");

            entity.ToTable("SCREENING_LKP");

            entity.Property(e => e.ScreeningId)
                .ValueGeneratedNever()
                .HasColumnName("SCREENING_ID");
            entity.Property(e => e.ScreeningAcronym)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SCREENING_ACRONYM");
            entity.Property(e => e.ScreeningName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SCREENING_NAME");
            entity.Property(e => e.ScreeningType)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SCREENING_TYPE");
            entity.Property(e => e.ScreeningWorkflowId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SCREENING_WORKFLOW_ID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
