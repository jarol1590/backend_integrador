using BackendIntegrador.Api;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace BackendIntegrador.IntegrationTests.Common;

public abstract class PostgresWebApplicationFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private PostgreSqlContainer? _postgres;
    private bool _started;

    public async Task StartAsync()
    {
        if (_started)
        {
            return;
        }

        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        await _postgres.StartAsync();
        _started = true;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureTestServices(services =>
        {
            ConfigureTestServices(services);

            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                    || d.ServiceType == typeof(DbContextOptions))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgres!.GetConnectionString()));
        });
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
    }

    public override async ValueTask DisposeAsync()
    {
        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}
