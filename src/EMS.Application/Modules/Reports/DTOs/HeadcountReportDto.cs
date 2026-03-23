namespace EMS.Application.Modules.Reports.DTOs;

public class HeadcountReportDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public int TotalEmployees { get; set; }
    public int Active { get; set; }
    public int OnProbation { get; set; }
    public int Resigned { get; set; }
    public int Terminated { get; set; }
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
}