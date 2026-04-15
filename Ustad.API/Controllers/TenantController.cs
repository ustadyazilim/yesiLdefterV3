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
        /// <summary>
        /// Tenant resolve success payload.
        /// </summary>
        public class TenantResolveResponse
        {
            /// <summary>
            /// Resolved tenant database name.
            /// </summary>
            public string DbName { get; set; } = string.Empty;
        }

        /// <summary>
        /// Error payload returned by tenant endpoints.
        /// </summary>
        public class TenantErrorResponse
        {
            /// <summary>
            /// Machine-readable or user-facing error.
            /// </summary>
            public string Error { get; set; } = string.Empty;

            /// <summary>
            /// Optional diagnostics.
            /// </summary>
            public string? Details { get; set; }
        }

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
        /// <response code="200">Tenant database was resolved.</response>
        /// <response code="400">The request is invalid (missing firmGUID).</response>
        /// <response code="404">No active tenant database was found for firmGUID.</response>
        /// <response code="500">A server or database error occurred.</response>
        [HttpGet("resolve")]
        [ProducesResponseType(typeof(TenantResolveResponse), 200)]
        [ProducesResponseType(typeof(TenantErrorResponse), 400)]
        [ProducesResponseType(typeof(TenantErrorResponse), 404)]
        [ProducesResponseType(typeof(TenantErrorResponse), 500)]
        public IActionResult ResolveTenant([FromQuery] string firmGUID)
        {
            if (string.IsNullOrWhiteSpace(firmGUID))
            {
                _logger.LogWarning("[TenantController] ResolveTenant called with empty firmGUID");
                return BadRequest(new TenantErrorResponse { Error = "firmGUID is required" });
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
                    return NotFound(new TenantErrorResponse { Error = $"No active database found for firmGUID: {firmGUID}" });
                }

                _logger.LogInformation("[TenantController] Resolved firmGUID {FirmGUID} to database {DbName}", firmGUID, dbName);

                return Ok(new TenantResolveResponse { DbName = dbName });
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "[TenantController] Database error resolving firmGUID: {FirmGUID}", firmGUID);
                return StatusCode(500, new TenantErrorResponse
                {
                    Error = "Database error occurred",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TenantController] Unexpected error resolving firmGUID: {FirmGUID}", firmGUID);
                return StatusCode(500, new TenantErrorResponse
                {
                    Error = "Internal server error",
                    Details = ex.Message
                });
            }
        }
    }
}