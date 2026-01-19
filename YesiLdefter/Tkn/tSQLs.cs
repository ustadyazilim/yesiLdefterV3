using DevExpress.XtraEditors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Tkn_DefaultValue;
using Tkn_ToolBox;
using Tkn_Variable;

namespace Tkn_SQLs
{
    public class tSQLs : tBase
    {
        tToolBox t = new tToolBox();

        #region Report SQLs

        public string RSQL_MasterTable(string DatabaseName, string Table_Name)
        {
            // , int Value
            // declare @value int
            // set @value = " + Value + @"
            //, @value LKP_VALUE 
            return
            @" 
            declare @table_name varchar(50)
        
            set @table_name = '" + Table_Name + @"'
        
            SELECT 1 BASAMAK, * FROM [" + v.active_DB.managerDBName + @"].dbo.MS_TABLES 
            WHERE TABLE_NAME = @table_name
        
            UNION ALL
        
            SELECT 2 BASAMAK, * FROM [" + v.active_DB.managerDBName + @"].dbo.MS_TABLES 
            WHERE MASTER_TABLE_NAME = @table_name 
                             
            UNION ALL
        
            SELECT 3 BASAMAK, * FROM [" + v.active_DB.managerDBName + @"].dbo.MS_TABLES 
            WHERE MASTER_TABLE_NAME in (
              SELECT TABLE_NAME FROM [" + v.active_DB.managerDBName + @"].dbo.MS_TABLES 
              WHERE MASTER_TABLE_NAME = @table_name
            )                             
                             
            ";

        }

        #endregion Report SQLs

        #region System SQLs

        public string SQL_Navigator()
        {
            string s =
            @"
            declare @onay bit 
            declare @newCaption varchar(100)
            declare @newWidth int

            set @onay = 0
            set @newWidth = 0

            select 
              TYPES_VALUE_INT ID
            , @onay LKP_ONAY 
            , '[' + convert(varchar(5),TYPES_VALUE_INT) + '] ' + TYPES_CAPTION LKP_CAPTION
            , @newCaption LKP_NEW_CAPTION
            , @newWidth LKP_NEW_WIDTH
 
            from MS_TYPES
            where TYPES_NAME = 'NAVIGATOR'
            order by TYPES_VALUE_INT 
            ";

            return s;
        }

        public void SQL_MSFieldsIPKrtr(string TableIPCode, ref string tSqlKist, ref string tSqlFull)
        {
            string softCode = "";
            string projectCode = "";
            string TableCode = string.Empty;
            string IPCode = string.Empty;

            t.TableIPCode_Get(TableIPCode, ref softCode, ref projectCode, ref TableCode, ref IPCode);

            string MsFieldList = t.MS_FIELDS_LIST("b");

            // and   isnull(b.FPICTURE, '') = ''

            // and   b.KRT_OPERAND_TYPE <> 9   Visible=False değilse

            tSqlKist =
            @" select a.* " + MsFieldList + @"
               from [" + v.active_DB.managerDBName + @"].dbo.MS_FIELDS_IP  a
                  left outer join [" + v.active_DB.managerDBName + @"].dbo.MS_FIELDS b on ( a.TABLE_CODE = b.TABLE_CODE and a.FIELD_NO = b.FIELD_NO ) 
                           where a.TABLE_CODE = '" + TableCode + @"'
               and   a.IP_CODE = '" + IPCode + @"'
               and   a.KRT_LINE_NO > 0
               and   isnull(a.KRT_OPERAND_TYPE,0) <> 9
               order by a.KRT_LINE_NO, a.GROUP_LINE_NO, a.FIELD_NO  ";

            tSqlFull =
            @" select a.* " + MsFieldList + @"
               from [" + v.active_DB.managerDBName + @"].dbo.MS_FIELDS_IP a
                  left outer join [" + v.active_DB.managerDBName + @"].dbo.MS_FIELDS b on ( a.TABLE_CODE = b.TABLE_CODE and a.FIELD_NO = b.FIELD_NO ) 
               where a.TABLE_CODE = '" + TableCode + @"'
               and   a.IP_CODE = '" + IPCode + @"'
               and   isnull(a.KRT_OPERAND_TYPE,0) <> 9    
               order by a.KRT_LINE_NO, a.GROUP_LINE_NO, a.FIELD_NO      ";
        }

        public void SQL_MSFieldsKrtr(string TableCode, ref string tSqlKist, ref string tSqlFull)
        {
            // and   isnull(b.FPICTURE, '') = ''

            // and   b.KRT_OPERAND_TYPE <> 9   Visible=False değilse
            tSqlKist =
            @" select a.* 
               from [" + v.active_DB.managerDBName + @"].dbo.MS_FIELDS  a
               where a.TABLE_CODE = '" + TableCode + @"'
               and   a.KRT_LINE_NO > 0
               and   isnull(a.KRT_OPERAND_TYPE,0) <> 9
               order by a.KRT_LINE_NO, a.GROUP_LINE_NO, a.FIELD_NO  ";

            tSqlFull =
            @" select a.* 
               from [" + v.active_DB.managerDBName + @"].dbo.MS_FIELDS  a
               where a.TABLE_CODE = '" + TableCode + @"'
               and   isnull(a.KRT_OPERAND_TYPE,0) <> 9 
               order by a.KRT_LINE_NO, a.GROUP_LINE_NO, a.FIELD_NO  ";
        }

        public string SQL_ProcedureParamList(string ProcedureName)
        {
            return @"
              Select a.[name]
                   , a.[object_id]
                   , b.[name] [param_name]
                   , b.[parameter_id]
                   , b.[system_type_id]
                   , b.[user_type_id]
                   , b.[max_length]
       
              from [sys].[procedures] a
                 , [sys].[parameters] b
              where a.[object_id] = b.[object_id] 
              and   a.[name] = '" + ProcedureName + @"' ";
        }

        public string SQL_Table_FieldsList(string DatabaseName, string Table_Name, string IPCode)
        {
            string s1 =
            @" select  
               a.column_id, a.name 
             , convert(smallInt, a.system_type_id) system_type_id 
             , convert(smallInt, a.user_type_id)   user_type_id 
             , (case a.system_type_id when 231 then  a.max_length / 2 else a.max_length end) max_length
             , a.precision, a.scale, a.is_nullable, a.is_identity  
             from [" + DatabaseName + @"].sys.columns a 
               left outer join [" + DatabaseName + @"].sys.tables b on (a.object_id = b.object_id ) 
             where b.name = '" + Table_Name + @"' 
             order by a.column_id ";


            /// FFOREING Bit                 NULL,
            /// FTRIGGER Bit                 NULL,
            /// PROP_EXPRESSION VarChar(4000)          NULL,              
            /// VALIDATION_INSERT Bit                NULL,
            /// XML_FIELD_NAME VarChar(30)            NULL,
            /// CMP_DISPLAY_FORMAT VarChar(50) 		NULL,

            string s2 =
            @" select  
               d.FIELD_NO
             , d.FIELD_NAME
             , d.FFOREING 
             , d.FTRIGGER 
             , d.PROP_EXPRESSION 
             , e.VALIDATION_INSERT
             , e.XML_FIELD_NAME
             , e.CMP_DISPLAY_FORMAT
             , e.CMP_EDIT_FORMAT

             from dbo.MS_TABLES c 
               left outer join dbo.MS_FIELDS d on ( c.TABLE_CODE = d.TABLE_CODE )
               left outer join dbo.MS_FIELDS_IP e on (
                     c.TABLE_CODE = d.TABLE_CODE 
                 and d.FIELD_NO = e.FIELD_NO
                 and e.IP_CODE = '" + IPCode + @"' 
                 )  
             where c.TABLE_NAME = '" + Table_Name + @"' 
             order by d.FIELD_NO 
            ";

            if ((Table_Name.IndexOf("_KIST") > -1) ||
                (Table_Name.IndexOf("_FULL") > -1))
                return "param";

            return s1 + "|ds|" + s2;
        }

        public string SQL_MSTable_FieldsList(string DatabaseName, string Table_Name)
        {
            return
            @" select  
               a.column_id, a.name 
             , convert(smallInt, a.system_type_id) system_type_id 
             , convert(smallInt, a.user_type_id) user_type_id 
             , a.max_length, a.precision, a.scale, a.is_nullable, a.is_identity  
             , d.FFOREING, d.TABLE_CODE, d.FCAPTION, d.LIST_TYPES_NAME  
             , tl.VALUE_TYPE, tl.CAPTION_FNAME, tl.WHERE_FNAME1, tl.WHERE_FNAME2, tl.TABLE_NAME 
             from [" + DatabaseName + @"].sys.columns a 
              left outer join [" + DatabaseName + @"].sys.tables b on (a.object_id = b.object_id )
              left outer join [" + v.active_DB.managerDBName + @"].dbo.MS_TABLES c on (b.name = c.TABLE_NAME)
              left outer join [" + v.active_DB.managerDBName + @"].dbo.MS_FIELDS d on (
                 a.name = d.FIELD_NAME 
                 and c.TABLE_CODE = d.TABLE_CODE
                 and a.system_type_id = d.FIELD_TYPE
                 )
              left outer join [" + DatabaseName + @"].dbo.VW_TYPES_LIST tl on ( d.LIST_TYPES_NAME = tl.TYPES_NAME )
             where 0 = 0 
             and   b.name = '" + Table_Name + @"' 
             order by a.column_id ";
        }

        public string SQL_FieldFind(string databaseName, string tableName, string fieldName)
        {
            return
                @"
                 select  
                 a.column_id, a.name, 
                 convert(smallInt, a.system_type_id) system_type_id, 
                 convert(smallInt, a.user_type_id) user_type_id, 
                 a.max_length, a.precision, a.scale, a.is_nullable, a.is_identity 
                 from 
                 [" + databaseName + @"].sys.columns a,  
                 [" + databaseName + @"].sys.tables b 
                 where b.object_id = a.object_id 
                 and   b.name = '" + tableName + @"'
                 and   a.name = '" + fieldName + @"' ";
        }
        public string SQL_FieldNameAndTypeFind(string databaseName, string tableName, string fieldName, Int16 fieldTypeId, string fieldLength)
        {
            string Sql =
                @"
                 select  
                 a.column_id, a.name, 
                 convert(smallInt, a.system_type_id) system_type_id, 
                 convert(smallInt, a.user_type_id) user_type_id, 
                 a.max_length, a.precision, a.scale, a.is_nullable, a.is_identity 
                 from 
                 [" + databaseName + @"].sys.columns a,  
                 [" + databaseName + @"].sys.tables b 
                 where b.object_id = a.object_id 
                 and   b.name = '" + tableName + @"'
                 and   a.name = '" + fieldName + @"'  ";
            if (fieldLength == "")
                 Sql += "  and    a.system_type_id <> " + fieldTypeId.ToString() + "  ";
            else Sql += "  and  ( a.system_type_id <> " + fieldTypeId.ToString() + " or  a.max_length <> " + fieldLength + " ) "; 

            return Sql;
        }

        public string SQL_Types_List(string Type_Name)
        {
            string s = string.Empty;

            // eğer buraya önce Type_Name isim listesi eklenirse geresiz vt sorgusu oluşmaz, 
            // onun için isim listesi ekle

            s =
              " declare @type_name varchar(100) "
            + " set @type_name = '" + Type_Name + "' "

            + " select "
            + "   'MS_TYPES' TABLE_NAME "
            + " , a.VALUE_TYPE "
            + " , a.IMAGE_ID "
            + " , a.TYPES_VALUE_INT  VALUE_INT "
            + " , a.TYPES_VALUE_STR  VALUE_STR "
            + " , a.TYPES_CAPTION    VALUE_NAME "
            + " , ''                 VALUE_INT_FNAME "
            + " from " + v.active_DB.managerDBName + ".dbo.MS_TYPES a "
            + " where a.TYPES_NAME = @type_name "

            + " union all "

            + " select "
            + "   'MS_GROUPS' TABLE_NAME "
            + " , -1          VALUE_TYPE "
            + " , -1          IMAGE_ID "
            + " , a.FGROUPNO  VALUE_INT "
            + " , ''          VALUE_STR "
            + " , a.FCAPTION  VALUE_NAME "
            + " , ''          VALUE_INT_FNAME "
            + " from " + v.active_DB.managerDBName + ".dbo.MS_GROUPS a "
            + " where a.TABLE_CODE = @type_name "

            + " union all "

            + " select "
            + "   'SYS_TYPES_L' TABLE_NAME "
            + " , a.VALUE_TYPE "
            + " ,  -1                IMAGE_ID "
            + " , a.TYPES_VALUE_INT  VALUE_INT "
            + " , a.TYPES_VALUE_STR  VALUE_STR "
            + " , a.TYPES_CAPTION    VALUE_NAME "
            + " , ''                 VALUE_INT_FNAME "
            + " from " + v.active_DB.projectDBName + ".dbo.SYS_TYPES_L a "
            + " where a.TYPES_NAME = @type_name "

            + " union all "

            /* Bu Select ile Sadece aranın nerede olduğunu tespit ediyoruz   */
            + " select "
            + "   TABLE_NAME "
            + " , VALUE_TYPE "
            + " , -9             IMAGE_ID "
            + " , -9             VALUE_INT "
            + " , STR_FNAME      VALUE_STR "
            + " , CAPTION_FNAME  VALUE_NAME "
            + " , INT_FNAME      VALUE_INT_FNAME "
            + " from " + v.active_DB.projectDBName + ".dbo.SYS_TYPES_T a "
            + " where a.TYPES_NAME = @type_name "
            //+ " where a.TABLE_NAME = @type_name "

            ;

            return s;
        }

        public void SQL_Types_List(ref string SysTypesL, ref string MsTypes)
        {
            // eğer buraya önce Type_Name isim listesi eklenirse geresiz vt sorgusu oluşmaz, 
            // onun için isim listesi ekle

            /// BULUTA geçince bu şekilde ulaşım olmuyor
            /// onun için MS_TYPES proje DB yede eklendi
            ///
            ///select 'MS_TYPES' TABLE_NAME
            ///from " + v.active_DB.managerDBName + @".dbo.MS_TYPES a
            ///
            /// MS_GROUPS ta devreden çıkarıldı 
            /// gerekli olunca ona göre çare bulunacak
            /// 

            MsTypes = @"
            select 
             'MS_TYPES' TABLE_NAME 
            , a.TYPES_NAME 
            , a.VALUE_TYPE 
            , a.TYPES_VALUE_INT  VALUE_INT 
            , a.TYPES_VALUE_STR  VALUE_STR 
            , a.TYPES_CAPTION    VALUE_CAPTION
            , ''                 VALUE_INT_FNAME
            , ''                 WHERE_SQL 
            , ''                 ORDER_BY
            , ''                 TABLE_SQL 
                
            from MS_TYPES a 

            union all

            select
              'MS_GROUPS'  TABLE_NAME
            , 'MS_GROUPS'  TYPES_NAME
            ,  2           VALUE_TYPE
            , a.FGROUPNO   VALUE_INT
            , ''           VALUE_STR
            , a.FCAPTION   VALUE_CAPTION
            , ''           VALUE_INT_FNAME
            , ''           WHERE_SQL
            , ''           ORDER_BY
            , ''           TABLE_SQL 
            
            from MS_GROUPS a  ";


            SysTypesL =
             @" Select * from ( 
                select 
                  'SYS_TYPES_L'  LKP_TABLE_NAME 
                , a.TYPES_NAME 
                , a.VALUE_TYPE 
                , a.TYPES_VALUE_INT  VALUE_INT 
                , a.TYPES_VALUE_STR  VALUE_STR 
                , a.TYPES_CAPTION    VALUE_CAPTION 
                , ''                 VALUE_INT_FNAME 
                , ''                 WHERE_SQL 
                , ''                 ORDER_BY
                , ''                 TABLE_SQL 
                , ''                 DB_NAMES

                from SYS_TYPES_L a 
                  
                union all 
                
                select 
                  a.TABLE_NAME  LKP_TABLE_NAME
                , a.TYPES_NAME 
                , a.VALUE_TYPE 
                , -9               VALUE_INT 
                , a.STR_FNAME      VALUE_STR 
                , a.CAPTION_FNAME  VALUE_CAPTION 
                , a.INT_FNAME      VALUE_INT_FNAME
                , a.WHERE_SQL      
                , a.ORDER_BY
                , a.TABLE_SQL                 
                , a.DB_NAMES

                from SYS_TYPES_T a 
                ) x order by TYPES_NAME  
                ";
        }

        public string SQL_SYS_Variables_List(v.dBaseType dbtype)
        {
            /// Bu SQL in doğru çalışması için 
            /// buraya sırayla FIRM_ID, SHOP_ID, PART_ID, DEPT_ID, PERSONEL_ID, COMP_ID
            /// gelmesi gerekiyor
            string s = "";
                        
            if (dbtype == v.dBaseType.MSSQL)
                s = @" EXEC prc_SYS_VARIABLES_FULL " + v.SP_FIRM_ID;

            return s;

            #region
            /*
            @" 
            Select [3S_SYSVAR].* 
               , ( CASE [MSVR].FJOIN_TABLE_NAME 
                   WHEN 'HP_FIRM'   THEN [HPFIRM].HP_NAME 
                   WHEN 'HP_CARI'   THEN [HPCARI].TAMADI 
                   WHEN 'HP_FINANS' THEN [HPFIN].HP_TAMADI 
                   WHEN 'HP_IL'     THEN [HPIL].IL_ADI
                   END ) LKP_CLIENT_VALUE_CAPTION

               from (
                   select 
                        [SYSVAR].[VARIABLE_CODE]
                   ,max([SYSVAR].[CLIENT_TYPE])  [CLIENT_TYPE]
                   ,max([SYSVAR].[CLIENT_ID])    [CLIENT_ID]
                   ,max([SYSVAR].[CLIENT_VALUE]) [CLIENT_VALUE] 
                   from VW_CLIENT_LIST [CLNT] 
                     left outer join [dbo].[MS_VARIABLES] [MSVR] on ( 0 = 0 )
                     left outer join [dbo].[SYS_VARIABLES] [SYSVAR] on 
                     (      [MSVR].[VARIABLE_CODE] = [SYSVAR].[VARIABLE_CODE] 
                        and [SYSVAR].[CLIENT_TYPE]   = [CLNT].CLIENT_TYPE 
                        and [SYSVAR].[CLIENT_ID]     = [CLNT].CLIENT_ID
                     )
                   where 0 = 0
                   -- and [MSVR].FJOIN_TYPE > 0
                   and [SYSVAR].ID > 0
                   and (
                       ( [SYSVAR].[CLIENT_TYPE] = 1 and [SYSVAR].CLIENT_ID = 1 )   -- firm
                    or ( [SYSVAR].[CLIENT_TYPE] = 2 and [SYSVAR].CLIENT_ID = 10 )  -- shop
                    or ( [SYSVAR].[CLIENT_TYPE] = 3 and [SYSVAR].CLIENT_ID = 0 )   -- part
                    or ( [SYSVAR].[CLIENT_TYPE] = 4 and [SYSVAR].CLIENT_ID = 0 )   -- departman
                    or ( [SYSVAR].[CLIENT_TYPE] = 5 and [SYSVAR].CLIENT_ID = 0 )   -- personel
                    or ( [SYSVAR].[CLIENT_TYPE] = 6 and [SYSVAR].CLIENT_ID = 0 )   -- computer
               ) 

               group by
                   [SYSVAR].[MODUL_CODE]
                  ,[SYSVAR].[VARIABLE_CODE]

               ) [3S_SYSVAR] 

              left outer join [dbo].[MS_VARIABLES] [MSVR] on ( 
                [3S_SYSVAR].[VARIABLE_CODE] = [MSVR].[VARIABLE_CODE] 
                )
              left outer join [dbo].[HP_FIRM] [HPFIRM] on ( 
                [MSVR].FJOIN_TABLE_NAME = 'HP_FIRM' 
                and Convert(Int, isnull([3S_SYSVAR].CLIENT_VALUE,0)) = [HPFIRM].ID 
                ) 
              left outer join [dbo].[HP_CARI] [HPCARI] on ( 
                [MSVR].FJOIN_TABLE_NAME = 'HP_CARI' 
                and Convert(Int, isnull([3S_SYSVAR].CLIENT_VALUE,0)) = [HPCARI].ID 
                ) 
              left outer join [dbo].[HP_FINANS] [HPFIN] on ( 
                [MSVR].FJOIN_TABLE_NAME = 'HP_FINANS' 
                and Convert(Int, isnull([3S_SYSVAR].CLIENT_VALUE,0)) = [HPFIN].ID 
                ) 
              left outer join [dbo].[HP_IL] [HPIL] on ( 
                [MSVR].FJOIN_TABLE_NAME = 'HP_IL' 
                and Convert(Int, isnull([3S_SYSVAR].CLIENT_VALUE, 0)) = [HPIL].IL_KODU
                ) ";
            */

            /*
Select [3S_SYSVAR].* 
 , ( CASE [MSVR].FJOIN_TABLE_NAME 
     WHEN 'HP_FIRM' THEN [HPFIRM].HP_NAME 
     WHEN 'HP_CARI' THEN [HPCARI].TAMADI 
     WHEN 'HP_FINANS' THEN [HPFIN].HP_TAMADI 
     END ) LKP_CLIENT_VALUE_CAPTION
     
 from (
   select 
           [SYSVAR].[VARIABLE_CODE]
      ,max([SYSVAR].[CLIENT_TYPE])  [CLIENT_TYPE]
      ,max([SYSVAR].[CLIENT_ID])    [CLIENT_ID]
      ,max([SYSVAR].[CLIENT_VALUE]) [CLIENT_VALUE] 
   from VW_CLIENT_LIST [CLNT] 
     left outer join [MSV3DFTR].[dbo].[MS_VARIABLES] [MSVR] on ( 0 = 0 )
     left outer join [dbo].[SYS_VARIABLES] [SYSVAR] on 
      (          [MSVR].[VARIABLE_CODE] = [SYSVAR].[VARIABLE_CODE] 
           and [SYSVAR].[CLIENT_TYPE]   = [CLNT].CLIENT_TYPE 
           and [SYSVAR].[CLIENT_ID]     = [CLNT].CLIENT_ID
      )

   where 0 = 0
   and [MSVR].FJOIN_TYPE > 0
   and [SYSVAR].ID > 0
   and (
       ( [SYSVAR].[CLIENT_TYPE] = 1 and [SYSVAR].CLIENT_ID = 1 )   -- firm
    or ( [SYSVAR].[CLIENT_TYPE] = 2 and [SYSVAR].CLIENT_ID = 10 )  -- shop
    or ( [SYSVAR].[CLIENT_TYPE] = 3 and [SYSVAR].CLIENT_ID = 0 )   -- part
    or ( [SYSVAR].[CLIENT_TYPE] = 4 and [SYSVAR].CLIENT_ID = 0 )   -- departman
    or ( [SYSVAR].[CLIENT_TYPE] = 5 and [SYSVAR].CLIENT_ID = 0 )   -- personel
    or ( [SYSVAR].[CLIENT_TYPE] = 6 and [SYSVAR].CLIENT_ID = 0 )   -- computer
       ) 

   group by
       [SYSVAR].[MODUL_CODE]
      ,[SYSVAR].[VARIABLE_CODE]

 ) [3S_SYSVAR] 
 
  left outer join [MSV3DFTR].[dbo].[MS_VARIABLES] [MSVR] on ( 
    [3S_SYSVAR].[VARIABLE_CODE] = [MSVR].[VARIABLE_CODE] 
    )
    
  left outer join HP_FIRM [HPFIRM] on ( 
    [MSVR].FJOIN_TABLE_NAME = 'HP_FIRM' 
    and Convert(Int, isnull([3S_SYSVAR].CLIENT_VALUE,0)) = [HPFIRM].ID 
    ) 
  left outer join HP_CARI [HPCARI] on ( 
    [MSVR].FJOIN_TABLE_NAME = 'HP_CARI' 
    and Convert(Int, isnull([3S_SYSVAR].CLIENT_VALUE,0)) = [HPCARI].ID 
    ) 
  left outer join HP_FINANS [HPFIN] on ( 
    [MSVR].FJOIN_TABLE_NAME = 'HP_FINANS' 
    and Convert(Int, isnull([3S_SYSVAR].CLIENT_VALUE,0)) = [HPFIN].ID 
    ) 
  
  
            */
            #endregion
        }

        public string Sql_UstadFirmsWithFirmGUID(string firmGUID)
        {
            return @" Select * from UstadFirms Where FirmGUID = '" + firmGUID + @"' ";
        }
        public string preparingUstadFirmListSql(string myGuid, v.tFirmListType flt)
        {
            string tSql = "";
            string selectFields = @"
Select distinct 
       f.[FirmId]
      ,f.[ParentFirmId]
      ,[IsActive]
      ,[FirmTypeId]
      ,[FirmGUID]
      ,[SectorTypeId]
      ,[FirmCode]
      ,[FirmLongName]
      ,[FirmShortName]
      ,[FirmPhone]
      ,[WebPage]
      ,[EMail]
      ,[Address1]
      ,[Address2]
      ,[AddressCode]
      ,[DistrictTypeId]
      ,[CityTypeId]
      ,[FirmOpenDate]
      ,[FounderName]
      ,[FounderPhone]
      ,[ManagerName]
      ,[ManagerPhone]
      ,[MenuCode]
      ,[ServerNameIP]
      ,[DatabaseName]
      ,[DbLoginName]
      ,[DbPass]
      ,[RecordDate]
            "; //FROM[dbo].[UstadFirms]

            #region Only Select Firm List

            /// kendisi ve kendisine bağlı alt firma, şubeleride getirir
            if (flt == v.tFirmListType.OnlySelect)
                tSql = @" 
             declare @_firmGUID varchar(50) 
             declare @_XID int
             set @_firmGUID = '" + myGuid + @"'
 
             Select @_XID = FirmId from UstadFirms
             where FirmGUID = @_firmGUID

             -- Select @_XID 
 
             ;WITH CTE
             AS
             (
                Select M1.FirmId, M1.FirmId U_FirmId
        	    From UstadFirms as M1
        	    Where M1.ParentFirmId = @_XID
		
                Union all
	
                Select M1.FirmId, M1.FirmId U_FirmId
        	    From UstadFirms as M1
        	    Where M1.FirmId = @_XID
	
        	    /* 
	            -- BUNU AÇINCA 
	            -- GRUBUN HERHANGİ BİR ÜYESİNİ İSTEYİNCE
	            -- GRUBUN TÜM ELEMANLARI GELİYOR 
	
	            UNION ALL
	
                SELECT M1.FirmId, M1.ParentFirmId U_FirmId
	            FROM UstadFirms as M1
	            WHERE M1.FirmId = @_XID
	            AND M1.ParentFirmId > 0
        	    */
	
        	    /*
	            UNION ALL
		
	            SELECT C.U_FirmId, M.ParentFirmId
                FROM CTE C
                    JOIN UstadFirms as M ON C.U_FirmId = M.FirmId
                */

	            Union all
		
        	    Select C.U_FirmId, M.FirmId 
                From CTE as C
                    Join UstadFirms as M ON C.U_FirmId = M.ParentFirmId
		        
             )
             " + selectFields + @"
              from CTE c
                 left outer join UstadFirms as f on ( c.U_FirmId = f.FirmId ) ";

            #endregion


            #region All Firm List

            if (flt == v.tFirmListType.AllFirm)
                tSql = @"" + selectFields + @" from UstadFirms as f ";

            #endregion


            #region guid = TEST

            // burayı daha mantıklı hale getir
            if (myGuid == "TEST")
                tSql = selectFields + @"
                From UstadFirms as f Where f.FirmId = ";
                //where f.OPERATION_MODE_TD = 1 ";

            #endregion


            return tSql;
        }

        public string SQL_SYS_FIRM_List(string myGuid, v.tFirmListType flt)
        {
            string tSql = "";
            string selectFields =
              @"
              Select distinct f.[ID]
                  ,f.[PARENT_ID]
                  ,f.[REF_ID]
                  ,f.[PARENT_REF_ID]
                  ,f.[ISACTIVE]
                  ,f.LOCAL_TD
                  ,f.[USE_PACKAGE]
                  ,f.[FIRM_GUID]
                  ,f.[FIRM_CODE]
                  ,f.[FIRM_NAME]
                  ,f.[FIRM_LONG_NAME1]
                  ,f.[FIRM_LONG_NAME2]
                  ,f.[FIRM_DB_TYPE]
                  ,f.[FIRM_DB_NAME]
                  ,f.[FIRM_MAINDB_FORMAT]
                  ,f.[FIRM_PERIODDB_FORMAT]
                  ,f.[FIRM_SERVER_TYPE]
                  ,f.[FIRM_SERVER_NAME]
                  ,f.[FIRM_AUTHENTICATION]
                  ,f.[FIRM_LOGIN]
                  ,f.[FIRM_PASSWORD]
                  ,f.[FIRM_MAINDB_CONNTEXT]
                  ,f.[FIRM_PERIODDB_CONNTEXT]
            ";

            #region Only Select Firm List

            /// kendisi ve kendisine bağlı alt firma, şubeleride getirir
            if (flt == v.tFirmListType.OnlySelect)
                tSql = @" 
             declare @_firmGUID varchar(50) 
             declare @_XID int
 
             set @_firmGUID = '" + myGuid + @"'
 
             Select @_XID = ID from SYS_FIRMS
             where FIRM_GUID = @_firmGUID

             -- Select @_XID 
 
             ;WITH CTE
             AS
             (
                SELECT M1.ID, M1.ID U_ID
        	    FROM SYS_FIRMS M1
        	    WHERE M1.PARENT_ID = @_XID
		
                UNION ALL
	
                SELECT M1.ID, M1.ID U_ID
        	    FROM SYS_FIRMS M1
        	    WHERE M1.ID = @_XID
	
        	    /* 
	            -- BUNU AÇINCA 
	            -- GRUBUN HERHANGİ BİR ÜYESİNİ İSTEYİNCE
	            -- GRUBUN TÜM ELEMANLARI GELİYOR 
	
	            UNION ALL
	
                SELECT M1.ID, M1.PARENT_ID U_ID
	            FROM SYS_FIRMS M1
	            WHERE M1.ID = @_XID
	            AND M1.PARENT_ID > 0
        	    */
	
        	    /*
	            UNION ALL
		
	            SELECT C.U_ID, M.PARENT_ID 
                FROM CTE C
                    JOIN SYS_FIRMS M ON C.U_ID = M.ID
                */

	            UNION ALL
		
        	    SELECT C.U_ID, M.ID 
                FROM CTE C
                    JOIN SYS_FIRMS M ON C.U_ID = M.PARENT_ID
		        
             )
             " + selectFields + @"
              from CTE c
                 left outer join SYS_FIRMS f on ( c.U_ID = f.ID ) ";

            #endregion

            #region All Firm List
            if (flt == v.tFirmListType.AllFirm)
                tSql = @" 
             declare @_firmGUID varchar(50) 
             declare @_XID int

             set @_firmGUID = '" + myGuid + @"'

             Select @_XID = ID from SYS_FIRMS
             where FIRM_GUID = @_firmGUID

             -- Select @_XID

             ; WITH CTE
             AS
             (
                SELECT M1.ID, M1.ID U_ID
                FROM SYS_FIRMS M1
                WHERE M1.PARENT_ID = @_XID

                UNION ALL

                SELECT M1.ID, M1.ID U_ID
                FROM SYS_FIRMS M1
                WHERE M1.ID = @_XID

	            -- BUNU AÇINCA 
	            -- GRUBUN HERHANGİ BİR ÜYESİNİ İSTEYİNCE
	            -- GRUBUN TÜM ELEMANLARI GELİYOR 
	
	            UNION ALL
	
                SELECT M1.ID, M1.PARENT_ID U_ID
	            FROM SYS_FIRMS M1
	            WHERE M1.ID = @_XID
	            AND M1.PARENT_ID > 0

                /*
	            UNION ALL
		
	            SELECT C.U_ID, M.PARENT_ID 
                FROM CTE C
                    JOIN SYS_FIRMS M ON C.U_ID = M.ID
                */
                UNION ALL

                SELECT C.U_ID, M.ID
                FROM CTE C
                    JOIN SYS_FIRMS M ON C.U_ID = M.PARENT_ID

             )
             " + selectFields + @"
              from CTE c
                 left outer join SYS_FIRMS f on(c.U_ID = f.ID)
            ";


            #endregion

            #region guid = TEST

            if (myGuid == "TEST")
                tSql = selectFields + @"
                from  SYS_FIRMS f 
                where f.OPERATION_MODE_TD = 1 ";

            #endregion

            return tSql;
        }

        public string SQL_SYS_UPDATES()
        {
            string tSql = "";

            tSql = @"
            Select TOP 1 * from SYS_UPDATES 
            where ISACTIVE = 1 
            order by REC_DATE desc 
            ";

            return tSql;
        }

        public string Sql_MsExeUpdates()
        {
            /// Son aktif exeyi bul
            /// 
            string tSql = @"
            Select TOP 1 * from MsExeUpdates
            where IsActive = 1 
            order by RecordDate desc ";
            return tSql;
        }

        public string preparingUstadUsersSql(string eMail, string pass, int userId)
        {
            string Sql = "";

            if ((eMail != "") && (pass == ""))
                Sql = @" Select * from UstadUsers where UserEMail = '" + eMail + @"' ";
            if ((eMail != "") && (pass != ""))
                Sql = @" Select * from UstadUsers where UserEMail = '" + eMail + @"' and UserKey = '" + pass + "' ";

            //if ((userId > 0) && (pass == ""))
            //    Sql = @" Update UstadUsers set IsActive = 1 where UserId = " + userId.ToString() + @" ";
            if ((userId > 0) && (eMail == "") && (pass == ""))
                Sql = @" Select * from UstadUsers where UserId = " + userId.ToString() + @" ";

            if ((userId > 0) && (pass != ""))
                Sql = @" Update UstadUsers set UserKey = '" + pass + @"' where UserId = " + userId.ToString() + @" ";

            return Sql;
        }

        public string preparingTabimUsersSql(string userName, string pass, int userId)
        {
            string Sql = "";

            if ((userName != "") && (userId == -1)) // UserName Control
                Sql = @" Select * from users where Username_ = '" + userName + @"' ";
            if ((userName != "") && (userId == 0))  // UserName ve password ile giriş
                Sql = @" Select * from users where Username_ = '" + userName + @"' and isnull(Password_, '') = '" + pass + "' ";
            if ((userId > 0) && (userName == "") && (pass == "")) // IserId ile giriş
                Sql = @" Select * from users where Ulas = " + userId.ToString() + @" ";
            if ((userId > 0) && (pass != "")) // Password Update
                Sql = @" Update users set Password_ = '" + pass + @"' where Ulas = " + userId.ToString() + @" ";

            return Sql;
        }


        public string SQL_TabimValues(string tableName, string where)
        {
            return @" Select * from dbo." + tableName + " where 0 = 0 " + where;
        }

        public string updateTabimFirmGUIDSql(string FirmGUID)
        {
            return @" 
  if ( Select count(*) ADET from [dbo].[paramalt] where ULASPARAMTOP = 1 and KOD = 119 ) = 0
  begin
    INSERT INTO [dbo].[paramalt]
           ([ULASPARAMTOP]
           ,[KOD]
           ,[BASLIK]
           ,[DEGER])
    VALUES
           ( 1, 119, 'FirmGUID', '" + FirmGUID + @"' )
  end else
  begin
    UPDATE [dbo].[paramalt]
    SET [DEGER] = '" + FirmGUID + @"'
    WHERE ULASPARAMTOP = 1
	and   KOD = 119
  end

  UPDATE [dbo].[users] SET [FirmGUID] = '" + FirmGUID + @"'

  Select 1 as onay
            ";
        }

        public string Sql_GetBildirimSablonlariList(string where)
        {
            return @" Select * from dbo.HubBildirimSablonlari where 0 = 0 " + where;
        }
        public string Sql_GetBildirimKanallariList(string where)
        {
            return @" Select * from Lkp.HubBildirimSablonlariKanalType where 0 = 0 " + where;
        }
        
        public string Sql_CrsSmsSettings()
        {
            string sql = @"
  Select Top 1 s.[Id]
      ,s.[FirmId]
      ,s.[IsActive]
      ,s.[ServisTypeId]
      ,s.[KullaniciAdi]
      ,s.[Sifre]
      ,s.[BayiiKodu]
      ,s.[Origin]
  FROM [dbo].[CrsSmsSettings] as s
  Where s.IsActive = 1
  And   s.FirmId = :FIRM_ID 
";
            return sql;
        }

        #endregion System SQLs

        #region Tabim Meb Aday/Sertifika Count
        public string Sql_TabimMebAdayCount()
        {
            string sql = @"
declare @FirmId varchar(10) 
declare @FirmGUID varchar(50)

set @FirmId = " + v.tMainFirm.FirmId.ToString() + @"
set @FirmGUID = '" + v.tMainFirm.FirmGuid + @"'

Select 
--  @FirmId as FirmId, @FirmGUID as FirmGUID, x.DonemTipiId, count(x.Ulas) Adet 
' if ( Select count(*) ADET from dbo.UstadFirmsMebAday where FirmId = ' + @FirmId + ' and DonemTipiId = ' + convert(varchar(6), x.DonemTipiId) + 
' ) = 0 begin ' +
'  Insert into dbo.UstadFirmsMebAday ( FirmId, FirmGUID, DonemTipiId, Adet ) values ( ' +
   @FirmId + ',' + '''' + @FirmGUID + ''', ' + convert(varchar(6), x.DonemTipiId) + ', ' + convert(varchar(22), count(x.Ulas)) + ' ) ' + 
'  end else begin ' + 
'   Update dbo.UstadFirmsMebAday set ' + 
'   Adet = ' + convert(varchar(22), count(x.Ulas)) + ' ' +
'  where FirmId = ' + @FirmId +  
'  and DonemTipiId = ' + convert(varchar(6), x.DonemTipiId) +
'  and Adet <> ' + convert(varchar(22), count(x.Ulas)) +  
' end  ' as SqlCumlesi

From (
   Select 
   k.Ulas  
  ,Convert(varchar(4), YEAR(g.[Baslama_Tarihi])) +
   (case Convert(varchar(2), MONTH(g.[Baslama_Tarihi])) 
   when 1 then '01'
   when 2 then '02'
   when 3 then '03'
   when 4 then '04'
   when 5 then '05'
   when 6 then '06'
   when 7 then '07'
   when 8 then '08'
   when 9 then '09'
   when 10 then '10'
   when 11 then '11'
   when 12 then '12'
   end) as DonemTipiId
 ,(case k.Ser_Sinif
   when 22 then 4 else 1 end  ) as TalepTipiId
 ,(case k.Eski_Sur_Sinif 
   when 0 then 0    -- 
   when 1 then 2513 -- M 
   when 2 then 2502 -- A1 
   when 3 then 2503 -- A2
   when 4 then 2514 -- A
   when 5 then 2515 -- B1
   when 6 then 2504 -- B
   when 7 then 2518 -- BE
   when 8 then 2516 -- C1
   when 9 then 2519 -- C1E
   when 10 then 2505 -- C
   when 11 then 2520 -- CE
   when 12 then 2517 -- D1
   when 13 then 2521 -- D1E
   when 14 then 2506 -- D
   when 15 then 2522 -- DE
   when 16 then 2509 -- G 
   when 17 then 0 -- K
   when 18 then 2507 -- E
   when 19 then 2508 -- F
   when 20 then 0 -- H(o)
   when 21 then 0 -- H(m)
   when 22 then 9200 -- 100 CEZA
   when 23 then 0 -- OPERATÖRLÜK
   when 24 then 0 -- R
   when 25 then 2504 -- B OTOMATİK
   when 26 then 2504 -- B ENGELLİ
   when 27 then 0 -- D ÇEKİCİ
   when 28 then 2502 -- A1 OTOMATİK
   when 29 then 2502 -- A1 ENGELLİ
   when 30 then 2503 -- A2 OTOMATİK 
   when 31 then 2503 -- A2 ENGELLİ
   when 32 then 2514 -- A OTOMATİK
   when 33 then 2514 -- A ENGELLİ
   else -1 
   end ) as [MevcutSertifikaTipiId]

 ,(case k.Ser_Sinif 
   when 0 then 0    -- 
   when 1 then 2513 -- M 
   when 2 then 2502 -- A1 
   when 3 then 2503 -- A2
   when 4 then 2514 -- A
   when 5 then 2515 -- B1
   when 6 then 2504 -- B
   when 7 then 2518 -- BE
   when 8 then 2516 -- C1
   when 9 then 2519 -- C1E
   when 10 then 2505 -- C
   when 11 then 2520 -- CE
   when 12 then 2517 -- D1
   when 13 then 2521 -- D1E
   when 14 then 2506 -- D
   when 15 then 2522 -- DE
   when 16 then 2509 -- G 
   when 17 then 0 -- K
   when 18 then 2507 -- E
   when 19 then 2508 -- F
   when 20 then 0 -- H(o)
   when 21 then 0 -- H(m)
   when 22 then 9200 -- 100 CEZA
   when 23 then 0 -- OPERATÖRLÜK
   when 24 then 0 -- R
   when 25 then 2504 -- B OTOMATİK
   when 26 then 2504 -- B ENGELLİ
   when 27 then 0 -- D ÇEKİCİ
   when 28 then 2502 -- A1 OTOMATİK
   when 29 then 2502 -- A1 ENGELLİ
   when 30 then 2503 -- A2 OTOMATİK 
   when 31 then 2503 -- A2 ENGELLİ
   when 32 then 2514 -- A OTOMATİK
   when 33 then 2514 -- A ENGELLİ
   else -1 
   end ) as [IstenenSertifikaTipiId]
 ,(case 
   when k.OTOMATIK_VITES = 0 then 0
   when k.OTOMATIK_VITES = 1 then 1
   when k.Ser_Sinif = 25 then 1
   when k.Ser_Sinif = 28 then 1
   when k.Ser_Sinif = 30 then 1
   when k.Ser_Sinif = 32 then 1
   end ) as [IsOtomatik]
 ,(case 
   when k.ENGELLI = 0 then 0
   when k.ENGELLI = 1 then 1
   when k.Ser_Sinif = 26 then 1
   when k.Ser_Sinif = 29 then 1
   when k.Ser_Sinif = 31 then 1
   when k.Ser_Sinif = 33 then 1
   end ) as [IsEngelli]

From KURSIYER as k
   left outer join [dbo].[GRUPTOP] as g on ( k.Grubu = g.Grup_Adi )
Where 0 = 0
and
   Convert(varchar(4), YEAR(g.[Baslama_Tarihi])) +
   (case Convert(varchar(2), MONTH(g.[Baslama_Tarihi])) 
   when 1 then '01'
   when 2 then '02'
   when 3 then '03'
   when 4 then '04'
   when 5 then '05'
   when 6 then '06'
   when 7 then '07'
   when 8 then '08'
   when 9 then '09'
   when 10 then '10'
   when 11 then '11'
   when 12 then '12'
   end) >= 202401

) as x

group by x.DonemTipiId

order by x.DonemTipiId
 ";

            return sql;
        }
        public string Sql_TabimMebAdaySertifikaCount()
        {
            string sql = @"
declare @FirmId varchar(10) 
declare @FirmGUID varchar(50)

set @FirmId = " + v.tMainFirm.FirmId.ToString() + @"
set @FirmGUID = '" + v.tMainFirm.FirmGuid + @"'

Select 
--  x.DonemTipiId, x.MevcutSertifikaTipiId, x.IstenenSertifikaTipiId, count(x.Ulas) Adet 

' if ( Select count(*) ADET from dbo.UstadFirmsMebAdaySertifika where FirmId = ' + @FirmId + 
' and DonemTipiId = ' + convert(varchar(6), x.DonemTipiId) + 
' and MevcutSertifikaTipiId = '  + convert(varchar(6), x.MevcutSertifikaTipiId) + 
' and IstenenSertifikaTipiId = '  + convert(varchar(6), x.IstenenSertifikaTipiId) + 
' ) = 0 begin ' +
+ ' Insert into dbo.UstadFirmsMebAdaySertifika ( FirmId, FirmGUID, DonemTipiId, MevcutSertifikaTipiId, IstenenSertifikaTipiId, Adet ) values ( ' +
@FirmId + ',' + '''' + @FirmGUID + ''', ' + convert(varchar(6), x.DonemTipiId) + ', ' + convert(varchar(6), x.MevcutSertifikaTipiId) + ', ' +  convert(varchar(6), x.IstenenSertifikaTipiId) + ', ' + convert(varchar(22), count(x.Ulas)) + ' ) ' + 
' end else begin ' +
' Update dbo.UstadFirmsMebAdaySertifika set ' +
'   Adet = ' + convert(varchar(22), count(x.Ulas)) + ' ' +
'  where FirmId = ' + @FirmId +  
'  and DonemTipiId = ' + convert(varchar(6), x.DonemTipiId) +
'  and MevcutSertifikaTipiId = '  + convert(varchar(6), x.MevcutSertifikaTipiId) + 
'  and IstenenSertifikaTipiId = '  + convert(varchar(6), x.IstenenSertifikaTipiId) + 
'  and Adet <> ' + convert(varchar(22), count(x.Ulas)) +  
' end  ' as SqlCumlesi

From (
   Select 
   k.Ulas  
  ,Convert(varchar(4), YEAR(g.[Baslama_Tarihi])) +
   (case Convert(varchar(2), MONTH(g.[Baslama_Tarihi])) 
   when 1 then '01'
   when 2 then '02'
   when 3 then '03'
   when 4 then '04'
   when 5 then '05'
   when 6 then '06'
   when 7 then '07'
   when 8 then '08'
   when 9 then '09'
   when 10 then '10'
   when 11 then '11'
   when 12 then '12'
   end) as DonemTipiId
 ,(case k.Ser_Sinif
   when 22 then 4 else 1 end  ) as TalepTipiId
 ,(case k.Eski_Sur_Sinif 
   when 0 then 0    -- 
   when 1 then 2513 -- M 
   when 2 then 2502 -- A1 
   when 3 then 2503 -- A2
   when 4 then 2514 -- A
   when 5 then 2515 -- B1
   when 6 then 2504 -- B
   when 7 then 2518 -- BE
   when 8 then 2516 -- C1
   when 9 then 2519 -- C1E
   when 10 then 2505 -- C
   when 11 then 2520 -- CE
   when 12 then 2517 -- D1
   when 13 then 2521 -- D1E
   when 14 then 2506 -- D
   when 15 then 2522 -- DE
   when 16 then 2509 -- G 
   when 17 then 0 -- K
   when 18 then 2507 -- E
   when 19 then 2508 -- F
   when 20 then 0 -- H(o)
   when 21 then 0 -- H(m)
   when 22 then 9200 -- 100 CEZA
   when 23 then 0 -- OPERATÖRLÜK
   when 24 then 0 -- R
   when 25 then 2504 -- B OTOMATİK
   when 26 then 2504 -- B ENGELLİ
   when 27 then 0 -- D ÇEKİCİ
   when 28 then 2502 -- A1 OTOMATİK
   when 29 then 2502 -- A1 ENGELLİ
   when 30 then 2503 -- A2 OTOMATİK 
   when 31 then 2503 -- A2 ENGELLİ
   when 32 then 2514 -- A OTOMATİK
   when 33 then 2514 -- A ENGELLİ
   else -1 
   end ) as [MevcutSertifikaTipiId]

 ,(case k.Ser_Sinif 
   when 0 then 0    -- 
   when 1 then 2513 -- M 
   when 2 then 2502 -- A1 
   when 3 then 2503 -- A2
   when 4 then 2514 -- A
   when 5 then 2515 -- B1
   when 6 then 2504 -- B
   when 7 then 2518 -- BE
   when 8 then 2516 -- C1
   when 9 then 2519 -- C1E
   when 10 then 2505 -- C
   when 11 then 2520 -- CE
   when 12 then 2517 -- D1
   when 13 then 2521 -- D1E
   when 14 then 2506 -- D
   when 15 then 2522 -- DE
   when 16 then 2509 -- G 
   when 17 then 0 -- K
   when 18 then 2507 -- E
   when 19 then 2508 -- F
   when 20 then 0 -- H(o)
   when 21 then 0 -- H(m)
   when 22 then 9200 -- 100 CEZA
   when 23 then 0 -- OPERATÖRLÜK
   when 24 then 0 -- R
   when 25 then 2504 -- B OTOMATİK
   when 26 then 2504 -- B ENGELLİ
   when 27 then 0 -- D ÇEKİCİ
   when 28 then 2502 -- A1 OTOMATİK
   when 29 then 2502 -- A1 ENGELLİ
   when 30 then 2503 -- A2 OTOMATİK 
   when 31 then 2503 -- A2 ENGELLİ
   when 32 then 2514 -- A OTOMATİK
   when 33 then 2514 -- A ENGELLİ
   else -1 
   end ) as [IstenenSertifikaTipiId]
 ,(case 
   when k.OTOMATIK_VITES = 0 then 0
   when k.OTOMATIK_VITES = 1 then 1
   when k.Ser_Sinif = 25 then 1
   when k.Ser_Sinif = 28 then 1
   when k.Ser_Sinif = 30 then 1
   when k.Ser_Sinif = 32 then 1
   end ) as [IsOtomatik]
 ,(case 
   when k.ENGELLI = 0 then 0
   when k.ENGELLI = 1 then 1
   when k.Ser_Sinif = 26 then 1
   when k.Ser_Sinif = 29 then 1
   when k.Ser_Sinif = 31 then 1
   when k.Ser_Sinif = 33 then 1
   end ) as [IsEngelli]

From KURSIYER as k
   left outer join [dbo].[GRUPTOP] as g on ( k.Grubu = g.Grup_Adi )
Where 0 = 0
and
   Convert(varchar(4), YEAR(g.[Baslama_Tarihi])) +
   (case Convert(varchar(2), MONTH(g.[Baslama_Tarihi])) 
   when 1 then '01'
   when 2 then '02'
   when 3 then '03'
   when 4 then '04'
   when 5 then '05'
   when 6 then '06'
   when 7 then '07'
   when 8 then '08'
   when 9 then '09'
   when 10 then '10'
   when 11 then '11'
   when 12 then '12'
   end) >= 202401

) as x

group by 
  x.DonemTipiId
, x.MevcutSertifikaTipiId
, x.IstenenSertifikaTipiId

order by x.DonemTipiId
";
            return sql;
        }

        #endregion Tabim Meb Aday/Sertifika Count

        #region ManagerServer Tables SQLs Preparing

        public string SQL_MS_TABLES_IP_LIST(string softCode, string projectCode, string TableCode, string IPCode)
        {
            string s =
                @" Select a.*  
               , b.SCHEMAS_CODE        LKP_SCHEMAS_CODE   
               , b.DBASE_TYPE          LKP_DBASE_TYPE 
               , b.MODUL_CODE          LKP_MODUL_CODE 
               , b.PARENT_TABLE_CODE   LKP_PARENT_TABLE_CODE 
               , b.TABLE_CODE          LKP_TABLE_CODE 
               , b.TABLE_NAME          LKP_TABLE_NAME 
               , b.KEY_FNAME           LKP_KEY_FNAME  
               , b.TABLE_TYPE          LKP_TABLE_TYPE 

               , b.MASTER_TABLEIPCODE  LKP_MASTER_TABLEIPCODE 
               , b.MASTER_TABLE_NAME   LKP_MASTER_TABLE_NAME 
               , b.MASTER_KEY_FNAME    LKP_MASTER_KEY_FNAME 
              
               , b.FOREING_FNAME       LKP_FOREING_FNAME 
               , b.PARENT_FNAME        LKP_PARENT_FNAME 
               , b.TB_CAPTION          LKP_TB_CAPTION 
               , b.TB_SELECT_SQL       LKP_TB_SELECT_SQL 
               , b.TB_WHERE_SQL        LKP_TB_WHERE_SQL 
               , b.TB_ORDER_BY         LKP_TB_ORDER_BY 
               , b.LOCK_FIELD_NAME     LKP_LOCK_FIELD_NAME 
               , b.PROP_SUBVIEW        LKP_PROP_SUBVIEW 
               , b.PROP_JOINTABLE      LKP_PROP_JOINTABLE 
                             
               from MS_TABLES_IP a 
                 left outer join MS_TABLES b on ( 
                        a.TABLE_CODE = b.TABLE_CODE 
                    and a.SOFTWARE_CODE = b.SOFTWARE_CODE
                    and a.PROJECT_CODE = b.PROJECT_CODE
                    ) 
               where 0 = 0 ";

            if ((TableCode != "") && (TableCode != "null"))
                s = s + " and a.TABLE_CODE = '" + TableCode + "' ";

            if ((IPCode != "") && (IPCode != "null"))
                s = s + " and a.IP_CODE = '" + IPCode + "' ";

            if ((softCode != "") && (softCode != "null"))
                s = s + " and a.SOFTWARE_CODE = '" + softCode + "' ";

            if ((projectCode != "") && (projectCode != "null"))
                s = s + " and a.PROJECT_CODE = '" + projectCode + "' ";

            return s;
        }
        
        public string SQL_MS_PROPERTIES_LIST(string TableName, string FieldName)
        {
            return
                " select a.* from MS_PROPERTIES a "
              + " where a.TABLENAME = '" + TableName + "' "
              + " and a.FIELDNAME = '" + FieldName + "' "
              + " order by a.ROW_CATEGORY_NO, a.ROW_TYPE, a.ROW_LINE_NO ";
        }

        public string SQL_MS_ITEMS_LIST(string MasterCode, byte MasterItemType)
        {
            return
                " select a.* "
              + " , g16.GLYPH LKP_GLYPH16 "
              + " , g32.GLYPH LKP_GLYPH32 "

              + " from MS_ITEMS a "
              + "   left outer join MS_GLYPH g16 on (a.GYLPH_16 = g16.GLYPH_NAME) "
              + "   left outer join MS_GLYPH g32 on (a.GYLPH_32 = g32.GLYPH_NAME) "
              + " where a.MASTER_CODE = '" + MasterCode + "' "
              + " and a.MASTER_ITEM_TYPE = " + MasterItemType.ToString() + " "
              + " order by a.MASTER_CODE, a.ITEM_CODE ";
            //+" order by a.MASTER_CODE, a.LINE_NO, a.ITEM_CODE ";
        }

        public string SQL_MS_GLYPH_LIST()
        {

            return @" select 
              [3S_MSGLY].[REF_ID]
            , [3S_MSGLY].[MAIN_GROUP_ID]
            , [3S_MSGLY].[GROUP_ID]
            , [3S_MSGLY].[GFILE_NAME]
            , [3S_MSGLY].[GSIZE]
            , [3S_MSGLY].[GLYPH_NAME]
            , [3S_MSGLY].[GLYPH]
            , CONVERT(varchar(10),[3S_MSGLY].MAIN_GROUP_ID) + '_' +
              CONVERT(varchar(10),[3S_MSGLY].GROUP_ID) + '_' + 
              [3S_MSGLY].GFILE_NAME LKP_FULL_NAME

            from MS_GLYPH [3S_MSGLY]
            
            where 0 = 0
            --and   [3S_MSGLY].MAIN_GROUP_ID <> 20
            and   isnull([3S_MSGLY].GSIZE,'') <> 'DIR'
            order by [3S_MSGLY].[MAIN_GROUP_ID]  
            ";
            //-- where [3S_MSGLY].MAIN_GROUP_ID<> 20 

            /*           
                       return  @"
                       Select[3S_MSGLY].*
                       , [GRP].GFILE_NAME LKP_GROUP_NAME
                       , CONVERT(varchar(10),[3S_MSGLY].MAIN_GROUP_ID) + '_' +
                         CONVERT(varchar(10),[3S_MSGLY].GROUP_ID) + '_' + 
                         [3S_MSGLY].GFILE_NAME LKP_FULL_NAME

                       from MS_GLYPH[3S_MSGLY]
                         left outer join MS_GLYPH[GRP] on(
                           [3S_MSGLY].MAIN_GROUP_ID = [GRP].MAIN_GROUP_ID and
                           [3S_MSGLY].GROUP_ID = [GRP].GROUP_ID  and
                           [GRP].GSIZE = 'DIR'
                         )

                       where [3S_MSGLY].MAIN_GROUP_ID<> 20 
                       and   isnull([3S_MSGLY].GSIZE,'') <> 'DIR'

                       ";

           */
        }

        public string SQL_IL_LIST()
        {
            return @" Select * from Lkp.ILTipi order by IlAdiBUYUK ";
        }
        public string SQL_ILCE_LIST()
        {
            return @" Select * from Lkp.IlceTipi order by IlKodu ";
        }

        public string SQL_Settings_UpdateSectorTypeId(int FirmId, Int16 SectorTypeId)
        {
            return @" Update dbo.Settings Set DefaultInt = " + SectorTypeId.ToString() 
                 + @" Where FirmId = " + FirmId.ToString()
                 + @" and   GroupNoTypeId = 1000 and SettingNo = 100 ";
        }
        public string SQL_MS_LAYOUT_LIST(string MasterCode, byte MasterItemType)
        {
            string myAnd = "";

            if (MasterItemType > 0)
                myAnd = " and a.MASTER_ITEM_TYPE = " + MasterItemType.ToString() + " ";

                return
                " select a.* from MS_LAYOUT a "
              + " where a.MASTER_CODE = '" + MasterCode + "' "
              //+ myAnd
              + " order by a.MASTER_CODE, isnull(a.GROUP_LINE_NO,0), isnull(a.LAYOUT_CODE,0) ";
        }

        public string SQL_MS_DC(string DC_Code)
        {
            return
              @" select a.* from [" + v.active_DB.managerDBName + @"].[dbo].MS_DC a  where a.DC_CODE = '" + DC_Code + @"' ";

            /*
            return
              @" select a.* from [" + v.active_DB.managerDBName + @"].[dbo].MS_DC a  where a.DC_CODE = '" + DC_Code + @"'  
                 union all
                 select a.* from [" + v.db_MAINMANAGER_DBNAME + @"].[dbo].MS_DC a where a.DC_CODE = '" + DC_Code + @"'  
               ";
            */
        }

        public string SQL_MS_DC_LINE(string DC_Code)
        {
            return
              @" select a.* from [" + v.active_DB.managerDBName + @"].[dbo].MS_DC_LINE a 
                 where  a.DC_CODE = '" + DC_Code + @"' 
                 order by a.LINE_NO ";

            /*
            return
              @"
               Select x.* from (
               select a.* from [" + v.active_DB.managerDBName + @"].[dbo].MS_DC_LINE a where a.DC_CODE = '" + DC_Code + @"' 
               union all
               select a.* from [" + v.db_MAINMANAGER_DBNAME + @"].[dbo].MS_DC_LINE a where a.DC_CODE = '" + DC_Code + @"' 
               ) x
               order by x.LINE_NO ";
        */

        }

        public string SQL_MS_GROUPS(string Table_IP_Code)
        {
            string softCode = "";
            string projectCode = "";
            string TableCode = string.Empty;
            string IPCode = string.Empty;

            tToolBox t = new tToolBox();
            t.TableIPCode_Get(Table_IP_Code, ref softCode, ref projectCode, ref TableCode, ref IPCode);

            return
               @" Select x.* from ( 
               select 
                 a.MAIN_TABLE_NAME 
               , a.TABLE_CODE 
               , a.GROUP_TYPES 
               , a.FCAPTION 
               , a.FGROUPNO 
               , a.FVISIBLE 
               , a.FIXED 
               , a.CMP_WIDTH 
               , a.MOVE_ITEM_TYPE 
               , a.MOVE_ITEM_NAME 
               , a.MOVE_TYPE 
               , a.MOVE_LOCATION 
               , a.MOVE_LAYOUT_TYPE 
               , a.PROP_VIEWS 
               , 1 TABLE_NO 

               from MS_GROUPS a 
               where a.MAIN_TABLE_NAME = 'MS_FIELDS_IP' 
               and a.TABLE_CODE = '" + TableCode + @"' 
               and a.IP_CODE = '" + IPCode + @"' 
               and a.SOFTWARE_CODE = '" + softCode + @"'
               and a.PROJECT_CODE = '" + projectCode + @"'

               union all 

               select 
                 a.MAIN_TABLE_NAME 
               , a.TABLE_CODE 
               , a.GROUP_TYPES 
               , a.FCAPTION 
               , a.FGROUPNO 
               , a.FVISIBLE 
               , a.FIXED 
               , a.CMP_WIDTH 
               , a.MOVE_ITEM_TYPE 
               , a.MOVE_ITEM_NAME 
               , a.MOVE_TYPE 
               , a.MOVE_LOCATION 
               , a.MOVE_LAYOUT_TYPE 
               , a.PROP_VIEWS
               , 2 TABLE_NO 

               from MS_GROUPS a 
               where a.MAIN_TABLE_NAME = 'MS_FIELDS' 
               and a.TABLE_CODE = '" + TableCode + @"' 
               and a.SOFTWARE_CODE = '" + softCode + @"'
               and a.PROJECT_CODE = '" + projectCode + @"'
               ) x 

              order by x.TABLE_NO, x.FGROUPNO ";
        }

        public string SQL_SYS_UPDATES_INSERT()
        {
            //[MSV3DFTRBLT]
            return //@" USE " + v.active_DB.managerDBName + @"  
@"
INSERT INTO [dbo].[SYS_UPDATES]
           ([ISACTIVE]
           ,[REC_DATE]
           ,[EXE_NAME]
           ,[UPDATE_TD]
           ,[UPDATE_NO]
           ,[VERSION_NO]
           ,[PACKET_NAME]
           ,[ABOUT])
     VALUES
           (1         
           ,getdate() 
           , " + "'" + v.tExeAbout.newFileName + "'" + @"
           , 2 
           , null 
           , " + "'" + v.tExeAbout.newVersionNo + "'" + @"
           , " + "'" + v.tExeAbout.newPacketName + "'" + @"
           , null 
		   )
            ";
            /*--UPDATE_TD 0: 1: normal 2: beta
             * 
             * 
           (<ISACTIVE, smallint,>
           ,<REC_DATE, datetime,>
           ,<EXE_NAME, varchar(30),>
           ,<UPDATE_TD, smallint,>
           ,<UPDATE_NO, int,>
           ,<VERSION_NO, varchar(20),>
           ,<PACKET_NAME, varchar(50),>
           ,<ABOUT, varchar(100),>)

            */
        }

        public string Sql_MsExeUpdates_Insert()
        {
            return @"
           INSERT INTO [dbo].[MsExeUpdates]
             ([IsActive]
             ,[RecordDate]
             ,[ExeName]
             ,[VersionNo]
             ,[PacketName]
             ,[About])
           VALUES
             (1         
             ,getdate() 
             , " + "'" + v.tExeAbout.newFileName + "'" + @"
             , " + "'" + v.tExeAbout.newVersionNo + "'" + @"
             , " + "'" + v.tExeAbout.newPacketName + "'" + @"
             , null 
		     ) ";
        }

        public string Sql_MsDbUpdates(string IdList)
        {
            // publishManager database
            //" Select * from dbo.MsDbUpdates Where Id not in ( " + IdList + " ) "
            string sectorTypeId = v.SP_Firm_SectorTypeId.ToString();
            // TabimSrc, TabimIsmak ise TabimMtsk dahil olarak hepsi aynı güncelleme olsun 
            // örnek : ( 0, 212, 211 ) veya ( 0, 213, 211 )
            if (v.SP_Firm_SectorTypeId == 212) sectorTypeId += ", 211";
            if (v.SP_Firm_SectorTypeId == 213) sectorTypeId += ", 211";

            return
                " Select * from dbo.MsDbUpdates Where Id > " + IdList + "  "
              + " and IsActive = 1 " 
              + " and SectorTypeId in ( 0, " + sectorTypeId + " ) ";
            //" Select * from " + v.publishManager_DB.databaseName + ".dbo.MsDbUpdates "
        }
        public string Sql_DataUpdate_On_MsDbUpdates(string tableList, string whereAdd)
        {
            // publishManager database
            //" Select * from dbo.MsDbUpdates Where Id not in ( " + IdList + " ) "
            string sectorTypeId = v.SP_Firm_SectorTypeId.ToString();
            // TabimSrc, TabimIsmak ise TabimMtsk dahil olarak hepsi aynı güncelleme olsun 
            // örnek : ( 0, 212, 211 ) veya ( 0, 213, 211 )
            if (v.SP_Firm_SectorTypeId == 212) sectorTypeId += ", 211";
            if (v.SP_Firm_SectorTypeId == 213) sectorTypeId += ", 211";

            return
                " Select * from dbo.MsDbUpdates Where TableName in ( " + tableList + " )  "
              + " and IsActive = 1 "
              + " and SectorTypeId in ( 0, " + sectorTypeId + " ) "
              + whereAdd;
            //" Select * from " + v.publishManager_DB.databaseName + ".dbo.MsDbUpdates "
        }

        public string Sql_MsFileUpdates(string IdList)
        {
            // read publishManager database
            return " Select * from dbo.MsFileUpdates Where Id > " + IdList + "  and IsActive = 1 order by RecordDate ";
        }
        public string Sql_MsFileUpdates_X64()
        {
            // read publishManager database
            return " Select * from dbo.MsFileUpdates Where FileName = 'x64'";
        }

        public string getGecerliTarihSql()
        {
            string sql = "";

            sql = @"
  Select
        ' if ( Select count(Id) as Adet From dbo.GecerlilikTarihi '
	  + ' Where FirmId = @FIRM_ID '  
      + ' and KonuTipiId = ' + Convert(varchar(10), [KonuTipiId]) 
      + ' and Convert(date,Tarih,103) = Convert(date,' + '''' + Convert(varchar(10), [Tarih], 103) + ''',103)  '
	  + ' ) = 0 '
	  + ' begin '
	  + ' Insert into [dbo].[GecerlilikTarihi] ( [FirmId],[IsActive],[KonuTipiId],[Tarih] ) Values ' 
      + ' ( ' + '@FIRM_ID, '  
      + Convert(varchar(2) , [IsActive]) + ', '
      + Convert(varchar(10), [KonuTipiId]) + ', '
      + ' Convert(date, ' + '''' + Convert(varchar(10), [Tarih], 103) + ''', 103) ) '
	  + ' end '

  From [dbo].[HubGecerlilikTarihi]
  Where IsActive = 1

";

            return sql;
        }

        public string getMtskSaatTeorikSql()
        {
            string sql = "";

            sql = @"
  Declare @GecerlilikTarihiId int

  Select top 1 @GecerlilikTarihiId = [Id]
  From [dbo].[HubGecerlilikTarihi]
  Where KonuTipiId = 21101
  and   IsActive = 1
  order by Tarih desc

  Select ' if ( Select count(Id) as Adet From dbo.MtskSaatTeorik '
  + ' Where FirmId = @FIRM_ID '  
  + ' and GecerlilikTarihiId = @TarihId ' 
  + ' ) = 0 begin '
  + ' Insert into [dbo].[MtskSaatTeorik] ( [FirmId],[IsActive],[GecerlilikTarihiId],[TrafikVeCevreDersSaati],[IlkYardimDersSaati],[AracTeknigiDersSaati],[TrafikAdabiDersSaati],[TeorikDersSaati]) Values '
  + ' ( @FIRM_ID, '
  + Convert(varchar(2), [IsActive]) + ', '
  + ' @TarihId, '
  + Convert(varchar(5), [TrafikVeCevreDersSaati]) + ', '
  + Convert(varchar(5), [IlkYardimDersSaati]) + ', '
  + Convert(varchar(5), [AracTeknigiDersSaati]) + ', '
  + Convert(varchar(5), [TrafikAdabiDersSaati]) + ', '
  + Convert(varchar(5), [TeorikDersSaati]) + ' ) end '
  + ' else begin '
  + ' UPDATE [dbo].[MtskSaatTeorik] '
  + ' SET [IsActive] = ' + Convert(varchar(2), [IsActive]) 
  + ' ,[TrafikVeCevreDersSaati] = ' + Convert(varchar(5), [TrafikVeCevreDersSaati]) 
  + ' ,[IlkYardimDersSaati] = ' + Convert(varchar(5), [IlkYardimDersSaati]) 
  + ' ,[AracTeknigiDersSaati] = ' + Convert(varchar(5), [AracTeknigiDersSaati]) 
  + ' ,[TrafikAdabiDersSaati] = ' + Convert(varchar(5), [TrafikAdabiDersSaati]) 
  + ' ,[TeorikDersSaati] = ' + Convert(varchar(5), [TeorikDersSaati])
  + ' Where FirmId = @FIRM_ID '  
  + ' and GecerlilikTarihiId = @TarihId ' 
  + ' end '
  From [dbo].[HubMtskSaatTeorik]
  Where GecerlilikTarihiId = @GecerlilikTarihiId

";

            return sql;
        }

        public string getMtskSaatUygulamaSql()
        {
            string sql = "";

            sql = @"
  Declare @GecerlilikTarihiId int

  Select top 1 @GecerlilikTarihiId = [Id]
  From [dbo].[HubGecerlilikTarihi]
  Where KonuTipiId = 21102
  and   IsActive = 1
  order by Tarih desc

  Select ' if ( Select count(Id) as Adet From dbo.MtskSaatUygulama '
  + ' Where FirmId = @FIRM_ID '  
  + ' and GecerlilikTarihiId = @TarihId ' 
  + ' and isnull([MevcutSertifikaTipiId],0) = ' + Convert(varchar(6),  isnull([MevcutSertifikaTipiId],0))
  + ' and isnull([IstenenSertifikaTipiId],0) = ' + Convert(varchar(6),  isnull([IstenenSertifikaTipiId],0))
  + ' ) = 0 begin '
  + ' INSERT INTO [dbo].[MtskSaatUygulama] '
  + '  ( [FirmId],[IsActive],[GecerlilikTarihiId],[MevcutSertifikaTipiId],[IstenenSertifikaTipiId],[SimulatorSaati],[EgitimAlaniSaati],[AkanTrafikSaati],[UygulamaToplamSaat], '
  + '    [UygulamaSaati2016Oncesi],[Arti4HakUygulamaSaati],[BasarisizEgitimSaati],[TelafiEgitimSaati],[YasSiniri],[IsEnAz],[DeneyimSertifikaTipiId],[DeneyimEnAzYilFarki] ) Values '
  + ' ( @FIRM_ID, '
  + Convert(varchar(2), [IsActive]) + ', '
  + ' @TarihId, '
  +  Convert(varchar(8), isnull([MevcutSertifikaTipiId],0)) + ', '
  +  Convert(varchar(8), isnull([IstenenSertifikaTipiId],0)) + ', '
  +  Convert(varchar(5), isnull([SimulatorSaati],0)) + ', '
  +  Convert(varchar(5), isnull([EgitimAlaniSaati],0)) + ', '
  +  Convert(varchar(5), isnull([AkanTrafikSaati],0)) + ', '
  +  Convert(varchar(5), isnull([UygulamaToplamSaat],0)) + ', '
  +  Convert(varchar(5), isnull([UygulamaSaati2016Oncesi],0)) + ', '
  +  Convert(varchar(5), isnull([Arti4HakUygulamaSaati],0)) + ', '
  +  Convert(varchar(5), isnull([BasarisizEgitimSaati],0)) + ', '
  +  Convert(varchar(5), isnull([TelafiEgitimSaati],0)) + ', '
  +  Convert(varchar(5), isnull([YasSiniri],0)) + ', '
  +  Convert(varchar(2), isnull([IsEnAz],0)) + ', '
  +  Convert(varchar(8), isnull([DeneyimSertifikaTipiId],0)) + ', '
  +  Convert(varchar(5), isnull([DeneyimEnAzYilFarki],0)) + ') end '
  + ' else begin '
  + ' UPDATE [dbo].[MtskSaatUygulama] '
  + ' SET [IsActive] = ' + Convert(varchar(2), [IsActive]) 
  + ' ,[SimulatorSaati] = ' + Convert(varchar(5), isnull([SimulatorSaati],0)) 
  + ' ,[EgitimAlaniSaati] = ' + Convert(varchar(5), isnull([EgitimAlaniSaati],0))
  + ' ,[AkanTrafikSaati] = ' + Convert(varchar(5), isnull([AkanTrafikSaati],0)) 
  + ' ,[UygulamaToplamSaat] = ' + Convert(varchar(5), isnull([UygulamaToplamSaat],0)) 
  + ' ,[UygulamaSaati2016Oncesi] = ' + Convert(varchar(5), isnull([UygulamaSaati2016Oncesi],0))
  + ' ,[Arti4HakUygulamaSaati] = ' + Convert(varchar(5), isnull([Arti4HakUygulamaSaati],0))
  + ' ,[BasarisizEgitimSaati] = ' + Convert(varchar(5), isnull([BasarisizEgitimSaati],0))
  + ' ,[TelafiEgitimSaati] = ' +  Convert(varchar(5), isnull([TelafiEgitimSaati],0))
  + ' ,[YasSiniri] = ' +  Convert(varchar(5), isnull([YasSiniri],0)) 
  + ' ,[IsEnAz] = ' +  Convert(varchar(2), isnull([IsEnAz],0))
  + ' ,[DeneyimSertifikaTipiId] = ' + Convert(varchar(8), isnull([DeneyimSertifikaTipiId],0))
  + ' ,[DeneyimEnAzYilFarki] = ' + Convert(varchar(5), isnull([DeneyimEnAzYilFarki],0))
  + ' Where FirmId = @FIRM_ID '  
  + ' and GecerlilikTarihiId = @TarihId ' 
  + ' and isnull([MevcutSertifikaTipiId],0) = ' + Convert(varchar(6),  isnull([MevcutSertifikaTipiId],0))
  + ' and isnull([IstenenSertifikaTipiId],0) = ' + Convert(varchar(6),  isnull([IstenenSertifikaTipiId],0))
  + ' end '
  From [dbo].[HubMtskSaatUygulama]
  Where GecerlilikTarihiId = @GecerlilikTarihiId

";
            return sql;
        }

        public string getMtskUcretSinavSql()
        {
            string sql = "";
            sql = @"
  Declare @GecerlilikTarihiId int

  Select top 1 @GecerlilikTarihiId = [Id]
  From [dbo].[HubGecerlilikTarihi]
  Where KonuTipiId = 21103
  and   IsActive = 1
  order by Tarih desc

  Select ' if ( Select count(Id) as Adet From dbo.MtskUcretSinav '
  + ' Where FirmId = @FIRM_ID '  
  + ' and GecerlilikTarihiId = @TarihId ' 
  + ' ) = 0 begin '
  + '  Insert into [dbo].[MtskUcretSinav] ([FirmId],[IsActive],[GecerlilikTarihiId],[ESinavUcreti],[UygulamaSinavUcreti]) Values '
  + ' ( @FIRM_ID, '
  + Convert(varchar(2), [IsActive]) + ', '
  + ' @TarihId, '
  + Convert(varchar(22), isnull([ESinavUcreti],0)) + ', ' 
  + Convert(varchar(22), isnull([UygulamaSinavUcreti],0)) + ') end '
  + ' else begin '
  + ' UPDATE [dbo].[MtskUcretSinav] '
  + ' SET [IsActive] = ' + Convert(varchar(2), [IsActive]) 
  + ' ,[ESinavUcreti] = ' + Convert(varchar(22), isnull([ESinavUcreti],0)) 
  + ' ,[UygulamaSinavUcreti] = ' + Convert(varchar(22), isnull([UygulamaSinavUcreti],0)) 
  + ' end '
  From [dbo].[HubMtskUcretSinav]
  Where GecerlilikTarihiId = @GecerlilikTarihiId

";
            return sql;
        }

        public string getMtskUcretTeorikSql()
        {
            string sql = "";
            sql = @"
  Declare @GecerlilikTarihiId int

  Select top 1 @GecerlilikTarihiId = [Id]
  From [dbo].[HubGecerlilikTarihi]
  Where KonuTipiId = 21104
  and IsActive = 1
  order by Tarih desc

  Select ' if ( Select count(Id) as Adet From dbo.MtskUcretTeorik '
  + ' Where FirmId = @FIRM_ID '  
  + ' and GecerlilikTarihiId = @TarihId ' 
  + ' ) = 0 begin '
  + '  Insert into [dbo].[MtskUcretTeorik] ([FirmId],[IsActive],[IlKodu],[GecerlilikTarihiId],[BirSaatTeorikDersUcreti]) Values '
  + ' ( @FIRM_ID, '
  + Convert(varchar(2), [IsActive]) + ', '
  + Convert(varchar(4), isnull([IlKodu],0)) + ', '
  + ' @TarihId, '
  + Convert(varchar(22), isnull([BirSaatTeorikDersUcreti],0)) + ') end '
  + ' else begin '
  + ' UPDATE [dbo].[MtskUcretTeorik] '
  + ' SET [IsActive] = ' + Convert(varchar(2), [IsActive]) 
  + ' ,[BirSaatTeorikDersUcreti] = ' + Convert(varchar(22), isnull([BirSaatTeorikDersUcreti],0)) 
  + ' Where FirmId = @FIRM_ID '  
  + ' and GecerlilikTarihiId = @TarihId ' 
  + ' end '
  From [dbo].[HubMtskUcretTeorik]
  Where GecerlilikTarihiId = @GecerlilikTarihiId
  and   IlKodu = :FIRM_ILKODU 

";
            return sql;
        }

        public string getMtskUcretUygulamaSql()
        {
            string sql = "";
            sql = @"
  Declare @GecerlilikTarihiId int

  Select top 1 @GecerlilikTarihiId = [Id]
  From [dbo].[HubGecerlilikTarihi]
  Where KonuTipiId = 21104
  and IsActive = 1
  order by Tarih desc

  Select ' if ( Select count(Id) as Adet From dbo.MtskUcretUygulama '
  + ' Where FirmId = @FIRM_ID '  
  + ' and GecerlilikTarihiId = @TarihId ' 
  + ' and isnull([MevcutSertifikaTipiId],0) = ' + Convert(varchar(6),  isnull([MevcutSertifikaTipiId],0))
  + ' and isnull([IstenenSertifikaTipiId],0) = ' + Convert(varchar(6),  isnull([IstenenSertifikaTipiId],0))
  + ' ) = 0 begin '
  + ' Insert into [dbo].[MtskUcretUygulama] ([FirmId],[IsActive],[IlKodu],[SiraNo],[GecerlilikTarihiId] '
  + '      ,[MevcutSertifikaTipiId],[IstenenSertifikaTipiId] '
  + '      ,[SertifikaUcreti],[SertifikaUcreti2016Oncesi],[Arti24HakUcreti],[NakilArti24HakUcreti] '
  + '      ,[BirSaatUygDersUcreti],[BirSaatUygDersUcreti2016Oncesi],[BirSaatArti24DersUcreti],[BirSaatNakilArti24DersUcreti] '
  + ' ) Values '
  + ' ( @FIRM_ID, '
  + Convert(varchar(2), [IsActive]) + ', '
  + Convert(varchar(4), isnull([IlKodu],0)) + ', '
  + Convert(varchar(4), [SiraNo]) + ', '
  + ' @TarihId, '
  + Convert(varchar(6),  isnull([MevcutSertifikaTipiId],0)) + ', '
  + Convert(varchar(6),  isnull([IstenenSertifikaTipiId],0)) + ', '
  + Convert(varchar(22), isnull([SertifikaUcreti],0)) + ', '
  + Convert(varchar(22), isnull([SertifikaUcreti2016Oncesi],0)) + ', '
  + Convert(varchar(22), isnull([Arti24HakUcreti],0)) + ', '
  + Convert(varchar(22), isnull([NakilArti24HakUcreti],0)) + ', '
  + Convert(varchar(22), isnull([BirSaatUygDersUcreti],0)) + ', '
  + Convert(varchar(22), isnull([BirSaatUygDersUcreti2016Oncesi],0)) + ', '
  + Convert(varchar(22), isnull([BirSaatArti24DersUcreti],0)) + ', '
  + Convert(varchar(22), isnull([BirSaatNakilArti24DersUcreti],0)) + ' '
  + ') end '
  + ' else begin '
  + ' UPDATE [dbo].[MtskUcretUygulama] '
  + ' SET [IsActive] = ' + Convert(varchar(2), [IsActive])
  + ' ,[SertifikaUcreti] = '+ Convert(varchar(22), isnull([SertifikaUcreti],0))
  + ' ,[SertifikaUcreti2016Oncesi] = ' + Convert(varchar(22), isnull([SertifikaUcreti2016Oncesi],0))
  + ' ,[Arti24HakUcreti] = ' + Convert(varchar(22), isnull([Arti24HakUcreti],0)) 
  + ' ,[NakilArti24HakUcreti] = ' + Convert(varchar(22), isnull([NakilArti24HakUcreti],0)) 
  + ' ,[BirSaatUygDersUcreti] = ' + Convert(varchar(22), isnull([BirSaatUygDersUcreti],0)) 
  + ' ,[BirSaatUygDersUcreti2016Oncesi] = ' + Convert(varchar(22), isnull([BirSaatUygDersUcreti2016Oncesi],0)) 
  + ' ,[BirSaatArti24DersUcreti] = ' + Convert(varchar(22), isnull([BirSaatArti24DersUcreti],0)) 
  + ' ,[BirSaatNakilArti24DersUcreti] = ' + Convert(varchar(22), isnull([BirSaatNakilArti24DersUcreti],0)) 
  + ' Where FirmId = @FIRM_ID '  
  + ' and GecerlilikTarihiId = @TarihId ' 
  + ' and isnull([MevcutSertifikaTipiId],0) = ' + Convert(varchar(6),  isnull([MevcutSertifikaTipiId],0))
  + ' and isnull([IstenenSertifikaTipiId],0) = ' + Convert(varchar(6),  isnull([IstenenSertifikaTipiId],0))
  + ' end '
  From [dbo].[HubMtskUcretUygulama]
  Where GecerlilikTarihiId = @GecerlilikTarihiId
  and   IlKodu = :FIRM_ILKODU 

";


/*
  + '      --,[BirSaatBasarisizDersUcreti],[BirSaatTelafiDersUcreti] '
  + '      --,[BirSaatOzelDersUcretiAday],[BirSaatOzelDersUcretiHarici] '

  -- +', ' + Convert(varchar(22), isnull([BirSaatBasarisizDersUcreti], 0)) +', '
  -- + Convert(varchar(22), isnull([BirSaatTelafiDersUcreti], 0)) +', '
  -- + Convert(varchar(22), isnull([BirSaatOzelDersUcretiAday], 0)) +', '
  -- + Convert(varchar(22), isnull([BirSaatOzelDersUcretiHarici], 0)) 

  --+' ,[BirSaatBasarisizDersUcreti] = ' + Convert(varchar(22), isnull([BirSaatBasarisizDersUcreti], 0)) 
  --+' ,[BirSaatTelafiDersUcreti] = ' + Convert(varchar(22), isnull([BirSaatTelafiDersUcreti], 0)) 
  --+' ,[BirSaatOzelDersUcretiAday] = ' + Convert(varchar(22), isnull([BirSaatOzelDersUcretiAday], 0)) 
  --+' ,[BirSaatOzelDersUcretiHarici] = ' + Convert(varchar(22), isnull([BirSaatOzelDersUcretiHarici], 0)) 
*/


            return sql;
        }

        public string getGecerlilikTarihiIdSql(Int16 KonuTipiId)
        {
            string sql = "";

            /*
            Id      KonuTipi              GrupTipiId
            21101	Teorik saatler        211
            21102   Uygulama saatleri     211
            21103   Sınav ücreti          211
            21104   Teodrik ders ücreti   211
            21105   Uygulama ders ücreti  211
            */
            sql = @"
  Select top 1 @TarihId = [Id]
  From [dbo].[GecerlilikTarihi]
  Where KonuTipiId = " + KonuTipiId.ToString() + @"
  and IsActive = 1
  order by Tarih desc

";
            return sql;
        }

        public string getDataUpdatesLastIdSql()
        {
            string lastId = "";
            lastId = getDataUpdateTableLastId(v.dataUpdateTable.GecerlilikTarihi);
            setUnionAll(ref lastId);
            lastId += getDataUpdateTableLastId(v.dataUpdateTable.MtskSaatTeorik);
            setUnionAll(ref lastId);
            lastId += getDataUpdateTableLastId(v.dataUpdateTable.MtskSaatUygulama);
            setUnionAll(ref lastId);
            lastId += getDataUpdateTableLastId(v.dataUpdateTable.MtskUcretSinav);
            setUnionAll(ref lastId);
            lastId += getDataUpdateTableLastId(v.dataUpdateTable.MtskUcretTeorik);
            setUnionAll(ref lastId);
            lastId += getDataUpdateTableLastId(v.dataUpdateTable.MtskUcretUygulama);
            return lastId;
        }

        public string getHubDataUpdatesFindNewRowsSql(DataSet dsLastIds)
        {
            string sql = "";
            
            sql = getHubDataUpdatesTableNewRows_(v.dataUpdateTable.GecerlilikTarihi, dsLastIds);
            setUnionAll(ref sql);
            sql += getHubDataUpdatesTableNewRows_(v.dataUpdateTable.MtskSaatTeorik, dsLastIds);
            setUnionAll(ref sql);
            sql += getHubDataUpdatesTableNewRows_(v.dataUpdateTable.MtskSaatUygulama, dsLastIds);
            setUnionAll(ref sql);
            sql += getHubDataUpdatesTableNewRows_(v.dataUpdateTable.MtskUcretSinav, dsLastIds);
            setUnionAll(ref sql);
            sql += getHubDataUpdatesTableNewRows_(v.dataUpdateTable.MtskUcretTeorik, dsLastIds);
            setUnionAll(ref sql);
            sql += getHubDataUpdatesTableNewRows_(v.dataUpdateTable.MtskUcretUygulama, dsLastIds);

            return sql;
        }

        private string getHubDataUpdatesTableNewRows_(v.dataUpdateTable dataUpdateTable, DataSet dsLastIds)
        {
            string sql = "";
            int pos = ((byte)dataUpdateTable);
            string hubTableName = getHubDataUpdateTabeleName(dataUpdateTable);
            string lastId = "0";

            int length = dsLastIds.Tables[0].Rows.Count;
            for (int i = 0; i < length; i++)
            {
                var xx = dsLastIds.Tables[0].Rows[i]["TableId"];
                if (dsLastIds.Tables[0].Rows[i]["TableId"].ToString() == pos.ToString())
                {
                    lastId = dsLastIds.Tables[0].Rows[i]["LastId"].ToString();
                    break;
                }
            }

            sql = " Select " + ((byte)dataUpdateTable).ToString() + " as TableId, isnull(min(Id),0) as startId From " + hubTableName + " Where Id > " + lastId + " ";

            return sql;
        }
        private string getDataUpdateTabeleName(v.dataUpdateTable dataUpdateTable)
        {
            string tName = "";
            if (dataUpdateTable == v.dataUpdateTable.GecerlilikTarihi) tName = "GecerlilikTarihi";
            if (dataUpdateTable == v.dataUpdateTable.MtskSaatTeorik) tName = "MtskSaatTeorik";
            if (dataUpdateTable == v.dataUpdateTable.MtskSaatUygulama) tName = "MtskSaatUygulama";
            if (dataUpdateTable == v.dataUpdateTable.MtskUcretSinav) tName = "MtskUcretSinav";
            if (dataUpdateTable == v.dataUpdateTable.MtskUcretTeorik) tName = "MtskUcretTeorik";
            if (dataUpdateTable == v.dataUpdateTable.MtskUcretUygulama) tName = "MtskUcretUygulama";
            return tName;
        }
        private string getHubDataUpdateTabeleName(v.dataUpdateTable dataUpdateTable)
        {
            string tName = "";
            if (dataUpdateTable == v.dataUpdateTable.GecerlilikTarihi) tName = "HubGecerlilikTarihi";
            if (dataUpdateTable == v.dataUpdateTable.MtskSaatTeorik) tName = "HubMtskSaatTeorik";
            if (dataUpdateTable == v.dataUpdateTable.MtskSaatUygulama) tName = "HubMtskSaatUygulama";
            if (dataUpdateTable == v.dataUpdateTable.MtskUcretSinav) tName = "HubMtskUcretSinav";
            if (dataUpdateTable == v.dataUpdateTable.MtskUcretTeorik) tName = "HubMtskUcretTeorik";
            if (dataUpdateTable == v.dataUpdateTable.MtskUcretUygulama) tName = "HubMtskUcretUygulama";
            return tName;
        }
        private string getDataUpdateTableLastId(v.dataUpdateTable dataUpdateTable)
        {
            string tableName = getDataUpdateTabeleName(dataUpdateTable);
            return " Select " + ((byte)dataUpdateTable).ToString() + " as TableId, isnull(max(Id),0) as LastId from " + tableName + " "; 
        }
        private void setUnionAll(ref string sql)
        {
            sql += " union all " + v.ENTER;
        }

        public string Sql_prc_MtskGunlukTakipler()
        {
            string sql = "";

            sql = @"
  DECLARE @FirmId int

  Set @FirmId = :FIRM_ID

  EXECUTE [dbo].[prc_MtskGunlukTakipler] @FirmId
";
            return sql;
        }
        public string Sql_MsProjectTables(string tableName, Int16 sectorTypeId)
        {
            return @"
 Select 
   [MsProjectTables].[Id]
 , [MsProjectTables].[IsActive]
 , [MsProjectTables].[SectorTypeId]
 , [MsProjectTables].[SchemaName]
 , [MsProjectTables].[TableName]
 , [MsProjectTables].[SqlScript]
 From [dbo].[MsProjectTables] as [MsProjectTables]
 Where 0 = 0 
 and [MsProjectTables].SectorTypeId = " + sectorTypeId.ToString() + @"
 and [MsProjectTables].TableName = '" + tableName + "' " + @" ";
        }

        public string Sql_prcUstadUserFirmsList()
        {
            return @" EXECUTE [dbo].[prc_UstadUserFirmsListeGet] ";
        }

        public string Sql_DbUpdatesIdList()
        {
            // müşteri database
            return " Select max(MsDbUpdateId) as MsDbUpdateId from DbUpdates ";
        }
        public string Sql_FileUpdatesIdList()
        {
            // müşteri database
            string networkKey = v.tComputer.Network_MACAddress;
            string pcName = v.tComputer.PcName;

            return " Select max(MsFileUpdateId) as MsFileUpdateId from FileUpdates " +
                " where ( isnull(NetworkMacAddress,'') = '" + networkKey + "' and isnull(PcName,'') = '" + pcName + "' ) ";
        }

        public string Sql_WebScrapingFieldsList(string pageCodes)
        {
            return @"
        Select 
          fieldsIP.WebScrapingPageCode
        , fieldsIP.WebScrapingGetNodeId
        , fieldsIP.WebScrapingGetColumnNo
        , fieldsIP.WebScrapingSetNodeId
        , fieldsIP.WebScrapingSetColumnNo
        , fieldsIP.KRT_OPERAND_TYPE as KrtOperandType
        , fieldsIP.SOFTWARE_CODE + '/' + 
          fieldsIP.PROJECT_CODE + '/' +
          fieldsIP.TABLE_CODE + '.' + fieldsIP.IP_CODE as TableIPCode 
        , isnull(fields.FIELD_NAME,'') as FieldName
        , isnull(fields.FLOOKUP_FIELD,'') as FLookUpField
        , isnull(fields.FIELD_TYPE,0) as FieldType

        From MS_FIELDS_IP as fieldsIP
          left outer join MS_FIELDS as fields on ( 
            fieldsIP.SOFTWARE_CODE = fields.SOFTWARE_CODE and
            fieldsIP.PROJECT_CODE = fields.PROJECT_CODE and
            fieldsIP.TABLE_CODE = fields.TABLE_CODE and
            fieldsIP.IP_CODE = fieldsIP.IP_CODE and
            fieldsIP.FIELD_NO = fields.FIELD_NO )
        Where 0 = 0 --fieldsIP.DEFAULT_TYPE = 25 or fieldsIP.DEFAULT_TYPE2 = 25)
        and fieldsIP.WebScrapingPageCode <> ''
        and fieldsIP.WebScrapingPageCode in (" + pageCodes + @")
        order by fieldsIP.WebScrapingPageCode, 
                 fieldsIP.WebScrapingGetNodeId,
                 fieldsIP.WebScrapingGetColumnNo 
            ";
        }
        public string Sql_WebScrapingFieldsListOld(string pageCodes)
        {
            return @"
        Select 
          fieldsIP.WebScrapingPageCode
        , fieldsIP.WebScrapingGetNodeId
        , fieldsIP.WebScrapingGetColumnNo
        , fieldsIP.WebScrapingSetNodeId
        , fieldsIP.WebScrapingSetColumnNo
        , fieldsIP.KRT_OPERAND_TYPE
        , fieldsIP.SOFTWARE_CODE + '/' + 
          fieldsIP.PROJECT_CODE + '/' +
          fieldsIP.TABLE_CODE + '.' + fieldsIP.IP_CODE as TableIPCode 
        , fields.FIELD_NAME
        , fields.FLOOKUP_FIELD
        , fields.FIELD_TYPE

        From MS_FIELDS_IP as fieldsIP
          left outer join MS_FIELDS as fields on ( 
            fieldsIP.SOFTWARE_CODE = fields.SOFTWARE_CODE and
            fieldsIP.PROJECT_CODE = fields.PROJECT_CODE and
            fieldsIP.TABLE_CODE = fields.TABLE_CODE and
            fieldsIP.IP_CODE = fieldsIP.IP_CODE and
            fieldsIP.FIELD_NO = fields.FIELD_NO )
        Where 0 = 0 --fieldsIP.DEFAULT_TYPE = 25 or fieldsIP.DEFAULT_TYPE2 = 25)
        and fieldsIP.WebScrapingPageCode <> ''
        and fieldsIP.WebScrapingPageCode in (" + pageCodes + @")
        order by fieldsIP.WebScrapingPageCode, 
                 fieldsIP.WebScrapingGetNodeId,
                 fieldsIP.WebScrapingGetColumnNo 
            ";
        }

        public string Sql_WebNodeItemsList(string pageCodes)
        {
            return @"
            Select 
              [Id]
             ,[NodeId]
             ,[PageCode]
             ,[IsActive]
             ,[ItemValue]
             ,[ItemText]
             From   [MsWebNodeItems]
             Where  [PageCode] in (" + pageCodes + @") 
             order by NodeId, ItemValue ";
        }

        #endregion ManagerServer Tables SQLs Preparing

        #region Preparing dsData

        public void Preparing_dsData(Form tForm,
            DataRow row, DataSet dsFields, ref DataSet dsData,
            string MultiPageID, vTableAbout vTA)
        {
            tToolBox t = new tToolBox();

            #region Tanımlar
            string function_name = "Preparing_dsData";
            string snc = string.Empty;
            string NewSQL = string.Empty;
            //string RefID_Target = string.Empty;
            string RefID_Second = string.Empty;
            string RefID_SubView = string.Empty;
            string REF_CALL = string.Empty;

            byte dBaseNo = t.Set(row["LKP_DBASE_TYPE"].ToString(), "", (byte)3);
            string DBaseName = t.Find_dBLongName(dBaseNo.ToString());
            string IP_Caption = t.Set(row["IP_CAPTION"].ToString(), "", "");
            string TableName = t.Set(row["LKP_TABLE_NAME"].ToString(), "", "null");
            string TableLabel = "[" + t.Set(row["TABLE_CODE"].ToString(), "", "") + "]";
            string Where_IP_Add = t.Set(row["IP_WHERE_SQL"].ToString(), row["LKP_TB_WHERE_SQL"].ToString(), "");
            string OrderBy = t.Set(row["IP_ORDER_BY"].ToString(), row["LKP_TB_ORDER_BY"].ToString(), "");
            string Block_Field = t.Set(row["BLOCK_PRIMARY_FNAME"].ToString(), "", "");

            string KeyFName = t.Set(row["LKP_KEY_FNAME"].ToString(), "", "null");
            if (t.IsNotNull(KeyFName) == false)
                KeyFName = t.Find_Table_Ref_FieldName(dsFields.Tables[0]);

            string SchemasDot = "";
            string Schemas = t.Set(row["LKP_SCHEMAS_CODE"].ToString(), "", "null");
            if (t.IsNotNull(Schemas))
                SchemasDot = Schemas + ".";

            // for know-how 
            string softCode = t.Set(row["SOFTWARE_CODE"].ToString(), "", "");
            string projectCode = t.Set(row["PROJECT_CODE"].ToString(), "", "");

            string TableCode = t.Set(row["TABLE_CODE"].ToString(), "", "");
            string IPCode = t.Set(row["IP_CODE"].ToString(), "", "");

            string TableIPCode = "";

            if ((softCode != "") && (projectCode != ""))
                 TableIPCode = softCode + "/" + projectCode + "/" + TableCode + "." + IPCode;
            else TableIPCode = TableCode + "." + IPCode;
                       
            string Data_Find = t.Set(row["DATA_FIND"].ToString(), "", ""); /* 0= Find yok, 1. standart, 2. List&Data   */
            string Find_FName = t.Set(row["FIND_FNAME"].ToString(), "", "");
            string Lock_FName = t.Set(row["LKP_LOCK_FIELD_NAME"].ToString(), "", "");
            string AutoInsert = t.Set(row["AUTO_INSERT"].ToString(), "", "");
            bool IsUseNewRefId = t.Set(row["IS_USE_NEW_REFID"].ToString(), "", false);
            string UseNewRefId = "";

            if (IsUseNewRefId)
                UseNewRefId = " and " + TableLabel + "." + KeyFName + " = -1 ";

            string Prop_SubView = t.Set(row["PROP_SUBVIEW"].ToString(), row["LKP_PROP_SUBVIEW"].ToString(), "");
            #region Prop_SubView (Kendi altında SUBVIEW var ise)

            // old
            if (Prop_SubView.IndexOf("=SV_ENABLED:TRUE;") > -1)
            {
                tForm.IsAccessible = true;
            } // json
            else if (t.Find_Properties_Value(Prop_SubView, "SUBVIEW_ENABLED") == "TRUE")
            {
                tForm.IsAccessible = true;
            }
            else
            {
                Prop_SubView = string.Empty;
            }
            #endregion Prop_SubView

            // Navigator Buton bilgileri
            // Kart butonları,  51.Kart Aç, 52.Yeni Kart Aç  
            // Fiş  butonları,  56.Fişi Aç, 56.Yeni Fiş Aç
            string Prop_Navigator = t.Set(row["PROP_NAVIGATOR"].ToString(), "", "");

            // Tespite gitmeden önce RunTime varmı yokmu kontrol için 
            string Prop_Runtime = t.Set(row["PROP_RUNTIME"].ToString(), "", "");
            #region Prop_Runtime var ise
            if (t.IsNotNull(Prop_Runtime))
                Prop_Runtime = "True";
            else Prop_Runtime = "False";
            #endregion Prop_Runtime var ise

            string ForeingID = string.Empty;
            string Kisitlama = string.Empty;
            string masterFields = string.Empty;
            string join_Fields = string.Empty;
            string join_Tables = string.Empty;
            string About_Detail_SubDetail = string.Empty;
            string Where_Lines = string.Empty;
            string declare = string.Empty;
            string setlist = string.Empty;
            string TargetValue = string.Empty;
            string Report_FieldsName = string.Empty;
            string Lkp_Memory_Fields = string.Empty;
            string DataWizardTabPage = string.Empty;
            string DataCopyCode = string.Empty;
            string MasterDetailColumns = string.Empty;
            string MaliDonemChanger = "null";
            string Master_Detail_Harmony = string.Empty;

            /// TABLE_TYPE  
            /// 1, 'Table'
            /// 2, 'View' 
            /// 3, 'Stored Procedure'
            /// 4, 'Function'
            /// 5, 'Trigger'
            /// 6, 'Select'

            int Table_Type = t.Set(row["TABLE_TYPE"].ToString(), row["LKP_TABLE_TYPE"].ToString(), (byte)1);

            /// DATA_READ_TYPE
            /// 1, '', 'Not Read Data'
            /// 2, '', 'Read RefID'
            /// 3, '', 'Detail Table'
            /// 4, '', 'SubDetail Table'
            /// 5, '', 'Data Collection'
            /// 6, '', 'Read SubView'

            int Data_Read_Type = t.Set(row["DATA_READ"].ToString(), "", (byte)1);

            // TableType.Select
            if (Table_Type == 6)
            {
                NewSQL = t.Set(row["IP_SELECT_SQL"].ToString(), row["LKP_TB_SELECT_SQL"].ToString(), "null");

                if (t.IsNotNull(NewSQL))
                    NewSQL = NewSQL + "     ";
            }

            #endregion Tanımlar

            #region Read Properties Values on Form

            string form_prp = t.Set(t.myFormBox_Values(tForm, v.FormLoad),  // Formun üzerindeki MemoEdit ten alınıyor
                                     v.con_FormLoadValue,                    // Gecici Hafızaya aktarılan bilgi
                                     string.Empty);

            // AddIP_ ile Form_Shown sırasında burada belirtilen TableIPCode da Create_InputPanel de oluşturulsun

            // RaporView de Konu Olan kısmını doldurmak için kullanılmakta
            // çünkü önceden neyin Konu Olan belli değil

            if (form_prp.IndexOf("AddIP_TableIPCode") > 0) v.con_AddIP = true;

            #endregion Read Properties Values on Form

            #region Not Read Data // Kısıtlama

            if ( //(Table_Type == 1) &&       // v.TableType.Table
                 ((Data_Read_Type == 1) || (Data_Find == "2") || (vTA.IsKisitlama) ) &&   // v.DataReadType.NotReadData veya List&Data ise 
                 (t.IsNotNull(KeyFName)))  // KefFName biliniyorsa
            {
                // tüm datanında okunmaması için geçiçi bir kısıtlama eklenmekte   
                Kisitlama =
                  " /*KISITLAMALAR|1|*/ " + v.ENTER +
                  " and " + TableLabel + "." + KeyFName + " = -1 " + v.ENTER +
                  " /*KISITLAMALAR|2|*/ " + v.ENTER;
            }

            #endregion Not Raed Data

            #region Detail Table (Master-Detail Bağlantısı)
            if (Data_Read_Type == 3)
            {
                //LKP_MASTER_TABLE_NAME
                //LKP_MASTER_KEY_FNAME
                //LKP_FOREING_FNAME

                string Master_TableIPCode = t.Set(row["MASTER_TABLEIPCODE"].ToString(), row["LKP_MASTER_TABLEIPCODE"].ToString(), "null");
                string Master_TableName = t.Set(row["MASTER_TABLE_NAME"].ToString(), row["LKP_MASTER_TABLE_NAME"].ToString(), "null");
                string Master_KeyFName = t.Set(row["MASTER_KEY_FNAME"].ToString(), row["LKP_MASTER_KEY_FNAME"].ToString(), "null");
                string Foreing_FName = t.Set(row["FOREING_FNAME"].ToString(), row["LKP_FOREING_FNAME"].ToString(), "null");

                string MasterValue1 = string.Empty;

                if (t.IsNotNull(Master_TableIPCode))
                    MasterValue1 = t.Find_TableIPCode_Value(tForm, Master_TableIPCode, Master_KeyFName);

                if (t.IsNotNull(MasterValue1) == false)
                {
                    MasterValue1 = "-1";
                    TargetValue = "NewRecord";
                }

                if ((t.IsNotNull(Foreing_FName)) && (t.IsNotNull(MasterValue1)))
                {
                    //  BASLIK_ID = xxx         veya
                    //  BASLIK_ID = -1          vaya
                    //  BASLIK_ADI = 'xxx'      vaya
                    //  BASLIK_ADI = ''         gibi sonuclar döner  

                    snc = t.Set_FieldName_Value(dsFields, Foreing_FName, MasterValue1, "and", "=");

                    if ((snc != string.Empty) && (MasterValue1 != "-1"))
                    {
                        ForeingID = " and " + TableLabel + "." + snc + v.ENTER;
                    }
                }


            }
            #endregion Detail Table

            #region SubDetail Table / Data Collection
            if ((Data_Read_Type == 4) || // v.DataReadType.SubDetailTable
                (Data_Read_Type == 5))   // v.DataReadType.DataCollection 
            {
                // form_shown sırasında üzerinde Detail-SubDetail bağlıtısı olan 
                // tabloların olduğunu işaret için bu özellik true ediliyor
                // bakınız [ myForm_Shown ] a 
                tForm.IsAccessible = true;
            }
            #endregion SubDetail Table

            #region v.DataReadType.ReadSubView
            if (Data_Read_Type == 6)
            {
                if (form_prp.IndexOf("SVIEWVALUE") > -1)
                {
                    if (form_prp.IndexOf("TABLEIPCODE_LIST") > -1)
                    {
                        Preparing_RefID(dsFields, ref NewSQL, ref TargetValue,
                                            TableIPCode, TableLabel, form_prp, Data_Read_Type);
                    }
                    else if (form_prp.IndexOf("[{") > -1)
                    {
                        Preparing_RefID_JSON(dsFields,
                                             ref NewSQL, ref TargetValue, ref DataCopyCode,
                                             TableIPCode, TableLabel, form_prp, UseNewRefId, Data_Read_Type);
                    }
                }

            }
            #endregion v.DataReadType.ReadSubView

            #region Block Table

            if (t.IsNotNull(Block_Field))
                Block_Field = " and " + TableLabel + "." + Block_Field + v.ENTER;

            #endregion Block Table

            #region DataWizardTabPage find, ( True or False )
            DataWizardTabPage = "False";
            if (vTA.dWTabPageCount > 0)
                DataWizardTabPage = "True";
            #endregion

            #region Preparing SubDetail, DataCollection, Join Tables and Fields

            Preparing_Join_Fields_Tables(tForm, row, dsFields,
                          dBaseNo,
                          TableIPCode,
                          TableCode,
                      ref masterFields,
                      ref join_Tables,
                      ref join_Fields,
                      ref Lkp_Memory_Fields,
                      ref About_Detail_SubDetail,
                      ref Where_Lines,
                      ref declare,
                      ref setlist,
                      ref NewSQL,
                      ref REF_CALL,
                      ref Report_FieldsName,
                      ref MasterDetailColumns,
                      ref MaliDonemChanger,
                      ref Master_Detail_Harmony);

            #endregion Preparing Join Tables and Fields

            #region Preparing SQL
            //ViewData_SQL =
            //    String.Format(" Select {0} * from [{1}].[dbo].[{2}] ", Adet, DatabaseName, TableName);

            #region Table için SQL hazırlanıyor
            if (Table_Type == 1)  // v.TableType.Table
            {
                //if (masterFields == "")
                if (TableIPCode.ToUpper().IndexOf("RAPOR") == -1)
                    masterFields = TableLabel + ".* " + v.ENTER;
                else
                    masterFields = getTableFieldsList(dsFields, TableLabel);

                NewSQL =
                    " Select " + masterFields + //TableLabel + ".* " + v.ENTER +
                    join_Fields + v.ENTER +
                    Lkp_Memory_Fields +
                    " from " + SchemasDot + TableName + " as " + TableLabel + " with (nolock) " + v.ENTER +
                    join_Tables + v.ENTER +
                    " where 0 = 0 " + v.ENTER +
                    " /*" + TableLabel + ".DEFAULT_VALUE*/" + v.ENTER +
                    //REF_CALL + // iptal kontrolü ( false = normal kayıt,  true = iptal edilmiş kayıt  )
                    //Block_Field +
                    //ForeingID +
                    //RefID_Target +
                    //Kisitlama +
                    //Where_IP_Add +
                    //Where_Lines +
                    //" /*" + TableLabel + ".KRITERLER*/ " + v.ENTER;
                    " /*KRITERLER*/ " + v.ENTER +
                    " /*KRITERLER_END*/ " + v.ENTER;
            }
            #endregion Table

            #region Stored Procedure için SQL hazırlanıyor
            if (Table_Type == 3)
            {
                string Master_TableName = t.Set(row["MASTER_TABLE_NAME"].ToString(), row["LKP_MASTER_TABLE_NAME"].ToString(), "null");
                // 
                //if ((dBaseNo == 4) && (v.dBTypes.ProjectDBType == v.dBaseType.MSSQL))
                if (v.active_DB.projectDBType == v.dBaseType.MSSQL)
                {
                    //NewSQL =
                    //    //"-- D.SD.DCL --" + v.ENTER +
                    //    //"-- D.SD.SET --" + v.ENTER +
                    //    " EXEC [dbo].[" + Master_TableName + "]" + v.ENTER +
                    //           Where_Lines;

                    string s = " EXEC [dbo].[" + Master_TableName + "]" + v.ENTER;
                    Where_Lines = Where_Lines.Replace("/*executeBegin*/", s);

                    NewSQL = Where_Lines;

                }
            }
            #endregion Stored Procedure

            #region Select Cümlesine Ekler

            if (t.IsNotNull(NewSQL)) // select var ise
            {
                #region // Kisitlama add
                if (t.IsNotNull(Kisitlama))
                {
                    NewSQL = t.SQLWhereAdd(NewSQL, TableLabel, Kisitlama + v.ENTER, "DEFAULT");
                }
                #endregion // Kisitlama

                #region // Where_IP_Add add
                if (t.IsNotNull(Where_IP_Add))
                {
                    /// "and BStokB.Id = -1" > " and BStokB.Id = -1 "  
                    /// şeklinde değiştirmezsek  changeKeyFieldValue çalışmıyor, bulamıyor
                    /// 
                    Where_IP_Add = " " + Where_IP_Add + " ";

                    NewSQL = t.SQLWhereAdd(NewSQL, TableLabel, Where_IP_Add + v.ENTER, "DEFAULT");
                }
                #endregion // Where_IP_Add add

                #region // UseNewRefId add
                if (t.IsNotNull(UseNewRefId))
                {
                    NewSQL = t.SQLWhereAdd(NewSQL, TableLabel, UseNewRefId + v.ENTER, "DEFAULT");
                }
                #endregion // UseNewRefId add

                #region // Where_Lines add
                if (t.IsNotNull(Where_Lines))
                {
                    NewSQL = t.SQLWhereAdd(NewSQL, TableLabel, Where_Lines, "DEFAULT");
                }
                #endregion // Where_Lines add

                #region // RefID add
                if (t.IsNotNull(form_prp) && (form_prp != "[]"))
                {
                    if (form_prp.IndexOf("TABLEIPCODE_LIST") > -1)
                    {
                        Preparing_RefID_OLD(dsFields, ref NewSQL, ref TargetValue,
                                            TableIPCode, TableLabel, form_prp, Data_Read_Type);
                    }
                    else if (form_prp.IndexOf("[{") > -1)
                    {
                        if (form_prp.IndexOf(TableIPCode) > -1)
                        {
                            /// Form açarken "READ" value ile istenen kayıt açılıyor
                            /// 42 Kart Aç
                            /// 
                            Preparing_RefID_JSON(dsFields,
                                                 ref NewSQL, ref TargetValue, ref DataCopyCode,
                                                 TableIPCode, TableLabel, form_prp, UseNewRefId, Data_Read_Type);
                        }
                    }
                    else
                    {
                        Preparing_Manuel_RefID(dsFields, ref NewSQL, ref TargetValue,
                            TableIPCode, TableLabel, KeyFName, form_prp);
                    }
                }
                #endregion // RefID add

                #region // SQL içinde Declare ve Set ibareleri kullanıldıysa
                if ((t.IsNotNull(declare)) &&
                    (Table_Type == 6))   // Select ise,  Stored Procedure değilse
                {
                    // Detail-SubDetail bağlantısı sırasında 
                    // master_fname önüne @ ibaresi konularak devreye giriyor

                    string str_dcl = "-- D.SD.DCL --";
                    string str_set = "-- D.SD.SET --";

                    int i_dcl = NewSQL.IndexOf(str_dcl);

                    if (i_dcl == -1)
                    {
                        str_dcl = "/*declareBegin*/";
                        i_dcl = NewSQL.IndexOf(str_dcl);
                    }

                    if (i_dcl > -1)
                    {
                        i_dcl = i_dcl + str_dcl.Length + 1;
                        NewSQL = NewSQL.Insert(i_dcl, v.ENTER + declare);
                    }
                    
                    if (i_dcl == -1)
                        MessageBox.Show("DİKKAT : SQL içinde aranan ibare bulunamadı ...(" + str_dcl + ")", function_name);
                    

                    int i_set = NewSQL.IndexOf(str_set);

                    if (i_set == -1)
                    {
                        str_set = "/*setBegin*/";
                        i_set = NewSQL.IndexOf(str_set);
                    }

                    if (i_set > -1)
                    {
                        i_set = i_set + str_set.Length + 1;
                        NewSQL = NewSQL.Insert(i_set, v.ENTER + setlist);
                    }
                        
                    if (i_set == -1)    
                        MessageBox.Show("DİKKAT : SQL içinde aranan ibare bulunamadı ...(" + str_set + ")", function_name);
                }
                #endregion // Declare
            }

            #endregion // Select Cümlesine Ekler

            #region OrderBy

            if ((t.IsNotNull(NewSQL)) &&
                 (t.IsNotNull(OrderBy)))
                NewSQL = NewSQL + " order by " + OrderBy + v.ENTER;

            #endregion OrderBy

            #endregion Preparing SQL

            #region Know-how Preparing (Bilgiler Hazırlanıyor)

            string myProp = string.Empty;
            string FieldsListSQL = SQL_Table_FieldsList(DBaseName, TableName, IPCode);

            //DBaseType >> DBase_Type >> DBaseName DBaseNo
            t.MyProperties_Set(ref myProp, "FormName", tForm?.Name.ToString());
            t.MyProperties_Set(ref myProp, "DBaseNo", dBaseNo.ToString());
            t.MyProperties_Set(ref myProp, "DBaseName", DBaseName.ToString());
            t.MyProperties_Set(ref myProp, "SchemasCode", Schemas);
            t.MyProperties_Set(ref myProp, "TableIPCode", TableIPCode);
            t.MyProperties_Set(ref myProp, "TableName", TableName);
            t.MyProperties_Set(ref myProp, "TableLabel", TableLabel);
            t.MyProperties_Set(ref myProp, "TableCaption", IP_Caption);
            t.MyProperties_Set(ref myProp, "TableType", Table_Type.ToString());
            t.MyProperties_Set(ref myProp, "Cargo", "data"); // cargo = data, param, report
            t.MyProperties_Set(ref myProp, "MultiPageID", MultiPageID);
            t.MyProperties_Set(ref myProp, "DataFind", Data_Find);
            t.MyProperties_Set(ref myProp, "FindFName", Find_FName);
            t.MyProperties_Set(ref myProp, "LockFName", Lock_FName);
            t.MyProperties_Set(ref myProp, "AutoInsert", AutoInsert);
            t.MyProperties_Set(ref myProp, "IsUseNewRefId", IsUseNewRefId.ToString());
            t.MyProperties_Set(ref myProp, "OrderBy", OrderBy);
            //t.MyProperties_Set(ref myProp, "JoinFields", join_Fields); // "LkpFields", Lkp_Fields);
            //t.MyProperties_Set(ref myProp, "JoinTables", join_Tables); // "LkpTables", Lkp_Tables);
            t.MyProperties_Set(ref myProp, "KeyFName", KeyFName);
            t.MyProperties_Set(ref myProp, "KeyIdValue", "-1");
            t.MyProperties_Set(ref myProp, "Kisitlama", Kisitlama);
            t.MyProperties_Set(ref myProp, "ForeingID", ForeingID);
            t.MyProperties_Set(ref myProp, "Where_IP_Add", Where_IP_Add);
            t.MyProperties_Set(ref myProp, "Prop_Runtime", Prop_Runtime);
            t.MyProperties_Set(ref myProp, "Prop_SubView", Prop_SubView);
            t.MyProperties_Set(ref myProp, "DataReadType", Data_Read_Type.ToString());
            t.MyProperties_Set(ref myProp, "DataWizardTabPage", DataWizardTabPage);
            t.MyProperties_Set(ref myProp, "DataCopyCode", DataCopyCode);
            t.MyProperties_Set(ref myProp, "MasterDetailColumns", MasterDetailColumns);
            t.MyProperties_Set(ref myProp, "MaliDonemChanger", MaliDonemChanger);
            t.MyProperties_Set(ref myProp, "MasterDetailHarmony", Master_Detail_Harmony);

            //t.MyProperties_Set(ref myProp, "FieldsListSQL", FieldsListSQL);

            if (t.IsNotNull(About_Detail_SubDetail))
            {
                //  About_Detail_SubDetail:MasterTableIPCode||MasterTableName||MasterKeyFName||DetailFName;
                t.MyProperties_Set(ref myProp, "DetailSubDetail", "True");
                myProp = myProp + About_Detail_SubDetail;

                // IsAccessible == true  ise formun üzerinde 
                // kendisine (detail'e) bağlı alt tablolar(subdetail) vardır 
                if (tForm != null)
                    tForm.IsAccessible = true;
            }

            t.MyProperties_Set(ref myProp, "SqlFirst", NewSQL);
            t.MyProperties_Set(ref myProp, "SqlSecond", "null");
            t.MyProperties_Set(ref myProp, "DataState", "null");


            // bu atamaylada dsDatanın işlenmemiş SQL ele alınarak üzerine kriterler eklenerek 
            // dsData kriterli yeniden çalıştırılacak
            dsData.DataSetName = TableIPCode;
            dsData.Namespace = myProp;

            v.con_SQL = NewSQL;

            //v.SQL = v.ENTER2 + NewSQL + v.SQL; 
            //v.SQL = v.ENTER +
            //   "[ " + IP_Caption + " - " + TableIPCode + " ]" + v.ENTER +
            //   "===========================================================" + v.ENTER + v.SQL;

            #endregion Know-how

            #region Read dsData
            
            t.Data_Read_Execute(tForm, dsData, ref NewSQL, TableIPCode, null);

            if ((TargetValue == "NewRecord") ||
                (AutoInsert == "True"))
            {
               // if (dsData.Tables[0].Rows.Count == 0)
               //     v.con_AutoNewRecords = true;
            }

            #endregion Read dsData

        }

        private string getTableFieldsList(DataSet dsFields, string tableLabel)
        {
            string fields = "";
            int ftype = 0;
            if (dsFields.Tables.Count > 0)
            {
                dsFields.Tables[0].DefaultView.Sort = "FIELD_NO";
                DataTable dt = dsFields.Tables[0].DefaultView.ToTable();

                foreach (DataRow row in dt.Rows)
                {
                    ftype = Convert.ToInt32(row["LKP_FIELD_TYPE"].ToString());

                    if (row["LKP_FIELD_NAME"].ToString().ToUpper().IndexOf("LKP_") == -1)
                    {
                        if (ftype == 40 || ftype == 58 || ftype == 61)
                        {
                            if (fields == "")
                                fields = " convert(date," + tableLabel + "." + row["LKP_FIELD_NAME"].ToString() + ",103) as " + row["LKP_FIELD_NAME"].ToString() + v.ENTER;
                            else fields += ", convert(date, " + tableLabel + "." + row["LKP_FIELD_NAME"].ToString() + ",103) as " + row["LKP_FIELD_NAME"].ToString() + v.ENTER;
                        }
                        if ((ftype == 56) | (ftype == 48) | (ftype == 127) | (ftype == 52) | (ftype == 60) | (ftype == 62) | (ftype == 59) | (ftype == 108))
                        {
                            if (fields == "")
                                fields = " isnull(" + tableLabel + "." + row["LKP_FIELD_NAME"].ToString() + ",0) as " + row["LKP_FIELD_NAME"].ToString() + v.ENTER;
                            else fields += ", isnull(" + tableLabel + "." + row["LKP_FIELD_NAME"].ToString() + ",0) as " + row["LKP_FIELD_NAME"].ToString() + v.ENTER;
                        }
                        else
                        {
                            if (fields == "")
                                fields = tableLabel + "." + row["LKP_FIELD_NAME"].ToString() + v.ENTER;
                            else fields += ", " + tableLabel + "." + row["LKP_FIELD_NAME"].ToString() + v.ENTER;
                        }
                    }
                }
            }
            
            return fields;
        }

        #region RefID Preparing

        private void Preparing_RefID_JSON(DataSet dsFields,
                     ref string NewSQL,
                     ref string TargetValue,
                     ref string DataCopyCode,
                     string Main_TableIPCode, string Main_TableLabel,
                     string form_prp,
                     string UseNewRefId,
                     int Data_Read_Type)
        {
            // kendinden önceki formdan gelen bilgiler

            // önceki formun Navigator Butonlarından gelen ve 
            // açılan yeni formun mymemo sundaki bilgileri analiz edeceğiz
            // Kart butonları,  51.Kart Aç, 52.Yeni Kart Aç  
            // Fiş  butonları,  56.Fişi Aç, 56.Yeni Fiş Aç

            //'WORKTYPE', 0, '', 'none');
            //'WORKTYPE', 0, 'READ', 'Read Data');
            //'WORKTYPE', 0, 'NEW', 'New Data');
            //'WORKTYPE', 0, 'GOTO', 'Goto Record');
            //'WORKTYPE', 0, 'SVIEW', 'Sub View');
            //'WORKTYPE', 0, 'SVIEWVALUE', 'Sub View Value');
            //'WORKTYPE', 0, 'CREATEVIEW', 'Create View');

            string TABLEIPCODE = string.Empty;
            string TABLEALIAS = string.Empty;
            string KEYFNAME = string.Empty;
            string MSETVALUE = string.Empty;
            string WORKTYPE = string.Empty;
            string RefID = string.Empty;

            List<TABLEIPCODE_LIST> prop_ = JsonConvert.DeserializeObject<List<TABLEIPCODE_LIST>>(form_prp);

            #region
            foreach (var item in prop_)
            {
                TABLEIPCODE = item.TABLEIPCODE.ToString();

                #region 1
                if ((item.WORKTYPE.ToString() == "SVIEWVALUE") &&
                    (TABLEIPCODE == "") &&
                    (Data_Read_Type == 6))
                {
                    TABLEIPCODE = Main_TableIPCode;
                }
                #endregion

                #region 2
                if (item.WORKTYPE.ToString() == "CREATEVIEW")
                {
                    v.con_FormAfterCreateView = true;
                }
                #endregion

                #region 3
                if (Main_TableIPCode == TABLEIPCODE)
                {
                    TABLEALIAS = t.Set(item.TABLEALIAS.ToString(), Main_TableLabel, "");
                    KEYFNAME = t.Set(item.KEYFNAME.ToString(), "", "");
                    MSETVALUE = t.Set(item.MSETVALUE.ToString(), "0", "0");
                    WORKTYPE = item.WORKTYPE.ToString();

                    if (t.IsNotNull(item.DCCODE))
                        DataCopyCode = item.DCCODE.ToString();

                    if (TABLEALIAS.IndexOf("[") == -1)
                        TABLEALIAS = "[" + TABLEALIAS + "]";

                    if ((WORKTYPE == "READ") ||
                        (WORKTYPE == "CREATEVIEW") ||
                        (WORKTYPE == "SVIEWVALUE") ||
                        (WORKTYPE == "SVIEW") ||
                        (WORKTYPE == "OPENFORM"))
                        Preparing_RefID_Set(dsFields, ref RefID, TABLEALIAS, KEYFNAME, MSETVALUE, ref NewSQL);

                    if (WORKTYPE == "NEW")
                    {
                        if (t.IsNotNull(KEYFNAME) == false)
                        {
                            MessageBox.Show("DİKKAT : " + TABLEIPCODE + " için NEW DATA tanımlarında " + v.ENTER2 + "[ Target Key FName ] tanımı eksik...");
                        }
                        else
                        {
                            Preparing_RefID_Set(dsFields, ref RefID, TABLEALIAS, KEYFNAME, "0", ref NewSQL);
                        }
                    }

                    if ((WORKTYPE == "GOTO") || (WORKTYPE == "SVIEW"))
                    {
                        TargetValue = "GotoRecord";
                        v.con_GotoRecord = "ON";
                        v.con_GotoRecord_TableIPCode = TABLEIPCODE;
                        v.con_GotoRecord_FName = KEYFNAME;
                        v.con_GotoRecord_Value = MSETVALUE;
                    }

                    if (WORKTYPE == "SETDATA")
                    {
                        NewSQL = t.SQLWhereAdd(NewSQL, TABLEALIAS, KEYFNAME, "DEFAULT");
                    }

                    // buna gerek yok : RefID den dolayı aşağıda zaten set ediliyor
                    //if (WORKTYPE == "READ")
                    //{
                    //    NewSQL = t.SQLWhereAdd(NewSQL, TABLEALIAS, RefID, "DEFAULT");
                    //}

                    if (t.IsNotNull(RefID))
                    {
                        NewSQL = t.SQLWhereAdd(NewSQL, TABLEALIAS, RefID, "DEFAULT");

                        /// RefId ye bir Id yükleniyorsa UseNewRefId yi iptal etmek gerekiyor
                        /// and [Alias].Id = -1   siliniyor 
                        if (t.IsNotNull(UseNewRefId))
                            NewSQL = NewSQL.Replace(UseNewRefId + v.ENTER, "");


                        if ((MSETVALUE == "0") || (WORKTYPE == "NEW"))
                            TargetValue = "NewRecord";
                    }

                }// if (Main_TableIPCode == TABLEIPCODE)
                #endregion

            }
            #endregion

        }

        private void Preparing_RefID(DataSet dsFields, ref string NewSQL, ref string TargetValue,
                     string Main_TableIPCode, string Main_TableLabel,
                     string form_prp,
                     int Data_Read_Type)
        {
            MessageBox.Show("DİKKAT : Preparing_RefID yi bekliyordun ");
            Preparing_RefID_OLD(dsFields, ref NewSQL, ref TargetValue,
                     Main_TableIPCode, Main_TableLabel,
                     form_prp,
                     Data_Read_Type);
        }


        private void Preparing_RefID_OLD(DataSet dsFields, ref string NewSQL, ref string TargetValue,
                     string Main_TableIPCode, string Main_TableLabel,
                     string form_prp,
                     int Data_Read_Type)
        {
            // kendinden önceki formdan gelen bilgiler

            // önceki formun Navigator Butonlarından gelen ve 
            // açılan yeni formun mymemo sundaki bilgileri analiz edeceğiz
            // Kart butonları,  51.Kart Aç, 52.Yeni Kart Aç  
            // Fiş  butonları,  56.Fişi Aç, 56.Yeni Fiş Aç

            //'WORKTYPE', 0, '', 'none');
            //'WORKTYPE', 0, 'READ', 'Read Data');
            //'WORKTYPE', 0, 'NEW', 'New Data');
            //'WORKTYPE', 0, 'GOTO', 'Goto Record');
            //'WORKTYPE', 0, 'SVIEW', 'Sub View');
            //'WORKTYPE', 0, 'SVIEWVALUE', 'Sub View Value');
            //'WORKTYPE', 0, 'CREATEVIEW', 'Create View');

            string RefID = string.Empty;
            string s = form_prp;

            if (s.IndexOf("TABLEIPCODE_LIST") > -1)
            {
                string TABLEIPCODE = string.Empty;
                string TABLEALIAS = string.Empty;
                string KEYFNAME = string.Empty;
                string MSETVALUE = string.Empty;
                string WORKTYPE = string.Empty;

                string row_block = string.Empty;
                string lockE = "=ROWE_";

                while (s.IndexOf(lockE) > -1)
                {
                    row_block = t.Find_Properies_Get_RowBlock(ref s, "TABLEIPCODE_LIST");

                    TABLEIPCODE = t.MyProperties_Get(row_block, "TABLEIPCODE:");

                    if ((row_block.IndexOf("SVIEWVALUE") > -1) &&
                        (TABLEIPCODE == "") &&
                        (Data_Read_Type == 6))
                    {
                        TABLEIPCODE = Main_TableIPCode;
                    }

                    if (row_block.IndexOf("CREATEVIEW") > -1)
                    {
                        v.con_FormAfterCreateView = true;
                    }

                    if (Main_TableIPCode == TABLEIPCODE)
                    {
                        TABLEALIAS = t.Set(t.MyProperties_Get(row_block, "TABLEALIAS:"), Main_TableLabel, "");
                        KEYFNAME = t.MyProperties_Get(row_block, "KEYFNAME:");
                        MSETVALUE = t.Set(t.MyProperties_Get(row_block, "MSETVALUE:"), "0", "0");
                        WORKTYPE = t.MyProperties_Get(row_block, "WORKTYPE:");

                        if ((WORKTYPE == "READ") ||
                            (WORKTYPE == "CREATEVIEW") ||
                            (WORKTYPE == "SVIEWVALUE") ||
                            (WORKTYPE == "SVIEW") && (MSETVALUE == "0"))
                            Preparing_RefID_Set(dsFields, ref RefID, TABLEALIAS, KEYFNAME, MSETVALUE, ref NewSQL);

                        if (WORKTYPE == "NEW")
                        {
                            if (t.IsNotNull(KEYFNAME) == false)
                            {
                                MessageBox.Show("DİKKAT : " + TABLEIPCODE + " için NEW DATA tanımlarında " + v.ENTER2 + "[ Target Key FName ] tanımı eksik...");
                            }
                            else
                            {
                                Preparing_RefID_Set(dsFields, ref RefID, TABLEALIAS, KEYFNAME, "0", ref NewSQL);
                            }
                        }

                        if ((WORKTYPE == "GOTO") || (WORKTYPE == "SVIEW"))
                        {
                            TargetValue = "GotoRecord";
                            v.con_GotoRecord = "ON";
                            v.con_GotoRecord_TableIPCode = TABLEIPCODE;
                            v.con_GotoRecord_FName = KEYFNAME;
                            v.con_GotoRecord_Value = MSETVALUE;
                        }
                    }
                }

                if (t.IsNotNull(RefID))
                {
                    NewSQL = t.SQLWhereAdd(NewSQL, TABLEALIAS, RefID, "DEFAULT");

                    if ((MSETVALUE == "0") || (WORKTYPE == "NEW")) TargetValue = "NewRecord";
                }
            }
        }

        private void Preparing_RefID_Set(DataSet dsFields, ref string RefID,
                     string TableAlias, string FieldName, string Value, ref string NewSQL)
        {
            string snc1 = string.Empty;
            string snc2 = string.Empty;
            string findFieldName = string.Empty;
            ///  and [xxx].REF_ID = xxx   veya  
            ///  and [xxx].REF_ID = -1    
            ///
            ///  veya
            ///
            ///  declare @KonuId int
            ///  / *prm * /
            ///  set @KonuId = -99  -- :D.SD.54834: --
            ///  EXEC[dbo].[prc_MtskAdayTakipKonulari]
            ///      @KonuId
            ///  / *paramEnd * /
            ///
            findFieldName = "@" + FieldName;
            // Sql  storedProcedure mi ?
            if (NewSQL.IndexOf(findFieldName) == -1)
            {
                snc1 = t.Set_FieldName_Value(dsFields, FieldName, Value, "and", "=");
                snc2 = " and " + TableAlias + "." + snc1 + v.ENTER;
                if (snc1 != "") RefID = RefID + snc2;
            }
            else
            {
                /// Burası form içinde ilk çalışan TableIPCode ve tek parametreli storedProcedure olduğunda düzgün çalışıyor
                /// birden fazla parametreli ve parametre dışarıdan gelecekse burayı yeniden kurgulamak gerekiyor
                ///
                snc1 = t.Set_FieldName_Value(dsFields, FieldName, Value, "@", "=");
                snc2 = " set " + snc1 + " ";

                findFieldName = "set @" + FieldName + " = -99";

                /// buraya kadar gelmişken valueyi değerini değiştir, dönünce daha da zorlaşıyor
                if (NewSQL.IndexOf(findFieldName) > -1)
                    NewSQL = NewSQL.Replace(findFieldName, snc2);
                //if (snc1 != "") RefID = RefID + snc2;
            }

        }

        private void Preparing_Manuel_RefID(DataSet dsFields, ref string NewSQL, ref string Main_TargetValue,
                     string Main_TableIPCode, string Main_TableLabel, string Main_KeyFName,
                     string form_prp)
        {

            // DİKKAT : Manuel form açılışları sırasında kullanılıyor
            // zamanla gerek kalmaz ise silinebilir bir fonsiyon

            // açılan yeni formun mymemo MANUEL set edilen diğer bilgileri analiz edeceğiz

            #region Tanımlar
            // 1. Tablo
            string TargetTableIPCode = string.Empty;
            string TargetTableAlias = string.Empty;
            string TargetFieldName = string.Empty;
            string TargetValue = string.Empty;
            string TargetLikeValue = string.Empty;
            // 2. Tablo
            string SecondTableIPCode = string.Empty;
            string SecondTableAlias = string.Empty;
            string SecondFieldName = string.Empty;
            string SecondValue = string.Empty;
            // 3. Tablo
            string ThirdTableIPCode = string.Empty;
            string ThirdTableAlias = string.Empty;
            string ThirdFieldName = string.Empty;
            string ThirdValue = string.Empty;

            // Read SubView için value
            string SubViewTargetValue = string.Empty;
            string SubViewTargetTableAlias = string.Empty;
            string SubViewTargetFieldName = string.Empty;
            // AddIP 
            string AddIP_TableIPCode = string.Empty;
            string AddIP_Value = string.Empty;

            string RefID_Target = string.Empty;
            string RefID_Second = string.Empty;
            string RefID_Third = string.Empty;
            string RefID_AddIP = string.Empty;
            string RefID_SubView = string.Empty;

            string snc = string.Empty;

            // 1. Tablonun verileri
            TargetTableIPCode = t.MyProperties_Get(form_prp, "TargetTableIPCode:");
            TargetTableAlias = t.MyProperties_Get(form_prp, "TargetTableAlias:");
            TargetFieldName = t.MyProperties_Get(form_prp, "TargetFieldName:");
            /// sıfır ataması butona SubView özelliği eklenirken iptal edildi
            ///
            /// TargetValue = t.Set(t.MyProperties_Get(form_prp, "TargetValue:"), "", "0");
            TargetValue = t.Set(t.MyProperties_Get(form_prp, "TargetValue:"), "", "");
            TargetLikeValue = t.Set(t.MyProperties_Get(form_prp, "TargetLikeValue:"), "", "");
            // 2. Tablonun verileri
            SecondTableIPCode = t.MyProperties_Get(form_prp, "SecondTableIPCode:");
            SecondTableAlias = t.MyProperties_Get(form_prp, "SecondTableAlias:");
            SecondFieldName = t.MyProperties_Get(form_prp, "SecondFieldName:");
            SecondValue = t.Set(t.MyProperties_Get(form_prp, "SecondValue:"), "", "");
            ///SecondValue = t.Set(t.MyProperties_Get(form_prp, "SecondValue:"), "", "0");
            // 3. Tablonun verileri
            ThirdTableIPCode = t.MyProperties_Get(form_prp, "ThirdTableIPCode:");
            ThirdTableAlias = t.MyProperties_Get(form_prp, "ThirdTableAlias:");
            ThirdFieldName = t.MyProperties_Get(form_prp, "ThirdFieldName:");
            ThirdValue = t.Set(t.MyProperties_Get(form_prp, "ThirdValue:"), "", "");
            ///ThirdValue = t.Set(t.MyProperties_Get(form_prp, "ThirdValue:"), "", "0");

            // AddIP
            AddIP_TableIPCode = t.MyProperties_Get(form_prp, "AddIP_TableIPCode:");
            AddIP_Value = t.Set(t.MyProperties_Get(form_prp, "AddIP_Value:"), "", "0");

            // Read SubView için value
            SubViewTargetTableAlias = t.MyProperties_Get(form_prp, "SubViewTargetTableAlias:");
            SubViewTargetFieldName = t.MyProperties_Get(form_prp, "SubViewTargetFieldName:");
            SubViewTargetValue = t.Set(t.MyProperties_Get(form_prp, "SubViewTargetValue:"), "", "-1");

            #endregion Tanımlar

            #region // 1. Tablonun veriler set ediliyor
            if ((t.IsNotNull(TargetTableIPCode)) && (TargetTableIPCode == Main_TableIPCode))
            {
                if (t.IsNotNull(TargetFieldName) == false) TargetFieldName = Main_KeyFName;

                if ((t.IsNotNull(TargetValue)) && (TargetFieldName.IndexOf("{0}") == -1))
                {
                    //  and [xxx].REF_ID = xxx   veya  
                    //  and [xxx].REF_ID = -1    
                    snc = t.Set_FieldName_Value(dsFields, TargetFieldName, TargetValue, "and", "=");
                    if (snc != string.Empty)
                        RefID_Target = " and " + Main_TableLabel + "." + snc + v.ENTER;
                    if (TargetTableAlias != string.Empty)
                        RefID_Target = " and " + TargetTableAlias + "." + snc + v.ENTER;
                }

                // KARIŞIK/parametreli ATAMALAR YAPILDIĞI ZAMANLAR
                // 
                if ((t.IsNotNull(TargetValue)) && (TargetFieldName.IndexOf("{0}") > -1))
                {
                    // TargetFieldName in içeriği
                    // AND ( [FNLNVOL].BHESAP_ID = {0} OR [FNLNVOL].AHESAP_ID = {0} )
                    RefID_Target = String.Format(TargetFieldName, TargetValue);
                }

                if (t.IsNotNull(TargetLikeValue))
                {
                    if (TargetLikeValue.ToUpper() != "FULL")
                    {
                        //  and [xxx].REF_NAME like xxx%   veya  
                        snc = t.Set_FieldName_Value(dsFields, TargetFieldName, TargetLikeValue, "andlike", "=");
                        if (snc != string.Empty)
                            RefID_Target = " and " + Main_TableLabel + "." + snc + v.ENTER;
                        if (TargetTableAlias != string.Empty)
                            RefID_Target = " and " + TargetTableAlias + "." + snc + v.ENTER;
                    }
                    else
                    {
                        // üst tarafta dolan atamanın iptal edilmesi gerekiyor
                        // if (t.IsNotNull(TargetValue)) { ... }
                        RefID_Target = "";
                    }
                }

                if (t.IsNotNull(RefID_Target))
                {
                    if (TargetTableIPCode == Main_TableIPCode)
                    {
                        if (t.IsNotNull(TargetTableAlias))
                        {
                            NewSQL = t.SQLWhereAdd(NewSQL, TargetTableAlias, RefID_Target, "DEFAULT");
                        }
                        else
                        {
                            NewSQL = t.SQLWhereAdd(NewSQL, Main_TableLabel, RefID_Target, "DEFAULT");
                        }

                        if ((TargetValue == "0") ||
                            (TargetValue == "-1") ||
                            (TargetValue == "NEW")) Main_TargetValue = "NewRecord";
                    }
                }

            }
            #endregion // 1. Tablonun veriler set ediliyor

            #region // 2. Tablonun veriler set ediliyor
            if ((t.IsNotNull(SecondTableIPCode)) && (SecondTableIPCode == Main_TableIPCode))
            {
                if (t.IsNotNull(SecondFieldName) == false) SecondFieldName = Main_KeyFName;

                if ((t.IsNotNull(SecondValue)) && (SecondFieldName.IndexOf("{0}") == -1))
                {
                    //  and [xxx].REF_ID = xxx   veya  
                    //  and [xxx].REF_ID = -1    
                    snc = t.Set_FieldName_Value(dsFields, SecondFieldName, SecondValue, "and", "=");
                    if (snc != string.Empty)
                        RefID_Second = " and " + Main_TableLabel + "." + snc + v.ENTER;
                    if (SecondTableAlias != string.Empty)
                        RefID_Second = " and " + SecondTableAlias + "." + snc + v.ENTER;
                }

                // KARIŞIK/parametreli ATAMALAR YAPILDIĞI ZAMANLAR
                // 
                if ((t.IsNotNull(SecondValue)) && (SecondFieldName.IndexOf("{0}") > -1))
                {
                    // SecondFieldName in içeriği
                    // AND ( [FNLNVOL].BHESAP_ID = {0} OR [FNLNVOL].AHESAP_ID = {0} )
                    RefID_Target = String.Format(SecondFieldName, SecondValue);
                }

                if (t.IsNotNull(RefID_Second))
                {
                    if (SecondTableIPCode == Main_TableIPCode)
                    {
                        if (t.IsNotNull(SecondTableIPCode))
                        {
                            NewSQL = t.SQLWhereAdd(NewSQL, SecondTableAlias, RefID_Second, "DEFAULT");
                        }

                        if ((SecondValue == "0") ||
                            (SecondValue == "-1") ||
                            (SecondValue == "NEW")) Main_TargetValue = "NewRecord";
                    }
                }
            }
            #endregion // 2. Tablonun veriler set ediliyor

            #region // 3. Tablonun veriler set ediliyor
            if ((t.IsNotNull(ThirdTableIPCode)) && (ThirdTableIPCode == Main_TableIPCode))
            {
                if (t.IsNotNull(ThirdFieldName) == false) ThirdFieldName = Main_KeyFName;

                if (t.IsNotNull(ThirdValue))
                {
                    //  and [xxx].REF_ID = xxx   veya  
                    //  and [xxx].REF_ID = -1    
                    snc = t.Set_FieldName_Value(dsFields, ThirdFieldName, ThirdValue, "and", "=");
                    if (snc != string.Empty)
                        RefID_Third = " and " + Main_TableLabel + "." + snc + v.ENTER;
                    if (ThirdTableAlias != string.Empty)
                        RefID_Third = " and " + ThirdTableAlias + "." + snc + v.ENTER;
                }

                if (t.IsNotNull(RefID_Third))
                {
                    if (ThirdTableIPCode == Main_TableIPCode)
                    {
                        if (t.IsNotNull(ThirdTableIPCode))
                        {
                            NewSQL = t.SQLWhereAdd(NewSQL, ThirdTableAlias, RefID_Third, "DEFAULT");
                        }

                        if ((ThirdValue == "0") ||
                            (ThirdValue == "-1") ||
                            (ThirdValue == "NEW")) Main_TargetValue = "NewRecord";
                    }
                }
            }
            #endregion // 3. Tablonun veriler set ediliyor

            #region // AddIP Tablonun veriler set ediliyor
            if ((t.IsNotNull(AddIP_TableIPCode)) && (AddIP_TableIPCode == Main_TableIPCode))
            {
                if (t.IsNotNull(AddIP_Value))
                {
                    //  and [xxx].REF_ID = xxx   veya  
                    //  and [xxx].REF_ID = -1    
                    snc = t.Set_FieldName_Value(dsFields, Main_KeyFName, AddIP_Value, "and", "=");
                    if (snc != string.Empty)
                        RefID_AddIP = " and " + Main_TableLabel + "." + snc + v.ENTER;

                    if (t.IsNotNull(RefID_AddIP))
                    {
                        if (AddIP_TableIPCode == Main_TableIPCode)
                        {
                            if (t.IsNotNull(AddIP_TableIPCode))
                            {
                                NewSQL = t.SQLWhereAdd(NewSQL, Main_TableLabel, RefID_AddIP, "DEFAULT");
                            }

                            if ((AddIP_Value == "0") ||
                                (AddIP_Value == "-1") ||
                                (AddIP_Value == "NEW")) Main_TargetValue = "NewRecord";
                        }
                    }

                }

            }
            #endregion // AddIP Tablonun veriler set ediliyor

            #region Read_SubView ( kendisi bir subview olup READ olması isteniyor)
            if (t.IsNotNull(SubViewTargetValue)) //&& ( ???TableIPCode == Main_TableIPCode))
            {
                //  and [xxx].REF_ID = xxx   veya  
                //  and [xxx].REF_ID = -1    
                //snc = t.Set_FieldName_Value(dsFields, SubViewTargetFieldName, SubViewTargetValue, "and");

                //if (snc != string.Empty)
                //    RefID_SubView = " and " + Main_TableLabel + "." + snc + v.ENTER;
                //if (SubViewTargetTableAlias != string.Empty)
                //    RefID_SubView = " and " + SubViewTargetTableAlias + "." + snc + v.ENTER;

                if (SubViewTargetTableAlias == Main_TableLabel)
                {
                    //  and [xxx].REF_ID = xxx   veya  
                    //  and [xxx].REF_ID = -1    
                    snc = t.Set_FieldName_Value(dsFields, SubViewTargetFieldName, SubViewTargetValue, "and", "=");

                    if (snc != string.Empty)
                    {
                        RefID_SubView = " and " + SubViewTargetTableAlias + "." + snc + v.ENTER;

                        if (t.IsNotNull(RefID_SubView))
                        {
                            if (t.IsNotNull(SubViewTargetValue))
                            {
                                NewSQL = t.SQLWhereAdd(NewSQL, SubViewTargetTableAlias, RefID_SubView, "DEFAULT");

                                if (SubViewTargetValue == "0") Main_TargetValue = "NewRecord";
                            }
                        }
                    }
                }
            }
            #endregion Read_SubView

        }

        #endregion RefID Preparing

        #region Preparing_Join_Fields_Tables

        private void Preparing_Join_Fields_Tables(
                          Form tForm,
                          DataRow row,
                          DataSet dsFields,
                          byte dBaseNo,
                          string Create_TableIPCode,
                          string tableCode, 
                          ref string masterFields,
                          ref string joinTables,
                          ref string joinFields,
                          ref string Lkp_Memory_Fields,
                          ref string About_Detail_SubDetail,
                          ref string Where_Lines,
                          ref string declare,
                          ref string setlist,
                          ref string NewSQL,
                          ref string REF_CALL,
                          ref string Report_FieldsName,
                          ref string MasterDetailColumns,
                          ref string MaliDonemChanger,
                          ref string Master_Detail_Harmony
                          )
        {

            //string function_name = "Join_Fields_Tables_Preparing";

            tToolBox t = new tToolBox();

            #region Tanımlar

            Int16 field_type = 0;
            int RefId = 0;
            string s = string.Empty;
            string fieldname = string.Empty;
            string mainFName = string.Empty;
            string fcaption = string.Empty;
            string fname = string.Empty;
            string jtable = string.Empty;
            string jfield = string.Empty;
            string jwhere = string.Empty;
            string jlabel = string.Empty;

            byte default_type = 0;
            byte default_type2 = 0;

            string mst_TableList = string.Empty;
            string mst_TableIPCode = string.Empty;
            //string mst_TableName = string.Empty;
            string mst_FName = string.Empty;
            string mst_CheckFName = string.Empty;
            string mst_CheckValue = string.Empty;
            string newAnd1_ = string.Empty;
            string newAnd2_ = string.Empty;

            // TABLE_TYPE  
            // 1, 'Table'            // 2, 'View'            // 3, 'Stored Procedure'
            // 4, 'Function'         // 5, 'Trigger'         // 6, 'Select' 

            byte Table_Type = t.Set(row["TABLE_TYPE"].ToString(), row["LKP_TABLE_TYPE"].ToString(), (byte)1);

            string softCode = t.Set(row["SOFTWARE_CODE"].ToString(), "", "");
            string projectCode = t.Set(row["PROJECT_CODE"].ToString(), "", "");

            string TableCode = t.Set(row["TABLE_CODE"].ToString(), "", "");
            string IPCode = t.Set(row["IP_CODE"].ToString(), "", "");
            string TableIPCode = "";

            if ((softCode != "") && (projectCode != ""))
                 TableIPCode = softCode + "/" + projectCode + "/" + TableCode + "." + IPCode;
            else TableIPCode = TableCode + "." + IPCode;

            string tLabel = "[" + t.Set(row["LKP_TABLE_CODE"].ToString(), "", "") + "]";

            /// DATA_READ_TYPE
            /// * 1,  'Not Read Data'
            /// * 2,  'Read RefID'
            /// * 3,  'Detail Table'
            /// * 4,  'SubDetail Table'
            /// * 5,  'Data Collection'
            
            byte Data_Read_Type = t.Set(row["DATA_READ"].ToString(), "", (byte)1); 
            
            int ga = 0; // güzel a veya @
            int find = 0;
            string virgul = "";
            string MasterValue1 = string.Empty;
            string default_value = string.Empty;
            string tkrt_alias = string.Empty;
            string tkrt_table_alias = string.Empty;
            string fforeing = string.Empty;

            string prop_jointable = string.Empty;
            string J_TABLE = string.Empty;
            string J_WHERE = string.Empty;
            string J_STN_FIELDS = string.Empty;
            string J_CASE_FIELDS = string.Empty;

            bool tVisible = false;
            bool tEnabled = false;
            Boolean tLkpMemoryField = false;
            Boolean tLookUpField = false;
            Boolean tUseLookUpField = false;
            byte toperand_type = 0;

            #endregion Tanımlar

            #region join bağlantısı

            prop_jointable = t.Set(row["PROP_JOINTABLE"].ToString(), row["LKP_PROP_JOINTABLE"].ToString(), "");

            string j1 = t.Set(row["PROP_JOINTABLE"].ToString(), "", "");
            string j2 = t.Set(row["LKP_PROP_JOINTABLE"].ToString(), "", "");

            if (t.IsNotNull(prop_jointable))
            {
                Preparing_JoinTable_JSON(prop_jointable, tLabel, ref joinTables, ref joinFields);
            }

            #endregion join bağlantısı

            int krtLineNo = 0;
            string declareEnds = "";
            string setEnds = "";
            string paramEnds = "";

            /// 
            /// dsFields.Tables[0].DefaultView.ToTable();  iki defa okunmuş bu sorunu bulmak gerekiyor
            /// 

            if (Table_Type == 3) // Stored Procedure ise
            {
                dsFields.Tables[0].DefaultView.Sort = "KRT_LINE_NO";
                DataTable dt = dsFields.Tables[0].DefaultView.ToTable();
                foreach (DataRow Row in dt.Rows)
                {
                    krtLineNo = t.Set(Row["KRT_LINE_NO"].ToString(), "0", 0);
                    if (krtLineNo > 0)
                    {
                        if (declareEnds.IndexOf(krtLineNo.ToString() + "*/") == -1)
                        {
                            declareEnds = declareEnds + "/*declareEnd." + krtLineNo.ToString() + "*/" + v.ENTER;
                            setEnds = setEnds + "/*setEnd." + krtLineNo.ToString() + "*/" + v.ENTER;
                            paramEnds = paramEnds + "/*paramEnd." + krtLineNo.ToString() + "*/" + v.ENTER;
                        }
                    }
                }

                // hiçbir parametresi yoksa
                if (declareEnds == "")
                    Where_Lines = "" +
                          "/*declareEnd*/" + v.ENTER +
                          "/*setEnd*/" + v.ENTER +
                          "/*executeBegin*/" + 
                          "/*paramEnd*/" + v.ENTER;
            }

            // DİKKAT : Bu döngüde sadece join bağlantılar kurulmuyor !
            //          Hazır döngü var iken diğer field işlemleride yapılıyor
            //          Nasıl iyi fikir değil mi ?
            #region foreach
            
            joinFields += " -- LookUp Fields" + v.ENTER;
            joinTables += "   -- LookUp Tables" + v.ENTER;

            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                MasterValue1 = "";
                default_value = "";

                mainFName = t.Set(Row["LKP_FIELD_NAME"].ToString(), "", "");
                fieldname = mainFName;

                /// 'DEFAULT_TYPE'
                /// 21,  'Source TableIPCode READ'
                /// 31,  'Master=Detail'
                /// 32,  'Master=Detail Multi'
                /// 33,  'Master=Detail Column'
                /// 34,  'Master=Detail Harmony'   
                /// 
                /// 41,  'Line No'
                /// 51,  'Kriter READ (Even.Bas)' 
                /// 52,  'Kriter READ (Even.Bit)' 
                /// 53,  'Kriter READ (Odd)'

                default_type = t.Set(Row["DEFAULT_TYPE"].ToString(), Row["LKP_DEFAULT_TYPE"].ToString(), (byte)0);
                default_type2 = t.Set(Row["DEFAULT_TYPE2"].ToString(), "0", (byte)0);

                ///  toperand_type
                ///  0,  '' yani =  
                ///  1,  'Even (Double)'
                ///  2,  'Odd  (Single)'
                ///  3,  'Speed'
                ///  4,  'On/Off'
                ///  9,  'Visible=False'
                /// 11,  '>='
                /// 12,  '>'
                /// 13,  '<='
                /// 14,  '<'
                /// 15,  '<>'
                /// 16,  'Benzerleri (%abc%)'
                /// 17,  'Benzerleri (abc%)'

                toperand_type = t.Set(Row["KRT_OPERAND_TYPE"].ToString(), "", (byte)0);
                krtLineNo = t.Set(Row["KRT_LINE_NO"].ToString(), "0", 0);

                tLkpMemoryField = t.Set(Row["LKP_FMEMORY_FIELD"].ToString(), "", false);
                tLookUpField    = t.Set(Row["LKP_FLOOKUP_FIELD"].ToString(), "", false);
                
                tVisible = t.Set(Row["CMP_VISIBLE"].ToString(), "", true);
                tEnabled = t.Set(Row["CMP_ENABLED"].ToString(), "", true);

                if (fieldname.ToUpper().IndexOf("LKP") == -1)
                    masterFields = masterFields + " , [" + tableCode + "]." + fieldname + v.ENTER;

                if (tVisible || tEnabled)
                {
                    fcaption = t.Set(Row["FCAPTION"].ToString(), Row["LKP_FCAPTION"].ToString(), fieldname);

                    fforeing = t.Set(Row["LKP_FFOREING"].ToString(), "", "");

                    //if (Report_FieldsName.Length == 0)
                    //     Report_FieldsName = Report_FieldsName + "   " + fieldname + "  [" + fcaption + "] " + v.ENTER;
                    //else Report_FieldsName = Report_FieldsName + " , " + fieldname + "  [" + fcaption + "] " + v.ENTER;
                    if (fforeing == "True")
                        Report_FieldsName = Report_FieldsName + " , " + fieldname + "  [" + fieldname + "] " + v.ENTER;

                    Report_FieldsName = Report_FieldsName + " , " + fieldname + "  [" + fcaption + "] " + v.ENTER;

                    if (tLookUpField)
                    {
                        tUseLookUpField = true;
                        Preparing_JoinTableForLookUpField(Row, ref joinTables, ref joinFields);
                    }

                    // aşağıda  specialCase_JoinTableForLookUpField uğrasın
                    if (fieldname == "LKP_ONAY")
                        tUseLookUpField = true;
                }

                if ((tLkpMemoryField) ||    // LKP_ MemoryField  ise
                    (toperand_type == 2) || // Odd Single 
                    (toperand_type == 3) || // SpeedDouble
                    (toperand_type == 4)    // SpeedSingle
                    )   // SpeedKriter ise
                {
                    // Tespit edilen Default Type Göre -------------------
                    ///'DEFAULT_TYPE'
                    /// 1, '', 'Var (Integer)');
                    /// 2, '', 'Var (Numeric)');
                    /// 3, '', 'Var (Text)');
                    /// 4, '', 'Var (SP)');
                    /// 5, '', 'Var (Setup/Value)');
                    /// 6, '', 'Var (Setup/Caption)');

                    #region default_type > Var ( 1, 2, 3, 4 )
                    if ((default_type >= 1) &&
                        (default_type <= 5))
                    {
                        if (default_type == 1) // Var (Integer)
                        {
                            default_value = t.Set(Row["DEFAULT_INT"].ToString(), Row["LKP_DEFAULT_INT"].ToString(), "0");
                        }

                        if (default_type == 2) // Var (Numeric)
                        {
                            default_value = t.Set(Row["DEFAULT_NUMERIC"].ToString(), Row["LKP_DEFAULT_NUMERIC"].ToString(), "0");
                        }

                        if (default_type == 3) // Var (Text)
                        {
                            default_value = t.Set(Row["DEFAULT_TEXT"].ToString(), Row["LKP_DEFAULT_TEXT"].ToString(), "");
                        }

                        if (default_type == 4) // Var (Standart) SP_xxxxx
                        {
                            tDefaultValue dv = new tDefaultValue();
                            s = t.Set(Row["DEFAULT_SP"].ToString(), Row["LKP_DEFAULT_SP"].ToString(), "0");
                            if (s != "0")
                                default_value = dv.tSP_Value_Load(s);

                            if (toperand_type == 2)
                                MasterValue1 = default_value;
                        }

                        if (default_type == 5) // Var SETUP_VALUE
                        {
                            tDefaultValue dv = new tDefaultValue();
                            s = t.Set(Row["DEFAULT_SETUP"].ToString(), Row["LKP_DEFAULT_SETUP"].ToString(), "0");
                            if (s != "0")
                                default_value = dv.tSETUP_Load(s, (byte)default_type);

                            if (toperand_type == 2)
                                MasterValue1 = default_value;
                        }
                    }
                    #endregion Var
                }

                #region LkpMemoryField

                if (tLkpMemoryField)
                {
                    field_type = t.Set(Row["LKP_FIELD_TYPE"].ToString(), "", (Int16)0);

                    Lkp_Memory_Fields = Lkp_Memory_Fields +
                        " , " + t.Set_FieldName_Value_(field_type, fieldname, default_value, "as", toperand_type.ToString()) + v.ENTER;
                }

                #endregion LkpMemoryField

                #region Detail-SubDetail ve Detail-SubDetail-Multi bağlantısı

                // bu satırların yerini değiştirme
                ga = -1;

                mst_FName = t.Set(Row["MASTER_KEY_FNAME"].ToString(), "", "");//Row["LKP_MASTER_KEY_FNAME"].ToString(), "");
                /*
                if (mst_FName != "")
                {
                    ga = mst_FName.IndexOf("@");  // declare değişkeni mi?
                    if (ga > -1)
                    {
                        //mst_FName = mst_FName.Substring(1, mst_FName.Length - 1);
                    }
                }
                */
                // @ işareti var ise declare ve set işlemleri başlayacak
                ga = Row["MASTER_KEY_FNAME"].ToString().IndexOf("@");
                if (ga == -1)
                    ga = Row["KRT_CAPTION"].ToString().IndexOf("@");


                // ***

                /// 'DEFAULT_TYPE'
                /// 21,  'Source TableIPCode READ'
                /// 31,  'Master=Detail'
                /// 32,  'Master=Detail Multi'
                /// 33,  'Master=Detail Column'
                /// 41,  'Line No'
                /// 42,  'MaliDonem Changer'
                /// 51,  'Kriter READ (Even.Bas)' 
                /// 52,  'Kriter READ (Even.Bit)' 
                /// 53,  'Kriter READ (Odd)'

                if ((default_type == 21) ||
                    (default_type == 31) ||
                    (default_type == 32) ||
                    (default_type == 33) ||
                    (default_type == 42) ||
                    (default_type == 51) ||
                    (default_type == 52) ||
                    (default_type == 53) ||
                    (toperand_type == 2) || // odd single
                    (toperand_type == 3) || // SpeedKriter Double
                    (toperand_type == 4) || // SpeedKriter Single
                    (default_type == 34) ||  // Master=Detail Harmony ise
                    (default_type2 == 34) || // Master=Detail Harmony ise
                    (ga > -1))
                {
                    // Master Table bağlantısı

                    #region Tanımlar

                    RefId = t.Set(Row["REF_ID"].ToString(), "", (int)0);
                    fname = t.Set(Row["KRT_CAPTION"].ToString(), Row["LKP_FIELD_NAME"].ToString(), "");
                    field_type = t.Set(Row["LKP_FIELD_TYPE"].ToString(), "", (Int16)0);

                    if (fname.IndexOf("@") > -1)
                        fname = fname.Replace("@", "");

                    // farklı bir where aliasına gitmesi gerekebilir 
                    tkrt_alias = t.Set(Row["KRT_ALIAS"].ToString(), tLabel, "");
                    // farklı aliası var ise esas table aliasına ulaşılıyor
                    tkrt_table_alias = t.Set(Row["KRT_TABLE_ALIAS"].ToString(), tLabel, "");

                    mst_TableIPCode = t.Set(Row["MASTER_TABLEIPCODE"].ToString(), "", ""); //Row["LKP_MASTER_TABLEIPCODE"].ToString(), "");
                    //src_TableIPCode = t.Set(Row["SEARCH_TABLEIPCODE"].ToString(), Row["LKP_SEARCH_TABLEIPCODE"].ToString(), "");
                    mst_CheckFName = t.Set(Row["MASTER_CHECK_FNAME"].ToString(), "", ""); //Row["LKP_MASTER_CHECK_FNAME"].ToString(), "");
                    mst_CheckValue = t.Set(Row["MASTER_CHECK_VALUE"].ToString(), "", ""); //Row["LKP_MASTER_CHECK_VALUE"].ToString(), "");

                    /// 3. SpeedKriter Double 
                    /// 4. SpeedKriter Single 
                    if ((toperand_type == 3) ||
                        (toperand_type == 4))
                    {
                        mst_TableIPCode = TableIPCode;
                        mst_CheckFName = fieldname;
                        mst_CheckValue = "";
                    }

                    #endregion Tanımlar

                    //  About_Detail_SubDetail:MasterTableIPCode||MasterTableName||MasterKeyFName||DetailFName;

                    #region About_Detail_SubDetail

                    /// 21,  'Source TableIPCode READ'
                    /// 31,  'Master=Detail'
                    /// 32,  'Master=Detail Multi'
                    /// 33,  'Master=Detail Column'
                    if ((default_type == 21) ||
                        (default_type == 31) ||
                        (default_type == 33))
                    {
                        About_Detail_SubDetail = About_Detail_SubDetail +
                            "=Detail_SubDetail:" +
                            mst_TableIPCode + "||" +
                            mst_FName + "||" +
                            tkrt_table_alias + "." + fname + "||" +
                            field_type.ToString() + "||" +
                            default_type.ToString() + "||" + // 21, 31, 32, 33
                            toperand_type.ToString() + "||" +
                            mst_CheckFName + "||" +
                            mst_CheckValue + "||" +
                            RefId.ToString() + "|ds|" + v.ENTER;
                    }

                    /// Master=Detail Harmony
                    /// Eğer master ile detail tablo arasındaki koşullar doğruysa data okumaları gerçekleşecek
                    /// Örnek : TalepListesi ile TalepFormu arasındaki ilişki bu şekilde kuruluyor
                    /// 
                    if (default_type == 34 || default_type2 == 34)
                    {
                        string defType = "34";

                        Master_Detail_Harmony = Master_Detail_Harmony +
                            "=Master_Detail_Harmony:" +
                            mst_TableIPCode + "||" +
                            mst_FName + "||" + // master field name
                            tkrt_table_alias + "." + fname + "||" +
                            fname + "||" + // target field name
                            field_type.ToString() + "||" +
                            defType.ToString() + "||" + // 34
                            toperand_type.ToString() + "||" +
                            mst_CheckFName + "||" +
                            mst_CheckValue + "||" +
                            RefId.ToString() + "|ds|" + v.ENTER;
                    }

                    /// column daki value değişikliğine göre tetikleyecek fieldlerin listesi toparlanıyor
                    /// public void tColumn_Changed(..)  bu column field name varsa 
                    /// kendisine bağlı SubDetail tableIPCode ler tetikleniyor
                    /// Kendisne bağlı olan tableIPCode ler yine normal Master=Default olarak bağlı
                    ///
                    if (default_type == 33)
                    {
                        MasterDetailColumns += "||" + fieldname + "||";
                    }
                    /// MaliDonemChanger
                    /// 
                    if (default_type == 42)
                    {
                        /// menu code ve MailDonemin okunacağı fieldName
                        MaliDonemChanger = mst_TableIPCode + "||" + mainFName + "||";
                    } 
                    /// 51,  'Kriter READ (Even.Bas)' 
                    /// 52,  'Kriter READ (Even.Bit)' 
                    /// 53,  'Kriter READ (Odd)'
                    if ((default_type == 51) ||
                        (default_type == 52) ||
                        (default_type == 53))
                    {
                        About_Detail_SubDetail = About_Detail_SubDetail +
                            "=Detail_SubDetail:" +
                            mst_TableIPCode + "||" +
                            mst_FName + "||" +
                            //tkrt_table_alias + "." + fname + "||" +
                            fname + "||" +
                            field_type.ToString() + "||" +
                            default_type.ToString() + "||" + //  51, 52, 53
                            toperand_type.ToString() + "||" +
                            mst_CheckFName + "||" +
                            mst_CheckValue + "||" +
                            RefId.ToString() + "|ds|" + v.ENTER;
                    }

                    #endregion About_Detail_SubDetail

                    #region Where_Lines

                    //string bbb = "";
                    //if (TableIPCode == "UST/MEB/MtskTeorikDers.TeoPlanlamaManuelSonucu"  && mainFName == "DersTarihi")
                    //    bbb = "aaa";

                    //  and  a.GCB_ID = xxx  --Detail_SubDetail:MasterTableIPCode||MasterKeyFName;--

                    // 'DEFAULT_TYPE'
                    // 21,  'Source TableIPCode READ'
                    // 31,  'Master=Detail'
                    // 32,  'Master=Detail Multi'
                    // 41,  'Line No'
                    // 51,  'Kriter READ (Even.Bas)' 
                    // 52,  'Kriter READ (Even.Bit)' 
                    // 53,  'Kriter READ (Odd)'

                    // declare field değil ise, @ yok ise
                    if (
                        ((default_type == 31) ||   // Master-Detail       >> ID = x
                         (default_type == 32) ||   // master-Detail Multi >> ID in ( .... )  
                         (default_type == 51) ||
                         (default_type == 52) ||
                         (default_type == 53) ||
                         (toperand_type == 3) ||   // SpeedKriter Double
                         (toperand_type == 4) ||   // SpeedKriter Single
                        ((toperand_type == 2) && (Table_Type == 3)) // odd single and stored procedre 
                        ) &&
                        //(ga < 0) &&   Table_Type = 3 için ve Where_Lines oluşmuyor 
                        (Data_Read_Type != 2) &&   // RefId        değil ise 
                        (Data_Read_Type != 6)      // Read SubView değil ise 
                        )
                    {
                        #region SpeedKriter 

                        /// Where işlemleri sırasında tarihSaat yerine sadece tarih şeklinde sorgulansın                          
                        //* smalldatetime = 58
                        //* date = 40, 61
                        if (field_type == 58) field_type = 40;

                        // Odd Single
                        // Stored Procedure hazırlanırken
                        // eğer kritername yoksa bu parametre atlaması için
                        // 
                        if ((toperand_type == 2) && (Table_Type == 3))
                        {
                            fname = t.Set(Row["KRT_CAPTION"].ToString(), "", "");
                            if (t.IsNotNull(fname) == false)
                            {
                                MessageBox.Show("DİKKAT :  [ " + mainFName + " ]  için KRT_CAPTION alanına gerekli fieldName ismini girin..." );
                            }
                        }

                        // Double
                        if (toperand_type == 3)
                        {
                            newAnd1_ = t.Set_FieldName_Value_(field_type, tkrt_table_alias + "." + fname, default_value, "and", ">=");
                            newAnd2_ = t.Set_FieldName_Value_(field_type, tkrt_table_alias + "." + fname, default_value, "and", "<=");


                            if (t.IsNotNull(newAnd1_) && newAnd1_.IndexOf("--") == -1)
                            {
                                Where_Lines =
                                        Where_Lines +
                                        " and " + newAnd1_ +
                                        "   -- :D.SD." + RefId.ToString() + ": -->=" + v.ENTER;
                            }
                            else
                            {
                                Where_Lines =
                                        Where_Lines +
                                        " --and " + newAnd1_.Substring(2) +
                                        ", 103)  >= ???   -- :D.SD." + RefId.ToString() + ": -->=" + v.ENTER;
                            }

                            if (t.IsNotNull(newAnd2_) && newAnd2_.IndexOf("--") == -1)
                            {
                                Where_Lines =
                                        Where_Lines +
                                        " and " + newAnd2_ +
                                        "   -- :D.SD." + RefId.ToString() + ": --<=" + v.ENTER;
                            }
                            else
                            {
                                Where_Lines =
                                        Where_Lines +
                                        " --and " + newAnd2_.Substring(2) +
                                        ", 103)  <= ???   -- :D.SD." + RefId.ToString() + ": --<=" + v.ENTER;
                            }

                            /* OLD
                            Where_Lines =
                                    Where_Lines +
                                    " and " + t.Set_FieldName_Value_(field_type, tkrt_table_alias + "." + fname, default_value, "and", ">=") +
                                    "   -- :D.SD." + RefId.ToString() + ": -->=" + v.ENTER +
                                    " and " + t.Set_FieldName_Value_(field_type, tkrt_table_alias + "." + fname, default_value, "and", "<=") +
                                    "   -- :D.SD." + RefId.ToString() + ": --<=" + v.ENTER;
                            */

                        }
                        // Single
                        if (toperand_type == 4)
                        {
                            newAnd1_ = t.Set_FieldName_Value_(field_type, tkrt_table_alias + "." + fname, default_value, "and", "=");

                            if (t.IsNotNull(newAnd1_) && newAnd1_.IndexOf("--") == -1)
                            {
                                Where_Lines =
                                        Where_Lines +
                                        " and " + newAnd1_ +
                                        "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER;
                            }
                            else
                            {
                                Where_Lines =
                                        Where_Lines +
                                        " --and " + newAnd1_.Substring(2) + 
                                        " = ???   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER;
                            }
                        }
                        #endregion SpeedKriter

                        #region (default_type == 31) || (default_type == 51,52,53)
                        // önce bilgiyi oku
                        if (default_type == 31)
                        {
                            // master bilgiye ulaşırsa (dsData) direk bu bağlantı sağlansın
                            MasterValue1 = t.Find_TableIPCode_Value(tForm, mst_TableIPCode, mst_FName);
                        }

                        if ((default_type == 51) ||
                            (default_type == 52) ||
                            (default_type == 53))
                        {
                            // kriterler nesnesine ulaşırsa (VGrid KRITER_TableIPCode) direk buradan bilgiyi alsın
                            MasterValue1 = t.Find_Kriter_Value(tForm, mst_TableIPCode, mst_FName, default_type);
                        }

                        if (mst_FName == ":DONEM_YILAY")
                        {
                            MasterValue1 = ":DONEM_YILAY"; // v.DONEMTIPI_YILAY.ToString();
                        }

                        // aldığın bilgiyi set et
                        if ((toperand_type == 2) ||  // odd single  
                            (default_type == 31) ||
                            (default_type == 51) ||
                            (default_type == 52) ||
                            (default_type == 53))
                        {
                            #region // Table ise
                            if (Table_Type == 1)
                            {
                                if (MasterValue1 == "")
                                {
                                    Where_Lines =
                                        Where_Lines +
                                        " and " + t.Set_FieldName_Value_(field_type, tkrt_table_alias + "." + fname, "first", "and", toperand_type.ToString()) +
                                        "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER;
                                }

                                if (MasterValue1 != "")
                                {
                                    Where_Lines =
                                        Where_Lines +
                                        " and " + t.Set_FieldName_Value_(field_type, tkrt_table_alias + "." + fname, MasterValue1, "and", toperand_type.ToString()) +
                                        "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER;
                                }

                            }
                            #endregion

                            #region // Stored Procedure
                            if ((Table_Type == 3) && t.IsNotNull(fname))
                            {
                                #region ** eski hali
                                /*
                                 ** eski hali                                

                                 EXEC [dbo].[prc_STK_FIYAT_GET]
                                  /*prm* / @URUN_ID = 2-- :D.SD.6596: --
                                , /*prm* / @BIRIM_NO = 2-- :D.SD.6597: --
                                , /*prm* / @GECERLI_BAS_TARIH = Convert(Date, '13.04.2018', 103)-- :D.SD.6606: --
                                , /*prm* / @GECERLI_BAS_SAAT = '16:36'-- :D.SD.6607: --

                                ** yeni hali

                                DECLARE @_URUN_ID int
                                DECLARE @_BIRIM_NO smallint
                                DECLARE @_ISLEM_TARIHI date
                                DECLARE @_ISLEM_SAAT varchar(8)

                                set @_URUN_ID = 2
                                set @_BIRIM_NO = 2
                                set @_ISLEM_TARIHI = CONVERT(date, '13.04.2018',104)
                                set @_ISLEM_SAAT = '16:05'

                                EXECUTE [dbo].[prc_STK_FIYAT_GET] 
                                   @_URUN_ID
                                  ,@_BIRIM_NO
                                  ,@_ISLEM_TARIHI
                                  ,@_ISLEM_SAAT
  
                                */
                                #endregion


                                //krtLineNo = 0;

                                // declareEnds = 
                                /*declareEnd.1*/
                                /*declareEnd.2*/
                                /*declareEnd.3*/

                                // setEnds = 
                                /*setEnd.1*/
                                /*setEnd.2*/
                                /*setEnd.3*/
                                /*setEnd.4*/
                                /*setEnd.5*/
                                // gibi


                                if ((Where_Lines.Length == 0) && (setEnds.Length == 0))
                                {
                                    Where_Lines = "" +
                                          "/*declareEnd*/" + v.ENTER +
                                          "/*setEnd*/" + v.ENTER +
                                          "/*executeBegin*/" + //v.ENTER +
                                          "/*paramEnd*/" + v.ENTER;
                                }
                                if ((Where_Lines.Length == 0) && (setEnds.Length > 0))
                                {
                                    Where_Lines = "" +
                                          declareEnds + v.ENTER +
                                          setEnds + v.ENTER +
                                          "/*executeBegin*/" + v.ENTER +
                                          paramEnds + v.ENTER +
                                          "/*paramEnd*/" + v.ENTER;
                                }


                                /// virgul ilk geldiğinde ="" olduğu için önce boşluk oluyor  ( virgul = "   " )
                                /// birinci döngüden sonra virgul gerçekten virgül değerine dönüşüyor ( virgul = " , " )  
                                /// aşağıdak, sıralamayı değiştirme
                                /// 
                                if (virgul == "   ") virgul = " , ";
                                if (virgul == "") virgul = "   ";


                                //if ((dBaseNo == 4) && (v.dBTypes.ProjectDBType == v.dBaseType.MSSQL))
                                if (v.active_DB.projectDBType == v.dBaseType.MSSQL)
                                {
                                    if ((MasterValue1 == "") && (default_value == ""))
                                    {
                                        //Where_Lines =
                                        //    Where_Lines +
                                        //    "/*prm*/ " + t.Set_FieldName_Value_(field_type, "@" + fname, "first", "@", toperand_type.ToString()) +
                                        //    "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER;

                                        if (krtLineNo == 0)
                                        {
                                            find = Where_Lines.IndexOf("/*declareEnd*/");
                                            Where_Lines = Where_Lines.Insert(find,
                                                "declare @" + fname + " " + t.Get_FieldTypeName(Convert.ToInt16(field_type), "800") + v.ENTER);

                                            find = Where_Lines.IndexOf("/*setEnd*/");
                                            Where_Lines = Where_Lines.Insert(find,
                                                "/*prm*/ set " + t.Set_FieldName_Value_(field_type, fname, "first", "@", toperand_type.ToString()) +
                                                "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER);

                                            //Where_Lines = Where_Lines.Insert(find,
                                            //    "/*prm*/ set " + t.Set_FieldName_Value_(field_type, "@" + fname, "first", "@", toperand_type.ToString()) +
                                            //    "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER);

                                            find = Where_Lines.IndexOf("/*paramEnd*/");
                                            Where_Lines = Where_Lines.Insert(find, virgul + " @" + fname + v.ENTER);
                                        }
                                        if (krtLineNo > 0)
                                        {
                                            if (krtLineNo > 1) virgul = " , ";
                                            if (krtLineNo == 1) virgul = "   ";

                                            s = "/*declareEnd."+krtLineNo.ToString()+"*/";
                                            
                                            Where_Lines = Where_Lines.Replace(s,
                                                "declare @" + fname + " " + t.Get_FieldTypeName(Convert.ToInt16(field_type), "800"));

                                            s = "/*setEnd."+krtLineNo.ToString()+"*/";
                                            Where_Lines = Where_Lines.Replace(s,
                                                "/*prm*/ set " + t.Set_FieldName_Value_(field_type, "@" + fname, "first", "@", toperand_type.ToString()) +
                                                "   -- :D.SD." + RefId.ToString() + ": --");

                                            s = "/*paramEnd."+krtLineNo.ToString()+"*/";
                                            Where_Lines = Where_Lines.Replace(s, virgul + " @" + fname);
                                        }
                                    }

                                    /// default value varsa onu set edelim
                                    /// 
                                    if ((MasterValue1 == "") && (default_value != ""))
                                        MasterValue1 = default_value;

                                    if (MasterValue1 != "")
                                    {
                                        //Where_Lines =
                                        //    Where_Lines +
                                        //    "/*prm*/ " + t.Set_FieldName_Value_(field_type, "@" + fname, MasterValue1, "@", toperand_type.ToString()) +
                                        //    "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER;

                                        if (krtLineNo == 0)
                                        {
                                            /// find -1 geliyorsa IP Fields de KrtLineNo field i kontrol et, sıralmada sorun var
                                            /// 

                                            find = Where_Lines.IndexOf("/*declareEnd*/");

                                            if (find == -1)
                                                MessageBox.Show("find -1 geliyorsa IP Fields de KrtLineNo field i kontrol et, sıralmada sorun var");
                                            else
                                            {
                                                Where_Lines = Where_Lines.Insert(find,
                                                    "declare @" + fname + " " + t.Get_FieldTypeName(Convert.ToInt16(field_type), "800") + v.ENTER);

                                                find = Where_Lines.IndexOf("/*setEnd*/");
                                                Where_Lines = Where_Lines.Insert(find,
                                                    "/*prm*/ set " + t.Set_FieldName_Value_(field_type, "@" + fname, MasterValue1, "@", toperand_type.ToString()) +
                                                    "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER);

                                                find = Where_Lines.IndexOf("/*paramEnd*/");
                                                Where_Lines = Where_Lines.Insert(find, virgul + " @" + fname + v.ENTER);
                                            }
                                        }
                                        if (krtLineNo > 0)
                                        {
                                            if (krtLineNo > 1) virgul = " , ";
                                            if (krtLineNo == 1) virgul = "   ";

                                            s = "/*declareEnd." + krtLineNo.ToString() + "*/";
                                            Where_Lines = Where_Lines.Replace(s,
                                                "declare @" + fname + " " + t.Get_FieldTypeName(Convert.ToInt16(field_type), "800"));


                                            if (MasterValue1 != ":DONEM_YILAY")
                                            {
                                                s = "/*setEnd." + krtLineNo.ToString() + "*/";
                                                Where_Lines = Where_Lines.Replace(s,
                                                    "/*prm*/ set " + t.Set_FieldName_Value_(field_type, "@" + fname, MasterValue1, "@", toperand_type.ToString()) +
                                                    "   -- :D.SD." + RefId.ToString() + ": --");
                                            }
                                            else
                                            {
                                                s = "/*setEnd." + krtLineNo.ToString() + "*/";
                                                Where_Lines = Where_Lines.Replace(s,
                                                    "/*prm*/ set " + t.Set_FieldName_Value_(field_type, "@" + fname, MasterValue1, "@", toperand_type.ToString()) +
                                                    "   -- :@@YILAY --");
                                            }

                                            s = "/*paramEnd." + krtLineNo.ToString() + "*/";
                                            Where_Lines = Where_Lines.Replace(s, virgul + " @" + fname);
                                        }
                                    }
                                }
                                                                
                            }
                            #endregion // sp

                            #region // Select ise
                            if (Table_Type == 6)
                            {
                                // burada tek tek sql in içine set ediliyor
                                // her bir bağlantı farklı ailas olabilir
                                //Where_Lines =
                                if (MasterValue1 == "")
                                {
                                    s = " and " + t.Set_FieldName_Value_(field_type, tkrt_table_alias + "." + fname, "first", "and", toperand_type.ToString()) +
                                    "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER;
                                }
                                if (MasterValue1 != "")
                                {
                                    s = " and " + t.Set_FieldName_Value_(field_type, tkrt_table_alias + "." + fname, MasterValue1, "and", toperand_type.ToString()) +
                                    "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER;
                                }

                                //NewSQL = t.SQLWhereAdd(NewSQL, tkrt_alias, s, "DEFAULT");
                                NewSQL = t.SQLWhereAdd(NewSQL, tkrt_table_alias, s, "DEFAULT");

                                s = string.Empty;
                            }
                            #endregion 
                        } // 31
                        #endregion (31) || (51,52,53)

                        #region (default_type == 32)
                        if (default_type == 32)
                        {

                            Where_Lines =
                                    Where_Lines +
                                    " and " + t.Set_FieldName_Value_(field_type, tkrt_table_alias + "." + fname, "first", "in", toperand_type.ToString()) +
                                    "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER;
                            /*
                             and [K].GRUP_ID in (   -- :D.SD.2774: --
                             / *[K].GRUP_ID_INLIST* /
                               2990
                             , 2983
                             / *[K].GRUP_ID_INEND* /
                             )
                            */

                        } // 32
                        #endregion (default_type == 32)

                    }

                    #endregion Where_Lines

                    // SubDetail_TableIPCode:TableIPCode;

                    #region tDataNavigator.IsAccessible = true

                    // Master-Detail ilişki sırasında bazen Tablonun kendisine 
                    // join le bağlı olan tablosundan LKP_XXXX li bir fieldin value değeri gerekebiliyor
                    // bu bilgiler Default_Value de Master-Detail sırasında lağzım olabiliyor
                    // ama burada bu ilgiyi kullanamamız gerekiyor
                    if (TableIPCode == mst_TableIPCode)
                    {
                        mst_TableList = mst_TableList + mst_TableIPCode + v.ENTER;
                    }

                    if (mst_TableList.IndexOf(mst_TableIPCode) == -1)
                    {
                        // MasterTable/DetailTable tespit edilecek ve üzerine 
                        // TableIPCode/SubDetailTable (lar) listesi yazılacak

                        mst_TableList = mst_TableList + mst_TableIPCode + v.ENTER;

                        // Master Table tespit ediliyor 
                        DataNavigator tDataNavigator = null;
                        tDataNavigator = t.Find_DataNavigator(tForm, mst_TableIPCode);//, function_name);// + "/" + Create_TableIPCode);

                        // Master/Detail Table Tespit edildiyse
                        if (tDataNavigator != null)
                        {
                            // gerekli bilgiler toplandıysa  
                            // MasterTable/DetailTable ın üzerine kendisine bağlı olan SubDetail_TableIPCode nin bilgiler işleniyor
                            s = "";
                            t.MyProperties_Set(ref s, "SubDetail_TableIPCode", TableIPCode);
                            if (tDataNavigator.Text.IndexOf(TableIPCode+";") == -1) /// Benzer isimlerde çalışmadığı için ; kontrolu eklendi
                            {
                                tDataNavigator.IsAccessible = true;
                                tDataNavigator.Text = tDataNavigator.Text + s;

                                //object tDataTable = tDataNavigator.DataSource;
                                //DataSet tdsData = ((DataTable)tDataTable).DataSet;
                            }
                        }
                    } // if (mst_TableList.IndexOf(mst_TableIPCode) == -1)

                    #endregion tDataNavigator.IsAccessible = true

                    // declare ve set  parametreleri kullanılıyor ise

                    #region Declare & Set

                    if (ga > -1) // @ var ise
                    {
                        string mst_FName2 = mst_FName;
                        
                        if (mst_FName.IndexOf("@") > -1)
                            mst_FName.Substring(1, mst_FName.Length - 1);

                        MasterValue1 = "";
                        default_value = t.Set(Row["DEFAULT_TEXT"].ToString(), Row["LKP_DEFAULT_TEXT"].ToString(), "");

                        if (default_value == "")
                        {
                            // master bilgiye ulaşırsa direk bu bağlantı sağlansın
                            MasterValue1 = t.Find_TableIPCode_Value(tForm, mst_TableIPCode, mst_FName2);
                        }

                        declare = declare +
                            "declare @" + fname + " " + t.Get_FieldTypeName(Convert.ToInt16(field_type), "800") + v.ENTER;

                        if (MasterValue1 == "")
                        {
                            setlist = setlist +
                                "set " + t.Set_FieldName_Value_(field_type, fname, default_value, "@", toperand_type.ToString())
                                + "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER;
                        }
                        else
                        {
                            setlist = setlist +
                                "set " + t.Set_FieldName_Value_(field_type, fname, MasterValue1, "@", toperand_type.ToString())
                                + "   -- :D.SD." + RefId.ToString() + ": --" + v.ENTER;
                        }
                    }

                    #endregion Declare & Set
                }

                #endregion Detail-SubDetail bağlantısı

                #region // REF_CALL field varmı ?
                if (fieldname == "REF_CALL")
                {
                    REF_CALL = " and " + tLabel + "." + fieldname + " = "
                        + t.Set(Row["DEFAULT_INT"].ToString(), Row["LKP_DEFAULT_INT"].ToString(), "0")
                        + v.ENTER;
                }
                #endregion

            }
            #endregion foreach

            if (tUseLookUpField)
                specialCase_JoinTableForLookUpField(dsFields, joinTables, ref joinFields);

            if (Report_FieldsName.Length > 3)
            {
                // en baştaki virgülü temizle
                Report_FieldsName = Report_FieldsName.Remove(0, 3);
                Report_FieldsName = "   " + Report_FieldsName;
            }

            if (masterFields.Length > 3)
            {
                masterFields = masterFields.Remove(1, 1);
            }

            #region About_Detail_SubDetail

            if (t.IsNotNull(About_Detail_SubDetail))
                About_Detail_SubDetail =
                    "About_Detail_SubDetail:" + v.ENTER +
                     About_Detail_SubDetail + ";" + v.ENTER;

            #endregion About_Detail_SubDetail

        }

        private void Preparing_JoinTableForLookUpField(
            DataRow Row,
            ref string joinTables,
            ref string joinFields)
        {
            tToolBox t = new tToolBox();

            // kendi tablosuna ait olmayan başka bir type tablosu kullanıyorsa çalışmasın
            // Lkp.MsSectorType||Id||SectorType
            string groupTables = Row["LKP_LIST_TYPES_NAME"].ToString();
            if (t.IsNotNull(groupTables)) return;

            string idFieldName = "Id";
            string tableCode = Row["TABLE_CODE"].ToString();
            string tableName = Row["LKP_TABLE_NAME"].ToString();
            string fieldName = Row["LKP_FIELD_NAME"].ToString();
            string orjTableName = Row["LKP_TABLE_NAME"].ToString();
            
            string lookUpFieldName = Row["LKP_FIELD_NAME"].ToString();
            string joinTableAlias = "FNo" + Row["LKP_FIELD_NO"].ToString();
            Int16 fieldType = Convert.ToInt16(Row["LKP_FIELD_TYPE"].ToString());

            bool isSubJoin = (fieldName.IndexOf("Lkp") == 0);

            /// joinFields += " -- LookUp Fields" + v.ENTER;
            /// joinTables += "   -- LookUp Tables" + v.ENTER;
            /// hazılandığı yer


            if (joinFields.IndexOf("as " + joinTableAlias) == -1)
            {
                if (t.IsNotNull(Row["KRT_CAPTION"].ToString()))
                    lookUpFieldName = Row["KRT_CAPTION"].ToString();
                if (t.IsNotNull(Row["KRT_TABLE_ALIAS"].ToString()))
                    tableCode = Row["KRT_TABLE_ALIAS"].ToString();
                if (t.IsNotNull(Row["KRT_ALIAS"].ToString()))
                    tableName = Row["KRT_ALIAS"].ToString();

                //if (fieldName.IndexOf("Lkp_") > -1)
                fieldName = fieldName.Replace("Lkp_", "");

                t.LookUpFieldNameChecked(ref tableName, ref fieldName, ref idFieldName, fieldType);

                if (joinTables.IndexOf(tableName + " as " + joinTableAlias) == -1)
                {
                    joinTables += "   left outer join Lkp." + tableName + " as " + joinTableAlias + " with (nolock) on ( " + tableCode + "." + lookUpFieldName.Replace("Lkp_", "") + " = " + joinTableAlias + "." + idFieldName + " ) " + v.ENTER;

                    if (lookUpFieldName.IndexOf("Lkp_") == -1)
                        lookUpFieldName = "Lkp_" + lookUpFieldName;

                    if (lookUpFieldName.IndexOf("TipiId") > -1)
                        lookUpFieldName = lookUpFieldName.Replace("TipiId", "Tipi");
                    if (lookUpFieldName.IndexOf("TypeId") > -1)
                        lookUpFieldName = lookUpFieldName.Replace("TypeId", "Type");
                    // 
                    if (fieldName == "Type") fieldName = fieldName + "Name";

                    joinFields += " , " + joinTableAlias + "." + fieldName + " as " + lookUpFieldName + v.ENTER;
                }
            }
        }

        private void specialCase_JoinTableForLookUpField(DataSet dsFields, string joinTables, ref string joinFields)
        {
            tToolBox t = new tToolBox();
            
            bool tvisible = false;
            bool tenabled = false;
            bool tLookUpField = false;

            string idFieldName = "";
            string tableName = "";

            string fieldName = "";
            string fieldName2 = "";
            string joinTableAlias = "";
            string aliasFieldName = "";
            string newLkpFieldName = "";
            string onayFieldName = "";
            string groupTables = "";
            Int16 fieldType = 0;

            /// burada Özel olarak   Lkp_ + Donem + Grup + Sube  field üretiliyor
            ///
            foreach (DataRow Row in dsFields.Tables[0].Rows)
            {
                tLookUpField = t.Set(Row["LKP_FLOOKUP_FIELD"].ToString(), "", false);

                tvisible = t.Set(Row["CMP_VISIBLE"].ToString(), "", true);
                tenabled = t.Set(Row["CMP_ENABLED"].ToString(), "", true);

                // kendi tablosuna ait olmayan başka bir type tablosu kullanıyorsa çalışmasın
                // Lkp.MsSectorType||Id||SectorType
                groupTables = Row["LKP_LIST_TYPES_NAME"].ToString();

                if (tvisible || tenabled) 
                {
                    fieldName = Row["LKP_FIELD_NAME"].ToString();

                    if (tLookUpField && t.IsNotNull(groupTables) == false)
                    {
                        idFieldName = "Id";
                        tableName = Row["LKP_TABLE_NAME"].ToString();
                        joinTableAlias = "FNo" + Row["LKP_FIELD_NO"].ToString();
                        fieldType = Convert.ToInt16(Row["LKP_FIELD_TYPE"].ToString());

                        //, FNo5.DonemTipi + ' : ' + FNo6.GrupTipi + ' : ' + FNo7.SubeTipi  as  Lkp_DonemTipiGrupTipiSubeTipi

                        if (((fieldName.IndexOf("MtskDonemTipi") > -1) ||
                             (fieldName.IndexOf("MtskGrupTipi") > -1) ||
                             (fieldName.IndexOf("MtskSubeTipi") > -1) ||
                             (fieldName.IndexOf("SrcDonemTipi") > -1) ||
                             (fieldName.IndexOf("SrcGrupTipi") > -1) ||
                             (fieldName.IndexOf("SrcSubeTipi") > -1) ||
                             (fieldName.IndexOf("DonemTipi") > -1) ||
                             (fieldName.IndexOf("GrupTipi") > -1) ||
                             (fieldName.IndexOf("SubeTipi") > -1)
                             ))
                        {
                            t.LookUpFieldNameChecked(ref tableName, ref fieldName, ref idFieldName, fieldType);

                            if (fieldName.IndexOf("SubeTipi") > -1)
                                fieldName2 = "substring(" + joinTableAlias + "." + fieldName +",1,1)";
                            else fieldName2 = joinTableAlias + "." + fieldName;

                            //if (aliasFieldName == "")
                            //    aliasFieldName = " , " + joinTableAlias + "." + fieldName;
                            //else aliasFieldName += " + ' , ' + " + joinTableAlias + "." + fieldName;
                            if (aliasFieldName == "")
                                aliasFieldName = " , " + fieldName2;
                            else aliasFieldName += " + ' , ' + " + fieldName2;

                            if (newLkpFieldName == "")
                                newLkpFieldName = "Lkp_" + fieldName.Replace("Tipi", "");
                            else newLkpFieldName += fieldName.Replace("Tipi", "");
                        }
                    }

                    if (fieldName == "LKP_ONAY")
                        onayFieldName = " , Convert(bit,0) as LKP_ONAY ";
                }
            }

            if (t.IsNotNull(aliasFieldName))
            {
                joinFields += " -- Özel field : Donem + Grup + Sube " + v.ENTER;
                joinFields += aliasFieldName + " as " + newLkpFieldName + v.ENTER;
            }

            if (t.IsNotNull(onayFieldName))
            {
                joinFields += " -- Özel field : Lkp_Onay " + v.ENTER;
                joinFields += onayFieldName + v.ENTER;
            }

        }


        private void Preparing_JoinTable_JSON(
                     string Prop_JoinTable,
                     string MasterTableAlias,
                     ref string joinTables,
                     ref string joinFields)
        {
            /*
            PROP_JOINTABLE packet = new PROP_JOINTABLE();
            Prop_JoinTable = Prop_JoinTable.Replace((char)34, (char)39);
            //var prop_ = JsonConvert.DeserializeAnonymousType(Prop_JoinTable, packet);
            */
            PROP_JOINTABLE prop_ = t.readProp<PROP_JOINTABLE>(Prop_JoinTable);

            string j_format = string.Empty;
            string j_type = string.Empty;
            string j_table_name = string.Empty;
            string j_table_alias = string.Empty;
            string join = string.Empty;
            string masterTabAlias = MasterTableAlias;
            //yeni ek : 2020.03.08
            // master table içinde field eklebiyor artık ( case > bit fieldler üretmek için )
            t.Str_Remove(ref masterTabAlias, "[");
            t.Str_Remove(ref masterTabAlias, "]");

            foreach (var item in prop_.J_TABLE)
            {
                j_format = item.J_FORMAT.ToString();
                j_type = item.J_TYPE.ToString();
                j_table_name = item.J_TABLE_NAME.ToString();
                j_table_alias = item.J_TABLE_ALIAS.ToString();

                if (t.IsNotNull(j_table_name))
                {
                    if (joinTables.IndexOf(j_table_name + " " + j_table_alias) == -1)
                    {
                        if (j_type == "LEFT") join = "   left outer join ";
                        if (j_type == "RIGHT") join = "   right outer join ";
                        if (j_type == "INNER") join = "   inner join ";

                        // J_TABLE + J_WHERE
                        joinTables = joinTables +
                            join + j_table_name + " as [" + j_table_alias + "] with (nolock) on ( " +
                              Preparing_JoinWhere_JSON(prop_.J_WHERE, j_table_alias, MasterTableAlias) + " ) " + v.ENTER;

                        // J_STN_FIELDS
                        if (j_format == "STANDART")
                            Preparing_Select_Standart_Fields_JSON(prop_.J_STN_FIELDS, ref joinFields, j_table_alias, masterTabAlias);
                    }
                }
            }

            //Preparing_Select_Standart_Fields_JSON(prop_.J_STN_FIELDS, ref joinFields, "", masterTabAlias);

            //J_CASE_FIELDS    (j_format == "CASE")
            Preparing_Select_Case_Fields_JSON(prop_.J_CASE_FIELDS, ref joinFields, j_table_alias, MasterTableAlias);
        
            if (joinTables != "")
            {
                joinTables = "   -- Join Tables " + v.ENTER + joinTables;
                joinFields = " -- Join Tables standart fields " + v.ENTER + joinFields;
            }
        }

        private string Preparing_JoinWhere_JSON(List<J_WHERE> J_WHERE,
            string j_table_alias, string MasterTableAlias)
        {
            string s = string.Empty;
            string m_alias = string.Empty;
            string mst_alias = string.Empty;
            string mst_fname = string.Empty;
            string j_fname = string.Empty;
            string fvalue = string.Empty;
            string where_add = string.Empty;

            #region J_WHERE
            foreach (var item in J_WHERE)
            {
                if ((item.J_TABLE_ALIAS == j_table_alias) ||
                    ("[" + item.J_TABLE_ALIAS + "]" == j_table_alias))
                {
                    mst_alias = item.M_TABLE_ALIAS.ToString();
                    mst_fname = item.MT_FNAME.ToString();
                    j_fname = item.J_FNAME.ToString();
                    fvalue = item.FVALUE.ToString();
                    if (item.WHERE_ADD != null)
                    where_add = item.WHERE_ADD.ToString();

                    m_alias = t.Set(mst_alias, MasterTableAlias, "");

                    if (m_alias.IndexOf("[") == -1)
                        m_alias = "[" + m_alias + "]";
                    if (j_table_alias.IndexOf("[") == -1)
                        j_table_alias = "[" + j_table_alias + "]";

                    if (s == "")
                    {
                        // a.mst_FieldName = b.FieldName
                        if (t.IsNotNull(mst_fname) &&
                           (t.IsNotNull(j_fname)) &&
                           (t.IsNotNull(fvalue) == false))
                            s = m_alias + "." + mst_fname + " = " + j_table_alias + "." + j_fname + " ";

                        // a.mst_FieldName = value
                        if (t.IsNotNull(mst_fname) &&
                           (t.IsNotNull(j_fname) == false) &&
                           (t.IsNotNull(fvalue)))
                            s = m_alias + "." + mst_fname + " = " + fvalue + " ";

                        // b.join_FieldName = value
                        if ((t.IsNotNull(mst_fname) == false) &&
                            (t.IsNotNull(j_fname)) &&
                            (t.IsNotNull(fvalue)))
                        {
                            try
                            {
                                /// burada value nin tipini bilmiyoruz
                                /// eğer bu çevrim hata vermezse int değerindedir
                                var x = Convert.ToInt64(fvalue);

                                s = j_table_alias + "." + j_fname + " = " + fvalue + " ";
                            }
                            catch (Exception)
                            {
                                /// eğer hata verdiyse string kabul ediyoruz 
                                s = j_table_alias + "." + j_fname + " = '" + fvalue + "' ";
                            }

                        }
                        // eğer üç değerde dolu ise value olduğu gibi eklenecek
                        if ((t.IsNotNull(mst_fname)) &&
                            (t.IsNotNull(j_fname)) &&
                            (t.IsNotNull(fvalue)))
                            s = s + " and  " + fvalue + " ";
                    }
                    else
                    {
                        // and a.mst_FieldName = b.join_FieldName
                        if (t.IsNotNull(mst_fname) &&
                           (t.IsNotNull(j_fname)) &&
                           (t.IsNotNull(fvalue) == false))
                            s = s + v.ENTER + " and  " + m_alias + "." + mst_fname + " = " + j_table_alias + "." + j_fname + " ";
                        // and a.mst_FieldName = value
                        if (t.IsNotNull(mst_fname) &&
                           (t.IsNotNull(j_fname) == false) &&
                           (t.IsNotNull(fvalue)))
                            s = s + v.ENTER + " and  " + m_alias + "." + mst_fname + " = " + fvalue + " ";
                        // and b.join_FieldName = value
                        if ((t.IsNotNull(mst_fname) == false) &&
                            (t.IsNotNull(j_fname)) &&
                            (t.IsNotNull(fvalue)))
                        //s = s + v.ENTER + " and  " + j_table_alias + "." + j_fname + " = " + fvalue + " ";
                        {
                            try
                            {
                                /// burada value nin tipini bilmiyoruz
                                /// eğer bu çevrim hata vermezse int değerindedir
                                var x = Convert.ToInt64(fvalue);

                                s = s + v.ENTER + " and  " + j_table_alias + "." + j_fname + " = " + fvalue + " ";
                            }
                            catch (Exception)
                            {
                                /// eğer hata verdiyse string kabul ediyoruz 
                                s = s + v.ENTER + " and  " + j_table_alias + "." + j_fname + " = '" + fvalue + "' ";
                            }

                        }
                        // eğer üç değerde dolu ise value olduğu gibi eklenecek
                        if ((t.IsNotNull(mst_fname)) &&
                            (t.IsNotNull(j_fname)) &&
                            (t.IsNotNull(fvalue)))
                            s = s + " and  " + fvalue + " ";
                    }
                }
            }

            if (t.IsNotNull(where_add))
                s = s + " " + where_add + " ";

            #endregion J_WHERE

            return s;
        }

        private void Preparing_Select_Standart_Fields_JSON(List<J_STN_FIELDS> J_STN_FIELDS,
            ref string joinFields, string j_table_alias, string m_table_alias)
        {
            string j_alias = j_table_alias;
            string j_fname = string.Empty;
            string fnl_fname = string.Empty;

            if (j_alias.IndexOf("[") == -1)
                j_alias = "[" + j_alias + "]";

            #region J_STN_FIELDS
            foreach (var item in J_STN_FIELDS)
            {
                if (item.J_TABLE_ALIAS == j_table_alias)
                {
                    j_fname = item.J_FNAME.ToString();
                    fnl_fname = item.FNL_FNAME.ToString();

                    joinFields = joinFields + " , " + j_alias + "." + j_fname.PadRight(5) + " as " + fnl_fname + v.ENTER;
                }

                if (item.J_TABLE_ALIAS == m_table_alias)
                {
                    j_fname = item.J_FNAME.ToString();
                    fnl_fname = item.FNL_FNAME.ToString();

                    joinFields = joinFields + " , " + m_table_alias + "." + j_fname.PadRight(5) + " as " + fnl_fname + v.ENTER;
                }
            }
            #endregion J_STN_FIELDS
        }

        private void Preparing_Select_Case_Fields_JSON(List<J_CASE_FIELDS> J_CASE_FIELDS,
            ref string joinFields, string j_table_alias, string MasterTableAlias)
        {
            string s = string.Empty;
            string case_for_fname = string.Empty;
            string krt_odd = string.Empty;
            string where_value = string.Empty;
            string then_alias = string.Empty;
            string then_fname = string.Empty;
            string final_fname = string.Empty;

            string satir = string.Empty;
            string satir_sonu = string.Empty;
            string nsatir = string.Empty;
            int i1 = 0;

            foreach (var item in J_CASE_FIELDS)
            {
                case_for_fname = item.CASE_FOR_FNAME.ToString();
                where_value = item.WHERE_VALUE.ToString();
                then_alias = item.THEN_ALIAS.ToString();
                then_fname = item.THEN_FNAME.ToString();
                final_fname = item.FINAL_FNAME.ToString();

                if (item.KRT_ODD != null)
                    krt_odd = item.KRT_ODD.ToString();

                if (t.IsNotNull(krt_odd) == false)
                    krt_odd = "=";

                //satir_sonu = "  else '' end ) " + final_fname + "  ";
                satir_sonu = " end ) as " + final_fname + "  ";

                /// genel çerçeveyi oluştur 
                /// 
                /// , ( case 
                /// end ) as Lkp_DerslikPersDonemGrupSube

                if (nsatir.IndexOf(final_fname) == -1)
                {
                    if ((MasterTableAlias != "") && (case_for_fname != ""))
                    {
                        //satir = " , ( case " + MasterTableAlias + "." + case_for_fname + satir_sonu + v.ENTER;
                        satir = " , ( case " + v.ENTER + satir_sonu + v.ENTER;
                        nsatir = nsatir + satir;
                    }
                }

                //if ((where_value != "") && (then_alias != "") && (then_fname != ""))
                //    satir = " when " + where_value + " then " + then_alias + "." + then_fname;

                ///   when OzelGunlerId = 0 then personel.TamAdiSoyadi + ', ' + FNo9.DerslikAdiTipi + ', ' + FNo5.DonemTipi + ', ' + FNo6.GrupTipi + ', ' + FNo7.SubeTipi 
                ///   when OzelGunlerId > 0 then ozelGunler.GunAdi
                ///   -- final_fname  ? : case için kullanılan anahtar field birden çok case de kullanılabiliyor
                if ((where_value != "") && (then_alias != "") && (then_fname != ""))
                    satir = " when " + case_for_fname + " " + krt_odd + " " + where_value + " then " + then_alias + "." + then_fname + " -- " + final_fname + v.ENTER;

                if (nsatir.IndexOf(satir) == -1)
                {
                    i1 = nsatir.IndexOf(satir_sonu);
                    nsatir = nsatir.Insert(i1, satir);
                }
            }

            /// Sonuç
            ///  , ( case 
            ///  when OzelGunlerId = 0 then personel.TamAdiSoyadi + ', ' + FNo9.DerslikAdiTipi + ', ' + FNo5.DonemTipi + ', ' + FNo6.GrupTipi + ', ' + FNo7.SubeTipi
            ///  when OzelGunlerId > 0 then ozelGunler.GunAdi
            ///  end ) as Lkp_DerslikPersDonemGrupSube

            if (nsatir.Length > 0)
                joinFields = joinFields + nsatir;
        }

        #endregion Preparing_Join_Fields_Tables

        #endregion Preparing_dsData

        #region CategoryList
        public string CategoryList(string TableCode)
        {
            // burası artık çalışmıyor
            // çünkü artık bazı fieldler yok
            //
            string s =
               " select a.LOOKUP_CODE, a.TLKP_TYPE, "
             + " b.DATA_ABOUT CAPTION, a.FIELD_NAME, a.FIELD_TYPE "
             + " from "
             + " [" + v.active_DB.managerDBName + "].dbo.[MS_FIELDS] a, " + v.ENTER
             + " [" + v.active_DB.projectDBName + "].dbo.[APP_LKP_TABLES] b " + v.ENTER
             + " where a.TLKP_TYPE = 107 " + v.ENTER
             + " and not a.LOOKUP_CODE is null " + v.ENTER
             + " and a.TABLE_CODE = '" + TableCode + "' " + v.ENTER
             + " and a.LOOKUP_CODE = b.TYPES_NAME " + v.ENTER

             + " union all " + v.ENTER

             + " select a.LOOKUP_CODE, a.TLKP_TYPE, "
             + " a.FCAPTION  CAPTION, "
             //+ " a.FCAPTION + ' : ' + b.TYPES_CAPTION CAPTION, "
             + " a.FIELD_NAME, a.FIELD_TYPE "
             + " from "
             + " [" + v.active_DB.managerDBName + "].dbo.[MS_FIELDS] a, " + v.ENTER
             + " [" + v.active_DB.projectDBName + "].dbo.[APP_TYPES_LIST] b " + v.ENTER
             + " where a.TLKP_TYPE >= 101 and a.TLKP_TYPE <= 105 " + v.ENTER
             + " and not a.LOOKUP_CODE is null " + v.ENTER
             + " and a.TABLE_CODE = '" + TableCode + "' " + v.ENTER
             + " and a.LOOKUP_CODE = b.TYPES_CODE " + v.ENTER;

            return s;
        }
        #endregion CategoryList

    }

}
