using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Dotnet0Auth.ApiService.Data;

public class Member
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
}


public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
        
    }

    public IQueryable<Member> GetMemberQuery()
    {
        return Members.AsQueryable();
    }

    public DbSet<Member> Members => Set<Member>();
}

public static class AuthDbContextExtension
{
    public static void Intit(this AuthDbContext context)
    {
        try
        {
            context.Database
                .EnsureCreated();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

