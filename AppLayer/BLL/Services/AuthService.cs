// BLL/Services/AuthService.cs
using BLL.DTOs;
using BLL.Interfaces;
using BLL.Settings;
using DAL.EF.Tables;
using DAL.UnitOfWork;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUnitOfWork uow, IOptions<JwtSettings> jwtSettings)
    {
        _uow = uow;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _uow.Users.PhoneExistsAsync(dto.Phone))
            throw new InvalidOperationException("Phone number already registered.");

        if (dto.Role != "Passenger" && dto.Role != "Driver")
            throw new ArgumentException("Role must be 'Passenger' or 'Driver'.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Users.AddAsync(user);

        // every user gets a TeslaPay wallet, starting at zero
        await _uow.Wallets.AddAsync(new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            BalancePaisa = 0
        });

        // driver role also gets a Driver profile row
        if (dto.Role == "Driver")
        {
            await _uow.Drivers.AddAsync(new Driver
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LicenseNo = "PENDING", // update later via a profile-completion endpoint
                IsOnline = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _uow.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            UserId = user.Id,
            Name = user.Name,
            Role = user.Role
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _uow.Users.GetByPhoneAsync(dto.Phone)
            ?? throw new UnauthorizedAccessException("Invalid phone or password.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid phone or password.");

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            UserId = user.Id,
            Name = user.Name,
            Role = user.Role
        };
    }

    private string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}