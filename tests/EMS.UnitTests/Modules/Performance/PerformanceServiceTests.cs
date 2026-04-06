using EMS.Application.Modules.Performance.DTOs;
using EMS.Application.Modules.Performance.Interfaces;
using EMS.Application.Modules.Performance.Services;
using EMS.Domain.Entities.Employee;
using EMS.Domain.Entities.Organization;
using EMS.Domain.Entities.Performance;
using EMS.Domain.Enums;
using Moq;

namespace EMS.UnitTests.Modules.Performance;

public class PerformanceServiceTests
{
    private readonly Mock<IPerformanceRepository> _mockRepo;
    private readonly PerformanceService _service;

    public PerformanceServiceTests()
    {
        _mockRepo = new Mock<IPerformanceRepository>();
        _service = new PerformanceService(_mockRepo.Object);
    }

    private static Goal MakeGoal(int id, Action<Goal>? customize = null)
    {
        var goal = new Goal
        {
            Id = id,
            EmployeeId = 1,
            Title = "Complete React Certification",
            Description = "Get certified",
            StartDate = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddMonths(3),
            ProgressPercent = 0,
            ReviewCycle = "2026-Q1",
            Status = GoalStatus.Active,
            Priority = GoalPriority.High,
            Category = GoalCategory.Technical,
            Tags = "React,TypeScript",
            SetByManagerId = 2,
            Employee = new EmployeeProfile
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                EmployeeCode = "EMP001",
                Department = new Department { Name = "Engineering" }
            },
            SetByManager = new EmployeeProfile
            {
                Id = 2,
                FirstName = "Manager",
                LastName = "Test"
            }
        };
        customize?.Invoke(goal);
        return goal;
    }

    [Fact]
    public async Task CreateGoalAsync_Should_Create_With_New_Fields()
    {
        var dto = new CreateGoalDto
        {
            EmployeeId = 1,
            Title = "Learn Blazor",
            Description = "Build a Blazor app",
            StartDate = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddMonths(2),
            ReviewCycle = "2026-Q2",
            Priority = "Medium",
            Category = "Professional",
            Tags = "Blazor,Frontend"
        };

        _mockRepo.Setup(r => r.CreateGoalAsync(It.IsAny<Goal>()))
            .ReturnsAsync(MakeGoal(1, g =>
            {
                g.Title = "Learn Blazor";
                g.Description = "Build a Blazor app";
                g.Priority = GoalPriority.Medium;
                g.Category = GoalCategory.Professional;
                g.Tags = "Blazor,Frontend";
            }));

        var result = await _service.CreateGoalAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Learn Blazor", result.Title);
        Assert.Equal("Build a Blazor app", result.Description);
        Assert.Equal("Medium", result.Priority);
        Assert.Equal("Professional", result.Category);
        Assert.Equal("Blazor,Frontend", result.Tags);
        Assert.Equal("Active", result.Status);
        Assert.Equal(0, result.ProgressPercent);
        _mockRepo.Verify(r => r.CreateGoalAsync(It.IsAny<Goal>()), Times.Once);
    }

    [Fact]
    public async Task CreateGoalAsync_Should_Set_Defaults_When_Fields_Are_Empty()
    {
        var dto = new CreateGoalDto
        {
            EmployeeId = 1,
            Title = "Simple Goal",
            Deadline = DateTime.UtcNow.AddMonths(1),
            ReviewCycle = "2026-Q1"
        };

        Goal? capturedGoal = null;
        _mockRepo.Setup(r => r.CreateGoalAsync(It.IsAny<Goal>()))
            .Callback<Goal>(g => capturedGoal = g)
            .ReturnsAsync(MakeGoal(1, g =>
            {
                g.Title = "Simple Goal";
                g.Priority = GoalPriority.Medium;
                g.Category = GoalCategory.Technical;
            }));

        var result = await _service.CreateGoalAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Medium", result.Priority);
        Assert.Equal("Technical", result.Category);
        Assert.NotNull(capturedGoal);
        Assert.Equal(GoalPriority.Medium, capturedGoal.Priority);
        Assert.Equal(GoalCategory.Technical, capturedGoal.Category);
    }

    [Fact]
    public async Task GetGoalsAsync_Should_Return_Goals_With_All_New_Fields()
    {
        var goals = new List<Goal> { MakeGoal(1) };

        _mockRepo.Setup(r => r.GetGoalsAsync(null, null)).ReturnsAsync(goals);

        var result = (await _service.GetGoalsAsync(null, null)).ToList();

        Assert.Single(result);
        Assert.Equal("High", result[0].Priority);
        Assert.Equal("Technical", result[0].Category);
        Assert.Equal("React,TypeScript", result[0].Tags);
    }

    [Fact]
    public async Task UpdateProgressAsync_Should_Update_Progress_When_Goal_Exists()
    {
        var goal = MakeGoal(1);
        _mockRepo.Setup(r => r.GetGoalByIdAsync(1)).ReturnsAsync(goal);
        _mockRepo.Setup(r => r.UpdateGoalAsync(1, It.IsAny<Goal>()))
            .ReturnsAsync(MakeGoal(1, g => g.ProgressPercent = 75));

        var updateDto = new UpdateGoalProgressDto { ProgressPercent = 75 };
        var (result, error) = await _service.UpdateProgressAsync(1, updateDto);

        Assert.NotNull(result);
        Assert.Null(error);
        Assert.Equal(75, result.ProgressPercent);
    }

    [Fact]
    public async Task UpdateProgressAsync_Should_Return_Error_When_Goal_Not_Found()
    {
        _mockRepo.Setup(r => r.GetGoalByIdAsync(999)).ReturnsAsync((Goal?)null);

        var (result, error) = await _service.UpdateProgressAsync(999,
            new UpdateGoalProgressDto { ProgressPercent = 50 });

        Assert.Null(result);
        Assert.Equal("Goal not found.", error);
    }

    [Fact]
    public async Task DeleteGoalAsync_Should_Succeed_When_Goal_Exists()
    {
        _mockRepo.Setup(r => r.GetGoalByIdAsync(1)).ReturnsAsync(MakeGoal(1));
        _mockRepo.Setup(r => r.DeleteGoalAsync(1)).ReturnsAsync(true);

        var (success, error) = await _service.DeleteGoalAsync(1);

        Assert.True(success);
        Assert.Null(error);
    }

    [Fact]
    public async Task DeleteGoalAsync_Should_Return_Error_When_Goal_Not_Found()
    {
        _mockRepo.Setup(r => r.GetGoalByIdAsync(999)).ReturnsAsync((Goal?)null);

        var (success, error) = await _service.DeleteGoalAsync(999);

        Assert.False(success);
        Assert.Equal("Goal not found.", error);
    }

    [Fact]
    public async Task MapGoalToDto_Should_Set_IsOverdue_When_Deadline_Passed_And_Active()
    {
        var goal = MakeGoal(1, g =>
        {
            g.Deadline = DateTime.UtcNow.AddDays(-5);
            g.Status = GoalStatus.Active;
        });

        _mockRepo.Setup(r => r.GetGoalsAsync(null, null)).ReturnsAsync(new List<Goal> { goal });
        var result = (await _service.GetGoalsAsync(null, null)).ToList();

        Assert.True(result[0].IsOverdue);
    }
}
