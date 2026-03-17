using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tkn_SQLs;
using Tkn_ToolBox;
using Tkn_Variable;

namespace Tkn_CreateDatabase
{
    public class tDatabase : tBase
    {
        tToolBox t = new tToolBox();
        tSQLs sqls = new tSQLs();

        #region DatabaseFind
        public bool tDatabaseFind(vTable vt)
        {
            bool onay = true; // kazayla okumazsa var kabul edilsin
            DataSet ds = new DataSet();

            String Sql = string.Format(" select * from [master].[sys].[databases] where name = '{0}' ", vt.DBaseName);

            t.SQL_Read_Execute(vt.DBaseNo, ds, ref Sql, "", "DataBase Find");

            // boş geldiyse database yok
            if (t.IsNotNull(ds) == false)
            {
                onay = false;
            }
            ds.Dispose();

            return onay;
        }

        public bool tDatabaseCreate(vTable vt)
        {
            bool onay = false;

            try
            {
                if (t.IsNotNull(vt.DBaseName))
                {
                    DataSet ds = new DataSet();

                    string Sql = " create database " + vt.DBaseName + " COLLATE Turkish_CI_AS ";

                    onay = t.Sql_ExecuteNon(ds, ref Sql, vt);

                    ds.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                //throw;
            }

            return onay;
        }

        public bool tSchemaCreate(vTable vt)
        {
            bool onay = false;

            try
            {
                if (t.IsNotNull(vt.SchemasCode))
                {
                    DataSet ds = new DataSet();

                    string Sql = " create SCHEMA " + vt.SchemasCode + " ";

                    onay = t.Sql_ExecuteNon(ds, ref Sql, vt);

                    ds.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                //throw;
            }

            return onay;
        }

        #endregion DatabaseFind

        #region TableFind

        public bool preparingCreateTable(vTable vt)
        {
            bool onay = true;
            bool tableOnay = false;

            //string fileName = "";
            string sqlScript = "";

            sqlScript = vt.SqlScript;
            //fileName = vt.TableName;
            
            // bunun sebebinin unuttum hatırlamıyorum
            //if (vt.SchemasCode != "dbo")
            //    fileName = vt.SchemasCode + "_" + vt.TableName;
            //----

            // parentTable olan tablolar başka bir tablonun Lkp tablolarıdır
            // onlar kendisinin bağlı olduğu tablo içerisinde create ediliyorlar
            // parentTable ismi olmayan Lkp_xxx tablolar ise bağımsız tablolardır
            //
            if ((vt.ParentTable == "") && (t.IsNotNull(sqlScript)))
            {
                v.active_DB.runDBaseNo = vt.DBaseNo; //v.dBaseNo.NewDatabase;

                // tablo var mı diye kontrol et
                //onay = tTableFind(fileName, vt);
                onay = tTableFind(vt);

                // tablo yok ise
                if (onay == false)
                {
                    tableOnay = tTableCreate(sqlScript, vt);
                }
                else tableOnay = onay; /// table var mesajı dönsün

                // tablo oluşturuldu şimdi trigger leri oluşturalım
                onay = false;
                onay = tTriggerCreate(sqlScript, vt);

                // tablo oluşturuldu şimdi Lkp.xxxx tabloları oluşturalım
                onay = false;
                onay = tLkpTableCreate(sqlScript, vt);

                // tablo oluşturuldu şimdi dataları insert edelim
                onay = false;
                onay = tDataCreate(sqlScript, vt);
            }

            return tableOnay;
        }

        //public bool tTableFind(string tableName, vTable vt)
        public bool tTableFind(vTable vt)
        {
            bool onay = true;
            String Sql = "";
            DataSet ds = new DataSet();

            v.active_DB.runDBaseNo = vt.DBaseNo;

            preparingVTable(vt);

            if (vt.DBaseType == v.dBaseType.MSSQL)
            {
                Sql = string.Format(" select * from {0}.sys.tables where name = '{1}' ", vt.DBaseName, vt.TableName);
            }

            t.SQL_Read_Execute(vt.DBaseNo, ds, ref Sql, "", "Table Find");

            if (t.IsNotNull(ds) == false)
            {
                onay = false;
            }

            ds.Dispose();

            return onay;
        }

        public bool tTableCreate(string SqlText, vTable vt)
        {
            bool onay = false;

            try
            {
                if (t.IsNotNull(SqlText))
                {
                    DataSet ds = new DataSet();
                    string Sql = preparingSqlScript(SqlText, "Table", vt);

                    onay = t.Sql_ExecuteNon(ds, ref Sql, vt);

                    ds.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                //throw;
            }

            return onay;
        }

        public bool tLkpTableCreate(string SqlText, vTable vt)
        {
            bool onay = false;

            try
            {
                if (t.IsNotNull(SqlText))
                {
                    DataSet ds = new DataSet();
                    string Sql = preparingSqlScript(SqlText, "LkpTable", vt);

                    onay = t.Sql_ExecuteNon(ds, ref Sql, vt); 

                    ds.Dispose();
                }
            }
            catch
            { }

            return onay;
        }

        public bool tDataCreate(string SqlText, vTable vt)
        {
            bool onay = false;

            try
            {
                if (t.IsNotNull(SqlText))
                {
                    DataSet ds = new DataSet();
                    string Sql = preparingSqlScript(SqlText, "Data", vt);

                    onay = t.Sql_ExecuteNon(ds, ref Sql, vt);

                    ds.Dispose();
                }
            }
            catch
            { }

            return onay;
        }

        public bool tTableCreateReadTextFile(string tableName, vTable vt)
        {
            // 
            // text file dan oku, sonra okuduğunu create et
            //
            bool onay = false;
            string fileName = string.Empty;

            try
            {
                fileName = v.EXE_ScriptsPath + "\\" + tableName + ".txt";
                if (File.Exists(@"" + fileName))
                {
                    DataSet ds = new DataSet();

                    string text = t.tReadTextFile(fileName);
                    string Sql = preparingSqlScript(text, "Table", vt);

                    onay = t.Sql_ExecuteNon(ds, ref Sql, vt);

                    ds.Dispose();
                }
            }
            catch
            { }

            return onay;
        }
        
        public void tPreparingData(string tableName)
        {
            String Sql = "";

            // Gerekli olan verileri topla
            DataSet ds = new DataSet();
            vTable vt = new vTable();

            tableName = tableName.Replace("data_", "");

            if (v.active_DB.runDBaseNo == v.dBaseNo.Project)
            {
                vt.DBaseNo = v.active_DB.projectDBaseNo;
                vt.DBaseType = v.active_DB.projectDBType;
                vt.DBaseName = v.active_DB.projectDBName;
            }

            if (vt.DBaseType == v.dBaseType.MSSQL)
            {
                Sql = string.Format(" select count(*) ADET from {0}.dbo.{1} ", vt.DBaseName, tableName);

                if (tableName == "SYS_TYPES_L")
                    Sql = @" select count(ID) ADET from  SYS_TYPES_L where TYPES_NAME not like 'ICRA_%' and TYPES_NAME not like 'AVK_%' ";

                if (tableName == "SYS_TYPES_L_AVK")
                    Sql = @" select count(ID) ADET from  SYS_TYPES_L where TYPES_NAME like 'ICRA_%' or TYPES_NAME like 'AVK_%' ";

                vt.msSqlConnection = v.active_DB.projectMSSQLConn;
            }

            t.SQL_Read_Execute(vt.DBaseNo, ds, ref Sql, "", "Preparing Data");

            if (t.IsNotNull(ds))
            {
                if (ds.Tables[0].Rows[0][0].ToString() == "0")
                {
                    MessageBox.Show(tableName + " tablosunda DATA yok, eklenecek...");

                    tDataInsert(tableName, vt);
                }
            }
            
            ds.Dispose();
        }

        private void tDataInsert(string tableName, vTable vt)
        {
            string fname = string.Empty;
            Boolean filenotfound = false;

            try
            {
                fname = v.EXE_PATH + "\\VT_MSSQL\\" + "data_" + tableName + ".txt";
                if (File.Exists(@"" + fname))
                {
                    //FileStream fileStream = new FileStream(@"" + fname, FileMode.Open, FileAccess.Read);

                    DataSet ds = new DataSet();
                    string Sql = null;
                    //StreamReader reader = new StreamReader(fileStream, Encoding.GetEncoding("iso-8859-9"), false);
                    // türkçe sorunu çözüldü
                    System.IO.TextReader readFile = new StreamReader(@"" + fname, Encoding.GetEncoding("iso-8859-9"), false);
                    //System.IO.TextReader readFile = new StreamReader(@"" + fname);

                    Sql = readFile.ReadToEnd();

                    int bgn = 0;
                    int end = 0;

                    if (vt.DBaseType == v.dBaseType.MSSQL)
                    {
                        bgn = Sql.IndexOf("-- INSERT_BEGIN;");
                        end = Sql.IndexOf("-- INSERT_END;");
                        Sql = Sql.Substring(bgn, (end - bgn) + 3);
                    }

                    if (t.Sql_ExecuteNon(ds, ref Sql, vt))
                    {
                        MessageBox.Show("Data başarıyla eklendi : " + tableName);
                    }

                    readFile.Close();
                    readFile = null;
                    filenotfound = false;
                    ds.Dispose();
                }
                else filenotfound = true;

                if (filenotfound == true)
                {
                    MessageBox.Show("File not found : " + fname);
                }
            }
            catch
            { }

        }

        #endregion

        #region StoredProcedure
        public bool tStoredProcedureFind(string procedureName, vTable vt)
        {
            bool onay = false;
            String Sql = "";
            DataSet ds = new DataSet();
            /*
            //if (v.active_DB.runDBaseNo == v.dBaseNo.Manager)
            //{
            //    vt.DBaseNo = v.active_DB.managerDBaseNo;
            //    vt.DBaseType = v.active_DB.managerDBType;
            //    vt.DBaseName = v.active_DB.managerDBName;
            //    vt.msSqlConnection = v.active_DB.managerMSSQLConn;
            //}

            //if (v.active_DB.runDBaseNo == v.dBaseNo.UstadCrm)
            //{
            //    vt.DBaseNo = v.active_DB.ustadCrmDBaseNo;
            //    vt.DBaseType = v.active_DB.ustadCrmDBType;
            //    vt.DBaseName = v.active_DB.ustadCrmDBName;
            //    vt.msSqlConnection = v.active_DB.ustadCrmMSSQLConn;
            //}

            //if (v.active_DB.runDBaseNo == v.dBaseNo.Project)
            //{
            //    vt.DBaseNo = v.active_DB.projectDBaseNo;
            //    vt.DBaseType = v.active_DB.projectDBType;
            //    vt.DBaseName = v.active_DB.projectDBName;
            //    vt.msSqlConnection = v.active_DB.projectMSSQLConn;
            //}

            //if (v.active_DB.runDBaseNo == v.dBaseNo.NewDatabase)
            //{
            //    vt.DBaseNo = v.newFirm_DB.dBaseNo;
            //    vt.DBaseType = v.newFirm_DB.dBType;
            //    vt.DBaseName = v.newFirm_DB.databaseName;
            //    vt.msSqlConnection = v.newFirm_DB.MSSQLConn;
            //}
            */
            preparingVTable(vt);

            if (vt.DBaseType == v.dBaseType.MSSQL)
            {
                Sql = string.Format(" select * from {0}.sys.procedures where name = '{1}' ", vt.DBaseName, procedureName);
            }

            t.SQL_Read_Execute(vt.DBaseNo, ds, ref Sql, "", "Procedure Find");

            if (t.IsNotNull(ds) == false)
            {
                onay = false;
            }

            ds.Dispose();
            return onay;
        }
        public bool tStoredProcedureDrop(vTable vt)
        {
            bool onay = false;
            String Sql = "";
            ///*
            //if (v.active_DB.runDBaseNo == v.dBaseNo.Manager)
            //{
            //    vt.DBaseNo = v.active_DB.managerDBaseNo;
            //    vt.DBaseType = v.active_DB.managerDBType;
            //    vt.DBaseName = v.active_DB.managerDBName;
            //    vt.msSqlConnection = v.active_DB.managerMSSQLConn;
            //}

            //if (v.active_DB.runDBaseNo == v.dBaseNo.UstadCrm)
            //{
            //    vt.DBaseNo = v.active_DB.ustadCrmDBaseNo;
            //    vt.DBaseType = v.active_DB.ustadCrmDBType;
            //    vt.DBaseName = v.active_DB.ustadCrmDBName;
            //    vt.msSqlConnection = v.active_DB.ustadCrmMSSQLConn;
            //}

            //if (v.active_DB.runDBaseNo == v.dBaseNo.Project)
            //{
            //    vt.DBaseNo = v.active_DB.projectDBaseNo;
            //    vt.DBaseType = v.active_DB.projectDBType;
            //    vt.DBaseName = v.active_DB.projectDBName;
            //    vt.msSqlConnection = v.active_DB.projectMSSQLConn;
            //}

            //if (v.active_DB.runDBaseNo == v.dBaseNo.NewDatabase)
            //{
            //    vt.DBaseNo = v.newFirm_DB.dBaseNo;
            //    vt.DBaseType = v.newFirm_DB.dBType;
            //    vt.DBaseName = v.newFirm_DB.databaseName;
            //    vt.msSqlConnection = v.newFirm_DB.MSSQLConn;
            //}

            //if (vt.SchemasCode == "")
            //    vt.SchemasCode = "dbo";
            //*/
            preparingVTable(vt);

            if (vt.DBaseType == v.dBaseType.MSSQL)
            {
                Sql = string.Format(" IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('[{0}].[{1}]') AND type in ('P')) " + v.ENTER
                + " DROP PROCEDURE [{0}].[{1}] ", vt.SchemasCode, vt.TableName);
            }

            try
            {
                onay = t.Sql_ExecuteNon(Sql, vt);
            }
            catch (Exception)
            {
                onay = false;
                //throw;
            }

            return onay;
        }
        public bool tStoredProcedureCreate(string SqlText, vTable vt)
        {
            bool onay = false;

            try
            {
                if (t.IsNotNull(SqlText))
                {
                    DataSet ds = new DataSet();
                    string Sql = preparingSqlScript(SqlText, "Procedure", vt);

                    onay = t.Sql_ExecuteNon(ds, ref Sql, vt);

                    ds.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                //throw;
            }

            return onay;
        }

        #endregion StoredProcedure

        #region Trigger
        public bool tTriggerFind(string triggerName, vTable vt)
        {
            bool onay = false;
            String Sql = "";
            DataSet ds = new DataSet();
            /*
            if (v.active_DB.runDBaseNo == v.dBaseNo.Manager)
            {
                vt.DBaseNo = v.active_DB.managerDBaseNo;
                vt.DBaseType = v.active_DB.managerDBType;
                vt.DBaseName = v.active_DB.managerDBName;
            }

            if (v.active_DB.runDBaseNo == v.dBaseNo.UstadCrm)
            {
                vt.DBaseNo = v.active_DB.ustadCrmDBaseNo;
                vt.DBaseType = v.active_DB.ustadCrmDBType;
                vt.DBaseName = v.active_DB.ustadCrmDBName;
            }

            if (v.active_DB.runDBaseNo == v.dBaseNo.Project)
            {
                vt.DBaseNo = v.active_DB.projectDBaseNo;
                vt.DBaseType = v.active_DB.projectDBType;
                vt.DBaseName = v.active_DB.projectDBName;
            }

            if (v.active_DB.runDBaseNo == v.dBaseNo.NewDatabase)
            {
                vt.DBaseNo = v.newFirm_DB.dBaseNo;
                vt.DBaseType = v.newFirm_DB.dBType;
                vt.DBaseName = v.newFirm_DB.databaseName;
                vt.msSqlConnection = v.newFirm_DB.MSSQLConn;
            }
            */
            preparingVTable(vt);

            if (vt.DBaseType == v.dBaseType.MSSQL)
            {
                Sql = string.Format(" select * from {0}.sys.triggers where name = '{1}' ", vt.DBaseName, triggerName);

                vt.msSqlConnection = v.active_DB.projectMSSQLConn;
            }

            t.SQL_Read_Execute(vt.DBaseNo, ds, ref Sql, "", "Trigger Find");

            if (t.IsNotNull(ds) == false)
            {
                onay = true;
            }

            ds.Dispose();

            return onay;
        }
        public bool tTriggerCreate(string SqlText, vTable vt)
        {
            bool onay = false;

            try
            {
                if (t.IsNotNull(SqlText))
                {
                    DataSet ds = new DataSet();
                    string Sql = "";

                    while (SqlText.IndexOf("/*CreateTrigger*/") > -1)
                    {
                        Sql = t.Get_And_Clear(ref SqlText, "/*CreateTriggerEnd*/") + "/*CreateTriggerEnd*/";

                        Sql = preparingSqlScript(Sql, "Trigger", vt);

                        // aynı trigger varsa sil
                        onay = triggerDrop(Sql, vt);

                        // triggeri create et
                        if (onay)
                        {
                            onay = t.Sql_ExecuteNon(ds, ref Sql, vt);
                        }
                    }
                    ds.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                //throw;
            }

            return onay;
        }
        private bool triggerDrop(string text, vTable vt)
        {
            string triggerName = triggerNameGet(text);

            bool onay = false;
            String Sql = "";
            /*
            if (v.active_DB.runDBaseNo == v.dBaseNo.Manager)
            {
                vt.DBaseNo = v.active_DB.managerDBaseNo;
                vt.DBaseType = v.active_DB.managerDBType;
                vt.DBaseName = v.active_DB.managerDBName;
            }

            if (v.active_DB.runDBaseNo == v.dBaseNo.UstadCrm)
            {
                vt.DBaseNo = v.active_DB.ustadCrmDBaseNo;
                vt.DBaseType = v.active_DB.ustadCrmDBType;
                vt.DBaseName = v.active_DB.ustadCrmDBName;
            }

            if (v.active_DB.runDBaseNo == v.dBaseNo.Project)
            {
                vt.DBaseNo = v.active_DB.projectDBaseNo;
                vt.DBaseType = v.active_DB.projectDBType;
                vt.DBaseName = v.active_DB.projectDBName;
            }

            if (v.active_DB.runDBaseNo == v.dBaseNo.NewDatabase)
            {
                vt.DBaseNo = v.newFirm_DB.dBaseNo;
                vt.DBaseType = v.newFirm_DB.dBType;
                vt.DBaseName = v.newFirm_DB.databaseName;
                vt.msSqlConnection = v.newFirm_DB.MSSQLConn;
            }
            */
            preparingVTable(vt);

            if (vt.DBaseType == v.dBaseType.MSSQL)
            {
                Sql = string.Format(@" if EXISTS (Select * From sys.triggers Where name = '{0}') " + v.ENTER
                + " drop trigger {0} ", triggerName);
            }

            try
            {
                onay = t.Sql_ExecuteNon(Sql, vt);
            }
            catch (Exception)
            {
                onay = false;
            }

            return onay;
        }
        private string triggerNameGet(string text)
        {
            //CREATE TRIGGER [dbo].[trg_MtskAday] on [dbo].[MtskAday]
            string name = "";
            name = text.Substring(0, text.ToUpper().IndexOf(" ON "));
            name = name.ToUpper().Replace("CREATE", "");
            name = name.ToUpper().Replace("TRIGGER", "");
            name = name.ToUpper().Replace("DBO", "");
            name = name.Replace("[", "");
            name = name.Replace("]", "");
            name = name.Replace(".", "");
            name = name.Trim();
            return name;
        }

        #endregion Trigger

        #region Function
        public bool tFunctionFind(string functionName, vTable vt)
        {
            bool onay = false;
            String Sql = "";
            DataSet ds = new DataSet();
            ///*
            //if (v.active_DB.runDBaseNo == v.dBaseNo.Manager)
            //{
            //    vt.DBaseNo = v.active_DB.managerDBaseNo;
            //    vt.DBaseType = v.active_DB.managerDBType;
            //    vt.DBaseName = v.active_DB.managerDBName;
            //    vt.msSqlConnection = v.active_DB.managerMSSQLConn;
            //}

            //if (v.active_DB.runDBaseNo == v.dBaseNo.UstadCrm)
            //{
            //    vt.DBaseNo = v.active_DB.ustadCrmDBaseNo;
            //    vt.DBaseType = v.active_DB.ustadCrmDBType;
            //    vt.DBaseName = v.active_DB.ustadCrmDBName;
            //    vt.msSqlConnection = v.active_DB.ustadCrmMSSQLConn;
            //}

            //if (v.active_DB.runDBaseNo == v.dBaseNo.Project)
            //{
            //    vt.DBaseNo = v.active_DB.projectDBaseNo;
            //    vt.DBaseType = v.active_DB.projectDBType;
            //    vt.DBaseName = v.active_DB.projectDBName;
            //    vt.msSqlConnection = v.active_DB.projectMSSQLConn;
            //}

            //if (v.active_DB.runDBaseNo == v.dBaseNo.NewDatabase)
            //{
            //    vt.DBaseNo = v.newFirm_DB.dBaseNo;
            //    vt.DBaseType = v.newFirm_DB.dBType;
            //    vt.DBaseName = v.newFirm_DB.databaseName;
            //    vt.msSqlConnection = v.newFirm_DB.MSSQLConn;
            //}
            //*/
            preparingVTable(vt);

            if (vt.DBaseType == v.dBaseType.MSSQL)
            {
                Sql = string.Format(" select * from {0}.sys.procedures where name = '{1}' ", vt.DBaseName, functionName);
            }

            t.SQL_Read_Execute(vt.DBaseNo, ds, ref Sql, "", "Function Find");

            if (t.IsNotNull(ds) == false)
            {
                onay = false;
            }

            ds.Dispose();
            return onay;
        }
        public bool tFunctionDrop(vTable vt)
        {
            bool onay = false;
            String Sql = "";
                        
            preparingVTable(vt);

            if (vt.DBaseType == v.dBaseType.MSSQL)
            {
                Sql = string.Format(" IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('[{0}].[{1}]') AND type in ('FN')) " + v.ENTER
                + " DROP FUNCTION [{0}].[{1}] ", vt.SchemasCode, vt.TableName);
            }

            try
            {
                onay = t.Sql_ExecuteNon(Sql, vt);
            }
            catch (Exception)
            {
                onay = false;
            }

            return onay;
        }
        public bool tFunctionCreate(string SqlText, vTable vt)
        {
            bool onay = false;

            try
            {
                if (t.IsNotNull(SqlText))
                {
                    DataSet ds = new DataSet();
                    string Sql = preparingSqlScript(SqlText, "Function", vt);

                    onay = t.Sql_ExecuteNon(ds, ref Sql, vt);

                    ds.Dispose();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                //throw;
            }

            return onay;
        }

        #endregion Function

        #region FieldFind

        public bool tFieldFind(vTable vt, string fieldName)
        {
            bool onay = true;
            String Sql = "";
            DataSet ds = new DataSet();

            v.active_DB.runDBaseNo = vt.DBaseNo;

            preparingVTable(vt);

            if (vt.DBaseType == v.dBaseType.MSSQL)
            {
                Sql = sqls.SQL_FieldFind(vt.DBaseName, vt.TableName, fieldName);
            }

            t.SQL_Read_Execute(vt.DBaseNo, ds, ref Sql, "", "Field Find");

            if (t.IsNotNull(ds) == false)
            {
                onay = false;
            }

            ds.Dispose();

            return onay;
        }

        #endregion

        private string preparingSqlScript(string SqlText, string ObjType, vTable vt)
        {
            string Sql = "";
            string create = "/*Create???*/";
            string createEnd = "/*Create???End*/";

            create = create.Replace("???", ObjType);
            createEnd = createEnd.Replace("???", ObjType);
            int length = create.Length;

            int i_bgn = SqlText.IndexOf(create); // ("/*CreateTable*/") + 15; 
            
            /// yoksa zaten mesaj olmaması gerekiyor
            /// begin var end yok ise o zaman mesaj versin
            //if (i_bgn < 0)
            //    MessageBox.Show("DİKKAT : " + vt.TableName + " için " + create + " ifadesi tespit edilemedi");

            if (i_bgn > -1) i_bgn += length;

            int i_end = SqlText.IndexOf(createEnd) - 1;   // ("/*CreateTableEnd*/") - 1;
            if (i_bgn > -1 && i_end < 0)
                MessageBox.Show("DİKKAT : " + vt.TableName + " için " + createEnd + " ifadesi tespit edilemedi");

            Sql = SqlText.Substring(i_bgn, i_end - i_bgn);
            Sql = Sql.Replace("--USE ", "USE ");
            Sql = Sql.Replace(":DBNAME", vt.DBaseName);
            Sql = Sql.Replace("GO\r\n", "\r\n");
            Sql = Sql.Replace("\"", "'");
            Sql = Sql.Replace("\':FIRM_ID\'", vt.FirmId.ToString());
            Sql = Sql.Replace(":FIRM_ID", vt.FirmId.ToString());

            return Sql;
        }

        public void preparingVTable(vTable vt)
        {
            if (v.active_DB.runDBaseNo == v.dBaseNo.Manager)
            {
                vt.DBaseNo = v.active_DB.managerDBaseNo;
                vt.DBaseType = v.active_DB.managerDBType;
                vt.DBaseName = v.active_DB.managerDBName;
                vt.msSqlConnection = v.active_DB.managerMSSQLConn;
            }

            if (v.active_DB.runDBaseNo == v.dBaseNo.UstadCrm)
            {
                vt.DBaseNo = v.active_DB.ustadCrmDBaseNo;
                vt.DBaseType = v.active_DB.ustadCrmDBType;
                vt.DBaseName = v.active_DB.ustadCrmDBName;
                vt.msSqlConnection = v.active_DB.ustadCrmMSSQLConn;
            }

            if (v.active_DB.runDBaseNo == v.dBaseNo.Project)
            {
                vt.DBaseNo = v.active_DB.projectDBaseNo;
                vt.DBaseType = v.active_DB.projectDBType;
                vt.DBaseName = v.active_DB.projectDBName;
                vt.msSqlConnection = v.active_DB.projectMSSQLConn;
            }

            if (v.active_DB.runDBaseNo == v.dBaseNo.Local)
            {
                vt.DBaseNo = v.active_DB.localDBaseNo;
                vt.DBaseType = v.active_DB.localDBType;
                vt.DBaseName = v.active_DB.localDBName;
                vt.msSqlConnection = v.active_DB.localMSSQLConn;
            }

            if (v.active_DB.runDBaseNo == v.dBaseNo.NewDatabase)
            {
                vt.DBaseNo = v.newFirm_DB.dBaseNo;
                vt.DBaseType = v.newFirm_DB.dBType;
                vt.DBaseName = v.newFirm_DB.databaseName;
                vt.msSqlConnection = v.newFirm_DB.MSSQLConn;
                vt.FirmId = v.newFirm_DB.firmId;
            }

            if (vt.SchemasCode == "")
                vt.SchemasCode = "dbo";
        }
    }


}
