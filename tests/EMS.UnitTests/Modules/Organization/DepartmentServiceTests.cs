using EMS.Application.Common.DTOs;
using EMS.Application.Modules.Organization.DTOs;
using EMS.Application.Modules.Organization.Interfaces;
using EMS.Application.Modules.Organization.Services;
using EMS.Domain.Entities.Organization;
using EMS.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace EMS.UnitTests.Modules.Organization;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _repoMock;
    private readonly DepartmentService _sut;

    public DepartmentServiceTests()
    {
        _repoMock = new Mock<IDepartmentRepository>();
        _sut = new DepartmentService(_repoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedResult()
    {
        // Arrange
        var departments = new List<Department>
        {
            TestDataBuilder.CreateDepartment(1, "Engineering", "ENG"),
            TestDataBuilder.CreateDepartment(2, "HR", "HR")
        };

        _repoMock
            .Setup(r => r.GetAllAsync(1, 10, null))
            .ReturnsAsync(new PaginatedResult<Department>
            {
                Data = departments,
                TotalCount = 2,
                Page = 1,
                PageSize = 10
            });

        // Act
        var result = await _sut.GetAllAsync(1, 10, null);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnDto()
    {
        // Arrange
        var dept = TestDataBuilder.CreateDepartment(1, "Engineering");

        _repoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(dept);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Engineering");
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Department?)null);

        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithUniqueName_ShouldReturnCreatedDto()
    {
        // Arrange
        var dto = new CreateDepartmentDto
        {
            Name = "Finance",
            Code = "FIN",
            Description = "Finance Department"
        };

        var created = TestDataBuilder.CreateDepartment(3, "Finance", "FIN");

        _repoMock
            .Setup(r => r.NameExistsAsync(dto.Name, null))
            .ReturnsAsync(false);

        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<Department>()))
            .ReturnsAsync(created);

        // Act
        var (result, error) = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        error.Should().BeNull();
        result!.Name.Should().Be("Finance");
        result.Code.Should().Be("FIN");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ShouldReturnError()
    {
        // Arrange
        var dto = new CreateDepartmentDto { Name = "Engineering" };

        _repoMock
            .Setup(r => r.NameExistsAsync(dto.Name, null))
            .ReturnsAsync(true); // Name already exists

        // Act
        var (result, error) = await _sut.CreateAsync(dto);

        // Assert
        result.Should().BeNull();
        error.Should().Contain("already exists");

        // Verify CreateAsync never called
        _repoMock.Verify(
            r => r.CreateAsync(It.IsAny<Department>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithEmployees_ShouldReturnError()
    {
        // Arrange
        var dept = TestDataBuilder.CreateDepartment(1);
        dept.Employees.Add(TestDataBuilder.CreateEmployee()); // Has employees

        _repoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(dept);

        // Act
        var (success, error) = await _sut.DeleteAsync(1);

        // Assert
        success.Should().BeFalse();
        error.Should().Contain("employees");

        // Verify DeleteAsync never called
        _repoMock.Verify(
            r => r.DeleteAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithNoEmployees_ShouldSucceed()
    {
        // Arrange
        var dept = TestDataBuilder.CreateDepartment(1);
        // No employees

        _repoMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(dept);

        _repoMock
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var (success, error) = await _sut.DeleteAsync(1);

        // Assert
        success.Should().BeTrue();
        error.Should().BeNull();
    }
}