using EMS.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS.Infrastructure.Persistence.Configurations;

public class SalaryStructureConfiguration : IEntityTypeConfiguration<SalaryStructure>
{
    public void Configure(EntityTypeBuilder<SalaryStructure> builder)
    {
        builder.ToTable("SalaryStructures");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.BasicSalary).HasColumnType("decimal(18,2)");
        builder.Property(s => s.HRA).HasColumnType("decimal(18,2)");
        builder.Property(s => s.TransportAllowance).HasColumnType("decimal(18,2)");
        builder.Property(s => s.MedicalAllowance).HasColumnType("decimal(18,2)");
        builder.Property(s => s.OtherAllowances).HasColumnType("decimal(18,2)");
        builder.Property(s => s.GrossSalary).HasColumnType("decimal(18,2)");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}