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

    public virtual DbSet<Episode> Episodes { get; set; }

    public virtual DbSet<ParticipantScreeningEpisode> ParticipantScreeningEpisodes { get; set; }

    public virtual DbSet<ParticipantScreeningProfile> ParticipantScreeningProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            entity.Property(e => e.NhsNumber)
                .HasMaxLength(50)
                .HasColumnName("NHS_NUMBER");
            entity.Property(e => e.OrganisationId)
                .HasMaxLength(50)
                .HasColumnName("ORGANISATION_ID");
            entity.Property(e => e.ParticipantId)
                .HasMaxLength(50)
                .HasColumnName("PARTICIPANT_ID");
            entity.Property(e => e.RecordInsertDatetime)
                .HasMaxLength(50)
                .HasColumnName("RECORD_INSERT_DATETIME");
            entity.Property(e => e.RecordUpdateDatetime)
                .HasMaxLength(50)
                .HasColumnName("RECORD_UPDATE_DATETIME");
            entity.Property(e => e.ScreeningId)
                .HasMaxLength(50)
                .HasColumnName("SCREENING_ID");
        });

        modelBuilder.Entity<ParticipantScreeningEpisode>(entity =>
        {
            entity.HasKey(e => e.EpisodeId).HasName("PK_EPISODE_ID");

            entity.ToTable("PARTICIPANT_SCREENING_EPISODE");

            entity.Property(e => e.EpisodeId)
                .HasMaxLength(50)
                .HasColumnName("EPISODE_ID");
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
            entity.HasKey(e => e.NhsNumber).HasName("PK_NHS_NUMBER");

            entity.ToTable("PARTICIPANT_SCREENING_PROFILE");

            entity.Property(e => e.NhsNumber)
                .HasMaxLength(50)
                .HasColumnName("NHS_NUMBER");
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
