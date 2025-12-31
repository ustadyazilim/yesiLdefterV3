using Microsoft.Extensions.Configuration;
using System;

namespace Ustad.API.Classes
{
    /// <summary>
    /// Helper class for ensuring SQL Server connection strings have proper SSL certificate trust settings
    /// </summary>
    public static class ConnectionStringHelper
    {
        /// <summary>
        /// Gets a connection string from configuration and ensures it has TrustServerCertificate and Encrypt settings
        /// </summary>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="connectionStringName">The name of the connection string in appsettings.json</param>
        /// <returns>Connection string with SSL trust settings applied</returns>
        public static string GetSecureConnectionString(this IConfiguration configuration, string connectionStringName)
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);
            return EnsureSslTrustSettings(connectionString);
        }

        /// <summary>
        /// Ensures a connection string has TrustServerCertificate=true and Encrypt=false settings
        /// </summary>
        /// <param name="connectionString">The original connection string</param>
        /// <returns>Connection string with SSL trust settings applied</returns>
        public static string EnsureSslTrustSettings(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return string.Empty;

            // Check if TrustServerCertificate is already set
            if (connectionString.Contains("TrustServerCertificate", StringComparison.OrdinalIgnoreCase))
            {
                // Already set, just ensure Encrypt is set
                if (!connectionString.Contains("Encrypt", StringComparison.OrdinalIgnoreCase))
                {
                    connectionString += ";Encrypt=false";
                }
                return connectionString;
            }

            // Add TrustServerCertificate and Encrypt settings
            var separator = connectionString.EndsWith(";") ? "" : ";";
            connectionString += $"{separator}TrustServerCertificate=true;Encrypt=false";

            return connectionString;
        }
    }
}

