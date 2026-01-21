using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Ustad.API.Classes;
using Ustad.API.Variables;

namespace Ustad.API.Controllers
{
    /// <summary>
    /// Tenant management controller for resolving firm GUIDs to database names
    /// Used by Go API for tenant database connection resolution
    /// </summary>
    [ApiController]
    [Route("api/core/tenant")]
    public class TenantController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TenantController> _logger;
        private readonly tVariables v = new tVariables();

        public TenantController(IConfiguration configuration, ILogger<TenantController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Resolves firm GUID to database name
        /// Used by Go API to determine which tenant database to connect to
        /// </summary>
        /// <param name="firmGUID">Firm GUID identifier</param>
        /// <returns>Database name for the firm</returns>
        [HttpGet("resolve")]
        public IActionResult ResolveTenant([FromQuery] string firmGUID)
        {
            if (string.IsNullOrWhiteSpace(firmGUID))
            {
                _logger.LogWarning("[TenantController] ResolveTenant called with empty firmGUID");
                return BadRequest(new { error = "firmGUID is required" });
            }

            try
            {
                _logger.LogInformation("[TenantController] Resolving tenant for firmGUID: {FirmGUID}", firmGUID);

                string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);

                // Query UstadFirms table to get DatabaseName from FirmGUID
                string query = @"
                    SELECT TOP 1 
                        DatabaseName
                    FROM UstadFirms 
                    WHERE FirmGUID = @FirmGUID
                        AND IsActive = 1
                ";

                string dbName = null;

                using (SqlConnection connection = new SqlConnection(sqlDataSource))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirmGUID", firmGUID);

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            dbName = result.ToString();
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(dbName))
                {
                    _logger.LogWarning("[TenantController] No database found for firmGUID: {FirmGUID}", firmGUID);
                    return NotFound(new { error = $"No active database found for firmGUID: {firmGUID}" });
                }

                _logger.LogInformation("[TenantController] Resolved firmGUID {FirmGUID} to database {DbName}", firmGUID, dbName);

                return Ok(new { dbName = dbName });
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "[TenantController] Database error resolving firmGUID: {FirmGUID}", firmGUID);
                return StatusCode(500, new { error = "Database error occurred", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TenantController] Unexpected error resolving firmGUID: {FirmGUID}", firmGUID);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}