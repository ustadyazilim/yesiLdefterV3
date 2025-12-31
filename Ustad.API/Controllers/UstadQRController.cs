using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ustad.API.Models;
using Ustad.API.Variables;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Ustad.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UstadQRController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly tVariables v = new tVariables();

        public UstadQRController(IConfiguration configration)
        {
            _configuration = configration;
        }

        /// <summary>
        /// Generate QR code data for a specific user and firm
        /// </summary>
        /// <param name="firmGUID">Firm GUID</param>
        /// <param name="userGUID">User GUID</param>
        /// <returns>QR code payload data</returns>
        [HttpGet("generate/{firmGUID}/{userGUID}")]
        public JsonResult GenerateQRCode(string firmGUID, string userGUID)
        {
            // Generate QR code data for firm selection
            string query = @"
                    SELECT 
                        [UstadFirmsUsers].[Id],
                        [UstadFirmsUsers].[IsActive],
                        [UstadFirmsUsers].[FirmId],
                        [UstadFirmsUsers].[FirmGUID],
                        [UstadFirmsUsers].[UserId],
                        [UstadFirmsUsers].[UserGUID],
                        [uf].FirmLongName as Lkp_FirmLongName,
                        [uu].UserFullName as Lkp_UserFullName,
                        [uu].UserFirstName,
                        [uu].UserLastName,
                        [uu].UserEMail,
                        [uu].UserKey as tcNoTelefonNo
                    FROM [dbo].[UstadFirmsUsers] as [UstadFirmsUsers]
                    LEFT OUTER JOIN UstadFirms as [uf] ON ([UstadFirmsUsers].FirmId = [uf].FirmId)
                    LEFT OUTER JOIN UstadUsers as [uu] ON ([UstadFirmsUsers].UserId = [uu].UserId)
                    WHERE [UstadFirmsUsers].FirmGUID = @FirmGUID
                    AND [UstadFirmsUsers].UserGUID = @UserGUID
            ";
                        
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
            
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@FirmGUID", firmGUID);
                    myCommand.Parameters.AddWithValue("@UserGUID", userGUID);
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        table.Load(myReader);
                    }
                }
            }

            if (table.Rows.Count == 0)
            {
                return new JsonResult(new { error = "User not found in this firm" });
            }

            var row = table.Rows[0];
            var qrPayload = new
            {
                firmGUID = row["FirmGUID"].ToString(),
                userGUID = row["UserGUID"].ToString(),
                tcNoTelefonNo = row["tcNoTelefonNo"].ToString(),
                userId = Convert.ToInt32(row["UserId"]),
                isActive = Convert.ToBoolean(row["IsActive"]),
                userFullName = row["Lkp_UserFullName"].ToString(),
                userFirstName = row["UserFirstName"].ToString(),
                userLastName = row["UserLastName"].ToString(),
                userEMail = row["UserEMail"].ToString()
            };

            return new JsonResult(qrPayload);
        }

        /// <summary>
        /// Get QR code data for all firms a user has access to
        /// </summary>
        /// <param name="userGUID">User GUID</param>
        /// <returns>List of QR code payloads for all accessible firms</returns>
        [HttpGet("user/{userGUID}")]
        public JsonResult GetUserQRCodes(string userGUID)
        {
            // Get all firms assigned to a specific user with QR data
            string query = @"
                SELECT 
                    [UstadFirmsUsers].[Id],
                    [UstadFirmsUsers].[IsActive],
                    [UstadFirmsUsers].[FirmId],
                    [UstadFirmsUsers].[FirmGUID],
                    [UstadFirmsUsers].[UserId],
                    [UstadFirmsUsers].[UserGUID],
                    [uf].FirmLongName as Lkp_FirmLongName,
                    [uu].UserFullName as Lkp_UserFullName,
                    [uu].UserFirstName,
                    [uu].UserLastName,
                    [uu].UserEMail,
                    [uu].UserKey as tcNoTelefonNo
                FROM [dbo].[UstadFirmsUsers] as [UstadFirmsUsers]
                LEFT OUTER JOIN UstadFirms as [uf] ON ([UstadFirmsUsers].FirmId = [uf].FirmId)
                LEFT OUTER JOIN UstadUsers as [uu] ON ([UstadFirmsUsers].UserId = [uu].UserId)
                WHERE [UstadFirmsUsers].UserGUID = @UserGUID
                AND [UstadFirmsUsers].IsActive = 1
                ORDER BY [uf].FirmLongName
            ";
                        
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
            
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserGUID", userGUID);
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        table.Load(myReader);
                    }
                }
            }

            var qrCodes = table.Rows.Cast<DataRow>().Select(row => new
            {
                firmGUID = row["FirmGUID"].ToString(),
                userGUID = row["UserGUID"].ToString(),
                tcNoTelefonNo = row["tcNoTelefonNo"].ToString(),
                userId = Convert.ToInt32(row["UserId"]),
                isActive = Convert.ToBoolean(row["IsActive"]),
                userFullName = row["Lkp_UserFullName"].ToString(),
                userFirstName = row["UserFirstName"].ToString(),
                userLastName = row["UserLastName"].ToString(),
                userEMail = row["UserEMail"].ToString(),
                firmLongName = row["Lkp_FirmLongName"].ToString()
            }).ToArray();

            return new JsonResult(qrCodes);
        }

        /// <summary>
        /// Validate QR code data
        /// </summary>
        /// <param name="qrData">QR code payload to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        public JsonResult ValidateQRCode([FromBody] QRCodePayload qrData)
        {
            if (qrData == null || string.IsNullOrEmpty(qrData.firmGUID) || string.IsNullOrEmpty(qrData.userGUID))
            {
                return new JsonResult(new { valid = false, error = "Invalid QR code data" });
            }

            // Validate the QR code data against database
            string query = @"
                SELECT 
                    [UstadFirmsUsers].[Id],
                    [UstadFirmsUsers].[IsActive],
                    [uf].FirmLongName as Lkp_FirmLongName,
                    [uu].UserFullName as Lkp_UserFullName
                FROM [dbo].[UstadFirmsUsers] as [UstadFirmsUsers]
                LEFT OUTER JOIN UstadFirms as [uf] ON ([UstadFirmsUsers].FirmId = [uf].FirmId)
                LEFT OUTER JOIN UstadUsers as [uu] ON ([UstadFirmsUsers].UserId = [uu].UserId)
                WHERE [UstadFirmsUsers].FirmGUID = @FirmGUID
                AND [UstadFirmsUsers].UserGUID = @UserGUID
                AND [UstadFirmsUsers].IsActive = 1
            ";
                        
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
            
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@FirmGUID", qrData.firmGUID);
                    myCommand.Parameters.AddWithValue("@UserGUID", qrData.userGUID);
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        table.Load(myReader);
                    }
                }
            }

            if (table.Rows.Count == 0)
            {
                return new JsonResult(new { valid = false, error = "QR code is invalid or expired" });
            }

            var row = table.Rows[0];
            var isValid = Convert.ToBoolean(row["IsActive"]);

            return new JsonResult(new 
            { 
                valid = isValid, 
                firmName = row["Lkp_FirmLongName"].ToString(),
                userFullName = row["Lkp_UserFullName"].ToString()
            });
        }
    }

    /// <summary>
    /// QR Code payload model
    /// </summary>
    public class QRCodePayload
    {
        public string firmGUID { get; set; } = string.Empty;
        public string userGUID { get; set; } = string.Empty;
        public string tcNoTelefonNo { get; set; } = string.Empty;
        public int userId { get; set; }
        public bool isActive { get; set; }
        public string userFullName { get; set; } = string.Empty;
        public string userFirstName { get; set; } = string.Empty;
        public string userLastName { get; set; } = string.Empty;
        public string userEMail { get; set; } = string.Empty;
    }
}
