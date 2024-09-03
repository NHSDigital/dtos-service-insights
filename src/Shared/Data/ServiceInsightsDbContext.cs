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

    public virtual DbSet<Episode> Episodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Episode>(entity =>
        {
            entity.ToTable("EPISODE");

            entity.Property(e => e.EpisodeId)
                .ValueGeneratedNever()
                .HasColumnName("EPISODE_ID");
            entity.Property(e => e.AppointmentMade)
                .HasMaxLength(50)
                .HasColumnName("APPOINTMENT_MADE");
            entity.Property(e => e.BsoBatchId)
                .HasMaxLength(50)
                .HasColumnName("BSO_BATCH_ID");
            entity.Property(e => e.BsoOrganisationCode)
                .HasMaxLength(50)
                .HasColumnName("BSO_ORGANISATION_CODE");
            entity.Property(e => e.CallRecallStatusAuthorisedBy)
                .HasMaxLength(50)
                .HasColumnName("CALL_RECALL_STATUS_AUTHORISED_BY");
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
            entity.Property(e => e.EpisodeType)
                .HasMaxLength(50)
                .HasColumnName("EPISODE_TYPE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
