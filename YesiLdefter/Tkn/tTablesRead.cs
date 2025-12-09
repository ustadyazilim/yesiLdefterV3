using DevExpress.Data.Linq.Helpers;
using DevExpress.XtraBars.Ribbon;
using Microsoft.JScript;
using System;
using System.Data;
using Tkn_SQLs;
using Tkn_ToolBox;
using Tkn_Variable;
using static FastReport.Fonts.TrueTypeFont;

namespace Tkn_TablesRead
{
    public class tTablesRead : tBase
    {
        #region System Tables Read 

        public void MS_Tables_IP_Read(DataSet ds, string TableIPCode)
        {
            string tSql = string.Empty;

            tToolBox t = new tToolBox();

            //tSQLs sql = new tSQLs();
            //string softCode = "";
            //string projectCode = "";
            //string TableCode = string.Empty;
            //string IPCode = string.Empty;
            //t.TableIPCode_Get(TableIPCode, ref softCode, ref projectCode, ref TableCode, ref IPCode);
            //tSql = sql.SQL_MS_TABLES_IP_LIST(softCode, projectCode, TableCode, IPCode);

            //tSql = t.msTableIPCodeTableList_SQL(TableIPCode);

            //if (ds != null)
            //    t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref tSql, "MS_TABLES_IP", function_name);

            t.preparing_TableIPCodeTableList(TableIPCode);

            DataTable dt = v.ds_TableIPCodeTable.Tables[TableIPCode];
            if (dt == null) return;
            ds.Tables.Add(dt.Copy());
            dt.Dispose();
        }

        public void MS_Fields_IP_Read(DataSet ds, string TableIPCode)
        {
            string function_name = "MS_Fields_IP_Read";
            string tSql = string.Empty;

            tToolBox t = new tToolBox();
            tSQLs sql = new tSQLs();

            //tSql = sql.SQL_MS_FIELDS_IP_LIST(TableIPCode);
            //tSql = t.msTableIPCodeFieldsList_SQL(TableIPCode);
            //t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref tSql, "MS_FIELDS_IP", function_name);

            t.preparing_TableIPCodeFieldsList(TableIPCode);

            DataTable dTable = v.ds_TableIPCodeFields.Tables[TableIPCode];
            if (dTable == null) return;
            ds.Tables.Add(dTable.Copy());
            dTable.Dispose();

            //tSql = sql.SQL_MS_GROUPS(TableIPCode);
            //t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref tSql, "GROUPS", function_name);

            t.preparing_TableIPCodeGroupsList(TableIPCode);

            DataTable dtGroup = v.ds_TableIPCodeGroups.Tables[TableIPCode + "_GROUPS"];
            if (dtGroup == null) return;
            ds.Tables.Add(dtGroup.Copy());
            dtGroup.Dispose();



            //if (TableIPCode.IndexOf("3S_") == -1)
            //{
            //    tSql = sql.SQL_MS_FIELDS_IP_LIST(TableIPCode);
            //    t.SQL_Read_Execute(v.dBName.Manager, ds, ref tSql, "MS_FIELDS_IP", function_name);
            //    tSql = sql.SQL_MS_GROUPS(TableIPCode);
            //    t.SQL_Read_Execute(v.dBName.Manager, ds, ref tSql, "GROUPS", function_name);
            //}
            //else
            //{
            //    // bu özel bir durum MS_TABLES e bak 
            //    if (TableIPCode != "3S_MSTBLIP_VWJ.3S_MSTBLIP_VWJ_L01")
            //    {
            //        tSql = sql.SQL_MS_FIELDS_IP_LIST(TableIPCode);
            //        t.SQL_Read_Execute(v.dBName.MainManager, ds, ref tSql, "MS_FIELDS_IP", function_name);
            //        tSql = sql.SQL_MS_GROUPS(TableIPCode);
            //        t.SQL_Read_Execute(v.dBName.MainManager, ds, ref tSql, "GROUPS", function_name);
            //    }
            //    else
            //    {  // 3S_MSTBLIP_VWJ.3S_MSTBLIP_VWJ_L01 için özel durum 
            //        tSql = sql.SQL_MS_FIELDS_IP_LIST(TableIPCode);
            //        t.SQL_Read_Execute(v.dBName.Manager, ds, ref tSql, "MS_FIELDS_IP", function_name);
            //        tSql = sql.SQL_MS_GROUPS(TableIPCode);
            //        t.SQL_Read_Execute(v.dBName.Manager, ds, ref tSql, "GROUPS", function_name);
            //    }
            //}

            //tSql = string.Empty;
        }

        public void MS_Properties_Read(DataSet ds, string TableName, string FieldName)
        {

            string function_name = "MS_Properties_Read";
            string tSql = string.Empty;

            tToolBox t = new tToolBox();
            tSQLs sql = new tSQLs();

            tSql = sql.SQL_MS_PROPERTIES_LIST(TableName, FieldName);
            t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref tSql, "MS_PROPERTIES", function_name);

        }

        public void MS_LayoutOrItems_Read(DataSet ds, string MasterCode, byte MasterItemType)
        {
            string tSql = string.Empty;

            tToolBox t = new tToolBox();
            //tSQLs sql = new tSQLs();

            // MasterItemType
            // 1 = Form
            // 2 = UserControl
            // 3 = Menu

            if (MasterItemType < 3)
            {
                //tSql = sql.SQL_MS_LAYOUT_LIST(MasterCode, MasterItemType);
                tSql = t.msLayoutItemsList_SQL(MasterCode);
            }

            if (MasterItemType == 3)
            {
                //tSql = sql.SQL_MS_ITEMS_LIST(MasterCode, MasterItemType);
                tSql = t.msMenuItemsList_SQL(MasterCode);
            }

            t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref tSql, "", "MS_LayoutOrItems Read");
        }

        public void MS_Layout_Read(DataSet ds, string MasterCode)
        {
            string tSql = string.Empty;

            tToolBox t = new tToolBox();
            //tSQLs sql = new tSQLs();

            // MasterItemType
            // 1 = Form
            // 2 = UserControl
            // 3 = Menu

            ////tSql = sql.SQL_MS_LAYOUT_LIST(MasterCode, MasterItemType);
            //tSql = t.msLayoutItemsList_SQL(MasterCode);
            //t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref tSql, "", "MS_LayoutOrItems Read");

            t.preparing_LayoutItemsList(MasterCode);

            DataTable dt = v.ds_MsLayoutItems.Tables[MasterCode];
            if (dt == null) return;
            ds.Tables.Add(dt.Copy());
            dt.Dispose();
        }

        public void MS_Menu_Read(DataSet ds, string MasterCode)
        {
            string tSql = string.Empty;

            tToolBox t = new tToolBox();
            //tSQLs sql = new tSQLs();

            // MasterItemType
            // 1 = Form
            // 2 = UserControl
            // 3 = Menu

            ////tSql = sql.SQL_MS_ITEMS_LIST(MasterCode, MasterItemType);
            //tSql = t.msMenuItemsList_SQL(MasterCode);
            //t.SQL_Read_Execute(v.dBaseNo.Manager, ds, ref tSql, "", "MS_LayoutOrItems Read");

            t.preparing_MenuItemsList(MasterCode);

            DataTable dt = v.ds_MsMenuItems.Tables[MasterCode];
            if (dt == null) return;
            ds.Tables.Add(dt.Copy());
            dt.Dispose();
        }

        public void preparingUserForm(DataSet ds)
        {
            DataTable dt = new DataTable("MS_LAYOUT");
            dt.Columns.Add("REF_ID", typeof(int));
            dt.Columns.Add("MODUL_CODE", typeof(string));
            dt.Columns.Add("MASTER_LAYOUT_TYPE", typeof(short));
            dt.Columns.Add("MASTER_CODE", typeof(string));
            dt.Columns.Add("LAYOUT_TYPE", typeof(string));
            dt.Columns.Add("LAYOUT_CODE", typeof(string));
            dt.Columns.Add("PARENT_CODE", typeof(string));
            dt.Columns.Add("LAYOUT_CAPTION", typeof(string));
            dt.Columns.Add("TABLEIPCODE", typeof(string));
            dt.Columns.Add("FIELD_NAME", typeof(string));
            dt.Columns.Add("TABLEIPCODE2", typeof(string));
            dt.Columns.Add("FIELD_NAME2", typeof(string));
            dt.Columns.Add("PROP_VIEWS", typeof(string));
            dt.Columns.Add("PROP_RUNTIME", typeof(string));
            dt.Columns.Add("PROP_HTMLATTRIBUTE", typeof(string));
            dt.Columns.Add("CMP_DOCK", typeof(short));
            dt.Columns.Add("CMP_WIDTH", typeof(short));
            dt.Columns.Add("CMP_HEIGHT", typeof(short));
            dt.Columns.Add("CMP_TOP", typeof(short));
            dt.Columns.Add("CMP_LEFT", typeof(short));
            dt.Columns.Add("CMP_READONLY", typeof(bool));
            dt.Columns.Add("CMP_ENABLED", typeof(bool));
            dt.Columns.Add("CMP_VISIBLE", typeof(bool));
            dt.Columns.Add("CMP_FONT_NAME", typeof(string));
            dt.Columns.Add("CMP_FONT_SIZE", typeof(string));
            dt.Columns.Add("CMP_FONT_STYLE", typeof(short));
            dt.Columns.Add("CMP_FONT_COLOR", typeof(int));
            dt.Columns.Add("CMP_BACK_COLOR", typeof(int));
            dt.Columns.Add("CMP_ROW_COUNT", typeof(int));
            dt.Columns.Add("CMP_COL_COUNT", typeof(int));
            dt.Columns.Add("CMP_LAYOUT_TYPE", typeof(short));
            dt.Columns.Add("CMP_FRONT_BACK", typeof(short));
            dt.Columns.Add("CMP_NAME", typeof(string));
            dt.Columns.Add("GROUP_NO", typeof(short));
            dt.Columns.Add("GROUP_LINE_NO", typeof(short));
            dt.Columns.Add("SOFTWARE_CODE", typeof(string));
            dt.Columns.Add("PROJECT_CODE", typeof(string));
            dt.Columns.Add("LYTMAIN_CODE", typeof(string));
            dt.Columns.Add("IMAGE_SOURCE", typeof(string));
            dt.Columns.Add("PROP_HTMLCONTENT", typeof(string));
            ds.Tables.Add(dt);

            DataRow row1 = ds.Tables[0].NewRow();
            fiilDataRow(row1,
                622, "ABO", 0, "UST/CRM/ABO/UstadUserLogin", "menu", "10", "NULL", "NULL", "UST/PMS/PMS/MSBOS", "NULL", "NULL", "NULL", "NULL", "NULL", "NULL", 0, 0, 0, 0, 0, false, true, true, "NULL", "0.000", 0, 0, 0, 0, 0, 0, 0, "NULL", 0, 0, "UST", "CRM", "UstadUserLogin", "NULL", "NULL");
            ds.Tables[0].Rows.Add(row1);

            DataRow row2 = ds.Tables[0].NewRow();
            fiilDataRow(row2, 619, "ABO", 0, "UST/CRM/ABO/UstadUserLogin", "backstageViewControl", "20","NULL",  "NULL",  "NULL",  "NULL",  "NULL",  "NULL",  "NULL",  "NULL",  "NULL",   5, 0, 0, 0, 0, false, true, true, "NULL",   "0.000", 0, 0, 0, 0, 0, 0, 0, "BACKVIEW", 0, 0, "UST", "CRM", "UstadUserLogin", "NULL", "NULL");
            ds.Tables[0].Rows.Add(row2);

            DataRow row3 = ds.Tables[0].NewRow();
            fiilDataRow(row3, 620, "ABO", 0, "UST/CRM/ABO/UstadUserLogin", "backstageViewTabItem", "20.10", "20", "Sisteme Giriş",  "NULL", "NULL", "NULL", "NULL", "NULL", "NULL", "NULL", 5, 0, 0, 0, 0, false, true, true, "NULL", "0.000", 0, 0, 0, 0, 0, 0, 0, "USERLOGIN", 0, 0, "UST", "CRM", "UstadUserLogin", "NULL", "NULL");
            ds.Tables[0].Rows.Add(row3);

        }

        private void fiilDataRow(DataRow row,
            int REF_ID,
            string MODUL_CODE,
            Int16 MASTER_LAYOUT_TYPE,
            string MASTER_CODE,
            string LAYOUT_TYPE,
            string LAYOUT_CODE,
            string PARENT_CODE,
            string LAYOUT_CAPTION,
            string TABLEIPCODE,
            string FIELD_NAME,
            string TABLEIPCODE2,
            string FIELD_NAME2,
            string PROP_VIEWS,
            string PROP_RUNTIME,
            string PROP_HTMLATTRIBUTE,
            Int16 CMP_DOCK,
            Int16 CMP_WIDTH,
            Int16 CMP_HEIGHT,
            Int16 CMP_TOP,
            Int16 CMP_LEFT,
            bool CMP_READONLY,
            bool CMP_ENABLED,
            bool CMP_VISIBLE,
            string CMP_FONT_NAME,
            string CMP_FONT_SIZE, //numeric(6, 3) 
            Int16 CMP_FONT_STYLE,
            int CMP_FONT_COLOR,
            int CMP_BACK_COLOR,
            int CMP_ROW_COUNT,
            int CMP_COL_COUNT,
            Int16 CMP_LAYOUT_TYPE,
            Int16 CMP_FRONT_BACK,
            string CMP_NAME,
            Int16 GROUP_NO,
            Int16 GROUP_LINE_NO,
            string SOFTWARE_CODE,
            string PROJECT_CODE,
            string LYTMAIN_CODE,
            string IMAGE_SOURCE,
            string PROP_HTMLCONTENT
            )
        {
            row["REF_ID"] = REF_ID;
            row["MODUL_CODE"] = MODUL_CODE;
            row["MASTER_LAYOUT_TYPE"] = MASTER_LAYOUT_TYPE;
            row["MASTER_CODE"] = MASTER_CODE;
            row["LAYOUT_TYPE"] = LAYOUT_TYPE;
            row["LAYOUT_CODE"] = LAYOUT_CODE;
            row["PARENT_CODE"] = PARENT_CODE;
            row["LAYOUT_CAPTION"] = LAYOUT_CAPTION;
            row["TABLEIPCODE"] = TABLEIPCODE;
            row["FIELD_NAME"] = FIELD_NAME;
            row["TABLEIPCODE2"] = TABLEIPCODE2;
            row["FIELD_NAME2"] = FIELD_NAME2;
            row["PROP_VIEWS"] = PROP_VIEWS;
            row["PROP_RUNTIME"] = PROP_RUNTIME;
            row["PROP_HTMLATTRIBUTE"] = PROP_HTMLATTRIBUTE;
            row["CMP_DOCK"] = CMP_DOCK;
            row["CMP_WIDTH"] = CMP_WIDTH;
            row["CMP_HEIGHT"] = CMP_HEIGHT;
            row["CMP_TOP"] = CMP_TOP;
            row["CMP_LEFT"] = CMP_LEFT;
            row["CMP_READONLY"] = CMP_READONLY;
            row["CMP_ENABLED"] = CMP_ENABLED;
            row["CMP_VISIBLE"] = CMP_VISIBLE;
            row["CMP_FONT_NAME"] = CMP_FONT_NAME;
            row["CMP_FONT_SIZE"] = CMP_FONT_SIZE;
            row["CMP_FONT_STYLE"] = CMP_FONT_STYLE;
            row["CMP_FONT_COLOR"] = CMP_FONT_COLOR;
            row["CMP_BACK_COLOR"] = CMP_BACK_COLOR;
            row["CMP_ROW_COUNT"] = CMP_ROW_COUNT;
            row["CMP_COL_COUNT"] = CMP_COL_COUNT;
            row["CMP_LAYOUT_TYPE"] = CMP_LAYOUT_TYPE;
            row["CMP_FRONT_BACK"] = CMP_FRONT_BACK;
            row["CMP_NAME"] = CMP_NAME;
            row["GROUP_NO"] = GROUP_NO;
            row["GROUP_LINE_NO"] = GROUP_LINE_NO;
            row["SOFTWARE_CODE"] = SOFTWARE_CODE;
            row["PROJECT_CODE"] = PROJECT_CODE;
            row["LYTMAIN_CODE"] = LYTMAIN_CODE;
            row["IMAGE_SOURCE"] = IMAGE_SOURCE;
            row["PROP_HTMLCONTENT"] = PROP_HTMLCONTENT;
        }

        #endregion System Tables Read

    }
}
