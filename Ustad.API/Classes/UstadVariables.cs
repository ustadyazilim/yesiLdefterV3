using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ustad.API.Variables
{
    public class tVariables
    {
        public string dbCrm { get; set; }
        public string dbManager { get; set; }
        public string dbProject { get; set; }

        public int BUGUN_GUN { get; set; }
        public int BUGUN_AY { get; set; }
        public int BUGUN_YIL { get; set; }


        public tVariables()
        {
            dbCrm = "BulutCrm";
            dbManager = "BulutManager";
            //dbManager = "LocalManager";
            dbProject = "BulutProject";


            // output : { 25.03.2019 22:59:22 }
            DateTime dt = DateTime.Now;
            BUGUN_GUN = dt.Day;
            BUGUN_AY = dt.Month;
            BUGUN_YIL = dt.Year;

            /*
            string yil = dt.Year.ToString();
            string ay = dt.Month.ToString();
            string gun = dt.Day.ToString();
            string saat = dt.Hour.ToString();
            string dakk = dt.Minute.ToString();

            if (ay.Length == 1) ay = "0" + ay;
            if (gun.Length == 1) gun = "0" + gun;
            if (saat.Length == 1) saat = "0" + saat;
            if (dakk.Length == 1) dakk = "0" + dakk;
            */

        }


    }

    public class tParameter
    {
        public string ParameterName { get; set; } = string.Empty;
        public string ParameterValue { get; set; } = string.Empty;
    }

    public class tQueryAbout
    {
        public tQueryAbout()
        {
            SqlDataSource = string.Empty;
            QuerySql = string.Empty;
            tParams = new List<tParameter>();
        }

        //public IConfiguration Configuration { get; set; }
        //public string ConnectionName { get; set; }
        public string SqlDataSource { get; set; } = string.Empty;
        public string QuerySql { get; set; } = string.Empty;
        public List<tParameter> tParams { get; set; }

        public void Clear()
        {
            //Configuration = null;
            //ConnectionName = "";
            SqlDataSource = "";
            QuerySql = "";
            tParams.Clear();
        }
    }


}