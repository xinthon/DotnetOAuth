using Newtonsoft.Json;
using Dotnet0Auth.Web;
using Dotnet0Auth.Web.Components;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure JWT Bearer authentication with proper options
builder.Services.AddAuthentication(options =>
{
    // We check the cookie to confirm that we are authenticated
    options.DefaultAuthenticateScheme = "ClientCookie";
    // When we sign in we will deal out a cookie
    options.DefaultSignInScheme = "ClientCookie";
    // Use this to check if we are allowed to do something.
    options.DefaultChallengeScheme = "OurServer";
})
.AddCookie("ClientCookie")
.AddOAuth("OurServer", config =>
{
    Uri serverUrl = new("https+http://apiservice");

    config.ClientId = "client_id";
    config.ClientSecret = "client_secret";
    config.CallbackPath = "/oauth/callback";
    config.AuthorizationEndpoint = "https://localhost:7533/oauth/authorize";
    config.TokenEndpoint = "https://localhost:7533/oauth/token";
    config.SaveTokens = true;

    config.Events = new OAuthEvents()
    {
        OnCreatingTicket = context =>
        {
            if (context.AccessToken is null)
                return Task.CompletedTask;

            var base64payload = context.AccessToken
                .Split('.')[1];

            var bytes = Convert
                .FromBase64String(base64payload);

            var jsonPayload = Encoding.UTF8
                .GetString(bytes);

            var claims = JsonConvert
                .DeserializeObject<Dictionary<string, string>>(jsonPayload);

            foreach (var claim in claims!)
            {
                context.Identity?.AddClaim(new Claim(
                    claim.Key, 
                    claim.Value));
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client => 
    client.BaseAddress = new("https+http://apiservice"));

var app = builder.Build();

if(!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/oauth/callback", (string access_token, string token_type, string raw_claim, string refresh_token) => {
    Console.WriteLine("This is a testing");
});

app.MapDefaultEndpoints();

app.Run();
