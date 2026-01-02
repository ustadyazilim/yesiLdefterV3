using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;
using Ustad.API.Models;

namespace Ustad.API.Services
{
    /// <summary>
    /// Service for fetching student data from database
    /// </summary>
    public class StudentDataService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StudentDataService> _logger;

        public StudentDataService(IConfiguration configuration, ILogger<StudentDataService> logger)
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
        /// Fetches full student data from database by student ID
        /// </summary>
        /// <param name="studentId">Student ID (AdayId)</param>
        /// <returns>Student data model or null if not found</returns>
        public async Task<StudentDataModel?> GetStudentByIdAsync(int studentId)
        {
            try
            {
                string connStr = BuildConnectionString();
                using var con = new SqlConnection(connStr);
                await con.OpenAsync();

                // Query to fetch student data from MtskAday table
                // Join with related tables if needed for complete profile
                string query = @"
                    SELECT 
                        COALESCE(u.TcNo, '') AS TC,
                        COALESCE(u.Ad, '') AS ADI,
                        COALESCE(u.Soyad, '') AS SOYADI,
                        COALESCE(u.Email, '') AS EMAIL,
                        COALESCE(u.IL, '') AS IL,
                        COALESCE(u.ILCE, '') AS ILCE,
                        COALESCE(u.Adres, '') AS ADRES,
                        COALESCE(u.Resim, '') AS IMG,
                        CASE 
                            WHEN u.CinsiyetTipiId = 1 THEN 'E'
                            WHEN u.CinsiyetTipiId = 2 THEN 'K'
                            ELSE ''
                        END AS CINSIYET,
                        COALESCE(u.Telefon, '') AS GSM
                    FROM MtskAday u
                    WHERE u.Id = @studentId
                      AND u.IsActive = 1";

                using var cmd = con.CreateCommand();
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@studentId", studentId);

                using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
                if (!await reader.ReadAsync())
                {
                    _logger.LogWarning("[StudentDataService] Student not found: {StudentId}", studentId);
                    return null;
                }

                // Map database fields to StudentDataModel
                var studentData = new StudentDataModel
                {
                    TC = reader.IsDBNull("TC") ? string.Empty : reader.GetString("TC"),
                    ADI = reader.IsDBNull("ADI") ? string.Empty : reader.GetString("ADI"),
                    SOYADI = reader.IsDBNull("SOYADI") ? string.Empty : reader.GetString("SOYADI"),
                    EMAIL = reader.IsDBNull("EMAIL") ? string.Empty : reader.GetString("EMAIL"),
                    IL = reader.IsDBNull("IL") ? string.Empty : reader.GetString("IL"),
                    ILCE = reader.IsDBNull("ILCE") ? string.Empty : reader.GetString("ILCE"),
                    ADRES = reader.IsDBNull("ADRES") ? string.Empty : reader.GetString("ADRES"),
                    IMG = reader.IsDBNull("IMG") ? string.Empty : reader.GetString("IMG"),
                    CINSIYET = reader.IsDBNull("CINSIYET") ? string.Empty : reader.GetString("CINSIYET"),
                    GSM = reader.IsDBNull("GSM") ? string.Empty : reader.GetString("GSM"),
                    BELGE = string.Empty, // Will be set from related tables if needed
                    BAKIYE = null // Will be calculated separately
                };

                _logger.LogInformation("[StudentDataService] Successfully fetched student data: {StudentId}", studentId);
                return studentData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StudentDataService] Error fetching student data for ID: {StudentId}", studentId);
                throw;
            }
        }
    }
}

