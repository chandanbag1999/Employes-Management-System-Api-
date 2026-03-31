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
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _authRepoMock = new Mock<IAuthRepository>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _jwtServiceMock = new Mock<IJwtService>();

        _sut = new AuthService(
            _authRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _jwtServiceMock.Object);
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
            Password = "Test@123A!"
        };

        var createdUser = TestDataBuilder.CreateUser(
            email: dto.Email,
            userName: dto.UserName);

        var fakeRefreshToken = new RefreshToken
        {
            Token = "fake-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserId = createdUser.Id
        };

        _authRepoMock
            .Setup(r => r.EmailExistsAsync(dto.Email))
            .ReturnsAsync(false);

        _authRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(createdUser);

        _authRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _jwtServiceMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<AppUser>()))
            .Returns("fake-access-token");

        _jwtServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("fake-refresh-token");

        _jwtServiceMock
            .Setup(j => j.GetAccessTokenExpiry())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        _jwtServiceMock
            .Setup(j => j.GetRefreshTokenExpiry())
            .Returns(DateTime.UtcNow.AddDays(7));

        _refreshTokenRepoMock
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _refreshTokenRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("fake-access-token");
        result.RefreshToken.Should().Be("fake-refresh-token");
        result.UserName.Should().Be(dto.UserName);
        result.Email.Should().Be(dto.Email);
        result.Role.Should().Be("Employee");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var dto = new RegisterDto
        {
            UserName = "John Doe",
            Email = "existing@ems.com",
            Password = "Test@123A!"
        };

        _authRepoMock
            .Setup(r => r.EmailExistsAsync(dto.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        result.Should().BeNull();

        _authRepoMock.Verify(
            r => r.CreateAsync(It.IsAny<AppUser>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldAlwaysCreateEmployeeRole()
    {
        // Arrange
        var dto = new RegisterDto
        {
            UserName = "John",
            Email = "john@ems.com",
            Password = "Test@123A!"
        };

        AppUser? capturedUser = null;
        var createdUser = TestDataBuilder.CreateUser();

        _authRepoMock
            .Setup(r => r.EmailExistsAsync(dto.Email))
            .ReturnsAsync(false);

        _authRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<AppUser>()))
            .Callback<AppUser>(u => capturedUser = u)
            .ReturnsAsync(createdUser);

        _authRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _jwtServiceMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<AppUser>()))
            .Returns("token");

        _jwtServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("fake-refresh-token");

        _jwtServiceMock
            .Setup(j => j.GetAccessTokenExpiry())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        _jwtServiceMock
            .Setup(j => j.GetRefreshTokenExpiry())
            .Returns(DateTime.UtcNow.AddDays(7));

        _refreshTokenRepoMock
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _refreshTokenRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _sut.RegisterAsync(dto);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Role.Should().Be(UserRole.Employee);
    }

    // ── Login Tests ───────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var password = "Test@123A!";
        var user = TestDataBuilder.CreateUser();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = password
        };

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(user.Email.ToLower().Trim()))
            .ReturnsAsync(user);

        _authRepoMock
            .Setup(r => r.GetByIdWithTokensAsync(user.Id))
            .ReturnsAsync(user);

        _authRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _jwtServiceMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<AppUser>()))
            .Returns("jwt-access-token");

        _jwtServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("fake-refresh-token");

        _jwtServiceMock
            .Setup(j => j.GetAccessTokenExpiry())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        _jwtServiceMock
            .Setup(j => j.GetRefreshTokenExpiry())
            .Returns(DateTime.UtcNow.AddDays(7));

        _refreshTokenRepoMock
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _refreshTokenRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.LoginAsync(dto, "127.0.0.1");

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("jwt-access-token");
        result.RefreshToken.Should().Be("fake-refresh-token");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldReturnNull()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword@1");

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = "WrongPassword@1"
        };

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(user.Email.ToLower().Trim()))
            .ReturnsAsync(user);

        _authRepoMock
            .Setup(r => r.GetByIdWithTokensAsync(user.Id))
            .ReturnsAsync(user);

        _authRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.LoginAsync(dto, "127.0.0.1");

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
            Password = "Test@123A!"
        };

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email.ToLower().Trim()))
            .ReturnsAsync((AppUser?)null);

        _authRepoMock
            .Setup(r => r.GetByIdWithTokensAsync(0))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result = await _sut.LoginAsync(dto, "127.0.0.1");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldReturnNull()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();
        user.IsActive = false;

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = "Test@123A!"
        };

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(user.Email.ToLower().Trim()))
            .ReturnsAsync(user);

        _authRepoMock
            .Setup(r => r.GetByIdWithTokensAsync(user.Id))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(dto, "127.0.0.1");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithLockedAccount_ShouldReturnNull()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser();
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = "Test@123A!"
        };

        _authRepoMock
            .Setup(r => r.GetByEmailAsync(user.Email.ToLower().Trim()))
            .ReturnsAsync(user);

        _authRepoMock
            .Setup(r => r.GetByIdWithTokensAsync(user.Id))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.LoginAsync(dto, "127.0.0.1");

        // Assert
        result.Should().BeNull();
    }
}