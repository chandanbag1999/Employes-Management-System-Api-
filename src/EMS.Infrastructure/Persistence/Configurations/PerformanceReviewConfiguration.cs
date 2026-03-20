using EMS.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS.Infrastructure.Persistence.Configurations;

public class PerformanceReviewConfiguration : IEntityTypeConfiguration<PerformanceReview>
{
    public void Configure(EntityTypeBuilder<PerformanceReview> builder)
    {
        builder.ToTable("PerformanceReviews");
        builder.HasKey(p => p.Id);

        // Ek employee ka ek cycle mein sirf ek review
        builder.HasIndex(p => new { p.EmployeeId, p.ReviewCycle })
            .IsUnique();

        builder.Property(p => p.ReviewCycle)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Quarter)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(20);

        // Ratings decimal precision
        builder.Property(p => p.TechnicalSkillRating)
            .HasColumnType("decimal(3,1)");
        builder.Property(p => p.CommunicationRating)
            .HasColumnType("decimal(3,1)");
        builder.Property(p => p.TeamworkRating)
            .HasColumnType("decimal(3,1)");
        builder.Property(p => p.LeadershipRating)
            .HasColumnType("decimal(3,1)");
        builder.Property(p => p.PunctualityRating)
            .HasColumnType("decimal(3,1)");
        builder.Property(p => p.OverallRating)
            .HasColumnType("decimal(3,1)");

        // Review → Employee
        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Review → Reviewer (Manager/HR)
        builder.HasOne(p => p.Reviewer)
            .WithMany()
            .HasForeignKey(p => p.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}