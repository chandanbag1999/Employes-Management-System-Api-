using EMS.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS.Infrastructure.Persistence.Configurations;

public class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
{
    public void Configure(EntityTypeBuilder<PayrollRecord> builder)
    {
        builder.ToTable("PayrollRecords");
        builder.HasKey(p => p.Id);

        // Ek employee ka ek mahine mein sirf ek record
        builder.HasIndex(p => new { p.EmployeeId, p.Month, p.Year })
            .IsUnique();

        builder.Property(p => p.BasicSalary).HasColumnType("decimal(18,2)");
        builder.Property(p => p.HRA).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TransportAllowance).HasColumnType("decimal(18,2)");
        builder.Property(p => p.MedicalAllowance).HasColumnType("decimal(18,2)");
        builder.Property(p => p.OtherAllowances).HasColumnType("decimal(18,2)");
        builder.Property(p => p.GrossEarnings).HasColumnType("decimal(18,2)");
        builder.Property(p => p.PfDeduction).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TaxDeduction).HasColumnType("decimal(18,2)");
        builder.Property(p => p.OtherDeductions).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TotalDeductions).HasColumnType("decimal(18,2)");
        builder.Property(p => p.NetSalary).HasColumnType("decimal(18,2)");

        builder.Property(p => p.Status)
            .HasConversion<string>();

        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}