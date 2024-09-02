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

    public virtual DbSet<AnalyticsDatum> Analytics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Episode>(entity =>
        {
            entity.ToTable("EPISODE");

            entity.Property(e => e.EpisodeId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("EPISODE_ID");
        });

        modelBuilder.Entity<AnalyticsDatum>(entity =>
        {
            entity.ToTable("ANALYTICS");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
