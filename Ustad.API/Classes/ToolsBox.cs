using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using Ustad.API.Variables;
using Ustad.API.Classes;
using QuickType;
using Newtonsoft.Json;

namespace Ustad.API.ToolBox
{

    public class tTools
    {
        private readonly tVariables v = new tVariables();
        private tQueryAbout _queryAbout = new tQueryAbout();

        public tTools()
        {
            //
        }

        public JsonResult RunQueryJson(tQueryAbout queryAbout)
        {

            DataTable table = new DataTable();
            //string sqlDataSource = queryAbout.Configuration.GetConnectionString(queryAbout.ConnectionName);
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(queryAbout.SqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(queryAbout.QuerySql, myCon))
                {
                    foreach (tParameter item in queryAbout.tParams)
                    {
                        myCommand.Parameters.AddWithValue(item.ParameterName, item.ParameterValue);
                    }
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(table);
        }

        public DataTable RunQueryTable(tQueryAbout queryAbout)
        {
            DataTable table = new DataTable();
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(queryAbout.SqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(queryAbout.QuerySql, myCon))
                {
                    foreach (tParameter item in queryAbout.tParams)
                    {
                        myCommand.Parameters.AddWithValue(item.ParameterName, item.ParameterValue);
                    }
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return table;
        }

        public List<MsTablesIp> ReadMsTablesIP(string tableIPCode, IConfiguration configration)
        {
            _queryAbout.Clear();
            _queryAbout.SqlDataSource = configration.GetConnectionString(v.dbManager);
            _queryAbout.QuerySql = Querys.MsTableIPSql(tableIPCode);
            DataTable dataTable = RunQueryTable(_queryAbout);

            // DataTable ile okunan datayı model class a atayalım 
            string json = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            List<MsTablesIp> packet = JsonConvert.DeserializeObject<List<MsTablesIp>>(json);

            return packet;
            // ---
        }

        public string GetTableIPCodeSql(string tableIPCode, IConfiguration configration)
        {
            string query = Querys.MsTableIPQuery(tableIPCode);

            _queryAbout.Clear();
            _queryAbout.SqlDataSource = configration.GetConnectionString(v.dbManager);
            _queryAbout.QuerySql = query;
            DataTable table = RunQueryTable(_queryAbout);

            string sql = "";

            if (table != null)
                if (table.Rows != null)
                    if (table.Columns != null)
                        sql = PreparingSql(table.Rows[0][0].ToString());

            return sql;
        }


        private string PreparingSql(string Sql)
        {
            Sql = Str_AntiCheck(Sql);
            Sql = Str_CheckYilAy(Sql);

            Str_Replace(ref Sql, ":VT_FIRM_ID", "21");// v.SP_FIRM_ID.ToString());
            Str_Replace(ref Sql, ":FIRM_ID", "21");// v.SP_FIRM_ID.ToString());

            Str_Replace(ref Sql, ":BUGUN_YIL", v.BUGUN_YIL.ToString());

            /*
            Str_Replace(ref Sql, ":VT_COMP_ID", v.tComp.SP_COMP_ID.ToString());
            Str_Replace(ref Sql, ":VT_PERIOD_ID", v.vt_PERIOD_ID.ToString());
            Str_Replace(ref Sql, ":VT_USER_ID", v.tUser.UserId.ToString());
            Str_Replace(ref Sql, ":USER_ID", v.tUser.UserId.ToString());
            Str_Replace(ref Sql, ":USER_TCNO", "'" + v.tUser.UserTcNo + "'");
            Str_Replace(ref Sql, ":USER_DBTYPE_ID", v.tUser.UserDbTypeId.ToString());

            Str_Replace(ref Sql, ":BUGUN_YILAY", v.BUGUN_YILAY.ToString());
            Str_Replace(ref Sql, ":BUGUN_GUN", v.BUGUN_GUN.ToString());
            Str_Replace(ref Sql, ":BUGUN_AY", v.BUGUN_AY.ToString());
            Str_Replace(ref Sql, ":BUGUN_YIL", v.BUGUN_YIL.ToString());

            Str_Replace(ref Sql, ":GECEN_YILAY", v.GECEN_YILAY.ToString());
            Str_Replace(ref Sql, ":GELECEL_YILAY", v.GELECEK_YILAY.ToString());

            Str_Replace(ref Sql, ":FORM_CODE", "'" + vt.FormCode + "'");
            Str_Replace(ref Sql, ":MANAGER_DBNAME", v.active_DB.managerDBName);
            Str_Replace(ref Sql, ":NEWFIRM_DBNAME", v.newFirm_DB.databaseName);
            */
            return Sql;
        }

        public string Str_Check(string Veri)
        {
            // string içindeki ' işaareti " işaretile değiştiriliyor
            Str_Replace(ref Veri, (char)39, (char)34);
            return Veri;
        }
        public string Str_AntiCheck(string Veri)
        {
            // string içindeki " işaareti ' işaretile değiştiriliyor
            Str_Replace(ref Veri, (char)34, (char)39);
            Str_Replace(ref Veri, '½', (char)39);
            return Veri;
        }
        private string Str_CheckYilAy(string Veri)
        {
            //-- :@@YILAY
            int i1 = Veri.IndexOf("-- :@@YILAY");
            if (i1 == -1)
                Veri.IndexOf("--  :@@YILAY");
            if (i1 == -1)
                i1 = Veri.IndexOf("--:@@YILAY");

            int i2 = Veri.IndexOf(":BUGUN_YILAY");

            // eğer :@@YILAY ifadesi var, :BUGUN_YILAY yoksa
            // :BUGUN_YILAY value değerini yenileyebilmek için tekrar düzenleme gerekiyor
            //
            // declare @DonemTipiId int = 202205 -- :@@YILAY   şeklindeki satır
            // declare @DonemTipiId int = :BUGUN_YILAY  -- :@@YILAY şekline dönüyor
            if ((i1 > -1) && (i2 == -1))
            {
                for (int i = i1; i > -1; i--)
                {
                    if (Veri[i] == '=' || Veri[i] == '>' || Veri[i] == '<')
                    {
                        i2 = i;
                        break;
                    }
                }

                Veri = Veri.Remove(i2 + 1, i1 - i2 - 1);

                Veri = Veri.Insert(i2 + 1, " :BUGUN_YILAY ");
            }

            return Veri;
        }
        private string Str_Replace(ref string Veri, char mevcut_ifade, char yeni_ifade)
        {
            if (Veri.Length > 0)
                Veri = Veri.Replace(mevcut_ifade, yeni_ifade);
            return Veri;
        }
        private string Str_Replace(ref string Veri, string mevcut_ifade, string yeni_ifade)
        {
            if (Veri.Length > 0)
                Veri = Veri.Replace(mevcut_ifade, yeni_ifade);
            return Veri;
        }
    }
}