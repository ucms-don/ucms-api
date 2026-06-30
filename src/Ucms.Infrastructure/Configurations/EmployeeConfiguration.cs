namespace Ucms.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ucms.Domain.Entities;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasIndex(e => e.OrganizationId);
        builder.HasIndex(e => e.BrigadeId);
        builder.HasIndex(e => e.UserId).IsUnique(); // bitta user — bitta employee

        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Position).HasMaxLength(128);
        builder.Property(e => e.Phone).HasMaxLength(64);
        builder.Property(e => e.Notes).HasMaxLength(1024);

        builder.HasOne(e => e.Brigade)
            .WithMany(b => b.Employees)
            .HasForeignKey(e => e.BrigadeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Salaries)
            .WithOne(s => s.Employee)
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
