using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using Ustad.API.Variables;

namespace Ustad.API.Controllers
{
    [ApiController]
    [Route("api/core")]
    public class CoreController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly tVariables v = new tVariables();

        public CoreController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Resolves database name from firmGUID
        /// Used by Go API for tenant resolution
        /// </summary>
        [HttpGet("tenant/resolve")]
        public IActionResult ResolveTenant([FromQuery] string firmGUID)
        {
            if (string.IsNullOrEmpty(firmGUID))
            {
                return BadRequest(new { error = "firmGUID is required" });
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

                return Ok(new { dbName = dbName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to resolve tenant", details = ex.Message });
            }
        }
    }
}