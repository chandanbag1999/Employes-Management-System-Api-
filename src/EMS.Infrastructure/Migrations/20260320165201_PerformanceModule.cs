using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PerformanceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewCycleYear",
                table: "Goals");

            migrationBuilder.AddColumn<string>(
                name: "ReviewCycle",
                table: "Goals",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SetByManagerId",
                table: "Goals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Goals",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PerformanceReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    ReviewerId = table.Column<int>(type: "integer", nullable: false),
                    ReviewCycle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Quarter = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TechnicalSkillRating = table.Column<decimal>(type: "numeric(3,1)", nullable: false),
                    CommunicationRating = table.Column<decimal>(type: "numeric(3,1)", nullable: false),
                    TeamworkRating = table.Column<decimal>(type: "numeric(3,1)", nullable: false),
                    LeadershipRating = table.Column<decimal>(type: "numeric(3,1)", nullable: false),
                    PunctualityRating = table.Column<decimal>(type: "numeric(3,1)", nullable: false),
                    OverallRating = table.Column<decimal>(type: "numeric(3,1)", nullable: false),
                    Strengths = table.Column<string>(type: "text", nullable: true),
                    AreasOfImprovement = table.Column<string>(type: "text", nullable: true),
                    ReviewerComments = table.Column<string>(type: "text", nullable: true),
                    EmployeeSelfComment = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_Employees_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Goals_EmployeeId",
                table: "Goals",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_SetByManagerId",
                table: "Goals",
                column: "SetByManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_EmployeeId_ReviewCycle",
                table: "PerformanceReviews",
                columns: new[] { "EmployeeId", "ReviewCycle" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_ReviewerId",
                table: "PerformanceReviews",
                column: "ReviewerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_Employees_EmployeeId",
                table: "Goals",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_Employees_SetByManagerId",
                table: "Goals",
                column: "SetByManagerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_Employees_EmployeeId",
                table: "Goals");

            migrationBuilder.DropForeignKey(
                name: "FK_Goals_Employees_SetByManagerId",
                table: "Goals");

            migrationBuilder.DropTable(
                name: "PerformanceReviews");

            migrationBuilder.DropIndex(
                name: "IX_Goals_EmployeeId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Goals_SetByManagerId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "ReviewCycle",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "SetByManagerId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Goals");

            migrationBuilder.AddColumn<string>(
                name: "ReviewCycleYear",
                table: "Goals",
                type: "text",
                nullable: true);
        }
    }
}
