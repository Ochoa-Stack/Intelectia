using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        // Leemos la clave secreta desde user-secrets
        var secret = _configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT Secret no está configurado.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Definimos los datos que viajan dentro del token
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Incluimos los roles activos del usuario como claims
        if (user.StudentProfile is not null)
            claims.Add(new Claim(ClaimTypes.Role, "Student"));
        if (user.VendorProfile is not null)
            claims.Add(new Claim(ClaimTypes.Role, "Vendor"));


        var expiration = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes());

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        // Generamos 64 bytes aleatorios criptográficamente seguros
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public int GetAccessTokenExpirationMinutes()
    {
        // Leemos la duración desde configuración, por defecto 60 minutos
        var raw = _configuration["JwtSettings:ExpirationMinutes"];
        return int.TryParse(raw, out var minutes) ? minutes : 60;
    }
}
