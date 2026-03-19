using EMS.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS.Infrastructure.Persistence.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status)
            .HasConversion<string>();

        // One employee ka ek din mein ek hi record
        builder.HasIndex(a => new { a.EmployeeId, a.Date })
            .IsUnique();

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}