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
    public class UstadFirmController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly tVariables v = new tVariables();

        public UstadFirmController(IConfiguration configration)
        {
            _configuration = configration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            // Get all firms (distinct FirmGUIDs from UstadUsers)
            string query = @"
                SELECT DISTINCT 
                    FirmGUID,
                    COUNT(*) as UserCount,
                    MIN(RecordDate) as CreatedDate,
                    MAX(RecordDate) as LastActivity
                FROM UstadUsers 
                WHERE FirmGUID IS NOT NULL AND FirmGUID != ''
                GROUP BY FirmGUID
                ORDER BY UserCount DESC
            ";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);
        }

        [HttpGet("{firmId}")]
        public JsonResult GetFirm(string firmId)
        {
            // Use proper UstadFirmsUsers table with joins for better performance and security
            string query = @"
                SELECT 
                    [UstadFirmsUsers].[Id],
                    [UstadFirmsUsers].[IsActive],
                    [UstadFirmsUsers].[FirmId],
                    [UstadFirmsUsers].[UserId],
                    [uf].FirmLongName as Lkp_FirmLongName,
                    [uu].UserFullName as Lkp_UserFullName,
                    [uu].UserEMail,
                    [uu].UserGUID,
                    [uf].FirmGUID
                FROM [dbo].[UstadFirmsUsers] as [UstadFirmsUsers]
                LEFT OUTER JOIN UstadFirms as [uf] ON ([UstadFirmsUsers].FirmId = [uf].FirmId)
                LEFT OUTER JOIN UstadUsers as [uu] ON ([UstadFirmsUsers].UserId = [uu].UserId)
                WHERE [UstadFirmsUsers].FirmGUID = @FirmId
                AND [UstadFirmsUsers].IsActive = 1
                ORDER BY [uu].UserFullName
            ";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@FirmId", firmId);
                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        table.Load(myReader);
                    }
                }
            }

            // Group by firm and create response
            var firmData = table.Rows.Cast<DataRow>().GroupBy(row => row["FirmGUID"]).FirstOrDefault();

            if (firmData == null)
            {
                return new JsonResult(new { error = "Firm not found" });
            }

            var response = new
            {
                Firm = new
                {
                    FirmId = firmData.First()["FirmId"], // Integer ID
                    FirmGUID = firmData.Key, // String GUID
                    FirmLongName = firmData.First()["Lkp_FirmLongName"],
                    UserCount = firmData.Count(),
                    Users = firmData.Select(row => new
                    {
                        Id = row["Id"],
                        UserId = row["UserId"], // Integer ID
                        UserGUID = row["UserGUID"], // String GUID
                        UserFullName = row["Lkp_UserFullName"],
                        UserEMail = row["UserEMail"],
                        IsActive = row["IsActive"]
                    }).ToArray()
                }
            };

            return new JsonResult(response);
        }

        [HttpGet("user/{userGUID}")]
        public JsonResult GetUserFirms(string userGUID)
        {
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);

            // First, try to get firms from UstadFirmsUsers table (new method)
            string query = @"
 Select 
   [UstadFirmsUsers].[Id]
 , [UstadFirmsUsers].[IsActive]
 , [UstadFirmsUsers].[FirmId]
 , [UstadFirmsUsers].[FirmGUID]
 , [UstadFirmsUsers].[UserId]
 , [UstadFirmsUsers].[UserGUID]
 -- JoinTables standart output fields 
 , [uf].FirmShortName
 , [uf].FirmLongName
 , [uf].MenuCode
 , [uf].SectorTypeId
 , [uf].DatabaseName
 , [uf].ServerNameIP
 , [uf].DbLoginName
 , [uf].DbPass
 , [uf].DbTypeId
 , [uf].DistrictTypeId
 , [uf].CityTypeId
 , [uf].MebbisCode
 , [uf].MebbisPass
 , [uf].FirmLongName as Lkp_FirmLongName 
 , [uu].UserFullName as Lkp_UserFullName 
 -- JoinTables case output fields 
 From [dbo].[UstadFirmsUsers] as [UstadFirmsUsers]
    left outer join UstadFirms as [uf] on ( [UstadFirmsUsers].FirmId = [uf].FirmId ) 
    left outer join UstadUsers as [uu] on ( [UstadFirmsUsers].UserId = [uu].UserId ) 

 Where 0 = 0 
 and [UstadFirmsUsers].UserGUID = @UserGUID
            ";

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

            // NOTE: Fallback to UstadUsers.FirmGUID disabled
            // Users should only access firms explicitly assigned in UstadFirmsUsers
            // If no firms found, return empty array (don't use legacy FirmGUID from UstadUsers)
            
            // If no firms found in UstadFirmsUsers, return empty array
            if (table.Rows.Count == 0)
            {
                // Return empty array - user has no firms assigned
                return new JsonResult(new object[0]);
            }

            // Return firms from UstadFirmsUsers table
            var firmsFromTable = table.Rows.Cast<DataRow>().Select(row => new
            {
            Id = row["Id"],
            FirmId = row["FirmId"],
            FirmGUID = row["FirmGUID"],
            FirmLongName = row["Lkp_FirmLongName"],
            UserId = row["UserId"],
            UserGUID = row["UserGUID"],
            UserFullName = row["Lkp_UserFullName"],
            IsActive = row["IsActive"],
            FirmShortName = row["FirmShortName"],
            MenuCode = row["MenuCode"],
            SectorTypeId = row["SectorTypeId"],
            DatabaseName = row["DatabaseName"],
            ServerNameIP = row["ServerNameIP"],
            DbLoginName = row["DbLoginName"],
            DbPass = row["DbPass"],
            DbTypeId = row["DbTypeId"],
            DistrictTypeId = row["DistrictTypeId"],
            CityTypeId = row["CityTypeId"],
            MebbisCode = row["MebbisCode"],
            MebbisPass = row["MebbisPass"]
            }).ToArray();

            return new JsonResult(firmsFromTable);
        }

        [HttpGet("{firmId}/users")]
        public JsonResult GetUsersByFirm(string firmId)
        {
            // Get users in specific firm
            string query = @"
                SELECT 
                    UserId,
                    UserFullName,
                    UserFirstName,
                    UserLastName,
                    UserEMail,
                    UserKey,
                    convert(varchar(10), RecordDate, 120) as RecordDate,
                    UserGUID,
                    FirmGUID
                FROM UstadUsers 
                WHERE FirmGUID = @FirmId
                ORDER BY UserFullName
            ";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@FirmId", firmId);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);
        }

        [HttpPost]
        public JsonResult Post(UstadUser user)
        {
            string query = @"
               Insert into UstadUsers 
               values (@UserFirstName, @UserLastName)
            ";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserFirstName", user.UserFirstName);
                    myCommand.Parameters.AddWithValue("@UserLastName", user.UserLastName);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);
        }

        [HttpPut]
        public JsonResult Put(UstadUser user)
        {
            string query = @"
               Update UstadUsers set
               UserFirstName = @UserFirstName
             , UserLastName = @UserLastName
               Where UserGUID = @UserGUID ";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserFirstName", user.UserFirstName);
                    myCommand.Parameters.AddWithValue("@UserLastName", user.UserLastName);
                    myCommand.Parameters.AddWithValue("@UserGUID", user.UserGUID);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Update successfully");
        }

        [HttpDelete]
        public JsonResult Delete(UstadUser user)
        {
            string query = @"
               Delete UstadUsers 
               Where UserGUID = @UserGUID ";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString(v.dbCrm);
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@UserGUID", user.UserGUID);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Delete successfully");
        }


    }
}