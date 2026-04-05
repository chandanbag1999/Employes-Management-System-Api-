using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.Infrastructure.Migrations;

/// <summary>
/// Data Migration: Fix orphaned EmployeeProfile records by linking them to AppUser
/// based on matching email addresses.
///
/// Problem: Many employees have UserId = NULL, causing 400 errors on attendance endpoints.
/// Solution: Auto-link employees to users where Email matches.
///
/// Run: dotnet ef database update
/// Date: 2026-04-04
/// </summary>
public partial class FixOrphanedEmployeeUserLinkage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // SQL to link orphaned employees to users by matching email
        // Only links if:
        //   - Employee.UserId IS NULL or 0
        //   - Employee.Email matches an AspNetUser.Email (case-insensitive)
        //   - Employee is not soft-deleted
        //
        // PostgreSQL specific syntax for UPDATE...FROM with RETURNING

        migrationBuilder.Sql(@"
            -- Log start of migration
            RAISE NOTICE 'Starting orphaned employee linkage fix...';

            -- Update orphaned employees by matching email
            UPDATE ""Employees"" e
            SET ""UserId"" = u.Id
            FROM ""AspNetUsers"" u
            WHERE e.""IsDeleted"" = false
              AND (e.""UserId"" IS NULL OR e.""UserId"" = 0)
              AND LOWER(e.Email) = LOWER(u.Email)
            RETURNING e.Id, e.""EmployeeCode"", e.Email, u.Id as linked_user_id;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Down migration not implemented for data fixes of this nature.
        // Reversing could break legitimate data entered after this migration.
        // If rollback is absolutely necessary, manual SQL review is required.
        migrationBuilder.Sql("RAISE NOTICE 'Down migration not supported for this data fix.'");
    }
}
