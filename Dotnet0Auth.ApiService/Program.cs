using Dotnet0Auth.ApiService;
using Dotnet0Auth.ApiService.Data;
using Dotnet0Auth.ApiService.Endpoints;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddMassTransit(
        x =>
        {
            x.AddConsumers(typeof(Program).Assembly);
            x.UsingRabbitMq(
                (context, cfg) =>
                {
                    cfg.Host(builder.Configuration.GetConnectionString("RabbitMQConnection"));
                    cfg.ConfigureEndpoints(context);
                });
        });

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Database")!,
        builder =>
        {
            builder.MigrationsHistoryTable(HistoryRepository.DefaultTableName);
            builder.EnableRetryOnFailure();
        });
});

builder.Services
    .AddAuthentication("OAuth")
    .AddJwtBearer(
        "OAuth",
        config =>
        {
            var secretBytes = Encoding.UTF8.GetBytes(Constants.Secret);
            var key = new SymmetricSecurityKey(secretBytes);

            config.Events = new JwtBearerEvents()
            {
                OnMessageReceived =
                    context =>
                    {
                        if (context.Request.Query.ContainsKey("access_token"))
                        {
                            context.Token = context.Request.Query["access_token"];
                        }

                        return Task.CompletedTask;
                    }
            };

            config.TokenValidationParameters = new TokenValidationParameters()
            {
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = Constants.Issuer,
                ValidAudience = Constants.Audiance,
                IssuerSigningKey = key,
            };
        });
builder.Services.AddAuthorization();

/// Register view and controllers
builder.Services.AddControllersWithViews();

builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    context.Intit();
}

app.UseExceptionHandler();

app.UseStaticFiles(); // Necessary for serving static content like CSS, JS, and images
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapWeatherForecastEndpoints();

app.MapDefaultEndpoints();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

