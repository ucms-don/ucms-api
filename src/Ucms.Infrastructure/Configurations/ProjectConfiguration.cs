namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasIndex(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(512).IsRequired();
        builder.Property(e => e.Address).HasMaxLength(1024);
        builder.Property(e => e.Description).HasMaxLength(2048);
        builder.Property(e => e.ContractNumber).HasMaxLength(256);
        builder.Property(e => e.OrganizationId).IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(e => e.Estimates)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId);

        builder.HasMany(e => e.WorkLogs)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId);

        builder.HasMany(e => e.ClientActs)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId);

        builder.HasMany(e => e.ClientPayments)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId);

        builder.HasMany(e => e.BrigadePayments)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId);

        builder.HasOne(e => e.Customer)
            .WithMany(e => e.Projects)
            .HasForeignKey(e => e.CustomerId)
            .IsRequired(false);
    }
}
