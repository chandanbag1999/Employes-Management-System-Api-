using EMS.Application.Modules.Employees.DTOs;
using EMS.Application.Modules.Employees.Interfaces;
using EMS.Application.Modules.Employees.Services;
using EMS.Domain.Entities.Employee;
using EMS.Domain.Enums;
using EMS.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace EMS.UnitTests.Modules.Employees;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _repoMock;
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _repoMock = new Mock<IEmployeeRepository>();
        _sut = new EmployeeService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldAutoGenerateCode()
    {
        // Arrange
        var dto = new CreateEmployeeDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@ems.com",
            Phone = "9876543210",
            Gender = Gender.Female,
            DateOfBirth = new DateTime(1995, 5, 10),
            JoiningDate = DateTime.Today,
            DepartmentId = 1
        };

        EmployeeProfile? capturedEmployee = null;

        _repoMock.Setup(r => r.EmailExistsAsync(dto.Email, null))
            .ReturnsAsync(false);

        // 5 employees already exist → next code = EMP006
        _repoMock.Setup(r => r.GetTotalCountAsync())
            .ReturnsAsync(5);

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<EmployeeProfile>()))
            .Callback<EmployeeProfile>(e => capturedEmployee = e)
            .ReturnsAsync(TestDataBuilder.CreateEmployee(6, "Jane", "Smith"));

        // Act
        var (result, error) = await _sut.CreateAsync(dto);

        // Assert
        error.Should().BeNull();
        capturedEmployee.Should().NotBeNull();
        capturedEmployee!.EmployeeCode.Should().Be("EMP006");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ShouldReturnError()
    {
        // Arrange
        var dto = new CreateEmployeeDto
        {
            Email = "existing@ems.com",
            FirstName = "John",
            LastName = "Doe",
            Gender = Gender.Male,
            DateOfBirth = DateTime.Today.AddYears(-25),
            JoiningDate = DateTime.Today,
            DepartmentId = 1
        };

        _repoMock.Setup(r => r.EmailExistsAsync(dto.Email, null))
            .ReturnsAsync(true);

        // Act
        var (result, error) = await _sut.CreateAsync(dto);

        // Assert
        result.Should().BeNull();
        error.Should().Contain("Email already");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingEmployee_ShouldReturnDto()
    {
        // Arrange
        var employee = TestDataBuilder.CreateEmployee(1, "John", "Doe");

        _repoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(employee);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
        result.FullName.Should().Be("John Doe");
        result.EmployeeCode.Should().Be("EMP001");
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingEmployee_ShouldReturnError()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((EmployeeProfile?)null);

        // Act
        var (success, error) = await _sut.DeleteAsync(999);

        // Assert
        success.Should().BeFalse();
        error.Should().Contain("not found");
    }
}