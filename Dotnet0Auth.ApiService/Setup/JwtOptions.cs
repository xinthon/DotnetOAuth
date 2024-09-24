using Microsoft.Extensions.Options;

namespace Dotnet0Auth.ApiService.Setup;

public class JwtOptions
{
    public required string SecretKey { get; init; }

    public required string Issuer { get; init; }

    public required string Audience { get; init; }
}

public class JwtOptionsSetup : IConfigureOptions<JwtOptions>
{
    const string Name = "Jwt";

    private readonly IConfiguration _config;
    public JwtOptionsSetup(IConfiguration config)
    {
        _config = config;
    }

    public void Configure(JwtOptions options)
    {
        _config.GetSection(Name).Bind(options);
    }
}
