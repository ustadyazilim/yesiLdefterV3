using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ustad.API.Classes
{
    public class Querys
    {
        public static string UstadUserLoginSql(string userEMail, string Password)
        {
            //string query =
            //    @" Select * from UstadUsers where UserEMail = @EMail and UserKey = @Password ";
            string query =
                " Select * from UstadUsers "
              + " where UserEMail = '" + userEMail + "'"
              + " and UserKey = '" + Password + "'";
            return query;
        }

        public static string MsTableIPQuery(string tableIPCode)
        {
            string query = " Select IP_SELECT_SQL from MS_TABLES_IP Where TABLEIPCODE = '" + tableIPCode + "' ";
            return query;
        }

        public static string MsTableIPSql(string tableIPCode)
        {
            string query =
            @" 
SELECT a.[REF_ID]
      ,a.[TABLE_CODE]
      ,a.[TABLE_TYPE]
      ,a.[IP_CODE]
      ,a.[TABLEIPCODE]
      ,a.[IP_TYPE]
      ,a.[IP_CAPTION]
      ,a.[IP_SELECT_SQL]
      ,a.[IP_WHERE_SQL]
      ,a.[IP_ORDER_BY]
      ,a.[EXTERNAL_IP_CODE]
      ,a.[VIEW_CMP_TYPE]
      ,a.[CMP_PROPERTIES]
      ,a.[NAVIGATOR]
      ,a.[DATA_READ]
      ,a.[FIND_FNAME]
      ,a.[DATA_FIND]
      ,a.[AUTO_INSERT]
      ,a.[MASTER_TABLEIPCODE]
      ,a.[MASTER_TABLE_NAME]
      ,a.[MASTER_KEY_FNAME]
      ,a.[PROP_SUBVIEW]
      ,a.[PROP_JOINTABLE]
      ,a.[PROP_VIEWS]
      ,a.[PROP_NAVIGATOR]
      ,a.[SOFTWARE_CODE]
      ,a.[PROJECT_CODE]
      , b.SCHEMAS_CODE        LKP_SCHEMAS_CODE   
      , b.DBASE_TYPE          LKP_DBASE_TYPE 
      , b.MODUL_CODE          LKP_MODUL_CODE 
      , b.TABLE_CODE          LKP_TABLE_CODE 
      , b.TABLE_NAME          LKP_TABLE_NAME 
      , b.KEY_FNAME           LKP_KEY_FNAME  
      , b.TABLE_TYPE          LKP_TABLE_TYPE 
      , b.MASTER_TABLEIPCODE  LKP_MASTER_TABLEIPCODE 
      , b.MASTER_TABLE_NAME   LKP_MASTER_TABLE_NAME 
      , b.MASTER_KEY_FNAME    LKP_MASTER_KEY_FNAME 
      , b.TB_CAPTION          LKP_TB_CAPTION 
      , b.TB_SELECT_SQL       LKP_TB_SELECT_SQL 
      , b.TB_WHERE_SQL        LKP_TB_WHERE_SQL 
      , b.TB_ORDER_BY         LKP_TB_ORDER_BY 
      , b.PROP_SUBVIEW        LKP_PROP_SUBVIEW 
      , b.PROP_JOINTABLE      LKP_PROP_JOINTABLE 
                             
      from MS_TABLES_IP a 
          left outer join MS_TABLES b on ( a.TABLE_CODE = b.TABLE_CODE and a.SOFTWARE_CODE = b.SOFTWARE_CODE and a.PROJECT_CODE = b.PROJECT_CODE ) 
      Where TABLEIPCODE = '" + tableIPCode + "' ";



            return query;
        }


    }
}