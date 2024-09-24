using Dotnet0Auth.ApiService.Setup;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Dotnet0Auth.ApiService;

public class JwtProvider
{
    private readonly JwtOptions _jwtOption;
    public JwtProvider(IOptions<JwtOptions> options)
    {
        _jwtOption = options.Value;
    }

    public string GenerateToken(Claim[] claims)
    {
        JwtSecurityTokenHandler tokenHandler
            = new JwtSecurityTokenHandler();

        byte[] key = Encoding
            .UTF8
            .GetBytes(_jwtOption.SecretKey);

        SigningCredentials credentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Audience = _jwtOption.Audience,
            Issuer = _jwtOption.Issuer,
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = credentials
        };

        SecurityToken token = tokenHandler
            .CreateToken(tokenDescriptor);

        string tokenValue = tokenHandler.WriteToken(token);

        return tokenValue;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);

            var base64Value = Convert
                .ToBase64String(randomNumber);

            return base64Value;
        }
    }

    public string? GetClinetNameFromExpiredToken(string accessToken)
    {
        byte[] key = Encoding.UTF8
            .GetBytes(_jwtOption.SecretKey);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false,
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(
            accessToken,
            tokenValidationParameters,
            out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken)
        {
            throw new SecurityTokenException("Invalid token");
        }

        if (jwtSecurityToken == null ||
            !jwtSecurityToken.Header.Alg
                .Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal.Identity?.Name;
    }
}

