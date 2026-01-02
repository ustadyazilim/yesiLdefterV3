using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Ustad.API.Services
{
    /// <summary>
    /// Service for logging activity timeline entries
    /// </summary>
    public class ActivityTimelineService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ActivityTimelineService> _logger;

        public ActivityTimelineService(IConfiguration configuration, ILogger<ActivityTimelineService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Builds database connection string from environment variables or configuration
        /// </summary>
        private string BuildConnectionString()
        {
            string host = Environment.GetEnvironmentVariable("DB_HOST") ?? _configuration["Db:Host"];
            string port = Environment.GetEnvironmentVariable("DB_PORT") ?? _configuration["Db:Port"];
            string user = Environment.GetEnvironmentVariable("DB_USER") ?? _configuration["Db:User"];
            string pass = Environment.GetEnvironmentVariable("DB_PASS") ?? _configuration["Db:Pass"];
            string db = Environment.GetEnvironmentVariable("DB_NAME") ?? _configuration["Db:Name"];

            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("Database host environment variable or Db:Host configuration is required");
            if (string.IsNullOrWhiteSpace(port))
                throw new InvalidOperationException("Database port environment variable or Db:Port configuration is required");
            if (string.IsNullOrWhiteSpace(user))
                throw new InvalidOperationException("Database user environment variable or Db:User configuration is required");
            if (string.IsNullOrWhiteSpace(db))
                throw new InvalidOperationException("Database name environment variable or Db:Name configuration is required");
            if (string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("Database password environment variable or Db:Pass configuration is required");
            
            return $"Data Source={host},{port}; Initial Catalog={db}; User ID={user}; Password={pass}; TrustServerCertificate=true; Encrypt=false; MultipleActiveResultSets=True";
        }

        /// <summary>
        /// Logs e-src.net external data sync result to activity timeline
        /// Uses MtskAdayTakip table if available, otherwise creates entry in ActivityTimeline table
        /// </summary>
        /// <param name="studentId">Student ID (AdayId)</param>
        /// <param name="status">Sync status ("success" or "failure")</param>
        /// <param name="message">Human-readable message</param>
        /// <param name="metadata">Additional metadata (JSON string or plain text)</param>
        public async Task LogESrcExternalDataSyncAsync(int studentId, string status, string message, string? metadata = null)
        {
            try
            {
                string connStr = BuildConnectionString();
                using var con = new SqlConnection(connStr);
                await con.OpenAsync();

                // First, try to use MtskAdayTakip table if it exists
                // Otherwise, create/use ActivityTimeline table
                string createTableQuery = @"
                    IF OBJECT_ID('ActivityTimeline') IS NULL
                    BEGIN
                        CREATE TABLE ActivityTimeline (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            AdayId INT NOT NULL,
                            ActionType NVARCHAR(50) NOT NULL,
                            Status NVARCHAR(20) NOT NULL,
                            Message NVARCHAR(500) NOT NULL,
                            Metadata NVARCHAR(MAX) NULL,
                            RecordDate DATETIME2 DEFAULT SYSUTCDATETIME(),
                            FOREIGN KEY (AdayId) REFERENCES MtskAday(Id)
                        );
                        CREATE INDEX IX_ActivityTimeline_AdayId ON ActivityTimeline(AdayId);
                        CREATE INDEX IX_ActivityTimeline_RecordDate ON ActivityTimeline(RecordDate);
                    END";

                using (var createCmd = con.CreateCommand())
                {
                    createCmd.CommandText = createTableQuery;
                    await createCmd.ExecuteNonQueryAsync();
                }

                // Insert activity log entry
                string insertQuery = @"
                    INSERT INTO ActivityTimeline (AdayId, ActionType, Status, Message, Metadata, RecordDate)
                    VALUES (@adayId, @actionType, @status, @message, @metadata, SYSUTCDATETIME())";

                using var cmd = con.CreateCommand();
                cmd.CommandText = insertQuery;
                cmd.Parameters.AddWithValue("@adayId", studentId);
                cmd.Parameters.AddWithValue("@actionType", "esrc-external-data-sync");
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@message", message);
                cmd.Parameters.AddWithValue("@metadata", (object?)metadata ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();

                _logger.LogInformation(
                    "[ActivityTimelineService] Logged e-src.net external data sync activity for student {StudentId}: {Status} - {Message}",
                    studentId, status, message);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - activity logging should not break the sync flow
                _logger.LogError(ex,
                    "[ActivityTimelineService] Error logging activity for student {StudentId}: {Error}",
                    studentId, ex.Message);
            }
        }
    }
}

