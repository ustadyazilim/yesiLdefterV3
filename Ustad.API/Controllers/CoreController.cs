using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using Ustad.API.Variables;

namespace Ustad.API.Controllers
{
    /// <summary>
    /// Core platform endpoints.
    /// </summary>
    [ApiController]
    [Route("api/core")]
    public class CoreController : ControllerBase
    {
        /// <summary>
        /// Tenant resolution success payload.
        /// </summary>
        public class TenantResolveResponse
        {
            /// <summary>
            /// Resolved database name for the tenant.
            /// </summary>
            public string DbName { get; set; } = string.Empty;
        }

        /// <summary>
        /// Error payload returned by core endpoints.
        /// </summary>
        public class CoreErrorResponse
        {
            /// <summary>
            /// Machine-readable or user-facing error message.
            /// </summary>
            public string Error { get; set; } = string.Empty;

            /// <summary>
            /// Extra diagnostic details when available.
            /// </summary>
            public string? Details { get; set; }
        }

        private readonly IConfiguration _configuration;
        private readonly tVariables v = new tVariables();

        public CoreController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Resolves database name from firmGUID
        /// Used by Go API for tenant resolution.
        /// This route is legacy and kept for backward compatibility.
        /// </summary>
        /// <param name="firmGUID">Tenant firm GUID.</param>
        /// <returns>Resolved tenant database name.</returns>
        /// <response code="200">Tenant database name was resolved.</response>
        /// <response code="400">The request is invalid (missing firmGUID).</response>
        /// <response code="500">An internal error occurred while resolving tenant.</response>
        [HttpGet("tenant/resolve-legacy")]
        [Obsolete("Use GET /api/core/tenant/resolve instead.")]
        [ProducesResponseType(typeof(TenantResolveResponse), 200)]
        [ProducesResponseType(typeof(CoreErrorResponse), 400)]
        [ProducesResponseType(typeof(CoreErrorResponse), 500)]
        public IActionResult ResolveTenant([FromQuery] string firmGUID)
        {
            if (string.IsNullOrEmpty(firmGUID))
            {
                return BadRequest(new CoreErrorResponse { Error = "firmGUID is required" });
            }

            try
            {
                string query = @"
                    SELECT TOP 1 
                        DatabaseName,
                        ServerNameIP,
                        DbLoginName,
                        DbPass
                    FROM UstadFirms 
                    WHERE FirmGUID = @FirmGUID 
                    AND IsActive = 1
                ";

                string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
                string dbName = null;

                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("@FirmGUID", firmGUID);
                        using (SqlDataReader myReader = myCommand.ExecuteReader())
                        {
                            if (myReader.Read())
                            {
                                // Get DatabaseName, fallback to default if null
                                dbName = myReader["DatabaseName"] != DBNull.Value 
                                    ? myReader["DatabaseName"].ToString() 
                                    : _configuration.GetConnectionString(v.dbCrm)?.Split(';')
                                        .FirstOrDefault(s => s.Contains("Database=") || s.Contains("Initial Catalog="))
                                        ?.Split('=').LastOrDefault() 
                                    ?? "UstadCrmV1";
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(dbName))
                {
                    // Fallback to default database name from connection string
                    var defaultDb = _configuration.GetConnectionString(v.dbCrm);
                    if (defaultDb != null)
                    {
                        // Try to extract database name from connection string
                        var dbMatch = System.Text.RegularExpressions.Regex.Match(
                            defaultDb, 
                            @"(?:Database|Initial Catalog)=([^;]+)", 
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase
                        );
                        dbName = dbMatch.Success ? dbMatch.Groups[1].Value : "UstadCrmV1";
                    }
                    else
                    {
                        dbName = "UstadCrmV1";
                    }
                }

                return Ok(new TenantResolveResponse { DbName = dbName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CoreErrorResponse
                {
                    Error = "Failed to resolve tenant",
                    Details = ex.Message
                });
            }
        }
    }
}