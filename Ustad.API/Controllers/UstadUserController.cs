using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Ustad.API.Models;
using Ustad.API.Classes;
using Ustad.API.ToolBox;
using Ustad.API.Variables;


namespace Ustad.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UstadUserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly tVariables v = new tVariables();
        private tTools t = new tTools();
        private tQueryAbout _queryAbout = new tQueryAbout();

        public UstadUserController(IConfiguration configration)
        {
            _configuration = configration;
        }

        [HttpGet]
        //[Route("{controller}/{action}/{userEMail}/pass/{userPass}")]
        [Route("{action}/{userEMail}/pass/{userPass}")]
        public JsonResult Login(string userEMail, string userPass)
        {
            _queryAbout.Clear();
            //_queryAbout.Configuration = _configuration;
            //_queryAbout.ConnectionName = v.dbCrm;
            _queryAbout.SqlDataSource = _configuration.GetConnectionString(v.dbCrm);
            _queryAbout.QuerySql = Querys.UstadUserLoginSql(userEMail, userPass);
            /*            
            tParameter _param1 = new tParameter();
            _param1.ParameterName = "@EMail";
            _param1.ParameterValue = userEMail;
            _queryAbout.tParams.Add(_param1);

            tParameter _param2 = new tParameter();
            _param2.ParameterName = "@Password";
            _param2.ParameterValue = userPass;
            _queryAbout.tParams.Add(_param2);
            */
            JsonResult result = t.RunQueryJson(_queryAbout);

            return result;
        }



        [HttpGet]
        [Route("{action}")]
        public JsonResult Get()
        {
            string query = @"
               Select 
               UserId
             , UserFullName
             , UserFirstName
             , UserLastName
             , UserEMail 
             , UserKey
             --, convert(varchar(10), RecordDate, 120) as RecordDate 
             , UserGUID
               From UstadUsers 
            ";

            _queryAbout.Clear();
            _queryAbout.SqlDataSource = _configuration.GetConnectionString(v.dbCrm); 
            _queryAbout.QuerySql = query;
            JsonResult result = t.RunQueryJson(_queryAbout);
            return result;
        }

        [HttpGet]
        public JsonResult GetDefault()
        {
            return new JsonResult("Üstad Yazılım Ltd. Şti. Ankara/Türkiye");
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