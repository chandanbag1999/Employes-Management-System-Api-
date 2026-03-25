using EMS.Application.Common.Interfaces;
using EMS.Application.Modules.Identity.DTOs;
using EMS.Application.Modules.Identity.Interfaces;
using EMS.Application.Modules.Identity.Services;
using EMS.Domain.Entities.Identity;
using EMS.Domain.Enums;
using EMS.UnitTests.Helpers;
using FluentAssertions;
using Moq;

namespace EMS.UnitTests.Modules.Identity;

public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _authRepoMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AuthService _sut; // System Under Test

    public AuthServiceTests()
    {
        _authRepoMock = new Mock<IAuthRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _sut = new AuthService(_authRepoMock.Object, _jwtServiceMock.Object);
    }

    // ── Register Tests ────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnAuthResponse()
    {
        // Arrange
        var dto = new RegisterDto
        {
            UserName = "John Doe",
            Email = "john@ems.com",
            Password = "Test@123",
            Role = "Employee"
        };

        var createdUser = TestDataBuilder.CreateUser(
            email: dto.Email,
            userName: dto.UserName);

        _authRepoMock
            .Setup(r => r.EmailExistsAsync(dto.Email))
            .ReturnsAsync(false);

        _authRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(createdUser);

        _jwtServiceMock
            .Setup(j => j.GenerateToken(It.IsAny<AppUser>()))
            .Returns("fake-jwt-token");

        _jwtServiceMock
            .Setup(j => j.GetExpiryTime())
            .Returns(DateTime.UtcNow.AddHours(1));

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("fake-jwt-token");
        result.UserName.Should().Be(dto.UserName);
        result.Email.Should().Be(dto.Email);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var dto = new RegisterDto
        {
            UserName = "John Doe",
            Email = "existing@ems.com",
            Password = "Test@123"
        };

        _authRepoMock
            .Setup(r => r.EmailExistsAsync(dto.Email))
            .ReturnsAsync(true); // Email already exists

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Should().BeNull();

        // Verify CreateAsync kabhi call nahi hua
        _authRepoMock.Verify(
            r => r.CreateAsync(It.IsAny<AppUser>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidRole_ShouldDefaultToEmployee()
    {
        // Arrange
        var dto = new RegisterDto
        {
            UserName = "John",
            Email = "john@ems.com",
            Password = "Test@123",
            Role = "InvalidRole"  // Invalid role
        };

        AppUser? capturedUser = null;

        _authRepoMock
            .Setup(r => r.EmailExistsAsync(dto.Email))
            .ReturnsAsync(false);

        _authRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<AppUser>()))
            .Callback<AppUser>(u => capturedUser = u)
            .ReturnsAsync(TestDataBuilder.CreateUser());

        _jwtServiceMock
            .Setup(j => j.GenerateToken(It.IsAny<AppUser>()))
            .Returns("token");

        _jwtServiceMock
            .Setup(j => j.GetExpiryTime())
            .Returns(DateTime.UtcNow.AddHours(1));

        // Act
        await _sut.RegisterAsync(dto);

        // Assert — invalid role → Employee
        capturedUser.Should().NotBeNull();
        capturedUser!.Role.Should().Be(UserRole.Employee);
    }

    // ── Login Tests ───────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var password = "Test@123";
        var user = TestDataBuilder.CreateUser();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = password
        };

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(j => j.GenerateToken(user))
            .Returns("jwt-token");

        _jwtServiceMock
            .Setup(j => j.GetExpiryTime())
            .Returns(DateTime.UtcNow.AddHours(1));

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldReturnNull()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = "WrongPassword"
        };

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "notexist@ems.com",
            Password = "Test@123"
        };

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((AppUser?)null); // User nahi mila

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldReturnNull()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();
        user.IsActive = false; // Inactive user

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = "Test@123"
        };

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        result.Should().BeNull();
    }
}