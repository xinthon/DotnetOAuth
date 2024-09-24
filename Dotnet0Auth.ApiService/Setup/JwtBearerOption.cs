using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Dotnet0Auth.ApiService.Setup;

public class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOption;
    public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOption)
    {
        _jwtOption = jwtOption.Value;
    }

    public void Configure(JwtBearerOptions options)
    {
        byte[] key = Encoding
           .UTF8
           .GetBytes(_jwtOption.SecretKey);

        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Query.ContainsKey("Authorize"))
                {
                    context.Token = context.Request.Query["Authorize"];
                }
                return Task.CompletedTask;
            }
        };
 

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOption.Issuer,
            ValidAudience = _jwtOption.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };
    }

    public void Configure(string? name, JwtBearerOptions options) => Configure(options);
}
