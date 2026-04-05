using EMS.Application.Modules.Attendance.Interfaces;
using EMS.Application.Modules.Leave.DTOs;
using EMS.Application.Modules.Leave.Interfaces;
using EMS.Application.Modules.Leave.Services;
using EMS.Domain.Entities.Leave;
using EMS.Domain.Enums;
using EMS.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace EMS.UnitTests.Modules.Leave;

public class LeaveServiceTests
{
    private readonly Mock<ILeaveRepository> _repoMock;
    private readonly Mock<IAttendanceRepository> _attendanceRepoMock;
    private readonly LeaveService _sut;

    public LeaveServiceTests()
    {
        _repoMock = new Mock<ILeaveRepository>();
        _attendanceRepoMock = new Mock<IAttendanceRepository>();
        _sut = new LeaveService(_repoMock.Object, _attendanceRepoMock.Object);
    }

    [Fact]
    public async Task ApplyAsync_WithValidData_ShouldReturnLeaveResponse()
    {
        // Arrange
        var dto = new ApplyLeaveDto
        {
            EmployeeId = 1,
            LeaveTypeId = 1,
            FromDate = DateTime.Today.AddDays(5),
            ToDate = DateTime.Today.AddDays(7),
            Reason = "Personal work"
        };

        var leaveType = TestDataBuilder.CreateLeaveType(maxDays: 12);
        var created = TestDataBuilder.CreateLeaveApplication();

        _repoMock.Setup(r => r.HasOverlappingLeaveAsync(
                dto.EmployeeId, It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), null))
            .ReturnsAsync(false);

        _repoMock.Setup(r => r.GetLeaveTypeByIdAsync(dto.LeaveTypeId))
            .ReturnsAsync(leaveType);

        _repoMock.Setup(r => r.GetUsedDaysAsync(
                dto.EmployeeId, dto.LeaveTypeId, It.IsAny<int>()))
            .ReturnsAsync(0);

        _repoMock.Setup(r => r.GetPendingDaysAsync(
                dto.EmployeeId, dto.LeaveTypeId, It.IsAny<int>()))
            .ReturnsAsync(0);

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<LeaveApplication>()))
            .ReturnsAsync(created);

        // Act
        var (result, error) = await _sut.ApplyAsync(dto);

        // Assert
        result.Should().NotBeNull();
        error.Should().BeNull();
    }

    [Fact]
    public async Task ApplyAsync_WithPastDate_ShouldReturnError()
    {
        // Arrange
        var dto = new ApplyLeaveDto
        {
            EmployeeId = 1,
            LeaveTypeId = 1,
            FromDate = DateTime.Today.AddDays(-3), // Past date
            ToDate = DateTime.Today.AddDays(-1),
            Reason = "Test"
        };

        // Act
        var (result, error) = await _sut.ApplyAsync(dto);

        // Assert
        result.Should().BeNull();
        error.Should().Contain("past");
    }

    [Fact]
    public async Task ApplyAsync_WhenFromDateAfterToDate_ShouldReturnError()
    {
        // Arrange
        var dto = new ApplyLeaveDto
        {
            EmployeeId = 1,
            LeaveTypeId = 1,
            FromDate = DateTime.Today.AddDays(5),
            ToDate = DateTime.Today.AddDays(2), // Before FromDate
            Reason = "Test"
        };

        // Act
        var (result, error) = await _sut.ApplyAsync(dto);

        // Assert
        result.Should().BeNull();
        error.Should().Contain("cannot be after");
    }

    [Fact]
    public async Task ApplyAsync_WithInsufficientBalance_ShouldReturnError()
    {
        // Arrange
        var dto = new ApplyLeaveDto
        {
            EmployeeId = 1,
            LeaveTypeId = 1,
            FromDate = DateTime.Today.AddDays(2),
            ToDate = DateTime.Today.AddDays(6), // 5 days
            Reason = "Vacation"
        };

        var leaveType = TestDataBuilder.CreateLeaveType(maxDays: 5);

        _repoMock.Setup(r => r.HasOverlappingLeaveAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), null))
            .ReturnsAsync(false);

        _repoMock.Setup(r => r.GetLeaveTypeByIdAsync(1))
            .ReturnsAsync(leaveType);

        // Already used 3 days, pending 2 days = 5 total, balance = 0
        _repoMock.Setup(r => r.GetUsedDaysAsync(1, 1, It.IsAny<int>()))
            .ReturnsAsync(3);

        _repoMock.Setup(r => r.GetPendingDaysAsync(1, 1, It.IsAny<int>()))
            .ReturnsAsync(2);

        // Act
        var (result, error) = await _sut.ApplyAsync(dto);

        // Assert
        result.Should().BeNull();
        error.Should().Contain("Insufficient");
    }

    [Fact]
    public async Task ApplyAsync_WithOverlappingLeave_ShouldReturnError()
    {
        // Arrange
        var dto = new ApplyLeaveDto
        {
            EmployeeId = 1,
            LeaveTypeId = 1,
            FromDate = DateTime.Today.AddDays(2),
            ToDate = DateTime.Today.AddDays(4),
            Reason = "Test"
        };

        _repoMock.Setup(r => r.HasOverlappingLeaveAsync(
                It.IsAny<int>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), null))
            .ReturnsAsync(true); // Overlap exists

        // Act
        var (result, error) = await _sut.ApplyAsync(dto);

        // Assert
        result.Should().BeNull();
        error.Should().Contain("already have a leave");
    }

    [Fact]
    public async Task ApproveOrRejectAsync_WithPendingApplication_ShouldApprove()
    {
        // Arrange
        var application = TestDataBuilder.CreateLeaveApplication(
            status: LeaveStatus.Pending);

        var dto = new LeaveActionDto
        {
            ActionById = 2,
            IsApproved = true
        };

        _repoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(application);

        _repoMock.Setup(r => r.UpdateStatusAsync(
                1, LeaveStatus.Approved, 2, null))
            .ReturnsAsync(application);

        // Act
        var (result, error) = await _sut.ApproveOrRejectAsync(1, dto);

        // Assert
        error.Should().BeNull();
        _repoMock.Verify(r => r.UpdateStatusAsync(
            1, LeaveStatus.Approved, 2, null), Times.Once);
    }

    [Fact]
    public async Task ApproveOrRejectAsync_WithAlreadyApproved_ShouldReturnError()
    {
        // Arrange
        var application = TestDataBuilder.CreateLeaveApplication(
            status: LeaveStatus.Approved); // Already approved

        _repoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(application);

        var dto = new LeaveActionDto { ActionById = 2, IsApproved = true };

        // Act
        var (result, error) = await _sut.ApproveOrRejectAsync(1, dto);

        // Assert
        result.Should().BeNull();
        error.Should().Contain("pending");
    }

    [Fact]
    public async Task CancelAsync_WithOtherEmployeeLeave_ShouldReturnError()
    {
        // Arrange
        var application = TestDataBuilder.CreateLeaveApplication(
            employeeId: 5); // Belongs to employee 5

        _repoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(application);

        // Act — Employee 1 tries to cancel Employee 5's leave
        var (success, error) = await _sut.CancelAsync(1, employeeId: 1);

        // Assert
        success.Should().BeFalse();
        error.Should().Contain("own");
    }
}