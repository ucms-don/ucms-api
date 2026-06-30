namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class SalaryConfiguration : IEntityTypeConfiguration<Salary>
{
    public void Configure(EntityTypeBuilder<Salary> builder)
    {
        builder.HasIndex(e => e.OrganizationId);
        builder.HasIndex(e => e.EmployeeId);
        builder.HasIndex(e => e.Month);

        builder.Property(e => e.Month).HasMaxLength(7).IsRequired(); // "2026-06"
        builder.Property(e => e.Notes).HasMaxLength(1024);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
    }
}
