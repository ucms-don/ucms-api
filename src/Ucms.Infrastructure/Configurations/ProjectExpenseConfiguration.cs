namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class ProjectExpenseConfiguration : IEntityTypeConfiguration<ProjectExpense>
{
    public void Configure(EntityTypeBuilder<ProjectExpense> builder)
    {
        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.OrganizationId);
        builder.HasIndex(e => e.Date);

        builder.Property(e => e.Category).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1024);
        builder.Property(e => e.PaymentMethod).HasMaxLength(64);
        builder.Property(e => e.Note).HasMaxLength(1024);
        builder.Property(e => e.Amount).HasPrecision(18, 2);

        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
