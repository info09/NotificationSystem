using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Observability.DataContext;

public static class DbContextExtensions
{
    /// <summary>
    /// Applies any pending migrations to the database automatically.
    /// Should be called after the app is built, before running.
    /// </summary>
    /// <param name="services">The service provider to resolve DbContext from.</param>
    /// <typeparam name="TContext">The DbContext type, typically IdentityDbContext.</typeparam>
    public static void ApplyMigrations<TContext>(this IServiceProvider services)
        where TContext : DbContext
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        context.Database.Migrate();
    }
}
