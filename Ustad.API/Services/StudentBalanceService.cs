using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Ustad.API.Services
{
    /// <summary>
    /// Service for calculating student balance from payment history
    /// </summary>
    public class StudentBalanceService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StudentBalanceService> _logger;

        public StudentBalanceService(IConfiguration configuration, ILogger<StudentBalanceService> logger)
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
        /// Calculates student balance from payment history
        /// Sums all payments from OnmOdemePlani table for the student
        /// </summary>
        /// <param name="studentId">Student ID (AdayId)</param>
        /// <returns>Balance as decimal (0 if no payments found)</returns>
        public async Task<decimal> CalculateBalanceAsync(int studentId)
        {
            try
            {
                string connStr = BuildConnectionString();
                using var con = new SqlConnection(connStr);
                await con.OpenAsync();

                // Query to calculate balance from payment plan table
                // Consider both paid and unpaid amounts
                // Balance = Total amount - Paid amount (or just total if all unpaid)
                string query = @"
                    SELECT 
                        COALESCE(SUM(
                            CASE 
                                WHEN op.OdemeTarihi IS NOT NULL THEN op.Tutar
                                ELSE 0
                            END
                        ), 0) AS PaidAmount,
                        COALESCE(SUM(op.Tutar), 0) AS TotalAmount
                    FROM OnmOdemePlani op
                    WHERE op.AdayId = @studentId
                      AND op.IsActive = 1";

                using var cmd = con.CreateCommand();
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@studentId", studentId);

                using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
                if (await reader.ReadAsync())
                {
                    decimal totalAmount = reader.IsDBNull("TotalAmount") ? 0 : reader.GetDecimal("TotalAmount");
                    decimal paidAmount = reader.IsDBNull("PaidAmount") ? 0 : reader.GetDecimal("PaidAmount");
                    
                    // Balance = Total - Paid (positive means outstanding, negative means overpaid)
                    decimal balance = totalAmount - paidAmount;

                    _logger.LogInformation(
                        "[StudentBalanceService] Calculated balance for student {StudentId}: {Balance} (Total: {Total}, Paid: {Paid})",
                        studentId, balance, totalAmount, paidAmount);

                    return balance;
                }

                _logger.LogInformation("[StudentBalanceService] No payments found for student {StudentId}, returning 0", studentId);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StudentBalanceService] Error calculating balance for student ID: {StudentId}", studentId);
                // Return 0 on error to prevent blocking sync
                return 0;
            }
        }
    }
}

