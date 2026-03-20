using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PayrollModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayrollRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HRA = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TransportAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MedicalAllowance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OtherAllowances = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GrossEarnings = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PfDeduction = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxDeduction = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OtherDeductions = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    WorkingDays = table.Column<int>(type: "integer", nullable: false),
                    PresentDays = table.Column<int>(type: "integer", nullable: false),
                    LeaveDays = table.Column<int>(type: "integer", nullable: false),
                    LopDays = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaidOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStructures_EmployeeId",
                table: "SalaryStructures",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_EmployeeId_Month_Year",
                table: "PayrollRecords",
                columns: new[] { "EmployeeId", "Month", "Year" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryStructures_Employees_EmployeeId",
                table: "SalaryStructures",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalaryStructures_Employees_EmployeeId",
                table: "SalaryStructures");

            migrationBuilder.DropTable(
                name: "PayrollRecords");

            migrationBuilder.DropIndex(
                name: "IX_SalaryStructures_EmployeeId",
                table: "SalaryStructures");
        }
    }
}
