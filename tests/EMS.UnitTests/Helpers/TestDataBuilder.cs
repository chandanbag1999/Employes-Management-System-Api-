using EMS.Domain.Entities.Employee;
using EMS.Domain.Entities.Identity;
using EMS.Domain.Entities.Leave;
using EMS.Domain.Entities.Organization;
using EMS.Domain.Enums;

namespace EMS.UnitTests.Helpers;

// Test data banane ka ek jagah — DRY principle
public static class TestDataBuilder
{
    public static AppUser CreateUser(
        int id = 1,
        string email = "test@ems.com",
        string userName = "Test User",
        UserRole role = UserRole.Employee)
    {
        return new AppUser
        {
            Id = id,
            UserName = userName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Department CreateDepartment(
        int id = 1,
        string name = "Engineering",
        string code = "ENG")
    {
        return new Department
        {
            Id = id,
            Name = name,
            Code = code,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static EmployeeProfile CreateEmployee(
        int id = 1,
        string firstName = "John",
        string lastName = "Doe",
        int departmentId = 1)
    {
        return new EmployeeProfile
        {
            Id = id,
            EmployeeCode = $"EMP00{id}",
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@ems.com",
            Phone = "9876543210",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            JoiningDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DepartmentId = departmentId,
            Status = EmploymentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Department = CreateDepartment(departmentId)
        };
    }

    public static LeaveType CreateLeaveType(
        int id = 1,
        string name = "Casual Leave",
        int maxDays = 12)
    {
        return new LeaveType
        {
            Id = id,
            Name = name,
            MaxDaysPerYear = maxDays,
            IsPaid = true,
            IsCarryForwardAllowed = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static LeaveApplication CreateLeaveApplication(
        int id = 1,
        int employeeId = 1,
        int leaveTypeId = 1,
        LeaveStatus status = LeaveStatus.Pending)
    {
        return new LeaveApplication
        {
            Id = id,
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            FromDate = DateTime.UtcNow.AddDays(2).Date,
            ToDate = DateTime.UtcNow.AddDays(4).Date,
            TotalDays = 3,
            Reason = "Personal work",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            Employee = CreateEmployee(employeeId),
            LeaveType = CreateLeaveType(leaveTypeId)
        };
    }
}