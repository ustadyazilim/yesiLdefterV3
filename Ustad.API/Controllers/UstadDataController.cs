using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Ustad.API.Classes;
using Ustad.API.Models;
using Ustad.API.Variables;
using Ustad.API.ToolBox;
using QuickType;
using Newtonsoft.Json;

namespace Ustad.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UstadDataController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly tVariables v = new tVariables();
        private tTools t = new tTools();
        private tQueryAbout _queryAbout = new tQueryAbout();
        public MsTablesIp _ms = new MsTablesIp();
        public UstadDataController(IConfiguration configration)
        {
            _configuration = configration;
        }

        [HttpGet]
        [Route("Get")]
        public JsonResult Get()
        {
            string key = "";
            string value = "";
            string tableIPCode = "";

            if (Request.Query.Count == 0)
                return new JsonResult("Lütfen parametre giriniz... ( UstadData/Get?TableIPCode=xxx/yyy/TableCode.IPCode ) ");

            foreach (var item in Request.Query)
            {
                key = item.Key;
                value = item.Value;

                if (key.ToUpper() == "TABLEIPCODE")
                    tableIPCode = value;
            }

            if (tableIPCode == "")
                return new JsonResult("Lütfen TableIPCode yi kontrol ediniz...");

            // Simplified query for existing tables
            string query = "";

            // Clean and normalize the tableIPCode with Turkish character handling
            string normalizedTableIPCode = tableIPCode?.Trim().ToUpper() ?? "";

            // Convert Turkish characters to English equivalents
            normalizedTableIPCode = normalizedTableIPCode
                .Replace("İ", "I")
                .Replace("Ğ", "G")
                .Replace("Ü", "U")
                .Replace("Ş", "S")
                .Replace("Ö", "O")
                .Replace("Ç", "C");

            if (normalizedTableIPCode == "UST/OMS/HMALIYET.MALIYET_L04")
            {
                query = @"
                    SELECT 
                        UserId,
                        UserFullName,
                        UserEMail,
                        FirmGUID,
                        RecordDate
                    FROM UstadUsers 
                    WHERE FirmGUID IS NOT NULL
                    ORDER BY UserFullName
                ";
            }
            else
            {
                return new JsonResult($"NEW CODE RUNNING - TableIPCode '{tableIPCode}' not supported. Available: UST/OMS/HMaliyet.Maliyet_L04");
            }

            _queryAbout.Clear();
            _queryAbout.SqlDataSource = _configuration.GetConnectionString(v.dbCrm);
            _queryAbout.QuerySql = query;

            JsonResult result;
            if (!string.IsNullOrEmpty(query))
            {
                result = t.RunQueryJson(_queryAbout);
            }
            else
            {
                result = new JsonResult(tableIPCode + " için SQL cümlesi yok..");
            }

            return result;
        }

        [HttpGet]
        [Route("")]
        public JsonResult GetDefault()
        {
            return new JsonResult("Merhaba. Data talebiniz için hazırım...");
        }

    }


}