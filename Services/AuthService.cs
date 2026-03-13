using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using EmployesManagementSystemApi.DTOs.Auth;
using EmployesManagementSystemApi.Models;
using EmployesManagementSystemApi.Repositories.Interfaces;
using EmployesManagementSystemApi.Services.Interfaces;

namespace EmployesManagementSystemApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            // Check if email already exists
            if (await _authRepository.EmailExistsAsync(dto.Email))
                return null; // Email already taken

            // Hash the password — NEVER store plain text
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = "User"
            };

            var created = await _authRepository.CreateAsync(user);

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(created), // We implement this in Phase 8
                UserName = created.UserName,
                Role = created.Role
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _authRepository.GetByEmailAsync(dto.Email);
            if (user == null) return null;

            // Verify password against stored hash
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid) return null;

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user),
                UserName = user.UserName,
                Role = user.Role
            };
        }

        private string GenerateJwtToken(AppUser user)
        {
            var jwtSecret = _configuration["JwtSettings:Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims = data stored inside the token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
            
        }
    }
}
