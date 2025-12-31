using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tkn_CreateDatabase;
using Tkn_Events;
using Tkn_ToolBox;
using Tkn_Variable;
using Tkn_Save;

namespace YesiLdefter
{
    public partial class ms_ProjectTables : Form
    {
        #region Tanımlar
        tToolBox t = new tToolBox();
        tDatabase db = new tDatabase();
        tEventsMenu evm = new tEventsMenu();
        tSave sv = new tSave();

        DataSet dsFirm = null;
        DataNavigator dNFirm = null;
        DataSet dsTables = null;
        DataNavigator dNTables = null;
        DataSet dsProcedures = null;
        DataNavigator dNProcedures = null;
        DataSet dsFunctions = null;
        DataNavigator dNFunctions = null;

        DataSet dsDataTransfer = null;
        DataNavigator dNDataTransfer = null;

        Control tabControl = null;
        Control editpanel_Sonuc = null;
        Control editpanel_Sql = null;

        List<string> fieldsNameList = new List<string>();
        List<string> imagefieldsNameList = new List<string>();
        bool imageFieldAvailable = false;
        bool DbUpdatesIsActive = false;
        bool IsImageScripts = false;

        string fSchemaName = "SchemaName";
        string fTableName = "TableName";
        string fProcedureName = "ProcedureName";
        string fFunctionName = "FunctionName";
        string fParentTable = "ParentTable";
        string fHasATrigger = "HasATrigger";
        string fSqlScript = "SqlScript";

        string fFirmId = "FirmId";
        string fServerNameIP = "ServerNameIP";
        string fDatabaseName = "DatabaseName";
        string fDbLoginName = "DbLoginName";
        string fDbPass = "DbPass";

        string sourceServerNameIP = "SourceServerNameIP";
        string sourcefDatabaseName = "SourceDatabaseName";
        string sourceDbLoginName = "SourceDbLoginName";
        string sourceDbPass = "SourceDbPass";

        string tableIPCode = "";
        string tablesTableIPCode = "";
        string proceduresTableIPCode = "";
        string functionsTableIPCode = "";
        string dataTransferTableIPCode = "";
        
        string menuName1 = "MENU_" + "UST/PMS/PMS/ProjectTables";
        string buttonSingFileReadDbWrite = "SINGLE_FILE_READ_DB_WRITE";
        string buttonAllFileReadDbWrite = "ALL_FILE_READ_DB_WRITE";

        string menuName2 = "MENU_" + "UST/CRM/ABO/ProjectTablesCreate";
        string menuName22 = "MENU_" + "UST/CRM/ABO/ProjectTablesCreate2";
        string buttonDbConnection = "DB_CONNECTION";
        string buttonDbCreate = "DB_CREATE";
        string buttonFieldList = "FIELDS_LIST";
        string buttonDataView = "DATA_VIEW";

        string buttonTablesList = "TABLES_LIST";
        string buttonSingleTableCreate = "SINGLE_TABLE_CREATE";
        string buttonTablesCreate = "TABLES_CREATE";
        string buttonTriggersCreate = "TRIGGERS_CREATE";

        string buttonProceduresList = "PROCEDURES_LIST";
        string buttonSingleProcedureCreate = "SINGLE_PROCEDURE_CREATE";
        string buttonProceduresCreate = "PROCEDURES_CREATE";

        string buttonFunctionsList = "FUNCTIONS_LIST";
        string buttonSingleFunctionCreate = "SINGLE_FUNCTION_CREATE";
        string buttonFunctionsCreate = "FUNCTIONS_CREATE";

        string buttonSourceDBConnect = "SOURCE_DB_CONNECTION";
        string buttonSourceTablesList = "SOURCE_TABLES";
        string buttonSingleTableTransfer = "SINGLE_TABLE_TRANSFER";
        string buttonSingleTableTransferContinue = "SINGLE_TABLE_TRANSFER_CONTINUE";
        string buttonSingleTableTransferDateContinue = "SINGLE_TABLE_TRANSFER_DATE";
        string buttonTablesTransfer = "TABLES_TRANSFER";

        string startTarih = "";
        
        #endregion
        public ms_ProjectTables()
        {
            InitializeComponent();

            tEventsForm evf = new tEventsForm();

            this.Load += new System.EventHandler(evf.myForm_Load);
            this.Shown += new System.EventHandler(evf.myForm_Shown);
            this.Shown += new System.EventHandler(this.ms_ProjectTables_Shown);

            this.KeyPreview = true;
        }
        private void ms_ProjectTables_Shown(object sender, EventArgs e)
        {
            // Table scriplerini MsProjectTables tablosuna kaydetmek
            if (dsTables == null)
            {
                tablesTableIPCode = "UST/PMS/MsProjectTables.List_L01";
                t.Find_DataSet(this, ref dsTables, ref dNTables, tablesTableIPCode);

                // Tables, storedPurocedures, function SqlScript insert işlemleri
                if (t.IsNotNull(dsTables))
                {
                    preparingTablesProceduresFunctionsScripts();
                }
            }

            // Yeni Firma için Database, tables, procedures ve functions oluşturmak için
            if (dsFirm == null)
            {
                tableIPCode = "UST/CRM/UstadFirms.DBKart_F01";
                t.Find_DataSet(this, ref dsFirm, ref dNFirm, tableIPCode);

                if (dsFirm == null)
                {
                    tableIPCode = "UST/CRM/UstadFirms.DBKart_F02";
                    t.Find_DataSet(this, ref dsFirm, ref dNFirm, tableIPCode);
                }

                // Database create işlemleri
                if (t.IsNotNull(dsFirm))
                {
                    preparingDBCreate();
                }
            }

            //
            // aranan nesne memoEdit ()
            // memoEdit aslında bir panelin içinde sıfırncı kontrol olarak duruyor
            //
            //editpanel_Sonuc = t.Find_Control(this, v.lyt_Name + "30_30");
            //if (editpanel_Sonuc != null)
            //{
            //    if (((DevExpress.XtraEditors.PanelControl)editpanel_Sonuc).Controls.Count > 0)
            //    {
            //        ((DevExpress.XtraEditors.PanelControl)editpanel_Sonuc).Controls[0].Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            //    }
            //}

            //editpanel_Sql = t.Find_Control(this, v.lyt_Name + "30_20_10_10");
            //if (editpanel_Sql != null)
            //{
            //    if (((DevExpress.XtraEditors.PanelControl)editpanel_Sql).Controls.Count > 0)
            //    {
            //        ((DevExpress.XtraEditors.PanelControl)editpanel_Sql).Controls[0].Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            //    }
            //}


        }
        private void myNavElementClick(object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
        {
            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.NavButton")
            {
                // menu1
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonSingFileReadDbWrite) singleFileReadDbWrite();
                if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == buttonAllFileReadDbWrite) allFileReadDbWrite();

                //menu2
                //if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == ) ();
                //if (((DevExpress.XtraBars.Navigation.NavButton)sender).Name == ) ();
            }
            // SubItem butonlar
            if (sender.GetType().ToString() == "DevExpress.XtraBars.Navigation.TileNavItem")
            {
                // database
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonDbConnection) databaseConnectTEST();
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonDbCreate) databaseCreate();
                // tables
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonTablesList) tablesList((DevExpress.XtraBars.Navigation.TileNavItem)sender);
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonSingleTableCreate) singleTableCreate();
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonTablesCreate) tablesCreate();
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonFieldList) fieldList();
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonDataView) dataView();
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonTriggersCreate) triggersCreate();
                
                // procedures
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonProceduresList) tablesList((DevExpress.XtraBars.Navigation.TileNavItem)sender);
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonSingleProcedureCreate) singleProcedureCreate();
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonProceduresCreate) proceduresCreate();
                // functions
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonFunctionsList) tablesList((DevExpress.XtraBars.Navigation.TileNavItem)sender);
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonSingleFunctionCreate) singleFunctionCreate();
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonFunctionsCreate) functionsCreate();

                // veri aktarım
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonSourceDBConnect) sourceDBConnect();
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonSourceTablesList) sourceTablesList((DevExpress.XtraBars.Navigation.TileNavItem)sender);
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonSingleTableTransfer) singleTableTransfer();
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonSingleTableTransferContinue) singleTableTransferContinue();
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonSingleTableTransferDateContinue) singleTableTransferDateContinue();
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name == buttonTablesTransfer) tablesTransfer();
            }
        }

        #region Table + Trigger + Lpp.Tables, Procedure ve Function scriplerini MsProjectTables tablosuna kaydetmek
        private void singleFileReadDbWrite()
        {
            int pageIndex = t.Find_TabControlPageIndex(this.tabControl);
            
            string fileName = "";
            string vSchemaName = "";
            string vTableName = "";
            string vParentTable = "";
            string vHasATrigger = "";

            v.active_DB.runDBaseNo = v.dBaseNo.UstadCrm;

            v.EXE_ScriptsPath = getPathName();

            /// tables
            if (pageIndex == 1)
            {
                if (t.IsNotNull(dsTables))
                {
                    vSchemaName = dsTables.Tables[0].Rows[dNTables.Position][fSchemaName].ToString();
                    vTableName = dsTables.Tables[0].Rows[dNTables.Position][fTableName].ToString();
                    vParentTable = dsTables.Tables[0].Rows[dNTables.Position][fParentTable].ToString();
                    vHasATrigger = dsTables.Tables[0].Rows[dNTables.Position][fHasATrigger].ToString();

                    fileName = vTableName;
                    if (vSchemaName != "dbo" && vSchemaName != "")
                        fileName = vSchemaName + "_" + vTableName;

                    // parentTable olan tablolar başka bir tablonun Lkp tablolarıdır
                    // onlar kendisinin bağlı olduğu tablo içerisinde create ediliyorlar
                    // parentTable ismi olmayan Lkp_xxx tablolar ise bağımsız tablolardır
                    //
                    if (vParentTable == "")
                    {
                        tFileReadAndWrite(dsTables, dNTables, fSqlScript, fileName);
                    }
                }
            }
            /// procedures
            if (pageIndex == 2)
            {
                fileName = dsProcedures.Tables[0].Rows[dNProcedures.Position][fProcedureName].ToString();
                
                if (fileName != "")
                {
                    tFileReadAndWrite(dsProcedures, dNProcedures, fSqlScript, fileName);
                }
            }
            /// functions
            if (pageIndex == 3)
            {
                fileName = dsFunctions.Tables[0].Rows[dNFunctions.Position][fFunctionName].ToString();

                if (fileName != "")
                {
                    tFileReadAndWrite(dsFunctions, dNFunctions, fSqlScript, fileName);
                }
            }
        }

        private string getPathName()
        {
            string pathName = "";

            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                // İletişim kutusunun başlığını ayarlayın.
                folderBrowserDialog.Description = "Dosyaların bulunduğu klasör seçin";

                // Varsayılan klasörü ayarlayabilirsiniz (opsiyonel).
                folderBrowserDialog.SelectedPath = v.EXE_PATH;//  @"C:\";

                // Kullanıcı bir klasör seçerse işlemi onaylar.
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    // Seçilen klasörü alın ve ekranda gösterin.
                    pathName = folderBrowserDialog.SelectedPath;
                }
            }
            return pathName;
        }



        private void allFileReadDbWrite()
        {
            int pageIndex = t.Find_TabControlPageIndex(this.tabControl);

            if (pageIndex == 1)
            {
                allTableFileReadDbWrite();
            }
            if (pageIndex == 2)
            {
                allProcedureFileReadDbWrite();
            }
            if (pageIndex == 3)
            {
                allFunctionFileReadDbWrite();
            }
        }

        private void allTableFileReadDbWrite()
        {
            allFileReadDbWrite(dsTables, dNTables, "Table",
                fSchemaName, fTableName, fSqlScript, fParentTable);
        }

        private void allProcedureFileReadDbWrite()
        {
            allFileReadDbWrite(dsProcedures, dNProcedures, "Procedure",
                fSchemaName, fProcedureName, fSqlScript, "");
        }

        private void allFunctionFileReadDbWrite()
        {
            allFileReadDbWrite(dsFunctions, dNFunctions, "Function",
                fSchemaName, fFunctionName, fSqlScript, "");
        }

        private void allFileReadDbWrite(DataSet ds, DataNavigator dN, 
            string fileTypeName, 
            string fieldSchemaName, 
            string fieldEntityName, 
            string fieldSqlScript,
            string fieldParentName)
        {
            string fileName = "";
            string vSchemaName = "";
            string vEntityName = "";
            string vParentTable = "";

            v.active_DB.runDBaseNo = v.dBaseNo.UstadCrm;

            if (t.IsNotNull(ds))
            {
                string soru = " Tüm  " + fileTypeName + "  ?.txt dosyalarından okunarak database yazılacak. Onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes == cevap)
                {
                    v.EXE_ScriptsPath = getPathName();

                    int length = ds.Tables[0].Rows.Count;
                    for (int i = 0; i < length; i++)
                    {
                        dN.Position = i;

                        if (fileTypeName != "Function")
                            vSchemaName = ds.Tables[0].Rows[i][fieldSchemaName].ToString();
                        vEntityName = ds.Tables[0].Rows[i][fieldEntityName].ToString();
                        vParentTable = "";

                        if (fieldParentName != "")
                            vParentTable = ds.Tables[0].Rows[i][fieldParentName].ToString();

                        /// parentTable ?
                        /// parentTable olan tablolar başka bir tablonun Lkp tablolarıdır
                        /// onlar kendisinin bağlı olduğu tablo içerisinde create ediliyorlar
                        /// parentTable ismi olmayan Lkp_xxx tablolar ise bağımsız tablolardır
                        ///
                        if (vParentTable == "")
                        {
                            fileName = vEntityName;
                            if (vSchemaName != "dbo" && vSchemaName != "") 
                                fileName = vSchemaName + "_" + vEntityName;

                            tFileReadAndWrite(ds, dN, fieldSqlScript, fileName);
                        }
                    }
                    t.FlyoutMessage(null, fileTypeName + " dosyalarını okuma ve database yazma", "İşlemler tamamlandı...");
                }
            }
        }

        private bool tFileReadAndWrite(DataSet dsData, DataNavigator dNData, string fieldName, string fileName)
        {
            bool onay = false;
            string fName = string.Empty;
            string text = "";
            try
            {
                fName = v.EXE_ScriptsPath + "\\" + fileName + ".txt";
                if (File.Exists(@"" + fName))
                {
                    text = t.tReadTextFile(fName);
                }

                if (text != "")
                {
                    dsData.Tables[0].Rows[dNData.Position][fieldName] = text;

                    // kaydı aç
                    dsData.Tables[0].CaseSensitive = false;

                    dNData.Tag = dNData.Position;
                    NavigatorButton btnEnd = dNData.Buttons.EndEdit;
                    dNData.Buttons.DoClick(btnEnd);
                }
            }
            catch
            { }

            return onay;
        }

        #endregion Tablo scriplerini MsProjectTables tablosuna kaydetmek

        #region Database Create İşlemleri
        private void databaseConnectTEST()
        {
            dbConnction(true, true);
        }
        private void databaseCreate()
        {
            bool onay = false;
            onay = dbConnction(false, true);

            if (onay)
            {
                vTable vt = new vTable();
                vt.DBaseNo = v.dBaseNo.NewDatabase;
                vt.DBaseType = v.dBaseType.MSSQL;
                vt.DBaseName = v.newFirm_DB.databaseName;
                vt.msSqlConnection = v.newFirm_DB.MSSQLConn;

                onay = db.tDatabaseFind(vt);

                if (onay)
                {
                    t.FlyoutMessage(this, "Database : " + vt.DBaseName, "Bu isimde bir database mevcut ....");
                }

                if (onay == false)
                {
                    string soru = v.newFirm_DB.databaseName + " isimli database yok, oluşturulacak. Onaylıyor musunuz ?";
                    DialogResult cevap = t.mySoru(soru);
                    if (DialogResult.Yes == cevap)
                    {
                        onay = db.tDatabaseCreate(vt);
                    }

                    // database oluşturuldu şimdi Lkp schema oluşturalım
                    if (onay)
                    {
                        // bu sefer yeni database in connection sağlansın
                        dbConnction(false, false);
                        
                        vt.SchemasCode = "Lkp";
                        vt.msSqlConnection = v.newFirm_DB.MSSQLConn;
                        
                        onay = false;
                        onay = db.tSchemaCreate(vt);

                        if (onay)
                        {
                            //MessageBox.Show("Database ve Schema (Lkp) başarıyla açılmıştır ...");
                            t.FlyoutMessage(this, ":) Tebrikler...", "Database ve Schema (Lkp) başarıyla açılmıştır ...");
                        }
                    }
                }
            }
        }

        #endregion Database Create İşlemleri

        #region Tables, Procedures, Functions List
        private void tablesList(object sender)
        {
            bool onay = dbConnction(false, false);
            
            if (onay)
            {
                string buttonName = "";
                
                // gerçekte istekde bulunan butonun ismi
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name != null)
                    buttonName = ((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name.ToString();

                string islemButtonName = "item_FOPEN_SUBVIEW";
                string myFormLoadValue = "";
                string tableIPCode_ = "";

                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Appearance.Name != null)
                {
                    if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Tag != null)
                        myFormLoadValue = ((DevExpress.XtraBars.Navigation.TileNavItem)sender).Tag.ToString();

                    // bu işlev çalışsın diye bu isim verilerek istek gönderiliyor
                    evm.commonMenuClick(this, islemButtonName, "", myFormLoadValue);
                    
                    // tables listesini bulmak için hangi IPCode olduğunu bul
                    tableIPCode_ = findListTableIPCode(myFormLoadValue);

                    if (buttonName == buttonTablesList)
                        this.tablesTableIPCode = tableIPCode_;
                    if (buttonName == buttonProceduresList)
                        this.proceduresTableIPCode = tableIPCode_;
                    if (buttonName == buttonFunctionsList)
                        this.functionsTableIPCode = tableIPCode_;

                    // bulunan tableIPCode_ ile dsTables ve dNTables i bul
                    preparingObjectList();
                }
            }
            else MessageBox.Show("DİKKAT : Database ile bağlantı kurulamadı...");

        }
        private string findListTableIPCode(string myFormLoadValue)
        {
            string tableIPCode_ = "";
            string propNavigator = string.Empty;
            string workType = "";
            PROP_NAVIGATOR prop1_ = null;
            List<PROP_NAVIGATOR> propList_ = null;

            if (t.IsNotNull(myFormLoadValue))
                propNavigator = myFormLoadValue;
            
            // propNavigator ü temizle ve json a çevir
            if (propNavigator != "")
            {
                t.readProNavigator(propNavigator, ref prop1_, ref propList_);

                if (propList_ != null)
                {
                    foreach (PROP_NAVIGATOR prop_ in propList_)
                    {
                        foreach (TABLEIPCODE_LIST item in prop_.TABLEIPCODE_LIST)
                        {
                            workType = item.WORKTYPE.ToString();

                            if (workType == "SVIEW")
                            {
                                tableIPCode_ = item.TABLEIPCODE.ToString();
                                //TableAlias = item.TABLEALIAS.ToString();
                                //KeyFName = item.KEYFNAME.ToString();
                                break;
                            }
                        }
                    }
                }
            }

            return tableIPCode_;
        }
        private void preparingObjectList()
        {
            if (this.tablesTableIPCode != "")
            {
                if (dsTables == null)
                {
                    t.Find_DataSet(this, ref dsTables, ref dNTables, this.tablesTableIPCode);
                }
            }
            if (this.proceduresTableIPCode != "")
            {
                if (dsProcedures == null)
                {
                    t.Find_DataSet(this, ref dsProcedures, ref dNProcedures, this.proceduresTableIPCode);
                }
            }
            if (this.functionsTableIPCode != "")
            {
                if (dsFunctions == null)
                {
                    t.Find_DataSet(this, ref dsFunctions, ref dNFunctions, this.functionsTableIPCode);
                }
            }
            if (this.dataTransferTableIPCode != "")
            {
                if (dsDataTransfer == null)
                {
                    t.Find_DataSet(this, ref dsDataTransfer, ref dNDataTransfer, this.dataTransferTableIPCode);
                }
            }
        }

        #endregion Tables, Procedures, Functions List

        #region Tables işlemleri
        private void singleTableCreate()
        {
            if (t.IsNotNull(dsTables))
            {
                bool onay = true;
                
                v.active_DB.runDBaseNo = v.dBaseNo.NewDatabase;

                string tName = dsTables.Tables[0].Rows[dNTables.Position][fTableName].ToString();

                string soru = tName + " isimli tablo oluşturulacak. Onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes == cevap)
                {
                    onay = preparingCreateTable(dsTables, dNTables.Position, v.dBaseNo.NewDatabase);

                    if (onay)
                        t.FlyoutMessage(this, tName, "Table oluşturuldu...");
                }
            }
        }
        private void tablesCreate()
        {
            if (t.IsNotNull(dsTables))
            {
                bool onay = true;
                vTable vt = new vTable();
                v.active_DB.runDBaseNo = v.dBaseNo.NewDatabase;

                string soru = " Tüm tablo listesi oluşturulacak. Onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes == cevap)
                {
                    // çalışmıyor sebebini anlamadım.
                    //t.WaitFormOpen(this, "Tables create...");
                    //v.SP_OpenApplication = true;
                    //---

                    int length = dsTables.Tables[0].Rows.Count;
                    for (int i = 0; i < length; i++)
                    {
                        dNTables.Position = i;
                        onay = preparingCreateTable(dsTables, dNTables.Position, v.dBaseNo.NewDatabase);
                    }

                    //--- çalışmıyor
                    //v.IsWaitOpen = false;
                    //t.WaitFormClose();
                    //----
                    t.FlyoutMessage(this, "Table oluşturma","İşlemler tamamlandı...");
                }
            }
        }
        private bool preparingCreateTable(DataSet ds, int pos, v.dBaseNo dBaseNo)  
        {
            bool onay = false;

            vTable vt = new vTable();
            vt.DBaseNo = dBaseNo;
            vt.SchemasCode = ds.Tables[0].Rows[pos][fSchemaName].ToString();
            vt.TableName = ds.Tables[0].Rows[pos][fTableName].ToString();
            vt.ParentTable = ds.Tables[0].Rows[pos][fParentTable].ToString();
            vt.SqlScript = ds.Tables[0].Rows[pos][fSqlScript].ToString();

            onay = db.preparingCreateTable(vt);
            
            Application.DoEvents();

            return onay;
        }
        private void fieldList()
        {

        }
        private void dataView()
        {

        }
        private void triggersCreate()
        {
            if (t.IsNotNull(dsTables))
            {
                bool onay = true;
                vTable vt = new vTable();
                v.active_DB.runDBaseNo = v.dBaseNo.NewDatabase;

                string soru = " Tüm trigger listesi yeniden oluşturulacak. Onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes == cevap)
                {
                    // çalışmıyor sebebini anlamadım.
                    //t.WaitFormOpen(this, "Tables create...");
                    //v.SP_OpenApplication = true;
                    //---

                    int length = dsTables.Tables[0].Rows.Count;
                    for (int i = 0; i < length; i++)
                    {
                        dNTables.Position = i;
                        onay = preparingCreateTrigger(dNTables.Position);
                    }

                    //--- çalışmıyor
                    //v.IsWaitOpen = false;
                    //t.WaitFormClose();
                    //----
                    t.FlyoutMessage(this, "Trigger oluşturma", "İşlemler tamamlandı...");
                }
            }
        }
        private bool preparingCreateTrigger(int pos)
        {
            bool onay = false;
            string fileName = "";
            string vSchemaName = "";
            string vTableName = "";
            string vParentTable = "";
            //string vHasATrigger = "";
            string vSqlScript = "";
            vTable vt = new vTable();

            vSchemaName = dsTables.Tables[0].Rows[pos][fSchemaName].ToString();
            vTableName = dsTables.Tables[0].Rows[pos][fTableName].ToString();
            vParentTable = dsTables.Tables[0].Rows[pos][fParentTable].ToString();
            //vHasATrigger = dsTables.Tables[0].Rows[pos][fHasATrigger].ToString();
            vSqlScript = dsTables.Tables[0].Rows[pos][fSqlScript].ToString();

            vt.DBaseNo = v.dBaseNo.NewDatabase;
            vt.SchemasCode = vSchemaName;
            vt.TableName = vTableName;

            fileName = vTableName;
            if (vSchemaName != "dbo")
                fileName = vSchemaName + "_" + vTableName;

            Application.DoEvents();

            // parentTable olan tablolar başka bir tablonun Lkp tablolarıdır
            // onlar kendisinin bağlı olduğu tablo içerisinde create ediliyorlar
            // parentTable ismi olmayan Lkp_xxx tablolar ise bağımsız tablolardır
            //
            if ((vParentTable == "") && (t.IsNotNull(vSqlScript)))
            {
                v.active_DB.runDBaseNo = v.dBaseNo.NewDatabase;

                onay = db.tTriggerCreate(vSqlScript, vt);
            }

            Thread.Sleep(100);

            return onay;
        }

        #endregion Tables işlemleri

        #region Procedures İşlemleri
        private void singleProcedureCreate()
        {
            if (t.IsNotNull(dsProcedures))
            {
                bool onay = true;

                v.active_DB.runDBaseNo = v.dBaseNo.NewDatabase;

                string tName = dsProcedures.Tables[0].Rows[dNProcedures.Position][fProcedureName].ToString();

                string soru = tName + " isimli procedure oluşturulacak. Onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes == cevap)
                {
                    onay = preparingCreateProcedure(dNProcedures.Position);

                    if (onay)
                        t.FlyoutMessage(this, tName, "Procedure oluşturuldu...");
                }
            }
        }
        private void proceduresCreate()
        {
            if (t.IsNotNull(dsProcedures))
            {
                bool onay = true;
                vTable vt = new vTable();
                v.active_DB.runDBaseNo = v.dBaseNo.NewDatabase;

                string soru = " Tüm procedure listesi oluşturulacak. Onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes == cevap)
                {
                    int length = dsProcedures.Tables[0].Rows.Count;
                    for (int i = 0; i < length; i++)
                    {
                        dNProcedures.Position = i;
                        onay = preparingCreateProcedure(dNProcedures.Position);
                        //if (onay == false) break;
                    }
                    t.FlyoutMessage(this, "Procedure oluşturma", "İşlemler tamamlandı...");
                }
            }
        }
        private bool preparingCreateProcedure(int pos)
        {
            bool onay = false;
            string vSchemaName = "";
            string vProcedureName = "";
            string vSqlScript = "";
            vTable vt = new vTable();

            vSchemaName = dsProcedures.Tables[0].Rows[pos][fSchemaName].ToString();
            vProcedureName = dsProcedures.Tables[0].Rows[pos][fProcedureName].ToString();
            vSqlScript = dsProcedures.Tables[0].Rows[pos][fSqlScript].ToString();

            vt.DBaseNo = v.dBaseNo.NewDatabase;
            vt.SchemasCode = vSchemaName;
            vt.TableName = vProcedureName;
            
            Application.DoEvents();

            if (t.IsNotNull(vSqlScript))
            {
                v.active_DB.runDBaseNo = v.dBaseNo.NewDatabase;
                onay = false;
                onay = db.tStoredProcedureDrop(vt);
                onay = db.tStoredProcedureCreate(vSqlScript, vt);
            }

            Thread.Sleep(100);

            return onay;
        }

        #endregion Procedures İşlemleri

        #region Functions İşlemleri
        private void singleFunctionCreate()
        {
            if (t.IsNotNull(dsFunctions))
            {
                bool onay = true;

                v.active_DB.runDBaseNo = v.dBaseNo.NewDatabase;

                string tName = dsFunctions.Tables[0].Rows[dNFunctions.Position][fFunctionName].ToString();

                string soru = tName + " isimli function oluşturulacak. Onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes == cevap)
                {
                    onay = preparingCreateFunction(dNFunctions.Position);

                    if (onay)
                        t.FlyoutMessage(this, tName, "Function oluşturuldu...");

                }
            }
        }
        private void functionsCreate()
        {
            if (t.IsNotNull(dsFunctions))
            {
                bool onay = true;
                vTable vt = new vTable();
                v.active_DB.runDBaseNo = v.dBaseNo.NewDatabase;

                string soru = " Tüm function listesi oluşturulacak. Onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes == cevap)
                {
                    int length = dsFunctions.Tables[0].Rows.Count;
                    for (int i = 0; i < length; i++)
                    {
                        dNFunctions.Position = i;
                        onay = preparingCreateFunction(dNFunctions.Position);
                        //if (onay == false) break;
                    }
                    t.FlyoutMessage(this, "Function oluşturma", "İşlemler tamamlandı...");
                }
            }
        }
        private bool preparingCreateFunction(int pos)
        {
            bool onay = false;
            string vFunctionName = "";
            string vSqlScript = "";
            vTable vt = new vTable();

            vFunctionName = dsFunctions.Tables[0].Rows[pos][fFunctionName].ToString();
            vSqlScript = dsFunctions.Tables[0].Rows[pos][fSqlScript].ToString();

            vt.DBaseNo = v.dBaseNo.NewDatabase;
            vt.SchemasCode = "dbo";
            vt.TableName = vFunctionName;

            Application.DoEvents();

            if (t.IsNotNull(vSqlScript))
            {
                v.active_DB.runDBaseNo = v.dBaseNo.NewDatabase;
                onay = false;
                onay = db.tFunctionDrop(vt);
                onay = db.tFunctionCreate(vSqlScript, vt);
            }

            Thread.Sleep(100);

            return onay;
        }

        #endregion Functions İşlemleri

        #region Veri Aktarma İşlemleri

        private void sourceDBConnect()
        {
            sourceDbConnction_(true);
        }
        private void sourceTablesList(object sender)
        {
            bool onay = sourceDbConnction_(false);

            if (onay)
            {
                string buttonName = "";

                // gerçekte istekde bulunan butonun ismi
                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name != null)
                    buttonName = ((DevExpress.XtraBars.Navigation.TileNavItem)sender).Name.ToString();

                string islemButtonName = "item_FOPEN_SUBVIEW";
                string myFormLoadValue = "";
                string tableIPCode_ = "";

                if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Appearance.Name != null)
                {
                    if (((DevExpress.XtraBars.Navigation.TileNavItem)sender).Tag != null)
                        myFormLoadValue = ((DevExpress.XtraBars.Navigation.TileNavItem)sender).Tag.ToString();

                    // bu işlev çalışsın diye bu isim verilerek istek gönderiliyor
                    evm.commonMenuClick(this, islemButtonName, "", myFormLoadValue);

                    // tables listesini bulmak için hangi IPCode olduğunu bul
                    tableIPCode_ = findListTableIPCode(myFormLoadValue);

                    if (buttonName == buttonSourceTablesList)
                        this.dataTransferTableIPCode = tableIPCode_;

                    // bulunan tableIPCode_ ile dsDataTransfer ve dNDataTransfer i bul
                    preparingObjectList();
                }
            }
            else MessageBox.Show("DİKKAT : Database ile bağlantı kurulamadı...");

        }
        private void singleTableTransfer()
        {
            if (t.IsNotNull(dsDataTransfer))
            {
                bool onay = true;

                //v.active_DB.runDBaseNo = v.dBaseNo.aktrilacakDb;

                string about = dsDataTransfer.Tables[0].Rows[dNDataTransfer.Position]["About"].ToString();
                
                string soru = "[ " + about + " ] tablosundan, en baştan başlayarak Data Transferi yapılacak, onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes == cevap)
                {
                    onay = preparingTableDataTransfer(dNDataTransfer.Position, false, "", "", "");

                    if (onay)
                        t.FlyoutMessage(this, "[ " + about + " ]", "Data transferi gerçekleştirildi...");
                }
            }

        }

        private void singleTableTransferContinue()
        {
            if (t.IsNotNull(dsDataTransfer))
            {
                bool onay = true;

                //v.active_DB.runDBaseNo = v.dBaseNo.aktrilacakDb;

                string about = dsDataTransfer.Tables[0].Rows[dNDataTransfer.Position]["About"].ToString();

                string soru = "[ " + about + " ] tablosundan, kaldığı yerden başlayarak Data Transferi yapılacak, onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes == cevap)
                {
                    onay = preparingTableDataTransfer(dNDataTransfer.Position, true, "/*LastIdControlSql*/", "/*LastIdControlSqlEnd*/", "");

                    if (onay)
                        t.FlyoutMessage(this, "[ " + about + " ]", "Data transferi gerçekleştirildi...");
                }
            }
        }

        private void singleTableTransferDateContinue()
        {
            if (t.IsNotNull(dsDataTransfer))
            {
                bool onay = true;

                //v.active_DB.runDBaseNo = v.dBaseNo.aktrilacakDb;

                string about = dsDataTransfer.Tables[0].Rows[dNDataTransfer.Position]["About"].ToString();

                string soru = "[ " + about + " ] tablosundan, beliryeceğiniz tarihden başlayarak Data Transferi yapılacak, onaylıyor musunuz ?";
                DialogResult cevap = t.mySoru(soru);
                if (DialogResult.Yes == cevap)
                {
                    this.startTarih = t.getTarih(this.startTarih);

                    onay = preparingTableDataTransfer(dNDataTransfer.Position, true, "/*StartDateSql*/", "/*StartDateSqlEnd*/", this.startTarih);

                    if (onay)
                        t.FlyoutMessage(this, "[ " + about + " ]", "Data transferi gerçekleştirildi...");
                }
            }
        }

        private void tablesTransfer()
        {

        }

        private bool preparingTableDataTransfer(int pos, bool continue_, string startContent, string endContent, string inputValue)
        {
            bool onay = false;
            string sAlias = string.Empty;
            string sTableName = string.Empty;
            string tAlias = string.Empty;
            string tTableName = string.Empty;

            string sourceTableName = "";
            string sourceDataReadSql = "";
            string targetTableName = "";
            string targetFirmId = "";
            string targetFieldList = "";
            string targetInsertHeaderSql = "";
            string editWhereSql = ""; // "and Id = :IdValue ";
            string lastIdControlSql = "";
            string whereLastId = "";

            bool isIdentityInsert = false; // IDENTITY_INSERT ON/OFF
            bool isEditScript = false; // transfer sırasında Select kontrolüne edit scriptini ekle

            // firmanın yeni Id si : Mtsk000000??? şeklinde database adı oluşacak 
            targetFirmId = dsFirm.Tables[0].Rows[dNFirm.Position]["FirmId"].ToString();
            sourceTableName = dsDataTransfer.Tables[0].Rows[pos]["SourceTableName"].ToString();
            targetTableName = dsDataTransfer.Tables[0].Rows[pos]["TargetTableName"].ToString();
            /// Source tablosunda aktarılacak dataları okuyan sql
            sourceDataReadSql = dsDataTransfer.Tables[0].Rows[pos]["SourceDataReadSql"].ToString();
            /// Target table için hazırlanan insert ve update scriptinde kullanılıyor
            editWhereSql = dsDataTransfer.Tables[0].Rows[pos]["EditWhereSql"].ToString();
            /// Sourcede Id si neyse yeni target tabloda da aynı Id kullanılsın  : KURSIYER.Ulas = MtskAday.Id
            isIdentityInsert = (dsDataTransfer.Tables[0].Rows[pos]["IsIdentityInsert"].ToString() == "True");
            /// Target table için hazırlanan insert ve update scriptinde kullanılan where koşulu
            isEditScript = (dsDataTransfer.Tables[0].Rows[pos]["IsEditScript"].ToString() == "True");
            /// Data aktarımı sırasında kırılmalar olduğunda yeniden aktarım başladğında kaldığı yerden devam etmesini sağlıyor
            lastIdControlSql = dsDataTransfer.Tables[0].Rows[pos]["LastIdControlSql"].ToString();
            /// Images data aktarımı mı ?
            this.IsImageScripts = Convert.ToBoolean(dsDataTransfer.Tables[0].Rows[pos]["IsImageScripts"].ToString());

            sAlias = "";
            sTableName = "";
            t.String_Parcala(sourceTableName, ref sAlias, ref sTableName, ".");
            if (sAlias == "") sAlias = "dbo";

            tAlias = "";
            tTableName = "";
            t.String_Parcala(targetTableName, ref tAlias, ref tTableName, ".");
            if (tAlias == "") tAlias = "dbo";

            /// işlem yapılırken kırılmış olabilir
            /// işleme kaldığı yerden devam etmesi için gerekli where koşulu uygulanıyor
            /// 
            if ((continue_) && (lastIdControlSql != ""))
            {
                int position = 0;
                lastIdControlSql = t.getContent(lastIdControlSql, startContent, endContent, ref position);

                if (startContent == "/*LastIdControlSql*/")
                {
                    whereLastId = preparingGetLastId(tAlias, tTableName, lastIdControlSql);

                    if (whereLastId != "")
                        sourceDataReadSql = sourceDataReadSql.Replace("--:WhereAnd", whereLastId);

                }

                if (startContent == "/*StartDateSql*/")
                {
                    if (inputValue == "")
                        inputValue = "01.01.1990";

                    lastIdControlSql = lastIdControlSql.Replace(":TARIH", inputValue);

                    sourceDataReadSql = sourceDataReadSql.Replace("--:WhereAnd", lastIdControlSql);
                }
            }

            /// Data transferi başlamadan önce 
            /// dbo.DbUpdates tablosuna işaret koyacağız 
            /// Böylece data transferi sırasında bazı trigger ler üzerindeki procedure lerin çalışmasını engelleyerek
            /// hızlı transferi sağlamış oluyoruz
            /// örnek : trg_MtskAday içindeki 
            /// --EXECUTE [dbo].[prc_MtskDonemKontrolu] @yFirmId 
            /// --EXECUTE[dbo].[prc_MtskAdayTakip] @yFirmId
            /// 

            if (this.DbUpdatesIsActive == false)
            {
                this.DbUpdatesIsActive = preparingSetDbUpdatesIsActive();
            }

            vTable vt = new vTable();
            preparingSourceVTable(vt, sAlias, sTableName);

            DataSet dsSource = new DataSet();
            sourceDataReadSql = t.Str_Replace(ref sourceDataReadSql, "\":FIRM_ID\"", targetFirmId);
            sourceDataReadSql = t.SQLPreparing(sourceDataReadSql, vt);

            v.SP_OpenApplication = false;
            t.WaitFormOpen(v.mainForm, "Kaynak veriler okunuyor...");
            viewText("Kaynak veriler okunuyor...");

            t.Sql_Execute(dsSource, ref sourceDataReadSql, vt);
            
            if (t.IsNotNull(dsSource))
            {
                targetFieldList = preparingTargetFieldList(dsSource);
                targetInsertHeaderSql = preparingInsertScript(tAlias, tTableName, targetFieldList);
                
                onay = preparingDataTransfer(dsSource, tAlias, tTableName, targetInsertHeaderSql, editWhereSql, isIdentityInsert, isEditScript);
            }
            else
            {
                MessageBox.Show("DİKKAT : Aktarılacak data bulunamadı...");
            }

            t.WaitFormClose();
            Thread.Sleep(100);

            return onay;
        }

        private string preparingTargetFieldList(DataSet dsSource)
        {
            fieldsNameList.Clear();
            imagefieldsNameList.Clear();
            this.imageFieldAvailable = false;
            string fName = "";
            string fieldsName = "";
            int colCount = dsSource.Tables[0].Columns.Count;
            for (int i = 0; i < colCount; i++)
            {
                fName = dsSource.Tables[0].Columns[i].ColumnName;
                fieldsNameList.Add(fName);
                if (fName.IndexOf("Resim") == -1)
                {
                    /// tüm field listesi
                    fieldsName += ",[" + fName + "]";
                }
                else
                {
                    /// image Field mevcut
                    this.imagefieldsNameList.Add(fName);
                    this.imageFieldAvailable = true;
                }
            }
            fieldsName = fieldsName.Remove(0, 1);
            return fieldsName;
        }

        private string preparingInsertScript(string alias, string tableName, string targetFieldList)
        {
            string insertSql = " INSERT INTO [" + alias + "].[" + tableName + "] (" + targetFieldList + ") values ";
            return insertSql;
        }

        private bool preparingDataTransfer(DataSet dsSource, 
            string alias, string tableName, 
            string targetInsertHeaderSql, string editWhereSql, 
            bool isIdentityInsert, bool isEditScript)
        {
            bool onay = false;

            /// Data aktarılacak tablo hakkında detaylı teknik bilgileri oku
            /// 
            vTable vt = new vTable();
            vt.TableName = tableName;
            dbConnction(false, false);
            vt.SchemasCode = "Lkp";
            vt.msSqlConnection = v.newFirm_DB.MSSQLConn;
            t.preparing_MsTableFields(vt);

            /// resim data aktarma işlemi ise bu kısım hiç çalışmasın
            ///
            if (this.IsImageScripts == false)
                onay = preparingDataTransfer_(dsSource, alias, tableName,
                                              targetInsertHeaderSql, editWhereSql,
                                              isIdentityInsert, isEditScript);

            /// Resim / image fields mevcut
            if (this.imageFieldAvailable)
            {
                v.SP_OpenApplication = false;
                t.WaitFormOpen(v.mainForm, "Resim aktarım işlemi başlıyor...");
                viewText("Resim aktarım işlemi başlıyor...");

                onay = executeImage(dsSource, alias, tableName, editWhereSql);

                v.IsWaitOpen = false;
                t.WaitFormClose();
            }

            return onay;
        }

        private bool preparingDataTransfer_(DataSet dsSource,
            string alias, string tableName,
            string targetInsertHeaderSql, string editWhereSql,
            bool isIdentityInsert, bool isEditScript)
        {
            bool onay = false;
            int rowCount = dsSource.Tables[0].Rows.Count;
            int recCount = 1;
            int counted = 0;
            bool IsCrmDb = false;

            string sql = "";
            string insertValue = "";
            string targetInsertSql = "";
            string targetEditSql = "";
            string targetSelectSql = "";

            IsCrmDb = (tableName.IndexOf("UstadFirms") > -1);

            v.SP_OpenApplication = false;
            t.WaitFormOpen(v.mainForm, "");
            t.WaitFormOpen(v.mainForm, "Veri aktarım işlemi yapılıyor...");
            viewText("Veri aktarım işlemi başlıyor...");
            
            for (int i = 0; i < rowCount; i++)
            {
                insertValue = preparingInsertValues(dsSource, i, tableName);
                targetInsertSql = targetInsertHeaderSql + " ( " + insertValue + " ) ";
                if (isEditScript)
                    targetEditSql = preparingEditValues(dsSource, i, alias, tableName, editWhereSql);
                targetSelectSql = preparingSelectControl(dsSource, i, alias, tableName, editWhereSql, isEditScript);
                sql += preparingTransferSql(targetSelectSql, targetInsertSql, targetEditSql, isEditScript);

                if (recCount == 50)
                {
                    Application.DoEvents();

                    if (isIdentityInsert)
                        sql = preparingIsIdentityInsert(alias, tableName, sql);

                    if (IsCrmDb == false)
                        onay = executeNonTargetSql(sql);

                    if (onay)
                    {
                        sql = "";
                        counted += recCount;
                        recCount = 0;

                        viewText(counted.ToString() + " / " + rowCount.ToString() + " veri aktarım işlemi devam ediyor...");
                        v.SP_OpenApplication = false;
                        t.WaitFormOpen(v.mainForm, counted.ToString() + "/" + rowCount.ToString() + " veri aktarım işlemi devam ediyor...");
                        Application.DoEvents();
                    }
                    else
                    {
                        viewText("DİKKAT : Veri aktarım işlemi durduruldu ...");
                        this.imageFieldAvailable = false;
                        recCount = 0;
                        Application.DoEvents();

                        v.IsWaitOpen = false;
                        t.WaitFormClose();
                        break;
                    }
                }

                recCount += 1;
            }

            if ((recCount > 0) && (sql != ""))
            {
                if (isIdentityInsert)
                    sql = preparingIsIdentityInsert(alias, tableName, sql);

                if (IsCrmDb == false)
                    onay = executeNonTargetSql(sql);
                else executeNonCrmSql(sql, tableName);

                v.SP_OpenApplication = false;
                t.WaitFormOpen(v.mainForm, rowCount.ToString() + "/" + rowCount.ToString() + " veri aktarım işlemi devam ediyor...");
                viewText(rowCount.ToString() + " / " + rowCount.ToString() + " veri aktarım işlemi devam ediyor...");
            }

            v.IsWaitOpen = false;
            t.WaitFormClose();

            return onay;
        }


        private string preparingInsertValues(DataSet dsSource, int rowNo, string tableName)
        {
            string type = "";
            string value = "";
            string values = "";
            string fName = "";
            int ftype = 0;
            int fmax_length = 0;

            //int aaa = 0;

            int colCount = dsSource.Tables[0].Columns.Count;
            for (int i = 0; i < colCount; i++)
            {
                /// Resim fieldları null geldiği zaman onu yakalayamıyorum 
                //  type = dsSource.Tables[0].Rows[rowNo][i].GetType().ToString();
                //  value = dsSource.Tables[0].Rows[rowNo][i].ToString();

                fName = fieldsNameList[i];

                if (fName.IndexOf("Resim") == -1)
                {
                    type = dsSource.Tables[0].Rows[rowNo][fName].GetType().ToString();
                    value = dsSource.Tables[0].Rows[rowNo][fName].ToString();

                    //if (type == "System.DateTime")
                    //    aaa = 1;

                    // data transfer sırasında bizim olmaya tablolar sırasında gerekiyor
                    if (t.IsNotNull(tableName) && tableName.IndexOf("WENNTEC") == -1)
                    {
                        value = preparingValue(value, tableName, i);
                    }
                    else
                    {
                        //if (value.IndexOf("HÜSEYİN VERAL") > -1)
                        //    value = t.Str_Check(value);
                        // özel durumlar burada hazırlanacak
                        if (type == "System.String")  
                            t.Str_Replace(ref value, (char)39, (char)32); // ' > boşluk yap
                    }
                    if (type == "System.Int16") values += " , " + value;
                    if (type == "System.Int32") values += " , " + value;
                    if (type == "System.Int64") values += " , " + value;
                    if (type == "System.DateTime") values += " , convert(Datetime, '" + value + "',103)";
                    if (type == "System.String") values += " , '" + value + "' ";
                    if (type == "System.DBNull") values += " , null";
                    if (type == "System.Double" || type == "System.Decimal")
                    {
                        value = value.Replace(",", ".");
                        values += " , " + value;
                    }
                    if (type == "System.Boolean")
                    {
                        if (value == "False") values += " , 0 ";
                        if (value == "True") values += " , 1 ";
                    }
                    if (type == "System.Byte[]")
                    {
                        /// resim fieldları burada olmadğı için devreye girmiyor artık

                        //string base64Encoded = Convert.ToBase64String((byte[])dsSource.Tables[0].Rows[rowNo][i]);
                        //values += ", cast('" + base64Encoded + "' as varbinary(max))";

                        //values += ", CAST( '" + Convert.ToBase64String((byte[])dsSource.Tables[0].Rows[rowNo][i]) + "' as varbinary(max) ) ";

                        //values += ", CONVERT(varchar(8000), convert(varbinary(8000), '" + Convert.ToBase64String((byte[])dsSource.Tables[0].Rows[rowNo][i]) + "')) ";
                        //values += ", convert(varbinary(8000), '" + Convert.ToBase64String((byte[])dsSource.Tables[0].Rows[rowNo][i]) + "') ";
                    }
                }

                //System.Int32
                //System.DateTime
                //System.String
                //System.DBNull
                //System.Double
                //System.Byte[]
            }
            values = values.Remove(0, 2);
            return values;
        }

        private string preparingValue(string value, string tableName, int i)
        {
            int ftype = Convert.ToInt32(v.ds_MsTableFields.Tables[tableName].Rows[i]["user_type_id"].ToString());
            int fmax_length = Convert.ToInt32(v.ds_MsTableFields.Tables[tableName].Rows[i]["max_length"].ToString());

            // adres verisinde görülebiliyor
            //if (value.IndexOf("<option value=\"") > -1)
            //    value = value.Replace("<option value=\"", "");

            if (value.IndexOf("option") > -1)
                value = "";

            if (value.IndexOf("\">") > -1)
                value = value.Replace("\">", "");

            if (value.IndexOf("\"") > -1)
                value = value.Replace("\"", "");

            if (value.IndexOf("'") > -1)
                value = value.Replace("'", " ");

            //* text = (char)175, (varchar)167, (ntext)99, (text)35, (nchar)239, (nvarchar)231, (uniqueidentifier)36 //39 
            if ((ftype == 175) | (ftype == 167) | (ftype == 99) | (ftype == 35) | (ftype == 239) | (ftype == 231) | (ftype == 36))
            {
                // varchar(max), nvarchar(max) olunca -1 geliyor
                if (fmax_length == -1) fmax_length = 128000;
                if (ftype == 36) // GUID / uniqueidentifier
                    fmax_length = 128000;
                if (ftype == 231) // nVarchar ise
                    fmax_length = fmax_length / 2;

                if (value.Length > fmax_length)
                    value = value.Substring(0, fmax_length);

                value = t.Str_Check(value);
            }

            return value;
        }
        private string preparingEditValues(DataSet dsSource, int rowNo, string alias, string tableName, string editWhereSql)
        {
            string type = "";
            string value = "";
            string values = "";
            string fieldName = "";
            string editSql = "";
            string editWhere = editWhereSql;
            int x = 0;
            int colCount = dsSource.Tables[0].Columns.Count;
            //int ftype = 0;
            //int fmax_length = 0;

            for (int i = 0; i < colCount; i++)
            {
                type = dsSource.Tables[0].Rows[rowNo][i].GetType().ToString();
                value = dsSource.Tables[0].Rows[rowNo][i].ToString();
                fieldName = dsSource.Tables[0].Columns[i].ColumnName;
                //fieldName = fieldsNameList[i]; ikiside olur

                // data transfer sırasında bizim olmaya tablolar sırasında gerekiyor
                if (t.IsNotNull(tableName) && tableName.IndexOf("WENNTEC") == -1)
                { 
                    value = preparingValue(value, tableName, i);
                }
                else
                {
                    // özel durumlar burada hazırlanacak
                    if (type == "System.String")
                        t.Str_Replace(ref value, (char)39, (char)32); // ' > boşluk yap
                }

                // Where koşulunda var mı?
                x = editWhere.IndexOf(":" + fieldName + "Value");
                
                if (x > 0)
                {
                    if (type == "System.Int16") editWhere = editWhere.Replace(":" + fieldName + "Value", value);
                    if (type == "System.Int32") editWhere = editWhere.Replace(":" + fieldName + "Value", value);
                    if (type == "System.Int64") editWhere = editWhere.Replace(":" + fieldName + "Value", value);
                    if (type == "System.String") editWhere = editWhere.Replace(":" + fieldName + "Value", "'" + value + "'");
                }
                else
                {
                    if (fieldName.IndexOf("Resim") == -1)
                    {
                        fieldName = "[" + fieldName + "]";
                        if (type == "System.Int16") values += " , " + fieldName + " = " + value;
                        if (type == "System.Int32") values += " , " + fieldName + " = " + value;
                        if (type == "System.Int64") values += " , " + fieldName + " = " + value;
                        if (type == "System.DateTime") values += " , " + fieldName + " = " + " convert(Datetime, '" + value + "',103)";
                        if (type == "System.String") values += " , " + fieldName + " = " + "'" + value + "' ";
                        if (type == "System.DBNull") values += " , " + fieldName + " = " + " null ";
                        if (type == "System.Double" || type == "System.Decimal")
                        {
                            value = value.Replace(",", ".");
                            values += " , " + fieldName + " = " + value;
                        }
                        if (type == "System.Boolean")
                        {
                            if (value == "False") values += " , " + fieldName + " = 0 ";
                            if (value == "True") values += " , " + fieldName + " = 1 ";
                        }
                        //if (type == "System.Byte[]") values += " , " + fieldName + " = " + value;
                    }
                }
            }
            values = values.Remove(0, 2);
            editSql = " update " + alias + "." + tableName + " set " + v.ENTER
                + values + v.ENTER
                + " Where 0 = 0 " + editWhere;
            return editSql;
        }

        private void preparingImageEditSql(DataSet dsSource, int rowNo, string alias, string tableName, string editWhereSql, ref string selectSql, ref string updateSql)
        {
            string type = "";
            string value = "";
            string imgFieldsNameSelect = "";
            string imgFieldsNameUpdate = "";
            string fieldName = "";
            string editWhere = editWhereSql;
            int x = 0;
            int count = dsSource.Tables[0].Columns.Count;
            //byte[] imgSourceValue;
            string imgSourceValue = "";

            for (int i = 0; i < count; i++)
            {
                type = dsSource.Tables[0].Rows[rowNo][i].GetType().ToString();
                value = dsSource.Tables[0].Rows[rowNo][i].ToString();
                fieldName = dsSource.Tables[0].Columns[i].ColumnName;
                // Where koşulunda var mı?
                x = editWhere.IndexOf(":" + fieldName + "Value");

                if (x > 0)
                {
                    if (type == "System.Int32") editWhere = editWhere.Replace(":" + fieldName + "Value", value);
                    if (type == "System.String") editWhere = editWhere.Replace(":" + fieldName + "Value", "'" + value + "'");
                }
            }

            count = imagefieldsNameList.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    imgSourceValue = "";
                    imgFieldsNameSelect += " , " + imagefieldsNameList[i];
                    imgSourceValue = dsSource.Tables[0].Rows[rowNo][imagefieldsNameList[i]].ToString();
                    if (imgSourceValue?.Length > 0)
                        imgFieldsNameUpdate += " , " + imagefieldsNameList[i] + " = @" + imagefieldsNameList[i];
                }

                if (imgFieldsNameSelect.Length > 0)
                    imgFieldsNameSelect = imgFieldsNameSelect.Remove(0, 2);
                if (imgFieldsNameUpdate.Length > 0)
                    imgFieldsNameUpdate = imgFieldsNameUpdate.Remove(0, 2);

                selectSql =
                  " select " + imgFieldsNameSelect + " from " + alias + "." + tableName + " "
                + " Where 0 = 0 " + editWhere;

                if (imgFieldsNameUpdate.Length > 0)
                {
                    updateSql =
                        " Update " + alias + "." + tableName + " set "
                    + imgFieldsNameUpdate
                    + " Where 0 = 0 " + editWhere;
                }
                else updateSql = "";

            }
        }

        private string preparingSelectControl(DataSet dsSource, int rowNo, string alias, string tableName, string editWhereSql, bool isEditScript)
        {
            string type = "";
            string value = "";
            string fieldName = "";
            string selectSql = "";
            string editWhere = editWhereSql;

            int x = 0;
            int colCount = dsSource.Tables[0].Columns.Count;
            for (int i = 0; i < colCount; i++)
            {

                type = dsSource.Tables[0].Rows[rowNo][i].GetType().ToString();
                value = dsSource.Tables[0].Rows[rowNo][i].ToString();
                fieldName = dsSource.Tables[0].Columns[i].ColumnName;
                // Where koşulunda var mı?
                x = editWhere.IndexOf(":" + fieldName + "Value");

                if (x > 0)
                {
                    if (type == "System.Int16") editWhere = editWhere.Replace(":" + fieldName + "Value", value);
                    if (type == "System.Int32") editWhere = editWhere.Replace(":" + fieldName + "Value", value);
                    if (type == "System.Int64") editWhere = editWhere.Replace(":" + fieldName + "Value", value);
                    if (type == "System.String") editWhere = editWhere.Replace(":" + fieldName + "Value", "'" + value + "'");
                }
            }

            selectSql =
                " if ( Select count(*) ADET from [" + alias + "].[" + tableName + "] where 0 = 0 " + editWhere + " ) = 0 " + v.ENTER
              + " begin " + v.ENTER
              + " --:insertCumlesi " + v.ENTER
              + " end " + v.ENTER;

            if (isEditScript)
              selectSql += 
                " else begin " + v.ENTER
              + " --:editCumlesi " + v.ENTER
              + " end ";

            return selectSql;
        }

        private string preparingTransferSql(string selectSql, string insertSql, string editSql, bool isEditScrit)
        {
            string sql = selectSql.Replace("--:insertCumlesi", insertSql);
            if (isEditScrit)
                sql = sql.Replace("--:editCumlesi", editSql); 
            return sql;
        }

        private string preparingIsIdentityInsert(string alias, string tableName, string sql)
        {
            sql = " SET IDENTITY_INSERT [" + alias + "].[" + tableName + "] ON " + v.ENTER
                + sql + v.ENTER
                + " SET IDENTITY_INSERT [" + alias + "].[" + tableName + "] OFF " + v.ENTER;
            return sql;
        }

        private string preparingGetLastId(string alias, string tableName, string lastIdControlSql)
        {
            string editWhere = "";

            t.Str_Replace(ref lastIdControlSql, "\"", "'");
            
            // yeni database in connection sağlansın
            dbConnction(false, false);

            vTable vt = new vTable();
            preparingTargetVTable(vt, alias, tableName);

            DataSet dsLastIdControl = new DataSet();

            try
            {
                executeTargetSql(dsLastIdControl, lastIdControlSql, vt);

                /// 0. kolonda where koşulu
                /// 1. kolonda sourceTable fieldName
                /// 2. kolonda target tableden okunan max Id value 
                /// 
                /// Select
                /// ' Where :IdFieldName > :IdFieldValue '
                /// , 'ULAS' as IdFieldName
                /// , max(Id) as LastId
                /// from dbo.MtskAday

                if (t.IsNotNull(dsLastIdControl))
                {
                    string fieldName = dsLastIdControl.Tables[0].Rows[0][1].ToString();
                    string type = dsLastIdControl.Tables[0].Rows[0][2].GetType().ToString();
                    string value = dsLastIdControl.Tables[0].Rows[0][2].ToString();

                    if ((value != "") || (value != null) || value != "0")
                    {
                        // 0. kolonda where
                        editWhere = dsLastIdControl.Tables[0].Rows[0][0].ToString();

                        if (editWhere != "")
                        {
                            t.Str_Replace(ref editWhere, "\"", "'");

                            if (type == "System.Int32") editWhere = editWhere.Replace(":IdFieldValue", value);
                            if (type == "System.String") editWhere = editWhere.Replace(":IdFieldValue", "'" + value + "'");

                            editWhere = editWhere.Replace(":IdFieldName", fieldName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                //throw;
            }

            return editWhere;
        }

        private bool executeNonTargetSql(string targetSql)
        {
            /// Hedef database ile bağlantı kuruluyor
            bool onay = dbConnction(false, false);

            if (onay)
            {
                vTable vt = new vTable();
                preparingTargetVTable(vt, "", "");

                DataSet dsTarget = new DataSet();

                onay = t.Sql_ExecuteNon(dsTarget, ref targetSql, vt);

                if (onay == false) viewSqlText(targetSql);
            }

            return onay;
        }

        private bool executeNonCrmSql(string targetSql, string tableName)
        {
            v.active_DB.runDBaseNo = v.dBaseNo.UstadCrm;

            DataSet ds = new DataSet();
            bool onay = t.SQL_Read_Execute(v.dBaseNo.UstadCrm, ds, ref targetSql, tableName, tableName);
            if (onay == false) viewSqlText(targetSql);

            return onay;
        }

        private bool executeTargetSql(DataSet ds, string sql, vTable vt)
        {
            /// Hedef database ile bağlantı kuruluyor
            bool onay = dbConnction(false, false);

            if (onay)
            {
                onay = t.Sql_Execute(ds, ref sql, vt);

                if (onay == false) viewSqlText(sql);
            }

            return onay;
        }

        private void preparingSourceVTable(vTable vt, string alias, string tableName)
        {
            vt.DBaseNo = v.dBaseNo.aktrilacakDb;
            vt.DBaseType = v.dBaseType.MSSQL;
            vt.DBaseName = v.source_DB.databaseName;
            vt.msSqlConnection = v.source_DB.MSSQLConn;
            vt.SchemasCode = alias;
            vt.TableName = tableName;
            vt.TableIPCode = "";
        }

        private void preparingTargetVTable(vTable vt, string alias, string tableName)
        {
            vt.DBaseNo = v.newFirm_DB.dBaseNo;
            vt.DBaseType = v.newFirm_DB.dBType;
            vt.DBaseName = v.newFirm_DB.databaseName;
            vt.msSqlConnection = v.newFirm_DB.MSSQLConn;
            vt.SchemasCode = alias;
            vt.TableName = tableName;
            vt.TableIPCode = "";
        }

        private bool executeImage(DataSet dsSource, string alias, string tableName, string editWhereSql)
        {
            bool onay = true;

            int rowCount = dsSource.Tables[0].Rows.Count;
            int imgFieldCount = imagefieldsNameList.Count;

            string selectSql = "";
            string updateSql = "";
            string imgFieldName = "";
            byte[] imgSourceValue;
            byte[] imgTargetValue;

            // test için
            //rowCount = 10;

            //if (Cursor.Current == Cursors.Default)
            //    Cursor.Current = Cursors.WaitCursor;
            v.SP_OpenApplication = false;
            t.WaitFormOpen(v.mainForm, "");
            t.WaitFormOpen(v.mainForm, "Resim aktarım işlemi yapılıyor...");

            vTable vt = new vTable();
            preparingTargetVTable(vt, alias, tableName);

            DataSet dsTarget = new DataSet();
            DataNavigator dNTarget = new DataNavigator();
            dNTarget.DataSource = dsTarget;

            for (int i = 0; i < rowCount; i++)
            {
                ///  Örnek sql
                ///  select BiyometrikResim , BiyometrikResimSmall from dbo.MtskAday  Where 0 = 0 and Id = ???  
                ///  update dbo.MtskAday set BiyometrikResim = @BiyometrikResim, BiyometrikResimSmall = @BiyometrikResimSmall Where 0 = 0 and Id = ???  
                preparingImageEditSql(dsSource, i, alias, tableName, editWhereSql, ref selectSql, ref updateSql);

                try
                {
                    executeTargetSql(dsTarget, selectSql, vt);

                    v.SP_OpenApplication = true;
                    t.WaitFormOpen(v.mainForm, "");
                    t.WaitFormOpen(v.mainForm, i.ToString() + "/" + rowCount.ToString() + " resim aktarım işlemi devam ediyor...");

                    if (t.IsNotNull(dsTarget))
                    {
                        v.con_Images = null;
                        v.con_Images2 = null;
                        v.con_Images3 = null;
                        v.con_Images4 = null;

                        for (int i2 = 0; i2 < imgFieldCount; i2++)
                        {
                            imgFieldName = imagefieldsNameList[i2];
                            imgSourceValue = null;
                            imgTargetValue = null;

                            if (dsSource.Tables[0].Rows[i][imgFieldName].ToString() != "")
                                imgSourceValue = (byte[])dsSource.Tables[0].Rows[i][imgFieldName];

                            if (dsTarget.Tables[0].Rows[i][imgFieldName].ToString() != "")
                                imgTargetValue = (byte[])dsTarget.Tables[0].Rows[i][imgFieldName];

                            // Kaynak img var, hedef img yok ise image taşı
                            //if ((imgSourceValue != null) && (imgTargetValue == null))
                            if (imgSourceValue != null)
                            {
                                if (imgFieldName.IndexOf("Small") > -1)
                                {
                                    imgSourceValue = preparingSmallImage(imgSourceValue);
                                }

                                if (i2 == 0)
                                {
                                    v.con_Images = imgSourceValue;
                                    v.con_Images_FieldName = imgFieldName;
                                }
                                if (i2 == 1)
                                {
                                    v.con_Images2 = imgSourceValue;
                                    v.con_Images_FieldName2 = imgFieldName;
                                }
                                if (i2 == 2)
                                {
                                    v.con_Images3 = imgSourceValue;
                                    v.con_Images_FieldName3 = imgFieldName;
                                }
                                if (i2 == 3)
                                {
                                    v.con_Images4 = imgSourceValue;
                                    v.con_Images_FieldName4 = imgFieldName;
                                }
                            }
                        }

                        if (((v.con_Images != null) ||
                            (v.con_Images2 != null) ||
                            (v.con_Images3 != null) ||
                            (v.con_Images4 != null)) &&
                            (updateSql != ""))
                            onay = sv.Record_SQL_RUN(dsTarget, vt, "dsEdit", dNTarget.Position, ref updateSql, "");
                        
                    }
                }
                catch (Exception ex)
                {
                    //if (Cursor.Current == Cursors.WaitCursor)
                    //    Cursor.Current = Cursors.Default;
                    onay = false;
                    MessageBox.Show(ex.Message.ToString());
                    //throw;
                }
            }

            //if (Cursor.Current == Cursors.WaitCursor)
            //    Cursor.Current = Cursors.Default;

            v.IsWaitOpen = false;
            t.WaitFormClose();

            return onay;
        }

        private byte[] preparingSmallImage(byte[] tImage)
        {
            long oldLength = tImage.Length;
            MemoryStream mStream = new MemoryStream(tImage);
            Bitmap workingImage = new Bitmap(mStream);

            // % 80 oranında küçült
            int newWidth = (int)(workingImage.Width * 0.2);
            int newHeight = (int)(workingImage.Height * 0.2);

            Bitmap _img = t.imageCompress(workingImage, newWidth, newHeight);

            ImageConverter converter = new ImageConverter();
            byte[] newImage = (byte[])converter.ConvertTo(_img, typeof(byte[]));
            long newLength = newImage.Length;
            return newImage;
        }

        private bool preparingSetDbUpdatesIsActive()
        {
            bool onay = false;
            string sql = t.DBUpdatesDataTransferOnSql();
            onay = executeNonTargetSql(sql);
            return onay;

        }


        #endregion Veri Aktarma İşlemleri

        #region sub functions
        private void preparingTablesProceduresFunctionsScripts()
        {
            proceduresTableIPCode = "UST/PMS/MsProjectProcedures.List_L01";
            functionsTableIPCode = "UST/PMS/MsProjectFunctions.List_L01";
            //
            t.Find_DataSet(this, ref dsProcedures, ref dNProcedures, proceduresTableIPCode);
            t.Find_DataSet(this, ref dsFunctions, ref dNFunctions, functionsTableIPCode);
            //
            t.Find_Button_AddClick(this, menuName1, buttonSingFileReadDbWrite, myNavElementClick);
            t.Find_Button_AddClick(this, menuName1, buttonAllFileReadDbWrite, myNavElementClick);
            //
            string TabControlName = "tabControl_SUBVIEW";
            this.tabControl = null;
            this.tabControl = t.Find_Control(this, TabControlName);

            if (this.tabControl == null)
                MessageBox.Show("DİKKAT : tabControl_SUBVIEW isimli control bulunamadı.");
        }
        private void preparingDBCreate()
        {
            if (t.IsNotNull(dsFirm))
            {
                #region create Tables, Proceduru, Function ve Veri aktarımı olan menu
                // preparing Tables, Proceduru, Function write database
                t.Find_Button_AddClick(this, menuName2, buttonSingFileReadDbWrite, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonAllFileReadDbWrite, myNavElementClick);
                // database
                t.Find_Button_AddClick(this, menuName2, buttonDbConnection, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonDbCreate, myNavElementClick);
                // tables
                t.Find_Button_AddClick(this, menuName2, buttonTablesList, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonSingleTableCreate, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonTablesCreate, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonFieldList, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonDataView, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonTriggersCreate, myNavElementClick);
                // procedures
                t.Find_Button_AddClick(this, menuName2, buttonProceduresList, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonSingleProcedureCreate, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonProceduresCreate, myNavElementClick);
                // functions
                t.Find_Button_AddClick(this, menuName2, buttonFunctionsList, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonSingleFunctionCreate, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonFunctionsCreate, myNavElementClick);
                // veri aktarımı
                t.Find_Button_AddClick(this, menuName2, buttonSourceDBConnect, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonSourceTablesList, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonSingleTableTransfer, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonSingleTableTransferContinue, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonSingleTableTransferDateContinue, myNavElementClick);
                t.Find_Button_AddClick(this, menuName2, buttonTablesTransfer, myNavElementClick);
                #endregion create Tables, Proceduru, Function ve Veri aktarımı olan menu

                // Sadece data aktar işlemi olan menü için
                #region Sadece Veri aktarımı olan menu
                // database
                t.Find_Button_AddClick(this, menuName22, buttonDbConnection, myNavElementClick);
                // veri aktarımı
                t.Find_Button_AddClick(this, menuName22, buttonSourceDBConnect, myNavElementClick);
                t.Find_Button_AddClick(this, menuName22, buttonSourceTablesList, myNavElementClick);
                t.Find_Button_AddClick(this, menuName22, buttonSingleTableTransfer, myNavElementClick);
                t.Find_Button_AddClick(this, menuName22, buttonSingleTableTransferContinue, myNavElementClick);
                t.Find_Button_AddClick(this, menuName22, buttonSingleTableTransferDateContinue, myNavElementClick);
                t.Find_Button_AddClick(this, menuName22, buttonTablesTransfer, myNavElementClick);
                #endregion Sadece Veri aktarımı olan menu
            }
        }
        private bool dbConnction(bool test, bool masterConnect)
        {
            bool onay = false;
            
            if (t.IsNotNull(dsFirm))
            {
                string vDatabaseName = "";
                string vDbPass = "";

                if (v.newFirm_DB.MSSQLConn != null)
                    v.newFirm_DB.MSSQLConn.Close();

                v.newFirm_DB.dBaseNo = v.dBaseNo.NewDatabase;
                v.newFirm_DB.dBType = v.dBaseType.MSSQL;
                v.newFirm_DB.serverName = dsFirm.Tables[0].Rows[dNFirm.Position][fServerNameIP].ToString();
                v.newFirm_DB.databaseName = dsFirm.Tables[0].Rows[dNFirm.Position][fDatabaseName].ToString();
                v.newFirm_DB.userName = dsFirm.Tables[0].Rows[dNFirm.Position][fDbLoginName].ToString();
                v.newFirm_DB.firmId = t.myInt32(dsFirm.Tables[0].Rows[dNFirm.Position][fFirmId].ToString());
                vDbPass = dsFirm.Tables[0].Rows[dNFirm.Position][fDbPass].ToString();

                // test veya database create için -master- database ile açılması gerekiyor
                // diğer durumlarda tanımlı olan database ismi atanıyor : 'Mtsk00001234' gibi
                vDatabaseName = v.newFirm_DB.databaseName;
                if (masterConnect)
                    vDatabaseName = "master";

                if (vDbPass != "")
                    v.newFirm_DB.psw = "Password = " + vDbPass + ";"; 
                else v.newFirm_DB.psw = "";

                v.newFirm_DB.connectionText =
                    string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                    v.newFirm_DB.serverName,
                    vDatabaseName,
                    v.newFirm_DB.userName,
                    v.newFirm_DB.psw);

                v.newFirm_DB.MSSQLConn = new SqlConnection(v.newFirm_DB.connectionText);
                //v.newFirm_DB.MSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateProject);

                //t.WaitFormOpen(v.mainForm, "Database bağlantısı test ediliyor...");
                //v.SP_OpenApplication = true;

                //v.newFirm_DB.projectMSSQLConn.Close();

                onay = t.Db_Open(v.newFirm_DB.MSSQLConn);

                //v.IsWaitOpen = false;
                //t.WaitFormClose();

                if (onay && test && masterConnect) 
                    MessageBox.Show(":) Bağlantı başarıyla sağlanmıştır ...");
            }

            return onay;
        }
        private bool sourceDbConnction_(bool test)
        {
            bool onay = false;
            
            if (t.IsNotNull(dsFirm))
            {
                string vDbPass = "";

                if (v.source_DB.MSSQLConn != null)
                    v.source_DB.MSSQLConn.Close();

                v.source_DB.dBaseNo = v.dBaseNo.aktrilacakDb;  //NewDatabase;
                v.source_DB.dBType = v.dBaseType.MSSQL;
                v.source_DB.serverName = dsFirm.Tables[0].Rows[dNFirm.Position][sourceServerNameIP].ToString();
                v.source_DB.databaseName = dsFirm.Tables[0].Rows[dNFirm.Position][sourcefDatabaseName].ToString();
                v.source_DB.userName = dsFirm.Tables[0].Rows[dNFirm.Position][sourceDbLoginName].ToString();
                vDbPass = dsFirm.Tables[0].Rows[dNFirm.Position][sourceDbPass].ToString();

                if (vDbPass != "")
                    v.source_DB.psw = "Password = " + vDbPass + ";";
                else v.source_DB.psw = "";

                v.source_DB.connectionText =
                    string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                    v.source_DB.serverName,
                    v.source_DB.databaseName,
                    v.source_DB.userName,
                    v.source_DB.psw);

                v.source_DB.MSSQLConn = new SqlConnection(v.source_DB.connectionText);
                
                t.WaitFormOpen(v.mainForm, "Kaynak database bağlantısı test ediliyor...");
                v.SP_OpenApplication = true;

                onay = t.Db_Open(v.source_DB.MSSQLConn);

                v.IsWaitOpen = false;
                t.WaitFormClose();

                if ((onay) && (test))
                    MessageBox.Show(":) " + v.source_DB.databaseName + " kaynak database bağlantı başarıyla sağlanmıştır ...");
            }

            return onay;
        }
        private void viewText(string text)
        {
            if (editpanel_Sonuc != null)
            {
                if (((DevExpress.XtraEditors.PanelControl)editpanel_Sonuc).Controls.Count > 0)
                {
                    string old = ((DevExpress.XtraEditors.PanelControl)editpanel_Sonuc).Controls[0].Text;

                    ((DevExpress.XtraEditors.PanelControl)editpanel_Sonuc).Controls[0].Text = text + v.ENTER + old;

                    Application.DoEvents();
                }
            }
        }
        private void viewSqlText(string text)
        {
            if (editpanel_Sql != null)
            {
                if (((DevExpress.XtraEditors.PanelControl)editpanel_Sql).Controls.Count > 0)
                {
                    string old = ((DevExpress.XtraEditors.PanelControl)editpanel_Sql).Controls[0].Text;

                    ((DevExpress.XtraEditors.PanelControl)editpanel_Sql).Controls[0].Text = text + v.ENTER + "/*  * * * * * * * * *  */" + v.ENTER + old;

                    Application.DoEvents();
                }
            }
        }
        #endregion sub database Create functions


    }
}
