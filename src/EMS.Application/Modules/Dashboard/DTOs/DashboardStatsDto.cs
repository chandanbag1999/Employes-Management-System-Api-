namespace EMS.Application.Modules.Dashboard.DTOs;

public class DashboardStatsDto
{
    // Employee Stats
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int NewJoineesThisMonth { get; set; }
    public int OnProbation { get; set; }

    // Today's Attendance
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
    public int OnLeaveToday { get; set; }
    public double AttendancePercentToday =>
        TotalEmployees > 0
            ? Math.Round((double)PresentToday / TotalEmployees * 100, 1)
            : 0;

    // Leave Stats
    public int PendingLeaveRequests { get; set; }
    public int ApprovedLeavesThisMonth { get; set; }

    // Payroll Stats
    public decimal TotalPayrollThisMonth { get; set; }
    public int PayrollProcessedCount { get; set; }

    // Organization
    public int TotalDepartments { get; set; }
    public int TotalDesignations { get; set; }

    // Performance
    public int PendingReviews { get; set; }
    public int GoalsCompletedThisQuarter { get; set; }
}