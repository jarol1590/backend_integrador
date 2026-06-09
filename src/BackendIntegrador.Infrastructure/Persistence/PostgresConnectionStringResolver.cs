using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BackendIntegrador.Infrastructure.Persistence;

public static class PostgresConnectionStringResolver
{
    public static void ApplyDatabaseUrlFallback(IConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")))
        {
            return;
        }

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(databaseUrl))
        {
            return;
        }

        configuration["ConnectionStrings:DefaultConnection"] = FromDatabaseUrl(databaseUrl);
    }

    public static string FromDatabaseUrl(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
            SslMode = SslMode.Require,
        };

        return builder.ConnectionString;
    }

    public static void Validate(string connectionString)
    {
        try
        {
            _ = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "La cadena de conexión no es válida para Npgsql. " +
                "Usa el formato: Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true. " +
                "No pegues la URL postgresql:// directamente y no omitas Password=.",
                ex);
        }
    }
}
