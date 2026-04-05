using EMS.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS.Infrastructure.Persistence.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("Goals");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.ReviewCycle)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(g => g.Status)
            .HasConversion<string>();

        // Goal → Employee
        builder.HasOne(g => g.Employee)
            .WithMany()
            .HasForeignKey(g => g.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Goal → Manager (who set it)
        builder.HasOne(g => g.SetByManager)
            .WithMany()
            .HasForeignKey(g => g.SetByManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(g => !g.IsDeleted);
    }
}