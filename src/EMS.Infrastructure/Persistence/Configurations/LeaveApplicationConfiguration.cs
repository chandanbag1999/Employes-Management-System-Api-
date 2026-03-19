using EMS.Domain.Entities.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS.Infrastructure.Persistence.Configurations;

public class LeaveApplicationConfiguration : IEntityTypeConfiguration<LeaveApplication>
{
    public void Configure(EntityTypeBuilder<LeaveApplication> builder)
    {
        builder.ToTable("LeaveApplications");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Status)
            .HasConversion<string>();

        builder.HasOne(l => l.LeaveType)
            .WithMany()
            .HasForeignKey(l => l.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}