using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraScheduler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

//using System.IO;


//using OpenQA.Selenium;

namespace Tkn_Variable
{

    public static class v
    {
        #region dataUpdateTable 
        public enum dataUpdateTable : byte
        {
            GecerlilikTarihi = 1,
            MtskSaatTeorik = 2,
            MtskSaatUygulama = 3,
            MtskUcretSinav = 4,
            MtskUcretTeorik = 5,
            MtskUcretUygulama = 6
        }
        #endregion dataUpdateTable 

        #region Database Yapısı
        public enum dBaseType : byte
        {
            None,
            MSSQL
        }

        public enum dBaseNo : byte
        {
            None = 0,
            Master = 1, 
            Manager = 2,
            UstadCrm = 3, 
            Project = 4,
            Local = 5,
            NewDatabase = 6,
            publishManager = 7,
            aktrilacakDb = 8,
        }

        public class DBTypes
        {
            /// o an hangi db işlem yapılacaksa onu nosunu ver 
            /// gitti fonksiyonda ona göre işlem yapılır
            /// 
            ///
            public DBTypes()
            {
                mainManagerDbUses = false;

                managerDBaseNo = v.dBaseNo.Manager;
                ustadCrmDBaseNo = v.dBaseNo.UstadCrm;

                projectDBaseNo = v.dBaseNo.Project;
                projectDBType = v.dBaseType.MSSQL;
                projectServerName = "";
                projectDBName = "";
                projectUserName = "";
                projectPsw = "";
                projectConnectionText = "";

                localDBaseNo = v.dBaseNo.Local;
                localDBType = v.dBaseType.MSSQL;
                localServerName = "";
                localDBName = "";
                localUserName = "";
                localPsw = "";
                localConnectionText = "";
            }
            /// <summary>
            /// runDBaseNo ile o anda hangi database üzerinde çalışacağı 
            /// hakkında bilgi vermek/almak için kullanılıyor
            /// Örnek : t.tTableFind() 
            /// </summary>
            public v.dBaseNo runDBaseNo { get; set; }
            public bool mainManagerDbUses { get; set; }
            //Tabim.Surucu07 için
            public bool localDbUses { get; set; }


            //--- Manager Database
            public v.dBaseNo managerDBaseNo { get; set; }
            public v.dBaseType managerDBType { get; set; }
            public string managerServerName { get; set; }
            public string managerDBName { get; set; }
            public string managerUserName { get; set; }
            public string managerPsw { get; set; }
            public string managerConnectionText { get; set; }
            public SqlConnection managerMSSQLConn { get; set; }

            //--- Project Database
            public v.dBaseNo projectDBaseNo { get; set; }
            public v.dBaseType projectDBType { get; set; }
            public string projectServerName { get; set; }
            public string projectDBName { get; set; }
            public string projectUserName { get; set; }
            public string projectPsw { get; set; }
            public string projectConnectionText { get; set; }
            public SqlConnection projectMSSQLConn { get; set; }

            //--- UstadCrm Database
            public v.dBaseNo ustadCrmDBaseNo { get; set; }
            public v.dBaseType ustadCrmDBType { get; set; }
            public string ustadCrmServerName { get; set; }
            public string ustadCrmDBName { get; set; }
            public string ustadCrmUserName { get; set; }
            public string ustadCrmPsw { get; set; }
            public string ustadCrmConnectionText { get; set; }
            public SqlConnection ustadCrmMSSQLConn { get; set; }

            //--- Local Database
            public v.dBaseNo localDBaseNo { get; set; }
            public v.dBaseType localDBType { get; set; }
            public string localServerName { get; set; }
            public string localDBName { get; set; }
            public string localUserName { get; set; }
            public string localPsw { get; set; }
            public string localConnectionText { get; set; }
            public SqlConnection localMSSQLConn { get; set; }

            //--- master Database
            public v.dBaseNo masterDBaseNo { get; set; }
            public string masterServerName { get; set; }
            public string masterDBName { get; set; }
            public string masterUserName { get; set; }
            public string masterPsw { get; set; }
            public string masterConnectionText { get; set; }
            public SqlConnection masterMSSQLConn { get; set; }

        }

        public static DBTypes active_DB = new DBTypes();

        public static string destekServiceToolCode = "875421";
        public static string destekTesterServiceToolCode = "784512";

        public class databaseAbout_
        {
            /// o an hangi db işlem yapılacaksa onu nosunu ver 
            /// gitti fonksiyonda ona göre işlem yapılır
            /// 
            ///
            public databaseAbout_()
            {
                mainManagerDbUses = false;
                dBaseNo = v.dBaseNo.Project;
                dBType = 0;
                serverName = "";
                databaseName = "";
                userName = "";
                psw = "";
                connectionText = "";
                firmId = 0;
            }
            /// <summary>
            /// runDBaseNo ile o anda hangi database üzerinde çalışacağı 
            /// hakkında bilgi vermek/almak için kullanılıyor
            /// Örnek : t.tTableFind() 
            /// </summary>
            public v.dBaseNo runDBaseNo { get; set; }
            public bool mainManagerDbUses { get; set; }

            //--- Project Database
            public v.dBaseNo dBaseNo { get; set; }
            public v.dBaseType dBType { get; set; }
            public string serverName { get; set; }
            public string databaseName { get; set; }
            public string userName { get; set; }
            public string psw { get; set; }
            public string connectionText { get; set; }
            public SqlConnection MSSQLConn { get; set; }
            public int firmId { get; set; }
        }

        public static databaseAbout_ newFirm_DB = new databaseAbout_();
        public static databaseAbout_ source_DB = new databaseAbout_();
        public static databaseAbout_ publishManager_DB = new databaseAbout_();

        #endregion Database Yapısı

        #region Ftp
        ///home/webadmin/web/ustadyazilim.com/desktopapp
        ///home/webadmin/web/ustadyazilim.com/desktoptester
        public static string ftpHostIp = @"ftp://ustadyazilim.com";
        public static string ftpUserName = "webadmin_ftp";
        public static string ftpTesterUserName = "webadmin_tester";
        public static string ftpUserPass = "Ustad+784512";
        #endregion Ftp

        // picture nesnesi ile Save() functionu arasında veri taşıyıcı
        //public static WebBrowser webMain = null;
        public static Form mainForm { get; set; }
        public static DevExpress.XtraScheduler.TimeRuler timeRuler = new TimeRuler();

        public static DevExpress.Utils.Behaviors.BehaviorManager behaviorManager1 = new DevExpress.Utils.Behaviors.BehaviorManager();

        public static string spFormName = "ustad";
        public static string registryPath = "Software\\Üstad\\YesiLdefter";
        public static Control formLastActiveControl { get; set; }

        public static string con_Save_dsState { get; set; }
        public static int con_EditSaveCount { get; set; }
        public static bool con_EditSaveControl { get; set; }
        public static bool con_ColumnChangesStop { get; set; }
        public static Int16 con_ColumnChangesCount { get; set; }

        public static byte[] con_Images = null;
        public static string con_Images_FieldName { get; set; }

        public static byte[] con_Images2 = null;
        public static string con_Images_FieldName2 { get; set; }
        public static byte[] con_Images3 = null;
        public static string con_Images_FieldName3 { get; set; }
        public static byte[] con_Images4 = null;
        public static string con_Images_FieldName4 { get; set; }

        public static Image con_Image_Original = null;
        public static string con_Images_Path { get; set; }
        public static string con_Images_MasterPath { get; set; }
        public static Rectangle con_Images_Selection;
        public static bool con_Images_Selecting;
        public static long con_Images_Length = 0;

        public static bool con_ResimEditorRun = false;
        public static bool con_CreatePopupContainer = false;
        public static bool con_NewFilesFound = false;
        public static bool con_SetFocus;
        public static string con_SetFocus_TableIPCode { get; set; }
        public static string con_SetFocus_FieldName { get; set; }
        public static string con_Listele_TableIPCode { get; set; }

        public static string con_Source_FormName { get; set; }
        public static string con_Source_FormCode { get; set; }
        //public static 
        public static string con_Source_FormCodeAndName { get; set; }
        public static string con_Source_ReportTableIPCode { get; set; }
        public static string con_Source_ReportFormCode { get; set; }

        public static Point myGrid_Location { get; set; }

        //public static string con_Images_Name { get; set; }

        //public static DevExpress.Utils.Animation.TransitionManager transitionManager1 = 
        //    new DevExpress.Utils.Animation.TransitionManager();

        public static WebCam_Capture.WebCamCapture webCamCapture1 = null;

        public static int sayac = 0;
        public static bool ControlListView = false;
        public static string ControlList { get; set; }

        // *** DB Variables *** // 
        public static string ENTER = "\r\n";
        public static string ENTER2 = "\r\n\r\n";

        //public static string SP_SERVER_PCNAME { get; set; }    // PC NAME
        public static string db_MASTER_DBNAME = "master";    // { get; set; }
        //public static string db_MANAGER_DBNAME { get; set; } // "ManagerServer"
        public static string db_MAINMANAGER_DBNAME = "SystemMS"; // { get; set; } // "ManagerServer"
        //public static string db_PROJE_DBNAME { get; set; }   // SP_DATABASENAME
        //public static string db_USER_NAME { get; set; }
        //public static string db_PASSWORD { get; set; }

        public static string SP_MENU { get; set; }
        public static string EXE_DRIVE = string.Empty;
        public static string EXE_PATH = string.Empty;
        public static string EXE_TempPath = string.Empty;
        public static string EXE_GIBDownloadPath = string.Empty;
        public static string EXE_FastReportsPath = string.Empty;
        public static string EXE_DevExReportsPath = string.Empty;
        public static string EXE_ScriptsPath = string.Empty;
        public static string EXE_Arguments = string.Empty;

        public static string sp_Sakla = string.Empty;

        public static string SP_TabimParamsKurumTipi = "";
        public static string SP_TabimParamsServerName = string.Empty;

        public static Boolean SP_TabimDbConnection = false;
        public static Boolean SP_TabimIniWrite = false;

        public static Boolean SP_Debug = false;
        public static Boolean SP_UserIN = false;
        public static Boolean SP_UserLOGIN = true;
        public static Boolean SP_CheckedNewApplication = false;
        public static Boolean SP_ApplicationExit = false;

        public static Boolean SP_ConnectBool = false;
        public static Boolean SP_ConnBool_Manager = false;
        public static Boolean SP_ConnBool_Manager_Old = false;
        public static Boolean SP_ConnBool_Project = false;
        public static Boolean SP_ConnBool_Project_Old = false;

        public static Boolean SP_ConnBool_FirmPeriod = false;
        public static Boolean SP_ConnBool_FirmMain = false;

        public static string SP_Conn_Caption { get; set; }
        public static string SP_Conn_Text_Master { get; set; }
        //public static string SP_Conn_Text_Manager { get; set; }
        public static string SP_Conn_Text_MainManager { get; set; }
        //public static string SP_Conn_Text_Project { get; set; }
        public static string SP_NewWorkType = "NEW";
        
        public static Boolean IsWaitOpen = false;
        public static Boolean SP_OpenApplication = false;
        public static Boolean SP_ConnectBool_TR = false;
        public static Boolean SP_ConnectBool_Source = false;
        public static Boolean SP_ConnectBool_Target = false;

        public static string SP_Connect_Source_DBType = "";
        public static string SP_Connect_Target_DBType = "";

        public static string db_MSSQL = "MSSQL";
        public static string db_MySQL = "MySQL";


        /* Transfer işlemleri için */
        public static SqlConnection SP_Conn_MsSQL = new SqlConnection(null);
        public static SqlConnection SP_Conn_MsSQL_TR = new SqlConnection(null);
        //--

        // *** Global DataSet *** //
        public static DataSet ds_Settings = new DataSet();
        public static DataSet ds_TypesList = new DataSet();
        public static DataSet ds_MsTypesList = new DataSet();
        public static DataSet ds_Firm = new DataSet();
        public static DataSet ds_Variables = new DataSet();
        public static DataSet ds_Icons = new DataSet();
        public static DataSet ds_Computer = new DataSet();
        public static DataSet ds_ExeUpdates = new DataSet();
        public static DataSet ds_LookUpTableList = new DataSet();
        public static DataSet ds_DonemTipiList = new DataSet();
        public static DataSet ds_ILList = new DataSet();
        public static DataSet ds_IlceList = new DataSet();
        public static DataTable dt_SearchLookUp = null;
        public static DataSet dsMsReports = null;
        public static DataNavigator dNMsReports = null;

        public static List<string> tableList = new List<string>();
        public static List<string> tableIPCodeTableList = new List<string>();
        public static List<string> tableIPCodeFieldsList = new List<string>();
        public static List<string> tableIPCodeGroupsList = new List<string>();
        public static List<string> msLayoutItemsList = new List<string>();
        public static List<string> msMenuItemsList = new List<string>();
        public static List<string> dataCopyList = new List<string>();
        public static List<string> dataCopyLinesList = new List<string>();
        public static List<List<string>> dynamicTable = new List<List<string>>();


        public static DataSet ds_MsTableFields = new DataSet();
        public static DataSet ds_TableIPCodeTable = new DataSet();
        public static DataSet ds_TableIPCodeFields = new DataSet();
        public static DataSet ds_TableIPCodeGroups = new DataSet();
        public static DataSet ds_MsLayoutItems = new DataSet();
        public static DataSet ds_MsMenuItems = new DataSet();
        public static DataSet ds_DataCopy = new DataSet();
        public static DataSet ds_DataCopyLines = new DataSet();

        /// Hangi SMS bilgisi isteniyorsa seçilir.
        //public static SP_SMS : string
        //{
        public static string SMS_KullaniciAdi { get; set; }
        public static string SMS_Sifre { get; set; }
        public static string SMS_BayiiKodu { get; set; }
        public static string SMS_Origin { get; set; }

        public static bool mailSent = false;
        //}

        #region IP_VIEW_TYPE
        // * 1,  'Data View'
        // * 2,  'Kriter View'
        // * 3,  'Kategori View'
        // * 4,  'HGS View'
        public static byte IPdataType_DataView = 1;
        public static byte IPdataType_Kriterler = 2;
        public static byte IPdataType_Kategori = 3;
        public static byte IPdataType_HGSView = 4;
        #endregion IP_VIEW_TYPE

        #region Layout type

        public static string lyt_Name = "tLayout_";
        public static string lyt_menu = "menu";
        public static string lyt_dockPanel = "dockPanel";
        public static string lyt_tableLayoutPanel = "tableLayoutPanel";
        public static string lyt_splitContainer = "splitContainer";
        public static string lyt_groupControl = "groupControl";
        public static string lyt_panelControl = "panelControl";
        public static string lyt_tabPane = "tabPane";
        public static string lyt_tabNavigationPage = "tabNavigationPage";
        public static string lyt_navigationPane = "navigationPane";
        public static string lyt_navigationPage = "navigationPage";
        public static string lyt_backstageViewControl = "backstageViewControl";
        public static string lyt_backstageViewTabItem = "backstageViewTabItem";
        public static string lyt_backstageViewButtonItem = "backstageViewButtonItem";
        public static string lyt_backstageViewItemSeparator = "backstageViewItemSeparator";
        public static string lyt_childBackstageView = "childBackstageView";
        public static string lyt_tabControl = "tabControl";
        public static string lyt_tabPage = "tabPage";

        public static string lyt_documentManager = "documentManager";
        public static string lyt_dataWizard = "dataWizard";

        public static string lyt_webBrowser = "webBrowser";
        public static string lyt_cefWebBrowser = "cefWebBrowser";
        public static string lyt_headerPanel = "headerPanel";
        public static string lyt_editPanel = "editPanel";
        public static string lyt_labelControl = "labelControl";
        public static string lyt_BarcodeControl1 = "barcodeControl1";
        public static string lyt_ComponentControl = "componentControl";
        public static string lyt_SimpleButton = "simpleButton";
        public static string lyt_DigitalGauge = "digitalGauge";
        
        public static string lyt_documentViewerFast = "documentViewerFast";
        public static string lyt_documentViewerDev = "documentViewerDev";


        //public static string lyt_ = "";
        //public static string lyt_ = "";
        //public static string lyt_ = "";

        #endregion

        // *** Save *** //
        #region Save

        public static byte SP_DataBaseType = 1; /* 1 = MSSQL, 2 = MySQl,  ???   */

        public static byte KAYDET = 0;
        public static byte N_SATIR_KAYDET = 1;
        public static byte SQL_OLUSTUR = 2;
        public static byte SQL_OLUSTUR_IDENTITY_YOK = 3;
        public static byte N_SATIR_SQL_OLUSTUR = 4;

        public enum Save 
        { 
            KAYDET, 
            N_SATIR_KAYDET, 
            SQL_OLUSTUR, 
            SQL_OLUSTUR_IDENTITY_YOK, 
            N_SATIR_SQL_OLUSTUR 
        }

        public static string SP_MSSQL_BEGIN = " begin transaction " + ENTER;
        public static string SP_MSSQL_END = " commit transaction " + ENTER;
        public static string SP_MSSQL_LINE_END = ENTER;

        public static string SP_MySQL_BEGIN = " start transaction; " + ENTER;
        public static string SP_MySQL_END = " commit; " + ENTER;
        public static string SP_MySQL_LINE_END = ";" + ENTER;

        public static string onlyTheseFields = "";

        #endregion Save 

        #region Enum

        public enum BackNext 
        {
            back,
            next
        }

        public enum settings : Int16
        {
            None = 0,
            BaslangictaYapilmasiGerekenlerMenu = 101
        }

        public enum msSectorType : Int16
        {
            None = 0,
            OnMuhasebe = 1,
            MaliMusavir = 2,
            ResmiMuhasebe = 3,
            Bordro = 4,
            UstadCrm = 5,
            UstadMtsk = 201,
            UstadIsmak = 202,
            UstadSrc = 203,
            TabimMtsk = 211,
            TabimIsmak = 212,
            TabimSrc = 213,
        }

        public enum formType
        {
            Dialog,
            Normal,
            Child
        };


        public enum myProperties
        {
            Block,
            Row,
            Column
        };
                
        public enum TableType
        {
            None, Table, View, StoredProcedure,
            Function, Trigger, Select
        }

        public enum ShowParamView : byte
        {
            TapPage = 0, Form = 1
        }

        public static byte ond_ShowParamView = 0;

        public enum Tumu : byte
        {
            None = 0, False = 1, True = 2
        }

        public enum ReportDesignerTool : byte
        {
            FastReport = 1,
            DevExpress = 2 
        }
                
        public enum DataReadType
        {
            None, NotReadData, ReadRefID, DetailTable,
            SubDetailTable, DataCollection, ReadSubView
        }

        public enum tWorkTD
        {
            NewAndRef,
            NewData,
            Refresh_SubDetail,
            Refresh_Data,
            Refresf_DataYilAy,
            Save,
        }

        public enum tWorkWhom
        {
            All,
            Only,
            Childs
        }

        public enum tFirmListType
        {
            OnlySelect,
            AllFirm
        }

        public enum cihazCalismaTipi : byte
        {
            None = 0,
            Kayit = 11,
            Giris = 12,
            Cikis = 13,
            GirisCikis = 14,
            Gorus = 15,
            Sayim = 16
        }

        public enum cihazTalepTipi : Int16
        {
            chConnect = 0,
            chDisconnnect = 1,
            chTest = 2,
            chLogCount = 3,
            chGetTarihSaat = 4,
            chSetTarihSaat = 5,
            chGetAllUserAndFPs = 6,
            chGetAllLogs = 7,
            chSetAllUserAndFPs = 8,
            chReset = 9,

            chSetNewUser = 11,
            chGetNewUserFP = 12,
            chSetNewUserSayim = 13,

            chSetOldUser = 14, // eski userin bio izini set için
            chGetOldUserFP = 15, // eski userin kayıt cihazından bio izini almak
            chSetOldUserSayim = 16, // eski userin yeni bio izini sayim cihazına gönder

            chSetSayim = 21,   // sadece yeni kul sayım cihaz ekle
            chSetTahliye = 22,
            chSetGorev = 23,
            chSetGorus = 24,

            chGetGorev = 33,
            chGetGorus = 34,
            chGetSayim = 35,

            chDelGorev = 43,
            chDelGorus = 44,
            chDelTahliye = 45,
            chDelLogData = 46,
            chDelUserFPLog = 47, // << chDelNewUser

            chIcmal = 50,
            chIcmalNewUser = 51,
            chIcmalOldUser = 52,
            chIcmalTahliye = 53,

            chNull = 100
        }
        public enum tEnabled
        {
            Enable,
            Disable
        }

        public enum fileType : byte
        {
            ActiveExe,
            OrderFile
        }

        #endregion Enum

        // *** Object Variables *** //
        #region OBJECT List

        public static int obj_CommonControl { get; set; }
        public static int obj_SimpleButton { get; set; }
        public static int obj_LabelControl { get; set; }

        public static int obj_BarcodControl { get; set; }
        public static int obj_PictureControl { get; set; }
        public static int obj_Load_RUN { get; set; }

        public static int obj_DataNavigator { get; set; }

        public static Int16 obj_vw_GridView = 1010; //{ get; set; }
        public static Int16 obj_vw_BandedGridView = 1020; //{ get; set; }
        public static Int16 obj_vw_AdvBandedGridView = 1030; //{ get; set; }
        public static Int16 obj_vw_GridLayoutView = 1040; //{ get; set; }
        public static Int16 obj_vw_WinExplorerView = 1050; //{ get; set; }
        public static Int16 obj_vw_TileView = 1060; //{ get; set; }
        public static Int16 obj_vw_CardView = 1070; //{ get; set; }


        public static Int16 obj_vw_VGridSingle = 2010; //{ get; set; }
        public static Int16 obj_vw_VGridMulti = 2012; //{ get; set; }
        public static Int16 obj_vw_TreeListView = 2020; //{ get; set; }
        public static Int16 obj_vw_PivotGrid = 2030; //{ get; set; }

        public static Int16 obj_vw_DataLayoutView = 3010; //{ get; set; }
        public static Int16 obj_vw_CalenderAndScheduler = 3020; //{ get; set; }
        public static Int16 obj_vw_ChartsView = 3030; //{ get; set; }
        public static Int16 obj_vw_WizardControl = 4010; //{ get; set; }

        public static Int16 obj_vw_HtmlEditorsView = 5010;
        public static Int16 obj_vw_HtmlEditorsMultiView = 5020;


        public static Int16 obj_vw_InputBoxControl { get; set; }
        public static byte obj_vw_KriterView { get; set; }
        public static byte obj_vw_CategoryView { get; set; }
        public static byte obj_vw_HVGView { get; set; }

        #endregion

        #region DockType
        public static byte dock_None = 0;
        public static byte dock_Left = 1;
        public static byte dock_Right = 2;
        public static byte dock_Top = 3;
        public static byte dock_Bottom = 4;
        public static byte dock_Fill = 5;
        #endregion


        #region /// *** Global Variables *** //

        public static string con_SQL { get; set; }
        public static string SQL { get; set; }
        public static string SQLSave { get; set; }
        public static string SQLState { get; set; }
        public static System.Windows.Forms.Timer timer_Kullaniciya_Mesaj_Var_ = null;
        public static string Kullaniciya_Mesaj_Var { get; set; }
        public static Boolean Kullaniciya_Mesaj_Show = false;
        public static Boolean SaveOnay { get; set; }

        public static Boolean Takip = false;
        public static Boolean Takip_Find = false;
        public static Int16 Takip_Adim = 0;
        public static string bosluk = string.Empty;
        public static string Takip_Listesi = string.Empty;
        public static string FormLoad = "FormLoad";
        public static string ProjectName { get; set; }

        public static string Wait_Caption = " Lütfen bekleyin...";
        public static string Wait_Desc_ProgramYukleniyor = "Program yükleniyor...";
        public static string Wait_Desc_ProgramYukDevam = "Program yüklenmeye devam ediyor...";
        public static string Wait_Desc_DBBaglanti = " veri tabanına bağlantı kuruluyor...";
        public static string DBRec_Insert = " Yeni kayıt işlemi gerçekleşti... ";
        public static string DBRec_Update = " adet kayıt düzeltme işlemi gerçekleşti... ";
        public static string DBRec_ListAdd = " Başarıyla listeye eklenmiştir...";
        public static string dataStateNull = "DataState:null";
        public static string dataStateUpdate = "DataState:Update";
        public static string masterDetailColumns = "MasterDetailColumns:null";

        public static string softCode { get; set; }
        public static string projectCode { get; set; }

        public static int Screen_Width { get; set; }
        public static int Screen_Height { get; set; }
        public static int Primary_Screen_Width { get; set; }
        public static int Primary_Screen_Height { get; set; }
        public static int Secondery_Screen_Width { get; set; }
        public static int Secondery_Screen_Height { get; set; }

        public static int NavBar_Width { get; set; }
        public static int Ribbon_Height { get; set; }
        public static int Padding4 = 4;
        public static int Padding8 = 8;
        public static int GroupPadding = 4;

        public static int dropTargetRowHandle = -1;
        public static int DropTargetRowHandle
        {
            get { return dropTargetRowHandle; }
            set
            {
                dropTargetRowHandle = value;
                //gridControl1.Invalidate();
            }
        }


        public static Boolean myFormBox_Visible = false;
        //public static Point myGrid_Location { get; set; } şimdilik gerek kalmadı
        //public static Boolean con_SubDetail_Refresh = true;
        public static bool con_FieldLockControl = true;
        public static Boolean con_Refresh = false;
        public static Boolean con_SubWork_Refresh = true;
        public static Boolean con_SubWork_Run = false;
        public static Boolean con_Cancel = false;
        public static Boolean con_AddIP = false;
        public static Boolean con_Parametre = true;
        public static Boolean con_AutoNewRecords = false;
        public static Boolean con_NewRecords = false;
        public static Boolean con_OnayChange = false;
        public static Boolean con_OnaySave = false;
        public static Boolean con_LkpOnayChange = false;
        public static Boolean con_ColumnValueChanging = false;
        public static Boolean con_PositionChange = false;
        public static Boolean con_ExtraChange = false;
        public static Boolean con_Expression = false;
        public static Boolean con_FormAfterCreateView = false;
        public static Boolean con_FormOpen = false;
        public static Boolean con_DefaultValuePreparing = false;
        public static Boolean con_CreateScriptPacket = false;
                
        public static DataRow con_SelectedSearchDataRow = null;

        public static Int16 con_FinansHesapTuru = 0;
        public static int con_FinansHesapID = 0;

        public static int con_SubView_FIRST_POSITION = 0;

        public static int con_Value_Max = 0;
        public static int con_Value_Min = 0;
        public static string con_Value_New = string.Empty;
        public static string con_Value_Old = string.Empty;
        public static string con_Expression_Send_Value = string.Empty;
        public static string con_Expression_View = string.Empty;
        public static string con_Expression_FieldName = string.Empty;
        public static string con_InputBox_FieldName = string.Empty;
        public static string con_AddIP_TableCode = string.Empty;
        public static string con_AddIP_FieldName = string.Empty;
        public static string con_AddIP_Value = string.Empty;
        public static string con_AddListIP_TableIPCode = string.Empty;

        public static string con_Source_ParentControl_Tag_Value = string.Empty;
        public static string con_Menu_Prop_Value = string.Empty;

        public static string con_GotoRecord = string.Empty;
        public static string con_GotoRecord_TableIPCode = string.Empty;
        public static string con_GotoRecord_FName = string.Empty;
        public static string con_GotoRecord_Value = string.Empty;
        public static int con_GotoRecord_Position = -1;

        public static Form con_DragDropForm { get; set; }
        public static string con_DragDropSourceTableIPCode = string.Empty; // veya = FIRST
        public static string con_DragDropTargetTableIPCode = string.Empty; // veya = SECOND 
        public static string con_DragDropOpenFormInTableIPCode = string.Empty; // ve = THIRD 

        public static string con_TableIPCode = string.Empty;
        public static string con_FormLoadValue = string.Empty;
        public static string con_GridMouseValue = string.Empty;
        public static Boolean con_DragDropEdit = false;

        public static string con_ImagesSourceFormName = string.Empty;
        public static string con_ImagesSourceTableIPCode = string.Empty;
        public static string con_ImagesSourceFieldName = string.Empty;
        public static string con_ImagesMasterTableIPCode = string.Empty;
        public static DataSet con_ImagesMasterDataSet = null;

        public static string con_ManuelFormLoadValue = string.Empty;
        public static string myReportViewFormLoadValue = string.Empty;
        public static string mySMSViewFormLoadValue = string.Empty;
        public static string mySMSSetupFormLoadValue = string.Empty;

        public static string Properties = "Properties";
        public static string PropertiesPlus = "PropertiesPlus";
        public static string PropertiesNav = "PropertiesNav";
        public static string ButtonEdit = "Buttons";



        public static Color ColorValidation = Color.LightGoldenrodYellow; 
        public static Color Validate_Ok = Color.GreenYellow;
        public static Color Validate_Not = Color.MistyRose;
        public static Color AppearanceFocusedColor = Color.MediumSpringGreen;
        public static Color ribbonColor = AppearanceFocusedColor;

        //Color.LightSeaGreen; //Color.GreenYellow;  //Color.PaleGreen;

        public static Color colorAutoSave = System.Drawing.Color.Orange;
        public static Color colorFocus = System.Drawing.Color.LightGreen; //AppearanceFocusedColor;// 
        public static Color colorSave = System.Drawing.Color.LimeGreen; //AppearanceFocusedColor;//
        public static Color colorNew = System.Drawing.Color.Turquoise; //AppearanceFocusedColor;
        public static Color colorNavigator = System.Drawing.Color.PaleTurquoise; 
        public static Color colorDelete = System.Drawing.Color.LightPink;
        public static Color colorOrder = System.Drawing.Color.SkyBlue; //CornflowerBlue;//  LemonChiffon;
        public static Color colorExit = System.Drawing.Color.Gold; //color3

        public static Color AppearanceTextColor = Color.DarkGreen;  //MediumSeaGreen;
        public static Color AppearanceFocusedTextColor = Color.Black;
        public static Color AppearanceItemCaptionColor1 = Color.Red;//   Color.CornflowerBlue; // System.Drawing.Color.Cornsilk;
        public static Color AppearanceItemCaptionColor2 = Color.Coral; // System.Drawing.Color.FloralWhite;


        // *** Sorgu Türleri *** //
        public static byte Bos = 0;
        public static byte EsitVeBuyuk = 1;
        public static byte Buyuk = 2;
        public static byte Esit = 3;
        public static byte EsitVeKucuk = 4;
        public static byte Kucuk = 5;
        public static byte Benzerleri_Tam = 6;
        public static byte Benzerleri_Sol = 7;
        public static byte EsitDegil = 8;




        // *** Date & Time Variables *** //
        public static DateTime DONEM_BASI_TARIH = DateTime.Now.AddDays(-1 * DateTime.Now.Day);
        public static DateTime DONEM_BITIS_TARIH;

        public static DateTime BUGUN_TARIH;// = DateTime.Now;
        public static DateTime BU_HAFTA_BASI_TARIH;
        public static DateTime BU_HAFTA_SONU_TARIH;
        public static DateTime BU_AY_BASI_TARIH;
        public static DateTime BU_AY_SONU_TARIH;
        public static DateTime BIR_HAFTA_ONCEKI_TARIH = DateTime.Now.AddDays(-7);
        public static DateTime IKI_HAFTA_ONCEKI_TARIH = DateTime.Now.AddDays(-14);
        public static DateTime ON_GUN_ONCEKI_TARIH = DateTime.Now.AddDays(-10);
        public static DateTime GECEN_AY_BUGUN = DateTime.Now.AddMonths(-1);
        public static DateTime GELECEK_AY_BUGUN = DateTime.Now.AddMonths(1);

        public static int BUGUN_GUN = 0;
        public static int BUGUN_AY = 0;
        public static int BUGUN_YIL = 0;
        public static int BUGUN_YILAY = 0;
        public static int GECEN_YILAY = 0;
        public static int GELECEK_YILAY = 0;
        public static int DONEMTIPI_YILAY = 0;
        public static int DONEMTIPI_YILAY_OLD = 0;

        public static string Declare_bugun = string.Empty;
        public static string Declare_Gun = string.Empty;
        public static string Declare_Ay = string.Empty;
        public static string Declare_Yil = string.Empty;

        public static string TARIH_SAAT = DateTime.Now.ToString();
        public static string SAAT_KISA = DateTime.Now.ToShortTimeString();
        public static string SAAT_UZUN = DateTime.Now.ToLongTimeString();
        public static string SAAT_UZUN2 = DateTime.Now.ToLongTimeString();

        // *** Proje Tanımları için *** //

        // *** Çalışılan Firma Tablosu *** //
        public static string ds_TypesNames = "";
        public static string ds_MsTypesNames = "";
        public static string ds_LookUpTableNames = "";

        public static string sp_OpenFormState = "";
        public static string sp_activeSkinName = "";
        public static string sp_SelectSkinName = ""; 
        public static string sp_deactiveSkinName = "Blueprint";//"High Contrast White";
        //public static SkinSvgPalette sp_DeactiveSkinPalette = SkinSvgPalette.Bezier.HighContrastWhite;
        public static SkinStyle sp_DeactiveSkin = SkinStyle.Blueprint;
        public static SkinStyle sp_LocalDeactiveSkin = SkinStyle.GlassOceans;

        //"iMaginary";//  SkinStyle.iMaginary;

        // firma bilgileri
        // 
        public static int SP_FIRM_ID = 0;
        public static Int16 SP_Firm_SectorTypeId = 0;
        // false olursa kullanıcı tek firmaya kullanabilir
        // true  olursa kullanıcı aynı anda birden fazla firma kullanabilir
        //
        public static bool SP_FIRM_MULTI = false;
        public static string SP_FIRM_USERLIST { get; set; }
        public static string SP_FIRM_FULLLIST { get; set; }
        
        public static string SP_FIRM_TABLENAME { get; set; }
        public static string SP_FIRM_TABLEKEYFNAME { get; set; }
        public static string SP_FIRM_TABLECAPTIONFNAME { get; set; }
        public static string SP_FIRM_REF_FNAME = "LOCAL_ID";
        public static string SP_FIRM_USE_MENU_TYPE = "toolBox";

        public static tUserType SP_tUserType = tUserType.EndUser;

        //public static int vt_FIRM_ID = 1;    // 2001   geçiçi değiştirildi, aslı 0
        //public static int vt_SHOP_ID = 2;    // 2002   geçiçi değiştirildi, aslı 0
        //public static int vt_COMP_ID = 6;    // 2003   geçiçi değiştirildi, aslı 0
        public static int vt_PERIOD_ID = 0;  // 2004   donem_ID 

        //public static int vt_USER_ID = 0;         // 2005  
        //public static string vt_USER_CODE = "";   // 2006
        //public static string vt_USER_NAME = "";   // 2007
        public static string vt_USER_SHOP_LIST = "";

        
        #endregion

        #region Keys
        
        public static Keys Key_Exit = Keys.Escape;
        public static Keys Key_YeniSatir = Keys.Insert;

        public static Keys Key_SearchEngine = Keys.F1;
        
        public static Keys Key_Yeni = Keys.F4;
        
        public static Keys Key_Kaydet = Keys.F3;
        public static Keys Key_KaydetYeni = Keys.F5;
        
        public static Keys Key_NewSub = Keys.F6;
        
        public static Keys Key_ListeHazirla = Keys.F7; 
        public static Keys Key_ListyeEkle = Keys.F7;   
        public static Keys Key_SecCik = Keys.Enter;    
        public static Keys Key_Listele = Keys.F8;      

        public static Keys Key_Goster = Keys.F9;
        public static Keys Key_BelgeyiAc = Keys.F9;
        public static Keys Key_HesapKartiniAc = Keys.F9;
        public static Keys Key_ResimEditor = Keys.F11;

        public static Keys Key_Next = Keys.Next;
        public static Keys Key_Prior = Keys.Prior;
        public static Keys Key_ExtraIslem = Keys.F16;
        
        #endregion

        #region Navigator

        /*
        //10 
        //'12 Ara'); 
        public static byte nv_11_Cikis = 11;
        public static byte nv_13_Sec = 13;
        //20
        public static byte nv_21_Vazgec = 21;
        public static byte nv_22_Kaydet = 22;//24;
        public static byte nv_23_Kaydet_Yeni = 23;
        public static byte nv_24_Kaydet_Cik = 24; //22;
        public static byte nv_26_Sil_Satir = 26;
        public static byte nv_27_Sil_Fis = 27;
        public static byte nv_28_Iptal_Navg_Ref = 28;
        public static byte nv_29_Iptal_Form_Kpt = 29;
        //30
        public static byte nv_31_Yeni_Form_Ici = 31;
        public static byte nv_32_Yeni_Form_Ile = 32;
        public static byte nv_33_Yeni_Fis_Form_Ici = 33;
        public static byte nv_34_Yeni_Fis_Form_Ile = 34;
        public static byte nv_35_Fis_Listesi = 35;
        //40
        public static byte nv_41_En_Sona = 41;
        public static byte nv_42_Sonraki_Syf = 42;
        public static byte nv_43_Sonraki = 43;
        public static byte nv_44_Kayit_Adedi = 44;
        public static byte nv_45_Onceki = 45;
        public static byte nv_46_Onceki_Syf = 46;
        public static byte nv_47_En_Basa = 47;
        //50
        public static byte nv_51_Karti_Ac = 51;
        public static byte nv_52_Fisi_Ac = 52;
        public static byte nv_53_Form_Ac = 53;
        public static byte nv_54_IP_Ac = 54;
        public static byte nv_55_IB_Ac = 55;
        public static byte nv_56_CP_Ac = 56;
        //101 Procedure RUN');
        public static byte nv_101_Pro_RUN = 101;
        public static byte nv_102_AUTO_INS = 102;
        */

        /* tNavigatorButton
        public enum tNavigatorButton : byte
        {
            nv_11_Cikis = 11,
            nv_12_Listele = 12,
            nv_13_Sec = 13,
            nv_14_Ekle = 14,
            nv_15_Liste_Hazirla = 15,
            nv_21_Vazgec = 21,
            nv_22_Kaydet_Cik = 22,
            nv_23_Kaydet_Yeni = 23,
            nv_24_Kaydet = 24,
            nv_26_Sil_Satir = 26,
            nv_27_Sil_Fis = 27,
            nv_28_Iptal_Navg_Ref = 28,
            nv_29_Iptal_Form_Kpt = 29,
            nv_41_En_Sona = 41,
            nv_42_Sonraki_Syf = 42,
            nv_43_Sonraki = 43,
            nv_44_Kayit_Adedi = 44,
            nv_45_Onceki = 45,
            nv_46_Onceki_Syf = 46,
            nv_47_En_Basa = 47,
            nv_50_Hesap_Ac = 50,
            nv_51_Karti_Ac = 51,
            nv_52_Yeni_Kart_FormIle = 52,
            nv_53_Yeni_Hesap = 53,
            nv_53_Yeni_Hesap_Vazgec = 153,
            nv_54_Yeni_Alt_Hesap = 54,
            nv_55_Run_Expression = 55,
            nv_56_Run_TableIPCode = 56,
            nv_57_Input_Box = 57,
            nv_58_Search = 58,
            nv_59_SubView_Open = 59,
            nv_71_Onay_EkleKaldir = 71,
            nv_72_Grup_AcKapa = 75,
            nv_81_Yazici = 81,

            nv_101_Pro_RUN = 101,
            nv_102_AutoInsert = 102
        }
        */
        
        public enum tButtonType : byte
        {
            btNone = 0,
            btEnter = 1,
            btEscape = 2,
            btCikis = 11,
            
            btSihirbazDevam = 68,
            btSihirbazGeri = 69,
            
            btSecCik = 12,
            btListeyeEkle = 13,
            btListeHazirla = 14,
            btListele = 15,

            btKaydetYeni = 21,
            btKaydet = 22,
            btKaydetDevam = 23,
            btKaydetCik = 24,

            btYeniKart = 31,
            btYeniHesap = 32,
            btYeniBelge = 33,
            btYeniAltHesap = 34,
            
            btYeniSatir = 35,

            btYeniKartSatir = 36,
            btYeniHesapSatir = 37,
            btYeniBelgeSatir = 38,
            btYeniAltHesapSatir = 39,

            btGoster = 41,
            btKartAc = 42,
            btHesapAc = 43,
            btBelgeAc = 44,
            btResimEditor = 45,
            btReportDesign = 46,

            btSilSatir = 51,
            btSilKart = 52,
            btSilHesap = 53,
            btSilBelge = 54,
            btSilListe = 55,

            btEnSona = 61,
            btSonrakiSayfa = 62,
            btSonraki = 63,
            btOnceki = 65,
            btOncekiSayfa = 66,
            btEnBasa = 67,


            btCollapse = 71,
            btExpanded = 72,
            btOnayEkle = 73,
            btOnayKaldir = 74,
            btMesajGonder = 75,

            btYazici = 81,
            btRapor = 82,
            btEk1 = 91,
            btEk2 = 92,
            btEk3 = 93,
            btEk4 = 94,
            btEk5 = 95,
            btEk6 = 96,
            btEk7 = 97,
            btRunTime = 101,
            btAutoInsert = 102,

            btNoneButton = 120,
            btArama = 121,
            btFormulleriHesapla = 122,
            btDataTransferi = 123,
            btInputBox = 124,
            btOpenSubView = 125,
            btExtraIslem = 126,
            btFindListData = 127
        }

        #endregion

        public enum tRDesignerType
        {
            none,
            FastReport,
            DevExpress
        }

        public enum tBrowserType
        {
            none,
            CefSharp,
            Selenium
        }
        public enum tUserType
        {
            EndUser,
            TesterUser
        }
        public enum tBeforeAfter
        {
            Before,
            After
        }
        public enum tFocus
        {
            True,
            False
        }
        public enum tRowCount
        {
            SingleRow,
            MultiRows
        }
        public enum tFindColumnType
        {
            searchColumn,
            firstActiveColumn //gerek kalmadı
        }
                
        public static cComputer tComputer = new cComputer();
        public static List<tUstadFirm> tFirmUserList = new List<tUstadFirm>();
        public static List<tUstadFirm> tFirmFullList = new List<tUstadFirm>();

        // Bilgisayar hakkında
        public static Computer tComp = new Computer();
        // User-Kullanıcı hakkında
        public static tUstadUser tUser = new tUstadUser();
        // user ın çalıştığı firma 
        public static tUstadFirm tMainFirm = new tUstadFirm();
        // user ın inceleme veya müdehale için geçici bağlandığı firma
        public static tUstadFirm tExternalFirm = new tUstadFirm();
        // user ın registere kaydedilen bilgileri
        public static tUserRegister tUserRegister = new tUserRegister();
        // TabimMtsk kurs hakkindaki bilggiler
        public static tTabimFirm tTabimFirm = new tTabimFirm();

        // Exe hakkında
        public static exeAbout tExeAbout = new exeAbout();
        // button hakkında bilgi
        public static vButtonHint tButtonHint = new vButtonHint();
        // resim editorunu açarken gerekli olan bilgiler
        public static vResimEditor tResimEditor = new vResimEditor();
        // search hakkında
        public static searchForm tSearch = new searchForm();

        public static vMsFileUpdate tMsFileUpdate = new vMsFileUpdate();
        public static vMsDbUpdate tMsDbUpdate = new vMsDbUpdate();

        public static vTableKeyFieldValue tTableKeyFieldValue = new vTableKeyFieldValue();

        public static bildirimPaketi tBildirim = new bildirimPaketi();
        public static vSMSSettings tSMSSettings = new vSMSSettings();

        public static vInvoiceUser tInvoiceUser = new vInvoiceUser();

        public static bool cefBrowserLoading = false;
        //public static CefSharp.WinForms.ChromiumWebBrowser cefBrowser_ = null;
        //public static CefSharp.WinForms.Host.ChromiumHostControl cefHostControl_ = null;

        public static OpenQA.Selenium.IWebDriver webDriver_ = null;
        public static string tWebLoginPageCode = "MEBBISLOGIN"; // bunu öndeğerlere bağlaman gerekiyor
        public static string tWebLoginPageUrl = "https://mebbis.meb.gov.tr/default.aspx?lg1"; // bunu öndeğerlere bağlaman gerekiyor
        public enum tWebEventsType
        {
            none = 0,
            load = 1, 
            displayNone = 2,
            buttonAlwaysSet = 140,
            button3 = 143, //b1 : 141,
            button4 = 144, //b2 : 142,
            button5 = 145, //b3 : 143,
            button6 = 146, //b4 : 144,
            button7 = 147, //b5 : 145,
            tableField = 148, //146,
            pageRefresh = 200
        }

        public enum tWebResponseType
        {
            none,
            html,
            image,
            xml,
            json
        };

        public enum tWebInvokeMember
        {
            none,
            click,
            onchange,
            onchangeDontDocComplate,
            submit,
            autoSubmit,
            clickAndNewPage
        };

        public enum tWebInjectType
        {
            none,
            AlwaysSet,
            Get,
            Set,
            GetAndSet
        };
        public enum tWebRequestType
        {
            none,
            get,
            post,
            put,
            delete,
            getNodeItems,
            postNodeItems,
            alwaysSet
        };

        public enum tSelect
        {
            Get,
            Set
        }

        public enum tFileName  
        {
            none,
            dbKayit,
            tarayici,
            setImageDPI,
            setImageQuality,
            transferFromDatabaseToWeb
        }

        public enum tCharacterType
        {
            OnlyDigit,
            OnlyLetter,
            Mixed,
            Empty
        }


        public static bool searchOnay = false;
        public static bool searchSet = false;
        public static bool searchEnter = true;
        public static int searchCount = 0;
        //public static int searchStartCount = 1; /* 1 ile 5 arası kullanıcı ayarlasın */
        public static string search_onList = "onList";
        public static string search_inData = "inData";
        public static string search_CARI_ARAMA_TD = search_inData; //search_onList;//
        public static string search_STOK_ARAMA_TD = "";
        public static string con_Search_NullText { get; set; } //+
        public static string con_SearchValue = string.Empty; //+
        public static string con_SearchTableIPCode = string.Empty;
        public static string search_readTableIPCode = "";


    }



    public class searchForm
    {
        public searchForm()
        {
            Clear();
        }
        public bool IsRun { get; set; } // çalışıyor / çalışmıyor
        public bool AutoSearch { get; set; }
        public bool IsSearchFound { get; set; }

        public string SearchTableIPCode { get; set; }

        public string searchEngine = "SearchEngine";
        public string searchNullText { get; set; }
        public string searchValue { get; set; }
        public string searchInputValue { get; set; }
        public string searchOutputValue { get; set; }
        public int searchStartCount { get; set; }
        public object messageObj { get; set; }

        public void Clear()
        {
            IsRun = false;
            AutoSearch = true;
            IsSearchFound = false;
            SearchTableIPCode = "";
            searchNullText = "";
            searchValue = "";
            searchInputValue = "";
            searchOutputValue = "";
            searchStartCount = 1;
        }
    }

    public class webWorkPageNodes
    {
        public webWorkPageNodes()
        {
            Clear();
        }

        public string aktifPageUrl { get; set; } // o anda hangi page Url için çalışıyor
        public string aktifPageCode { get; set; } // o anda hangi pageCode için çalışıyor
        public string nodeIdList { get; set; } // myTriggerList nin işini yapıyor
        public string tableIPCode { get; set; }
        public bool siraliIslemVar { get; set; }
        public bool siraliIslemAktif { get; set; }
        public string siraliIslemTableIPCode { get; set; }
        public DataSet siraliIslem_ds = null;
        public DataNavigator siraliIslem_dN = null;
        public DataSet aktif_ds = null;
        public DataNavigator aktif_dN = null;
        public Control siraliIslem_Btn = null;
        public void Clear()
        {
            aktifPageCode = "";
            nodeIdList = "";
            tableIPCode = "";
            siraliIslemVar = false;
            siraliIslemAktif = false;
            siraliIslemTableIPCode = "";
            siraliIslem_Btn = null;
            siraliIslem_ds = null;
            siraliIslem_dN = null;
            aktif_ds = null;
            aktif_dN = null;
        }

    }
    public class webForm
    {
        public webForm()
        {
            Clear();
        }
        public v.tBrowserType browserType { get; set; }
        public OpenQA.Selenium.IWebDriver wbSel { get; set; }
        //public CefSharp.WinForms.ChromiumWebBrowser wbCef { get; set; }
        public string aktifPageCode { get; set; }
        public string talepEdilenUrl { get; set; }
        public string talepEdilenUrl2 { get; set; }
        public string talepOncesiUrl { get; set; }
        public string errorPageUrl { get; set; }
        public string loginPageUrl { get; set; }
        public bool isLoginPage { get; set; }
        public bool loginPageRun { get; set; }
        public string tableIPCodesInLoad { get; set; }
        public string tableIPCodeIsSave { get; set; }
        public string aktifUrl { get; set; }
        public string sessionIdAndToken { get; set; }
        public string securityCode { get; set; }
        public bool pageRefreshWorking { get; set; }
        public bool loadWorking { get; set; }
        public bool anErrorOccurred { get; set; }
        public bool autoSubmit { get; set; }
        public string seleniumMainPage { get; set; } // sitenin açıldığı ana sayfa
        public string seleniumNewSubPage { get; set; }  // ana sayfadadn sonra başka sekme açılınca o yeni açılan sayfa : Örn E-Sınav sayfasında rapor al ile açılan sayfa
        public string seleniumActivePage { get; set; }  // wb diye çalışan driver da şu anda aktif olan sayfayı işaret ediyor

        public Int16 talepPageLeft { get; set; }
        public Int16 talepPageTop { get; set; }
        public Int16 analysisNodeId { get; set; }
        public Int16 analysisParentId { get; set; }


        public Control btn_PageView = null;
        public Control btn_Analysis = null;
        public Control btn_AnalysisNodeView = null;
        public Control btn_AlwaysSet = null;
        public Control btn_FullGet1 = null;  // birinci get butonu
        public Control btn_FullGet2 = null;  // ikinci  get butonu
        public Control btn_FullPost1 = null; // birinci post butonu
        public Control btn_FullPost2 = null; // ikinci  post butonu
        public Control btn_FullSave = null;  // save    post butonu
        public Control btn_AutoSubmit = null;// auto    kaydet butonu 

        public Form tForm = null;
        public void Clear()
        {
            browserType = v.tBrowserType.none;
            aktifPageCode = "";
            talepEdilenUrl = "";
            talepEdilenUrl2 = "";
            talepOncesiUrl = "";
            errorPageUrl = "";
            //loginPageUrl = "";  açma
            //loginPageRun = false;  açma
            tableIPCodesInLoad = "";
            tableIPCodeIsSave = "";
            aktifUrl = "";
            sessionIdAndToken = "";
            securityCode = "";
            //seleniumMainPage = ""; açma
            //seleniumNewSubPage = "";
            //seleniumActivePage = ""; 
            pageRefreshWorking = false;
            loadWorking = false;
            anErrorOccurred = false;
            //autoSubmit = false;
            talepPageLeft = 0;
            talepPageTop = 0;
            analysisNodeId = 1;
            analysisParentId = 0;
        }
    }
    public class saveVariables
    {
        public saveVariables()
        {
            Clear();
        }
        public bool isFieldFind { get; set; }
        public bool identityInsertOnOff { get; set; }
        public bool fIdentity { get; set; }
        public bool IsChanges { get; set; }

        public string State { get; set; }
        public string SchemasCode { get; set; } //vt.SchemasCode;
        public string tableName { get; set; } //vt.TableName;
        public string IPCode { get; set; }
        public string Key_Id_FieldName { get; set; } //= vt.KeyId_FName;
        public string sonuc { get; set; }
        public string MyInsert { get; set; }
        public string MyEdit { get; set; }
        public string MyStr2 { get; set; }
        public string MyStr3 { get; set; }
        public string MyField { get; set; }
        public string MyValue { get; set; }
        public string MyIfW { get; set; }
        public string fname { get; set; }
        public string onceki_fname { get; set; }
        public string Lkp_fname { get; set; }
        public string fvalue { get; set; }
        public string bos { get; set; }
        public string ValidationInsert { get; set; }
        public string fForeing { get; set; }
        public string fTrigger { get; set; }
        public string fTriggerFields { get; set; }
        public string displayFormat { get; set; }
        public string fVisible { get; set; }
        public string fEnabled { get; set; }
        public string fieldNewValue { get; set; }
        public string myProp { get; set; }
        public string SqlF { get; set; }
        public string TableIPCode { get; set; }
        public string line_end { get; set; }

        // new variables

        public string _setInsField { get; set; }
        public string _setInsValue { get; set; }
        public string _setEditField { get; set; }
        public string _setSelectControl { get; set; }
        public string _insFields { get; set; }
        public string _insValues { get; set; }
        public string _editFields { get; set; }
        public string _editWhere { get; set; }
        public string _selectControls { get; set; }


        public byte TableType { get; set; }
        public int DataReadType { get; set; }
        public int ftype { get; set; }
        public int fmax_length { get; set; }
        public int count { get; set; }
        public int position { get; set; }
        public void Clear()
        {
            isFieldFind = false;
            identityInsertOnOff = false;
            fIdentity = false;
            IsChanges = false;

            State = "";
            SchemasCode = "";// vt.SchemasCode;
            tableName = "";// vt.TableName;
            IPCode = ""; // vt.IPCode;
            Key_Id_FieldName = "";// = vt.KeyId_FName;
            sonuc = "";
            MyInsert = "";
            MyEdit = "";
            MyStr2 = "";
            MyStr3 = "";
            MyField = "";
            MyValue = "";
            MyIfW = string.Empty;
            fname = "";
            onceki_fname = "";
            Lkp_fname = "";
            fvalue = "";
            bos = "   ";
            ValidationInsert = "False";
            fForeing = string.Empty;
            fTrigger = string.Empty;
            fTriggerFields = string.Empty;
            displayFormat = string.Empty;
            fVisible = "False";
            fEnabled = "False";
            fieldNewValue = "";
            myProp = "";
            SqlF = "";
            TableIPCode = "";
            line_end = "";

            _setInsField = "";
            _setInsValue = "";
            _setEditField = "";
            _setSelectControl = "";

            _insFields = "";
            _insValues = "";
            _editFields = "";
            _editWhere = "";
            _selectControls = "";

            TableType = 0;
            DataReadType = 0;
            ftype = 0;
            fmax_length = 0;
            count = 0;
            position = 0;
        }
    }
    public class vMsFileUpdate
    {
        public vMsFileUpdate()
        {
            Clear();
        }
        public int id { get; set; }
        public string fileName { get; set; }
        public string extension { get; set; }
        public string versionNo { get; set; }
        public string packetName { get; set; }
        public string pathName { get; set; }
        public string about { get; set; }

        public void Clear()
        {
            id = 0;
            fileName = "";
            extension = "";
            versionNo = "";
            packetName = "";
            pathName = "";
            about = "";
        }
    }
    public class vMsDbUpdate
    {
        public vMsDbUpdate()
        {
            Clear();
        }

        public int id { get; set; }
        public Int16 sectorTypeId { get; set; }
        public Int16 updateTypeId { get; set; }
        public Int16 dBaseNoTypeId { get; set; }
        public string schemaName { get; set; }
        public string tableName { get; set; }
        public string fieldName { get; set; }
        public Int16 fieldTypeId { get; set; }
        public string fieldLength { get; set; }
        public bool fieldNotNull { get; set; }
        public string about { get; set; }
        public string sqlScript { get; set; }

        public void Clear()
        {
            id = 0;
            sectorTypeId = 0;
            updateTypeId = 0;
            dBaseNoTypeId = 0;
            schemaName = "";
            tableName = "";
            fieldName = "";
            fieldTypeId = 0;
            fieldLength = "";
            fieldNotNull = false;
            about = "";
            sqlScript = "";
        }
    }

    public class vTableKeyFieldValue
    {
        public vTableKeyFieldValue()
        {
            Clear();
        }
        public DataSet ds { get; set; }
        public DataNavigator dN { get; set; }
        public string keyFieldName { get; set; }
        public int beforeKeyValue { get; set; }
        public int afterKeyValue { get; set; }

        public void Clear()
        {
            ds = null;
            dN = null;
            keyFieldName = "";
            beforeKeyValue = 0;
            afterKeyValue = 0;
        }
    }

    // computer bilgileri 
    public class Computer
    {
        public int SP_COMP_ID = 0;
        public bool SP_COMP_ISACTIVE = false;
        public string SP_COMP_FIRM_GUID { get; set; }
        public string SP_COMP_SYSTEM_NAME { get; set; }
        public string SP_COMP_MACADDRESS { get; set; }
        public string SP_COMP_PROCESSOR_ID { get; set; }
        public int SP_COMP_FIRM_ID { get; set; }
    }
    // user/kullanıcı bilgileri
    public class tUstadUser
    {
        public tUstadUser()
        {
            Clear();
        }

        public bool IsActive = true;
        public int UserId { get; set; }
        public string UserGUID { get; set; }
        public string UserFirmGUID { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string eMail { get; set; }
        public string MobileNo { get; set; }
        public string Key { get; set; } /* user password ü */
        public Int16 UserDbTypeId { get; set; } /* user ustad firmasının hangi veritabanlarını görebilir işareti */
        public int MainFirmId { get; set; } /* user ın esas çalıştığı yani il giriş yaptığı firmanın ID si : FirmId, SP_FIRM_ID */
        public int ExternalFirmId { get; set; } /* user ın inceleme veya müdehale etmek için giriş yaptığı firmanın ID si */
        public string MainFirmMenuCode { get; set; } /* user ın esas çalıştığı yani il giriş yaptığı firmanın menü kodu */
        public string ExternalFirmMenuCode { get; set; } /* user ın inceleme veya müdehale etmek için giriş yaptığı firmanın menü kodu */
        public string MebbisCode { get; set; }
        public string MebbisPass { get; set; }
        public string UserTcNo { get; set; }
        public string Username_ { get; set; } /* Tabim users dan gelen bilgi */
        public string JwtToken { get; set; } // JWT authentication token from API login

        public void Clear()
        {
            IsActive = true;
            UserId = 0;
            UserGUID = "";
            UserFirmGUID = "";
            FullName = "";
            FirstName = "";
            LastName = "";
            eMail = "";
            MobileNo = "";
            Key = "";
            UserDbTypeId = 0;
            MainFirmId = 0;
            ExternalFirmId = 0;
            MainFirmMenuCode = "";
            ExternalFirmMenuCode = "";
            MebbisCode = "";
            MebbisPass = "";
            UserTcNo = "";
            Username_ = "";
            JwtToken = "";
        }

    }
    // firm/kullanıcının bağlanıp işlem yaptığı firma bilgileri
    public class tUstadFirm
    {
        public int FirmId { get; set; }
        public string FirmLongName { get; set; }
        public string FirmShortName { get; set; }
        public string FirmGuid { get; set; }
        public string IlKodu { get; set; }
        public string IlceKodu { get; set; }
        public string IlAdi { get; set; }
        public string IlceAdi { get; set; }
        public string MenuCode { get; set; }
        public string MenuCodeOld { get; set; }

        public Int16 SectorTypeId { get; set; }
        public string DatabaseType { get; set; }
        public string DatabaseName { get; set; }
        public string ServerNameIP { get; set; }
        public string DbAuthentication { get; set; }
        public string DbLoginName { get; set; }
        public string DbPassword { get; set; }
        public Int16 DbTypeId { get; set; }
        public string FirmMebbisCode { get; set; }
        public string FirmMebbisPass { get; set; }
        public string FirmGIBeArsivFaturaCode { get; set; }
        public string FirmGIBeArsivFaturaPass { get; set; }

        public void Clear()
        {
            FirmId = 0;
            FirmLongName = "";
            FirmShortName = "";
            FirmGuid = "";
            IlKodu = "";
            IlceKodu = "";
            IlAdi = "";
            IlceAdi = "";
            MenuCode = "";
            MenuCodeOld = "";
            SectorTypeId = 0;
            DbTypeId = 0;
            FirmMebbisCode = "";
            FirmMebbisPass = "";
            FirmGIBeArsivFaturaCode = "";
            FirmGIBeArsivFaturaPass = "";
        }
    }
    // Surucu07
    public class tTabimFirm
    {
        public tTabimFirm()
        {
            Clear();
        }
        public string KursunAdi { get; set; }
        public string Adres1 { get; set; }
        public string Adres2 { get; set; }
        public string Telefon { get; set; }
        public string KursMuduru { get; set; }
        public string KurucuAdi { get; set; }
        public string KursunKodu { get; set; }
        public string MebbisKullaniciAdi { get; set; }
        public string MebbisSifresi { get; set; }
        public string GIBeArsivFaturaKullaniciAdi { get; set; }
        public string GIBeArsivFaturaSifresi { get; set; }

        public string FirmGUID { get; set; }
        public void Clear()
        {
            KursunAdi = "";
            Adres1 = "";
            Adres2 = "";
            Telefon = "";
            KursMuduru = "";
            KurucuAdi = "";
            KursunKodu = "";
            MebbisKullaniciAdi = "";
            MebbisSifresi = "";
            GIBeArsivFaturaKullaniciAdi = "";
            GIBeArsivFaturaSifresi = "";
            FirmGUID = "";
        }
    }

    public class tUserRegister
    {
        public tUserRegister()
        {
            eMailList = new List<object>();
            userNameList = new List<object>();
        }
        public int UserId { get; set; }
        public string eMail { get; set; }
        public bool UserRemember { get; set; }
        public string UserLastLoginEMail { get; set; }
        public string UserLastKey { get; set; }
        public int UserLastFirmId { get; set; }
        public List<object> eMailList { get; set; }
        public List<object> userNameList { get; set; }
    }

    // exe hakkındaki bilgiler
    public class exeAbout
    {
        // şu an çalışan exe
        public string activeVersionNo { get; set; }
        public string activeExeName { get; set; }
        public string activePath { get; set; }

        // diğer dosyalar
        public string orderFileVersionNo { get; set; }
        public string orderFileName { get; set; }
        public string orderFileExtension { get; set; }
        public string orderFilePath { get; set; }


        // ftp deki exe, file
        public string ftpVersionNo { get; set; }
        public string ftpFileName { get; set; }
        public string ftpFileExtension { get; set; }
        public string ftpPacketName { get; set; }
        public string ftpPathName { get; set; }

        // yeni hazırlanan exe
        public string newVersionNo { get; set; }
        public string newFileName { get; set; }
        public string newPacketName { get; set; }
        
        public v.fileType FileType { get; set; }
    }

    public class FirmUserList
    {
        public FirmUserList()
        {
            firmAbout = new List<tUstadFirm>();
        }
        public List<tUstadFirm> firmAbout { get; set; }
    }

    public class FirmFullList
    {
        public FirmFullList()
        {
            firmFullList = new List<tUstadFirm>();
        }
        public List<tUstadFirm> firmFullList { get; set; }
    }

    public class eMail
    {
        private string toMailAddress_ = "";
        private string toCCMailAddress_ = "";
        private string subject_ = "";
        private string message_ = "";

        public string toMailAddress
        {
            get { return toMailAddress_; }
            set { toMailAddress_ = value; }
        }
        public string toCCMailAddress
        {
            get { return toCCMailAddress_; }
            set { toCCMailAddress_ = value; }
        }
        public string subject
        {
            get { return subject_; }
            set { subject_ = value; }
        }
        public string message
        {
            get { return message_; }
            set { message_ = value; }
        }

        /*
            string userName = "tekinucar70@gmail.com";
            string userPass = "canberk98";
            string smtp = "smpt.gmail.com";
            string port = "587";
            string toMail = "";   // gideceği adres
            string toCCMail = ""; // bilgi verilen mail
            string subject = "";  // konu başlığı
            string bodyMessage = ""; // gönderilecek mesaj
        */
    }

    public class cComputer
    {
        public int UstadCrmComputerId { get; set; }
        public string PcName { get; set; }
        public string Network_MACAddress { get; set; }
        public string CPUType { get; set; }
        public string CPUSpeed { get; set; }
        public string OperatingSystem { get; set; }

        public string Processor_Name { get; set; }
        public string Processor_Id { get; set; }

        public string DiskDrive_Name { get; set; }
        public string DiskDrive_Model { get; set; }
        public string DiskDrive_SerialNumber { get; set; }


        ///Win32_Processor
        ///
        ///string SystemName
        ///string Name 
        ///string ProcessorId
        ///
        ///Win32_DiskDrive
        ///
        ///string   Name;
        ///string   Model;
        ///string   SerialNumber;

    }

    public class tColumn
    {
        public string value { get; set; }
    }

    public class tColumns
    {
        public tColumns()
        {
            Columns = new List<tColumn>();
        }
        public List<tColumn> Columns { get; set; }
    }

    public class tRow
    {
        public tRow()
        {
            tColumns = new List<tColumn>();
        }
        public List<tColumn> tColumns { get; set; }
    }

    public class tRows
    {
        public tRows()
        {
            Rows = new List<tRow>();
        }
        public List<tRow> Rows { get; set; }
    }
    public class tTable
    {
        public tTable()
        {
            tRows = new List<tRow>();
        }
        public List<tRow> tRows { get; set; }
    }

    public class webNodeValue
    {
        public webNodeValue()
        {
            elements = new List<HtmlElement>();
            elementsSelenium = new List<OpenQA.Selenium.IWebElement>();
        }

        public webNodeValue Copy()
        {
            return (webNodeValue)this.MemberwiseClone();
        }

        public List<HtmlElement> elements { get; set; }

        public IList<OpenQA.Selenium.IWebElement> htmlTable { get; set; }

        public List<OpenQA.Selenium.IWebElement> elementsSelenium { get; set; }

        public int nodeId { get; set; }
        public string pageCode { get; set; }
        public string TagName { get; set; }
        public string AttId { get; set; }
        public string AttName { get; set; }
        public string AttClass { get; set; }
        public string AttType { get; set; }
        public string AttRole { get; set; }
        public string AttHRef { get; set; }
        public string AttSrc { get; set; }
        public string XPath { get; set; }

        public string InnerText { get; set; }
        public string OuterText { get; set; }

        public v.tWebEventsType EventsType { get; set; }
        public v.tWebInjectType InjectType { get; set; } 
        public v.tWebInvokeMember InvokeMember { get; set; }
        public bool IsInvoke { get; set; }
        public v.tWebEventsType workEventsType { get; set; } // kod çalırken işe göre atanıyor
        public v.tWebRequestType workRequestType { get; set; } // kod çalırken işe göre atanıyor

        public string readValue { get; set; } // get sırasında kullanılacak
        public string writeValue { get; set; } // set sırasında kullanılacak
        public string keyValue { get; set; } // set sırasında key (TcNo == "xxx") anahtarı olarak kullanılacak

        public tTable tTable { get; set; }
        public List<List<string>> dynamicTable { get; set; }
        public Int16 tTableColNo {get; set; }
        public string TableIPCode { get; set; }
        public string dbFieldName { get; set; }
        public bool dbLookUpField { get; set; }

        public Int16 dbFieldType { get; set; }
        public Int16 dbColNo { get; set; }
        public bool DontSave { get; set; }
        public bool GetSave { get; set; }

        public string KrtOperandType { get; set; }
        public string CheckOperandValue { get; set; }
        public int CheckNodeId { get; set; }
        public string CheckReadValue { get; set; }

        public DataSet ds { get; set; }
        public DataNavigator dN { get; set; }
        public void Clear()
        {
            nodeId = 0;
            pageCode = "";
            TagName = "";
            AttId = "";
            AttName = "";
            AttClass = "";
            AttType = "";
            AttRole = "";
            AttHRef = "";
            AttSrc = "";
            XPath = "";
            InnerText = "";
            OuterText = "";
            EventsType = v.tWebEventsType.none;
            InjectType = v.tWebInjectType.none;
            InvokeMember = v.tWebInvokeMember.none;
            IsInvoke = false;

            workRequestType = v.tWebRequestType.none;
            workEventsType = v.tWebEventsType.none;

            readValue = "";
            writeValue = "";
            keyValue = "";
            tTable = null;
            dynamicTable = null;
            tTableColNo = 0;
            TableIPCode = "";
            dbFieldName = "";
            dbLookUpField = false;
            dbFieldType = 0;
            dbColNo = 0;
            DontSave = false;
            GetSave = false;
            KrtOperandType = "";
            CheckOperandValue = "";
            CheckNodeId = 0;
            htmlTable = null;

            ds = null; // o anda işlem yapılan dataset 
            dN = null; //   ve datanavigator
        }
    }

    public class webNodeItemsList
    {
        public webNodeItemsList()
        {
            Clear();
        }

        public int Id { get; set; }
        public int NodeId { get; set; }
        public string PageCode { get; set; }
        public bool IsActive { get; set; }
        public string ItemValue { get; set; }
        public string ItemText { get; set; }
        public void Clear()
        {
            Id = 0;
            NodeId = 0;
            PageCode = "";
            IsActive = false;
            ItemValue = "";
            ItemText = "";
        }
    }

    public class tSetAttribute
    {
        private string caption = string.Empty;
        private string elemanName = string.Empty;
        private string setValue = string.Empty;
        private v.tWebInvokeMember invokeMember;

        public string _01_Caption
        {
            get { return caption; }
            set { caption = value; }
        }
        public string _02_ElemanName
        {
            get { return elemanName; }
            set { elemanName = value; }
        }
        public string _03_SetValue
        {
            get { return setValue; }
            set { setValue = value; }
        }
        internal v.tWebInvokeMember _04_InvokeMember
        {
            get { return invokeMember; }
            set { this.invokeMember = value; }
        }
    }

    public class vSubWork
    {
        private Form tForm = null;
        private string TableIPCode = string.Empty;
        private v.tWorkTD tWorkTD;
        private v.tWorkWhom tWorkWhom;
        private Control tabPageControl = null;
        public Form _01_tForm
        {
            get { return tForm; }
            set { tForm = value; }
        }
        public string _02_TableIPCode
        {
            get { return TableIPCode; }
            set { TableIPCode = value; }
        }
        internal v.tWorkTD _03_WorkTD
        {
            get { return tWorkTD; }
            set { this.tWorkTD = value; }
        }
        internal v.tWorkWhom _04_WorkWhom
        {
            get { return tWorkWhom; }
            set { this.tWorkWhom = value; }
        }
        public Control _05_tabPageControl
        {
            get { return tabPageControl; }
            set { tabPageControl = value; }
        }
    }

    public class vSubView
    {
        private string SV_KeyFName_ = string.Empty;
        private string SV_CaptionFName_ = string.Empty;
        private string SV_CmpType_ = string.Empty;
        private string SV_CmpLocation_ = string.Empty;
        private string SV_ViewType_ = string.Empty;
        private string SV_List_ = string.Empty;

        public string SV_KeyFName
        {
            get { return SV_KeyFName_; }
            set { SV_KeyFName_ = value; }
        }
        public string SV_CaptionFName
        {
            get { return SV_CaptionFName_; }
            set { SV_CaptionFName_ = value; }
        }
        public string SV_CmpType
        {
            get { return SV_CmpType_; }
            set { SV_CmpType_ = value; }
        }
        public string SV_CmpLocation
        {
            get { return SV_CmpLocation_; }
            set { SV_CmpLocation_ = value; }
        }
        public string SV_ViewType
        {
            get { return SV_ViewType_; }
            set { SV_ViewType_ = value; }
        }
        public string SV_List
        {
            get { return SV_List_; }
            set { SV_List_ = value; }
        }
    }

    public class vNavigatorButton
    {
        public Form tForm { get; set; }
        public Control navigatorPanel { get; set; }
        public DataSet ds_Fields { get; set; }
        public string navigatorList { get; set; }
        public string TableIPCode { get; set; }
        public string buttonName { get; set; }
        public string buttonText { get; set; }
        public string buttonKey { get; set; }

        public Int16 tabIndex { get; set; }
        public bool tabStop { get; set; }
        public int width { get; set; }
        public DockStyle dock { get; set; }
        public string imageName { get; set; }
        public bool events { get; set; }
        public string propNavigator { get; set; }
        public string propSearch { get; set; }
        public System.Drawing.Color backColor { get; set; }
    }

    public class vTable
    {
        public vTable()
        {
            //Clear()
        }

        public v.dBaseNo DBaseNo { get; set; }
        public v.dBaseType DBaseType { get; set; }
        public string DBaseName { get; set; }
        public string TableName { get; set; }
        public string TableIPCode { get; set; }
        public string TableCode { get; set; }
        public string IPCode { get; set; }
        public string Cargo { get; set; }
        public byte TableCount { get; set; }
        public byte TableType { get; set; }
        public string KeyId_FName { get; set; }
        public string Lock_FName { get; set; }
        public string functionName { get; set; }
        public string SoftwareCode { get; set; }
        public string ProjectCode { get; set; }
        public string SchemasCode { get; set; }
        public string FormCode { get; set; }
        public string ParentTable { get; set; }
        public string SqlScript { get; set; }

        public bool RunTime { get; set; }
        public bool IdentityInsertOnOff { get; set; } //IDENTITY_INSERT 
        public int FirmId { get; set; }
        public SqlConnection msSqlConnection { get; set; }
        public void Clear()
        {
            DBaseNo = 0;
            DBaseType = v.dBaseType.None;
            DBaseName = "";
            TableName = "";
            TableIPCode = "";
            TableCode = "";
            IPCode = "";
            KeyId_FName = "";
            Lock_FName = "";
            functionName = "";
            Cargo = "";
            TableCount = 0;
            SoftwareCode = "";
            ProjectCode = "";
            SchemasCode = "";
            FormCode = "";
            ParentTable = "";
            RunTime = false;
            IdentityInsertOnOff = false;
            FirmId = 0;
            msSqlConnection = null;
        }
    }

    public class vScripts
    {
        public vScripts()
        {
            Clear();
        }
        public v.dBaseNo DBaseNo { get; set; }
        public v.dBaseType DBaseType { get; set; }
        public string SourceDBaseName { get; set; }
        public string SchemaName { get; set; }
        public string TableIPCode { get; set; }
        public string SourceTableName { get; set; }
        public string Where { get; set; }
        public bool IdentityInsertOnOff { get; set; } //IDENTITY_INSERT 
        public void Clear()
        {
            DBaseNo = 0;
            DBaseType = v.dBaseType.None;
            SourceDBaseName = "";
            SchemaName = "";
            TableIPCode = "";
            SourceTableName = "";
            Where = "";
            IdentityInsertOnOff = false;
        }
    }

    public class vTableAbout
    {
        public int TablesCount { get; set; }
        public int RecordCount { get; set; }
        public int FieldsCount { get; set; }
        public int GroupsCount { get; set; }
        public int groupPanelCount { get; set; } 
        public int dLTabPageCount { get; set; }
        public int dWTabPageCount { get; set; }
        public bool IsKisitlama { get; set; }
    }

    public class vUserInputBox
    {
        private string title_ = string.Empty;
        private string promptText_ = string.Empty;
        private string value_ = string.Empty;
        private string displayFormat_ = string.Empty;
        private int fieldType_ = 0;

        public string title
        {
            get { return title_; }
            set { title_ = value; }
        }
        public string promptText
        {
            get { return promptText_; }
            set { promptText_ = value; }
        }
        public string value
        {
            get { return value_; }
            set { value_ = value; }
        }
        public string displayFormat
        {
            get { return displayFormat_; }
            set { displayFormat_ = value; }
        }
        public int fieldType
        {
            get { return fieldType_; }
            set { fieldType_ = value; }
        }

        public void Clear()
        {
            title_ = "";
            promptText_ = "";
            value_ = "";
            displayFormat_ = "";
            fieldType_ = 0;
        }
    }

    public class vGridHint
    {
        public Form tForm { get; set; }
        public DataSet dataSet { get; set; }
        public DataNavigator dataNavigator { get; set; }
        public object view { get; set; }
        public DevExpress.XtraGrid.Columns.GridColumn currentColumn { get; set; }
        public string tableIPCode { get; set; }
        public string keyFieldName { get; set; }
        public string gridPropNavigator { get; set; }
        public string columnPropNavigator { get; set; }
        public string columnOldValue { get; set; }
        public string columnEditValue { get; set; }
        public int editValueCount { get; set; }
        public string columnFieldName { get; set; }
        public string viewType { get; set; }
        public string parentObject { get; set; }
        public v.tButtonType buttonType { get; set; }
        public object focusedRow { get; set; }
        public bool isLastColumn { get; set; }
        public bool isLastRow { get; set; }

        public void Clear()
        {
            tForm = null;
            dataSet = null;
            dataNavigator = null;
            view = null;
            currentColumn = null;
            tableIPCode = "";
            keyFieldName = "";
            gridPropNavigator = "";
            columnPropNavigator = "";
            columnOldValue = "";
            columnEditValue = "";
            editValueCount = 0;
            columnFieldName = "";
            viewType = "";
            parentObject = "";
            buttonType = 0;
            focusedRow = null;
            isLastColumn = false;
            isLastRow = false;
        }

    }

    public class vButtonHint
    {
        public vButtonHint()
        {
            propList_ = new List<PROP_NAVIGATOR>();
        }

        public Form tForm { get; set; }
        public object sender { get; set; }
        public string senderType { get; set; }
        public string tableIPCode { get; set; }
        public string buttonName { get; set; }
        public string caption { get; set; }
        public string checkedValue { get; set; }
        public string columnOldValue { get; set; }
        public string columnEditValue { get; set; }
        public string columnFieldName { get; set; }
        public string propNavigator { get; set; }
        public PROP_NAVIGATOR prop_ { get; set; }
        public List<PROP_NAVIGATOR> propList_ { get; set; }
        public v.tButtonType buttonType { get; set; }
        public string keyFieldName_ { get; set; }
        public string masterKeyFieldName_ { get; set; }
        public string foreingFieldName_ { get; set; }
        public string parentFieldName_ { get; set; }
        public string parentObject { get; set; }
        public string viewType { get; set; }

        public void Clear()
        {
            tForm = null;
            sender = null;
            senderType = "";
            tableIPCode = "";
            buttonName = "";
            caption = "";
            checkedValue = "";
            columnOldValue = "";
            columnEditValue = "";
            columnFieldName = "";
            propNavigator = "";
            prop_ = null;
            propList_ = null;
            buttonType = 0;
            keyFieldName_ = "";
            masterKeyFieldName_ = "";
            foreingFieldName_ = "";
            parentFieldName_ = "";
            parentObject = "";
            viewType = "";
        }
    }

    public class vResimEditor
    {
        public string imagesSourceFormName { get; set; }
        public string imagesSourceTableIPCode { get; set; }
        public string imagesSourceFieldName { get; set; }
        public string imagesMasterFormCode { get; set; }
        public string imagesMasterTableIPCode { get; set; }
        public string listTableIPCode { get; set; }
        public string formCode { get; set; }
        public void Clear()
        {
            imagesSourceFormName = "";
            imagesSourceTableIPCode = "";
            imagesSourceFieldName = "";
            imagesMasterFormCode = "";
            imagesMasterTableIPCode = "";
            listTableIPCode = "";
            formCode = "";
        }
    }

    public class vImageProperties
    {
        public int width { get; set; }
        public int height { get; set; }
        public float horizontalResolation { get; set; }
        public float verticalResolation { get; set; }
        public long byteSize { get; set; }
        public decimal kbSize { get; set; }
        public System.Drawing.Imaging.ImageFormat imageFormat { get; set; }
        public string imagePath { get; set; }
        public string imageName { get; set; }

        public void Clear()
        {
            width = 0;
            height = 0;
            horizontalResolation = 0;
            verticalResolation = 0;
            byteSize = 0;
            kbSize = 0;
            imageFormat = null;
            imagePath = string.Empty;
            imageName = string.Empty;
        }
    }


    public class bildirimPaketi
    {
        public bildirimPaketi()
        {
            Clear();
            ClearLines();
        }
        public string formName { get; set; }
        public string TableIPCodeHeader { get; set; }
        public string TableIPCodeLines { get; set; }
        public string bildirimListesiFormCode { get; set; }
        public string bildirimListesiHeader { get; set; }
        public string bildirimListesiLines { get; set; }
        public string readTableIPCode { get; set; }
        public string workType { get; set; }
        public int headerTableIdValue { get; set; }

        public Int16 secilenKanalTypeId { get; set; }
        public int secilenSablonId { get; set; }
        public string secilenMesajKodu { get; set; }
        public string secilenBildirimMetni { get; set; }
        public Int16 secilenGonderimTypeId { get; set; }
        public DateTime secilenGonderimTarihi { get; set; }
        public string secilenGonderimSaati { get; set; }


        public string hedefTalepIdFName { get; set; }
        public string kaynakTalepIdFName { get; set; }
        public int kaynakTalepIdValue { get; set; }


        public string hedefCariIdFName { get; set; }
        public string kaynakCariIdFName { get; set; }
        public int kaynakCariIdValue { get; set; }


        public string hedefTelefonNoFName { get; set; }
        public string kaynakTelefonNoFName { get; set; }
        public string kaynakTelefonNoValue { get; set; }


        public string hedefAliciAdiFName { get; set; }
        public string kaynakAliciAdiFName { get; set; }
        public string kaynakAliciAdiValue { get; set; }


        public string hedefSourceTypeIdFName { get; set; }
        //public string kaynekSourceTypeIdFName { get; set; } buna gerek yok
        public Int16 kaynakSourceTypeIdValue { get; set; }


        public string hedefSourceIdFName { get; set; }
        public string kaynakSourceIdFName { get; set; }
        public int kaynakSourceIdValue { get; set; }


        public string hedefDinamik1FName { get; set; }
        public string kaynakDinamik1FName { get; set; }
        public string kaynakDinamik1Value { get; set; }

        public string hedefDinamik2FName { get; set; }
        public string kaynakDinamik2FName { get; set; }
        public string kaynakDinamik2Value { get; set; }


        public string hedefDinamik3FName { get; set; }
        public string kaynakDinamik3FName { get; set; }
        public string kaynakDinamik3Value { get; set; }


        public void Clear()
        {
            formName = "";
            TableIPCodeHeader = "";
            TableIPCodeLines = "";
            bildirimListesiFormCode = "";
            bildirimListesiHeader = "";
            bildirimListesiLines = "";
            readTableIPCode = "";
            workType = "";
            headerTableIdValue = 0;

            secilenKanalTypeId = 0;
            secilenSablonId = 0;
            secilenMesajKodu = "";
            secilenBildirimMetni = "";
            secilenGonderimTypeId = 0;
            //secilenGonderimTarihi = ;
            secilenGonderimSaati = "";
        }

        public void ClearLines()
        {
            hedefTalepIdFName = "";
            kaynakTalepIdFName = "";
            kaynakTalepIdValue = 0;

            hedefCariIdFName = "";
            kaynakCariIdFName = "";
            kaynakCariIdValue = 0;

            hedefTelefonNoFName = "";
            kaynakTelefonNoFName = "";
            kaynakTelefonNoValue = "";

            hedefAliciAdiFName = "";
            kaynakAliciAdiFName = "";
            kaynakAliciAdiValue = "";

            hedefSourceTypeIdFName = "";
            kaynakSourceTypeIdValue = 0;

            hedefSourceIdFName = "";
            kaynakSourceIdFName = "";
            kaynakSourceIdValue = 0;

            hedefDinamik1FName = "";
            kaynakDinamik1FName = "";
            kaynakDinamik1Value = "";

            hedefDinamik2FName = "";
            kaynakDinamik2FName = "";
            kaynakDinamik2Value = "";

            hedefDinamik3FName = "";
            kaynakDinamik3FName = "";
            kaynakDinamik3Value = "";

        }

    }

    public class vSMSSettings
    {
        public int Id { get; set; }
        public int FirmId { get; set; }
        public bool IsActive { get; set; }
        public Int16 ServisTypeId { get; set; }
        public string KullaniciAdi { get; set; }
        public string Sifre { get; set; }
        public string BayiiKodu { get; set; }
        public string Origin { get; set; }
    }

    public class vInvoiceUser
    {
        public bool UseTestEnvironment { get; set; }
        public bool LoggingEnabled { get; set; }
        public string WebServiceTestUsername { get; set; }
        public string WebServiceTestPassword { get; set; }
        public string WebServiceTestUri { get; set; }
        public string WebServiceLiveUsername { get; set; }
        public string WebServiceLivePassword { get; set; }
        public string WebServiceLiveUri { get; set; }


        public void Clear()
        {
            UseTestEnvironment = true;
            LoggingEnabled = false;
            WebServiceTestUsername = "Uyumsoft";
            WebServiceTestPassword = "Uyumsoft";
            WebServiceTestUri = "http://efatura-test.uyumsoft.com.tr/Services/BasicIntegration";
            WebServiceLiveUsername = "";
            WebServiceLivePassword = "";
            WebServiceLiveUri = "";
        }
    }


    // ------------------------------------------------------------------
    // ------------------------------------------------------------------

    #region PROP_SUBVIEW in ( MS_TABLES, MS_TABLES_IP, SYS_TYPES_L )

    // --- OLD
    public class PROP_SUBVIEW_OLD
    {
        public PROP_SUBVIEW_OLD()
        {
            SV_LIST = new List<SV_LIST>();
        }
        public string SV_ENABLED { get; set; }
        public string SV_KEYFNAME { get; set; }
        public string SV_CAPTION_FNAME { get; set; }
        public string SV_CMP_TYPE { get; set; }
        public string SV_CMP_LOCATION { get; set; }
        public string SV_VIEW_TYPE { get; set; }
        public List<SV_LIST> SV_LIST { get; set; }
    }
    public class SV_LIST
    {
        public string CAPTION { get; set; }
        public string SV_VALUE { get; set; }
        public string TABLEIPCODE { get; set; }
        public string SM_PAGENAME { get; set; }
    }
    // --- OLD

    // SubView işlemi 3 değişik şekilde çalışmakta
    // 1. PROB_SUBVIEW   : DataNavigator Change üzerinde çalışıyor
    // 2. Buton Click te : PROP_NAVIGATOR üzerinde  tButtonType.btOpenSubView = 125 şeklinde
    // 3. Yine Button Click ExtraIslemler için çalışmaktadır : PROP_NAVIGATOR.TABLEIPCODE_LIST.WORKTYPE üzerinde 
    public class PROP_SUBVIEW
    {
        public PROP_SUBVIEW()
        {
            SUBVIEW_LIST = new List<SUBVIEW_LIST>();
        }
        public string SUBVIEW_ENABLED { get; set; }
        public string SUBVIEW_KEYFNAME { get; set; }
        public string SUBVIEW_KEYFNAME2 { get; set; }
        public string SUBVIEW_CAPTION_FNAME { get; set; }
        public string SUBVIEW_CMP_TYPE { get; set; }
        public string SUBVIEW_CMP_LOCATION { get; set; }
        public string SUBVIEW_TYPE { get; set; }
        public List<SUBVIEW_LIST> SUBVIEW_LIST { get; set; }
    }

    public class SUBVIEW_LIST
    {
        public string CAPTION { get; set; }
        public string SUBVIEW_VALUE { get; set; }
        public string SUBVIEW_TABLEIPCODE { get; set; }
        public string SUBVIEW_FORMCODE { get; set; }
        public string SHOWMENU_PAGENAME { get; set; }
    }

    
    #endregion PROP_SUBVIEW

    #region PROP_JOINTABLE in ( MS_TABLES )

    public class PROP_JOINTABLE
    {
        public PROP_JOINTABLE()
        {
            J_TABLE = new List<J_TABLE>();
            J_WHERE = new List<J_WHERE>();
            J_STN_FIELDS = new List<J_STN_FIELDS>();
            J_CASE_FIELDS = new List<J_CASE_FIELDS>();
        }
        public List<J_TABLE> J_TABLE { get; set; }
        public List<J_WHERE> J_WHERE { get; set; }
        public List<J_STN_FIELDS> J_STN_FIELDS { get; set; }
        public List<J_CASE_FIELDS> J_CASE_FIELDS { get; set; }
    }

    public class J_TABLE
    {
        public string CAPTION { get; set; }
        public string J_FORMAT { get; set; }
        public string J_TYPE { get; set; }
        public string J_TABLE_NAME { get; set; }
        public string J_TABLE_ALIAS { get; set; }
    }

    public class J_WHERE
    {
        public string CAPTION { get; set; }
        public string M_TABLE_ALIAS { get; set; }
        public string MT_FNAME { get; set; }
        public string J_TABLE_ALIAS { get; set; }
        public string J_FNAME { get; set; }
        public string FVALUE { get; set; }
        public string WHERE_ADD { get; set; }
    }

    public class J_STN_FIELDS
    {
        public string CAPTION { get; set; }
        public string J_TABLE_ALIAS { get; set; }
        public string J_FNAME { get; set; }
        public string FNL_FNAME { get; set; }
    }

    public class J_CASE_FIELDS
    {
        public string CAPTION { get; set; }
        public string CASE_FOR_FNAME { get; set; }
        public string KRT_ODD { get; set; }
        public string WHERE_VALUE { get; set; }
        public string THEN_ALIAS { get; set; }
        public string THEN_FNAME { get; set; }
        public string FINAL_FNAME { get; set; }
    }

    #endregion PROP_JOINTABLE

    #region PROP_VIEWS in ( MS_TABLES_IP )

    public class PROP_VIEWS_IP
    {
        public PROP_VIEWS_IP()
        {
            ALLPROP = new ALLPROP();
            GRID = new GRID();
            TREE = new TREE();
            TILE = new TILE();
            DATALAYOUT = new DATALAYOUT();
            SCHEDULER = new SCHEDULER();
        }
        public ALLPROP ALLPROP { get; set; }
        public GRID GRID { get; set; }
        public string GRIDADB { get; set; }
        public string GRIDLYT { get; set; }
        public string WINEXP { get; set; }
        public string VGRID { get; set; }
        public TREE TREE { get; set; }
        public string PIVOT { get; set; }
        public DATALAYOUT DATALAYOUT { get; set; }
        public TILE TILE { get; set; }
        public SCHEDULER SCHEDULER { get; set; }
    }

    public class TREE
    {
        public string COLLEXPAND { get; set; }
    }

    public class DATALAYOUT
    {
        public string MOVEFOCUS { get; set; }
    }

    public class ALLPROP
    {
        public string TABSTOP { get; set; }
        public string GROUPFNAME1 { get; set; }
        public string GROUPFNAME2 { get; set; }
        public string GROUPFNAME3 { get; set; }
    }

    public class GRID
    {
        public string ALLOWCELLMERGE { get; set; }
        public string COLUMNAUTOVIEW { get; set; }
        public string NEWITEMROWPOSITION { get; set; }
        public string EVENROW { get; set; }
        public string ODDROW { get; set; }
        public string SAUTOFILTERROW { get; set; }
        public string SCOLUMNHEADERS { get; set; }
        public string SFOOTER { get; set; }
        public string SGROUPEDCOLUMNS { get; set; }
        public string SGROUPPANEL { get; set; }
        public string SHORIZONTALLINES { get; set; }
        public string SINDICATOR { get; set; }
        public string SPRIVIEW { get; set; }
        public string PRIVIEWFIELDNAME { get; set; }
        public string SVERTICALLINES { get; set; }
        public string SVIEWCAPTION { get; set; }
        public string INVERTSELECTION { get; set; }
        public string MULTISELECTION { get; set; }
    }

    public class TILE
    {
        public string TL_ITEMWIDTH { get; set; }
        public string TL_ITEMHEIGHT { get; set; }
        public string TL_ORIENTATION { get; set; }
    }

    public class SCHEDULER
    {
        public string AppointmentId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Location { get; set; }
        public string Subject { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string SchedulerViewType { get; set; }

    }


    #endregion

    #region PROP_RUNTIME in ( MS_TABLES_IP )

    public class PROP_RUNTIME
    {
        public PROP_RUNTIME()
        {
            DRAGDROP = new List<DRAGDROP>();
            PRL_KRT = new List<PRL_KRT>();
            AUTO_LST = new List<AUTO_LST>();
        }
        public List<DRAGDROP> DRAGDROP { get; set; }
        public List<PRL_KRT> PRL_KRT { get; set; }
        public string AUTO_INS_MST_IP { get; set; }
        public List<AUTO_LST> AUTO_LST { get; set; }
        public string AUTO_REFRESH_IP { get; set; }
        public string AUTO_REFRESH_IP2 { get; set; }
        public string AUTO_REFRESH_IP3 { get; set; }

    }

    public class DRAGDROP
    {
        public string CAPTION { get; set; }
        public string JBTYPE { get; set; }
        public string JBCODE { get; set; }
        public string JBAFTER { get; set; }
        public string JBNAVIGATOR { get; set; }
    }

    public class PRL_KRT
    {
        public string CAPTION { get; set; }
        public string TABLEIPCODE { get; set; }
    }

    public class AUTO_LST
    {
        public AUTO_LST()
        {
            TABLEIPCODE_LIST2 = new List<TABLEIPCODE_LIST2>();
        }
        public string CAPTION { get; set; }
        public string BUTTONTYPE { get; set; }
        public List<TABLEIPCODE_LIST2> TABLEIPCODE_LIST2 { get; set; }
    }

    public class TABLEIPCODE_LIST2
    {
        public string CAPTION { get; set; }
        public string TABLEIPCODE { get; set; }
    }

    #endregion

    #region PROP_NAVIGATOR in ( MS_TABLES_IP, MS_ITEMS )
    public class PROP_NAVIGATOR
    {
        public PROP_NAVIGATOR()
        {
            TABLEIPCODE_LIST = new List<TABLEIPCODE_LIST>();
        }
        public string CAPTION { get; set; }
        public string BUTTONTYPE { get; set; }
        public List<TABLEIPCODE_LIST> TABLEIPCODE_LIST { get; set; }
        public string READ_TABLEIPCODE { get; set; } // new
        public string TARGET_TABLEIPCODE { get; set; } // new
        public string FORMNAME { get; set; }
        public string FORMCODE { get; set; }
        public string FORMTYPE { get; set; }
        public string FORMSTATE { get; set; }
        public string FORM_WIDTH { get; set; } // new
        public string FORM_HEIGHT { get; set; } // new
        public string KRITER { get; set; } // new
        public string CHC_IPCODE { get; set; } // FIRST 
        public string CHC_FNAME { get; set; }
        public string CHC_VALUE { get; set; }
        public string CHC_OPERAND { get; set; }
        public string CHC_IPCODE_SEC { get; set; } // SECOND
        public string CHC_FNAME_SEC { get; set; }
        public string CHC_VALUE_SEC { get; set; }
        public string CHC_OPERAND_SEC { get; set; }
        public bool TransactionRun { get; set; }
    }

    public class TABLEIPCODE_LIST
    {
        public string CAPTION { get; set; }
        public string TABLEIPCODE { get; set; }
        public string TABLEALIAS { get; set; }
        public string KEYFNAME { get; set; }
        public string RTABLEIPCODE { get; set; }
        public string RKEYFNAME { get; set; }
        public string MSETVALUE { get; set; }
        public string WORKTYPE { get; set; }
        public string CONTROLNAME { get; set; }
        public string DCCODE { get; set; }
        public string BEFOREAFTER { get; set; }
        public string CHC_IPCODE3 { get; set; } // FIRST 
        public string CHC_FNAME3 { get; set; }
        public string CHC_VALUE3 { get; set; }
        public string CHC_OPERAND3 { get; set; }
    }
    //DCCODE { get; set; } // DC = DataCopy 
    #endregion

    #region PROP_SEARCH in ( MS_TABLES_IP, MS_FIELDS_IP, MS_VARIABLES )

    public class PROP_SEARCH
    {
        public PROP_SEARCH()
        {
            GET_FIELD_LIST = new List<GET_FIELD_LIST>();
        }
        public string CAPTION { get; set; }
        public string TABLEIPCODE { get; set; }
        public string FORMCODE { get; set; }
        public List<GET_FIELD_LIST> GET_FIELD_LIST { get; set; }
        public string FORM_WIDTH { get; set; }
        public string FORM_HEIGHT { get; set; }
        public string KRITER { get; set; }
    }

    public class GET_FIELD_LIST
    {
        public string CAPTION { get; set; }
        public string T_FNAME { get; set; }
        public string R_FNAME { get; set; }
        public string MSETVALUE { get; set; }
    }

    #endregion

    #region PROP_VIEWS in ( MS_ITEMS )

    public class PROP_VIEWS_ITEMS
    {
        public PROP_VIEWS_ITEMS()
        {
            ALLMENU = new ALLMENU();
            NAVBAR = new NAVBAR();
            NAVIGATIONPANE = new NAVIGATIONPANE();
            ACCORDION = new ACCORDION();
        }
        public ALLMENU ALLMENU { get; set; }
        public string BARMANAGER { get; set; }
        public string RIBBON { get; set; }
        public NAVBAR NAVBAR { get; set; }
        public string TILEBAR { get; set; }
        public string TILECONTROL { get; set; }
        public string POPUPMENU { get; set; }
        public NAVIGATIONPANE NAVIGATIONPANE { get; set; }
        public ACCORDION ACCORDION { get; set; }
    }

    public class ALLMENU
    {
        public string DONTREPORT { get; set; }
        public string DONTEDI { get; set; } // Eğitim/Destek/İstek
        public string DONTEXIT { get; set; }
        public string RPRT_TABLEIPCODE { get; set; } //Report_TableIPCode, ReportTableIPCode
        public string RPRT_FORMCODE { get; set; } //Report_FormCode, ReportFormCode
    }

    public class NAVBAR
    {
        public string NB_PAINTSTYLENAME { get; set; }
    }

    public class NAVIGATIONPANE
    {
        public string NP_IPCODE { get; set; }
        public string NP_MENUCODE { get; set; }
    }

    public class ACCORDION
    {
        public string ACCFILTER { get; set; }
        public string ACCEXPANDED { get; set; }
    }

    #endregion

    #region PROP_VIEWS_LAYOUT in ( MS_LAYOUT, MS_GROUPS ) 

    public class PROP_VIEWS_LAYOUT
    {
        public PROP_VIEWS_LAYOUT()
        {
            DOCKPANEL = new DOCKPANEL();
            SPLIT = new SPLIT();
            EDITPANEL = new EDITPANEL();
            TLP = new TLP();
            TABCONTROL = new TABCONTROL();
        }
        public DOCKPANEL DOCKPANEL { get; set; }
        public SPLIT SPLIT { get; set; }
        public EDITPANEL EDITPANEL { get; set; }
        public TLP TLP { get; set; }
        public TABCONTROL TABCONTROL { get; set; }
    }

    public class PROP_VIEWS_GROUPS
    {
        public PROP_VIEWS_GROUPS()
        {
            TLP = new TLP();
        }
        public TLP TLP { get; set; }
    }

    public class DOCKPANEL
    {
        public string DP_TABBED { get; set; }
    }

    public class SPLIT
    {
        public string SC_ORIENTATION { get; set; }
        public string SC_FIXED { get; set; }
        public string SC_ISSPLITTERFIXED { get; set; }
        public string SC_PARENTPANEL { get; set; }
        public string SC_DISTANCE { get; set; }
        public string SC_P1_IPCODE { get; set; }
        public string SC_P2_IPCODE { get; set; }
    }

    public class EDITPANEL
    {
        public string EP_CMP_TYPE { get; set; }
    }

    public class TLP
    {
        public TLP()
        {
            TLP_COLUMNS = new List<TLP_COLUMNS>();
            TLP_ROWS = new List<TLP_ROWS>();
        }
        public string TLP_BORDER { get; set; }
        public List<TLP_COLUMNS> TLP_COLUMNS { get; set; }
        public List<TLP_ROWS> TLP_ROWS { get; set; }
        public string TLP_COLNO { get; set; }
        public string TLP_ROWNO { get; set; }
        public string TLP_COLSPAN { get; set; }
        public string TLP_ROWSPAN { get; set; }
    }

    public class TLP_COLUMNS
    {
        public string TLPC_CAPTION { get; set; }
        public string TLPC_SIZETYPE { get; set; }
        public string TLPC_SIZEVALUE { get; set; }
    }

    public class TLP_ROWS
    {
        public string TLPR_CAPTION { get; set; }
        public string TLPR_SIZETYPE { get; set; }
        public string TLPR_SIZEVALUE { get; set; }
    }

    public class TABCONTROL
    {
        public string TABHEADER { get; set; }
    }


    #endregion

    #region PROP EXPRESSION
    public class PROP_EXPRESSION
    {
        public string CAPTION { get; set; }
        public string EXP_TYPE { get; set; }  // 
        public string EXP_VALUE { get; set; }
        public string EXTRA_FNAME { get; set; }
        public string FOCUS_FIELD { get; set; }

        public string CHC_IPCODE { get; set; } // FIRST 
        public string CHC_FNAME { get; set; }
        public string CHC_VALUE { get; set; }
        public string CHC_OPERAND { get; set; }

        public string CHC_IPCODE_SEC { get; set; } // SECOND
        public string CHC_FNAME_SEC { get; set; }
        public string CHC_VALUE_SEC { get; set; }
        public string CHC_OPERAND_SEC { get; set; }
    }
    #endregion PROP EXPRESSION

    #region itemsInfo (WebScraping)
    public class itemsInfoo
    {
        public itemsInfoo()
        {
            itemsInfo = new List<itemInfo_>();
        }
        public List<itemInfo_> itemsInfo { get; set; }
    }

    public class itemInfo_
    {
        public string value { get; set; }
        public string text { get; set; }
    }

    /*
   "itemsInfo":[
      {
         "value":"-1",
         "text":"Seçiniz!"
      },

    */
    #endregion


    /*
declare @tname varchar(50) 
declare @fname varchar(50)

set @tname = 'MS_TABLES'
set @fname = 'PROP_SUBVIEW'

Select 'public class ' + @fname + ' { '  

Select '   public string  ' + ROW_FIELDNAME + ' { get; set; } ' from MS_PROPERTIES

where ROW_TYPE = 2 
and TABLENAME = @tname
and FIELDNAME = @fname

order by ROW_LINE_NO

Select '     } '

    */

}
