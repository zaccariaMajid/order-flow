using System.Net;

namespace OrderService.Infrastructure.Persistence;

internal static class ConnectionStringNormalizer
{
    public static string NormalizePostgresConnectionString(string connectionString)
    {
        connectionString = connectionString.Trim();

        if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
            connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = string.Concat(connectionString.Where(character => !char.IsWhiteSpace(character)));
        }

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out Uri? uri) ||
            (uri.Scheme != "postgresql" && uri.Scheme != "postgres"))
        {
            return connectionString;
        }

        string[] credentials = uri.UserInfo.Split(':', 2);
        string username = credentials.Length > 0 ? WebUtility.UrlDecode(credentials[0]) : string.Empty;
        string password = credentials.Length > 1 ? WebUtility.UrlDecode(credentials[1]) : string.Empty;
        string database = uri.AbsolutePath.TrimStart('/');

        List<string> parts =
        [
            $"Host={uri.Host}",
            $"Database={database}",
            $"Username={username}",
            $"Password={password}"
        ];

        if (!uri.IsDefaultPort)
        {
            parts.Insert(1, $"Port={uri.Port}");
        }

        foreach (string queryPart in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] keyValue = queryPart.Split('=', 2);
            string key = WebUtility.UrlDecode(keyValue[0]);
            string value = keyValue.Length > 1 ? WebUtility.UrlDecode(keyValue[1]) : string.Empty;

            if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add($"SSL Mode={value}");
            }
            else if (key.Equals("channel_binding", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add($"Channel Binding={value}");
            }
        }

        return string.Join(';', parts);
    }
}
