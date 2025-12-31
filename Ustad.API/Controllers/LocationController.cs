using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Ustad.API.Models;
using Ustad.API.Variables;
using Microsoft.Data.SqlClient;

namespace Ustad.API.Controllers
{
    /// <summary>
    /// Location Controller
    /// Provides endpoints for province (İl) and district (İlçe) lookups
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        #region Private Fields
        private readonly IConfiguration _configuration;
        private readonly ILogger<LocationController> _logger;
        private readonly tVariables v = new tVariables();
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the LocationController
        /// </summary>
        public LocationController(IConfiguration configuration, ILogger<LocationController> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        #endregion

        #region SQL Query Constants
        private const string GET_ALL_PROVINCES_QUERY = @"
            SELECT 
                Id,
                IlKodu,
                IlAdiBUYUK,
                IlAdiKucuk,
                IlGIBKodu,
                IlKoduSifirli
            FROM ILTipi
            WHERE IlKodu > 0
            ORDER BY IlKodu";

        private const string GET_PROVINCE_BY_CODE_QUERY = @"
            SELECT 
                Id,
                IlKodu,
                IlAdiBUYUK,
                IlAdiKucuk,
                IlGIBKodu,
                IlKoduSifirli
            FROM ILTipi
            WHERE IlKodu = @IlKodu";

        private const string GET_DISTRICTS_BY_PROVINCE_QUERY = @"
            SELECT 
                d.Id,
                d.IlKodu,
                d.DistrictKodu,
                d.DistrictAdi
            FROM UstadFirmsDistrictType d
            WHERE d.IlKodu = @IlKodu
            ORDER BY d.DistrictAdi";

        private const string GET_ALL_DISTRICTS_QUERY = @"
            SELECT 
                d.Id,
                d.IlKodu,
                d.DistrictKodu,
                d.DistrictAdi
            FROM UstadFirmsDistrictType d
            WHERE d.IlKodu > 0
            ORDER BY d.IlKodu, d.DistrictAdi";

        private const string GET_DISTRICT_BY_ID_QUERY = @"
            SELECT 
                d.Id,
                d.IlKodu,
                d.DistrictKodu,
                d.DistrictAdi
            FROM UstadFirmsDistrictType d
            WHERE d.Id = @Id";

        private const string GET_DISTRICTS_WITH_PROVINCE_QUERY = @"
            SELECT 
                d.Id,
                d.IlKodu,
                d.DistrictKodu,
                d.DistrictAdi,
                p.IlAdiKucuk AS ProvinceName
            FROM UstadFirmsDistrictType d
            LEFT JOIN ILTipi p ON d.IlKodu = p.IlKodu
            WHERE d.IlKodu = @IlKodu
            ORDER BY d.DistrictAdi";
        #endregion

        #region Public API Methods - Provinces

        /// <summary>
        /// GET /api/location/provinces
        /// Gets all provinces (İller)
        /// </summary>
        /// <returns>List of all provinces</returns>
        [HttpGet("provinces")]
        public async Task<ActionResult<List<ProvinceResponse>>> GetProvinces()
        {
            try
            {
                _logger.LogInformation("Getting all provinces");

                var provinces = new List<ProvinceResponse>();
                string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);

                using (var connection = new SqlConnection(sqlDataSource))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(GET_ALL_PROVINCES_QUERY, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            provinces.Add(new ProvinceResponse
                            {
                                Id = reader.GetInt32("Id"),
                                ProvinceCode = reader.GetInt32("IlKodu"),
                                Name = reader.IsDBNull("IlAdiKucuk") ? string.Empty : reader.GetString("IlAdiKucuk"),
                                NameUppercase = reader.IsDBNull("IlAdiBUYUK") ? string.Empty : reader.GetString("IlAdiBUYUK"),
                                GIBCode = reader.IsDBNull("IlGIBKodu") ? null : reader.GetString("IlGIBKodu"),
                                ProvinceCodePadded = reader.IsDBNull("IlKoduSifirli") ? null : reader.GetString("IlKoduSifirli")
                            });
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} provinces", provinces.Count);
                return Ok(provinces);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provinces");
                return StatusCode(500, new { error = "Failed to retrieve provinces", message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/location/provinces/{provinceCode}
        /// Gets a specific province by code
        /// </summary>
        /// <param name="provinceCode">Province code (1-81)</param>
        /// <returns>Province information</returns>
        [HttpGet("provinces/{provinceCode}")]
        public async Task<ActionResult<ProvinceResponse>> GetProvinceByCode(int provinceCode)
        {
            try
            {
                _logger.LogInformation("Getting province with code: {ProvinceCode}", provinceCode);

                if (provinceCode <= 0 || provinceCode > 81)
                {
                    return BadRequest(new { error = "Invalid province code. Must be between 1 and 81." });
                }

                string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
                ProvinceResponse? province = null;

                using (var connection = new SqlConnection(sqlDataSource))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(GET_PROVINCE_BY_CODE_QUERY, connection))
                    {
                        command.Parameters.AddWithValue("@IlKodu", provinceCode);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                province = new ProvinceResponse
                                {
                                    Id = reader.GetInt32("Id"),
                                    ProvinceCode = reader.GetInt32("IlKodu"),
                                    Name = reader.IsDBNull("IlAdiKucuk") ? string.Empty : reader.GetString("IlAdiKucuk"),
                                    NameUppercase = reader.IsDBNull("IlAdiBUYUK") ? string.Empty : reader.GetString("IlAdiBUYUK"),
                                    GIBCode = reader.IsDBNull("IlGIBKodu") ? null : reader.GetString("IlGIBKodu"),
                                    ProvinceCodePadded = reader.IsDBNull("IlKoduSifirli") ? null : reader.GetString("IlKoduSifirli")
                                };
                            }
                        }
                    }
                }

                if (province == null)
                {
                    _logger.LogWarning("Province with code {ProvinceCode} not found", provinceCode);
                    return NotFound(new { error = "Province not found" });
                }

                return Ok(province);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting province with code: {ProvinceCode}", provinceCode);
                return StatusCode(500, new { error = "Failed to retrieve province", message = ex.Message });
            }
        }
        #endregion

        #region Public API Methods - Districts

        /// <summary>
        /// GET /api/location/districts
        /// Gets all districts (İlçeler)
        /// </summary>
        /// <param name="provinceCode">Optional province code to filter districts</param>
        /// <returns>List of districts</returns>
        [HttpGet("districts")]
        public async Task<ActionResult<List<DistrictResponse>>> GetDistricts([FromQuery] int? provinceCode = null)
        {
            try
            {
                _logger.LogInformation("Getting districts {Filter}", provinceCode.HasValue ? $"for province: {provinceCode}" : "(all)");

                var districts = new List<DistrictResponse>();
                string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
                string query = provinceCode.HasValue ? GET_DISTRICTS_BY_PROVINCE_QUERY : GET_ALL_DISTRICTS_QUERY;

                using (var connection = new SqlConnection(sqlDataSource))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    {
                        if (provinceCode.HasValue)
                        {
                            command.Parameters.AddWithValue("@IlKodu", provinceCode.Value);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                districts.Add(new DistrictResponse
                                {
                                    Id = reader.GetInt32("Id"),
                                    ProvinceCode = reader.GetInt32("IlKodu"),
                                    DistrictCode = reader.GetInt32("DistrictKodu"),
                                    Name = reader.IsDBNull("DistrictAdi") ? string.Empty : reader.GetString("DistrictAdi")
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} districts", districts.Count);
                return Ok(districts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting districts");
                return StatusCode(500, new { error = "Failed to retrieve districts", message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/location/districts/{districtId}
        /// Gets a specific district by ID
        /// </summary>
        /// <param name="districtId">District ID</param>
        /// <returns>District information</returns>
        [HttpGet("districts/{districtId}")]
        public async Task<ActionResult<DistrictResponse>> GetDistrictById(int districtId)
        {
            try
            {
                _logger.LogInformation("Getting district with ID: {DistrictId}", districtId);

                if (districtId <= 0)
                {
                    return BadRequest(new { error = "Invalid district ID" });
                }

                string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
                DistrictResponse? district = null;

                using (var connection = new SqlConnection(sqlDataSource))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(GET_DISTRICT_BY_ID_QUERY, connection))
                    {
                        command.Parameters.AddWithValue("@Id", districtId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                district = new DistrictResponse
                                {
                                    Id = reader.GetInt32("Id"),
                                    ProvinceCode = reader.GetInt32("IlKodu"),
                                    DistrictCode = reader.GetInt32("DistrictKodu"),
                                    Name = reader.IsDBNull("DistrictAdi") ? string.Empty : reader.GetString("DistrictAdi")
                                };
                            }
                        }
                    }
                }

                if (district == null)
                {
                    _logger.LogWarning("District with ID {DistrictId} not found", districtId);
                    return NotFound(new { error = "District not found" });
                }

                return Ok(district);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting district with ID: {DistrictId}", districtId);
                return StatusCode(500, new { error = "Failed to retrieve district", message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/location/provinces/{provinceCode}/districts
        /// Gets all districts for a specific province with province information
        /// </summary>
        /// <param name="provinceCode">Province code</param>
        /// <returns>List of districts with province information</returns>
        [HttpGet("provinces/{provinceCode}/districts")]
        public async Task<ActionResult<List<DistrictWithProvinceResponse>>> GetDistrictsByProvince(int provinceCode)
        {
            try
            {
                _logger.LogInformation("Getting districts for province: {ProvinceCode}", provinceCode);

                if (provinceCode <= 0 || provinceCode > 81)
                {
                    return BadRequest(new { error = "Invalid province code. Must be between 1 and 81." });
                }

                var districts = new List<DistrictWithProvinceResponse>();
                string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);

                using (var connection = new SqlConnection(sqlDataSource))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(GET_DISTRICTS_WITH_PROVINCE_QUERY, connection))
                    {
                        command.Parameters.AddWithValue("@IlKodu", provinceCode);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                districts.Add(new DistrictWithProvinceResponse
                                {
                                    Id = reader.GetInt32("Id"),
                                    DistrictCode = reader.GetInt32("DistrictKodu"),
                                    DistrictName = reader.IsDBNull("DistrictAdi") ? string.Empty : reader.GetString("DistrictAdi"),
                                    ProvinceCode = reader.GetInt32("IlKodu"),
                                    ProvinceName = reader.IsDBNull("ProvinceName") ? string.Empty : reader.GetString("ProvinceName")
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} districts for province {ProvinceCode}", districts.Count, provinceCode);
                return Ok(districts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting districts for province: {ProvinceCode}", provinceCode);
                return StatusCode(500, new { error = "Failed to retrieve districts", message = ex.Message });
            }
        }
        #endregion
    }
}

