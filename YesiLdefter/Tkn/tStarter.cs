using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tkn_ToolBox;
using Tkn_UserFirms;
using Tkn_Variable;
using YesiLdefter;

namespace Tkn_Starter
{
    public class tStarter : tToolBox
    {
        // NOTE(@Janberk): Flag to suppress connection warning during API handoff
        private bool suppressManagerConnWarning = false;
        
        public void InitStart()
        {
            Application.DoEvents();

            tToolBox t = new tToolBox();

            /*
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            
            string version = fileVersionInfo.ProductVersion;
            MessageBox.Show(fileVersionInfo.ToString());
            MessageBox.Show(
                fileVersionInfo.ProductMajorPart.ToString() + "/" +
                fileVersionInfo.ProductMinorPart.ToString() + "/" +
                fileVersionInfo.ProductBuildPart.ToString() + "/" +
                fileVersionInfo.ProductPrivatePart.ToString());
            
            string w_file = fileVersionInfo.FileName.ToString(); //"MyProgram.exe";
            string w_directory = Directory.GetCurrentDirectory();
            */

            v.EXE_PATH = Path.GetDirectoryName(Application.ExecutablePath);
            v.tExeAbout.activeExeName = Application.ProductName + ".exe";
            v.tExeAbout.activePath = Application.StartupPath;

            v.EXE_TempPath = v.EXE_DRIVE + "UstadYazilim\\Temp";
            v.EXE_ScriptsPath = v.EXE_DRIVE + "UstadYazilim\\Scripts";
            v.EXE_FastReportsPath = v.EXE_PATH + "\\ReportsFast\\";
            v.EXE_DevExReportsPath = v.EXE_PATH + "\\ReportsDevEx\\";
            v.EXE_GIBDownloadPath = v.EXE_PATH + "\\GIBDownload\\";
            //MakeFolderWritable(v.EXE_PATH);

            System.IO.Directory.CreateDirectory(v.EXE_TempPath);
            System.IO.Directory.CreateDirectory(v.EXE_ScriptsPath);
            System.IO.Directory.CreateDirectory(v.EXE_FastReportsPath);
            System.IO.Directory.CreateDirectory(v.EXE_DevExReportsPath);
            System.IO.Directory.CreateDirectory(v.EXE_GIBDownloadPath);
            System.IO.Directory.CreateDirectory(v.EXE_GIBDownloadPath+"\\Temp\\");


            // output = { 20190325_2259 }
            // output : { 25.03.2019 22:59:22 }
            DateTime dt = File.GetLastWriteTime(System.IO.Path.Combine(v.tExeAbout.activePath, v.tExeAbout.activeExeName));
            // get : yyyymmdd_hhmm
            v.tExeAbout.activeVersionNo = t.getDateTimeString(dt);

            //var versionInfo = FileVersionInfo.GetVersionInfo(v.tExeAbout.activePath +"\\"+ v.tExeAbout.activeExeName);
            //string version = versionInfo.FileVersion;
            
            System.Globalization.CultureInfo tr = new System.Globalization.CultureInfo("tr-TR");
            System.Threading.Thread.CurrentThread.CurrentCulture = tr;


            /*
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            ci.NumberFormat.CurrencySymbol = "tkn";
            ci.NumberFormat.CurrencyDecimalDigits = 2;
            ci.NumberFormat.CurrencyDecimalSeparator = ",";
            ci.NumberFormat.CurrencyGroupSeparator = ".";
            
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            */

            // YesiLdefter.Ini
            // YesiLdefterConnection.Ini
            //
            ms_WebViewSplash.UpdateStatus("Ini dosyalar okunuyor...");
            t.ftpDownloadIniFile();

            // NOTE(@Janberk): Ensures API base URL and JWT key are set in registry if not already configured
            Tkn_UstadAPI.tApiConfig.InitializeDefaults();

            //Version clrVersion = Environment.Version;
            //string appVersion = Application.ProductVersion;
/*
            /// Computer hakkındaki verileri topla
            /// 
            t.WaitFormOpen(v.mainForm, "Bilgisayar hakkındaki bilgiler okunuyor...");
            //Task task1 = new Task(() =>
            //{
                Get_MacAddress();
            //});
            //task1.Start();
            //Task task2 = new Task(() =>
            //{
                Get_ComputerAbout();
            //});
            //task2.Start();
*/
            // 1. SECURE AUTHENTICATION FLOW: Authenticate user FIRST before any database connections
            // NOTE(@Janberk): Authentication happens via API. The login form (ms_User) uses checkedInputApi() which calls /auth/login endpoint. ms_User_Standalone is used for WebView2-based login with LoginTemplate.html
            ms_WebViewSplash.UpdateStatus("Kullanıcı Girişi...");
            if (v.active_DB.localDbUses == false)
            {
                // NOTE(@Janberk): Ustad YesiLdester user girişi - NO DB CONNECTION NEEDED
                InitLoginUser(); 
            }
            else
            {
                /// exe ilk çalıştığında [] args ile userId / ExternalUserId ile çalıştırılabilir
                if (v.tUser.UserId != 0)
                {
                    /// Kullanıcı hakkındaki bilgileri oku
                    t.getUserInfo();
                }
                if ((t.IsNotNull(v.tUser.UserFirmGUID) == false) ||
                    (t.IsNotNull(v.tUser.MebbisCode) == false))
                {
                    /// Exe yeni bir local database üzerinde ilk defa açıldığında 
                    /// vaya kullanıcının MebbisCode si yok ise
                    /// veya exe manuel olarak direkt olarak çalıştırılmış ise
                    /// Sırayla yapılanlar    
                    /// ms_TabimMtsk formunu aç
                    /// MsSql connection için gereken bilgileri toparla
                    /// Toparlanan bilgileri YesiLdefterTabim.ini file yaz
                    /// Local db de DbUpdates tablosunu oluştur ve update leri uygula
                    /// User listesini getir ve kullanıcı girişini sağla
                    /// 
                    InitTabimLoginUser();
                }
                else
                {
                    /// ExternalUserId ile açılış
                    /// 
                    tUserFirms userFirms = new tUserFirms();
                    userFirms.getFirmAboutWithUserFirmGUID(v.tMainFirm.FirmGuid);
                }
            }
            if (v.SP_ApplicationExit)
            {
                //Application.Exit();
                return;
            }
            
            // 2. SECURE AUTHENTICATION FLOW: After successful authentication, get database connection info from API and establish database connections.
            // NOTE(@Janberk): Database connections are established ONLY after user authentication. Connection strings are retrieved from API and decrypted using JWT key.
            if (v.SP_UserLOGIN == true && v.active_DB.localDbUses == false)
            {
                ms_WebViewSplash.UpdateStatus("Database bağlantı bilgileri API'den alınıyor...");
                bool dbConnectionsEstablished = InitPreparingConnectionFromApi();
                if (!dbConnectionsEstablished)
                {
                    MessageBox.Show("Database bağlantı bilgileri alınamadı. Lütfen sistem yöneticinize başvurun.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_ApplicationExit = true;
                    ms_WebViewSplash.CloseSplash();
                    return;
                }
                if (v.active_DB.managerMSSQLConn == null)
                {
                    MessageBox.Show("Database bağlantısı başlatılamadı. Lütfen sistem yöneticinize başvurun.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_ApplicationExit = true;
                    ms_WebViewSplash.CloseSplash();
                    return;
                }
                
                ms_WebViewSplash.UpdateStatus("ManagerDB bağlantısı gerçekleşiyor...");
                bool dbOpened = Db_Open(v.active_DB.managerMSSQLConn);
                if (!dbOpened || v.active_DB.managerMSSQLConn.State != System.Data.ConnectionState.Open)
                {
                    MessageBox.Show("Database bağlantısı açılamadı. Lütfen sistem yöneticinize başvurun.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_ApplicationExit = true;
                    ms_WebViewSplash.CloseSplash();
                    return;
                }
                System.Diagnostics.Debug.WriteLine($"✓ ManagerDB connection verified: State={v.active_DB.managerMSSQLConn.State}, Database={v.active_DB.managerDBName}");
            }
            else if (v.active_DB.localDbUses == true)
            {
                ms_WebViewSplash.UpdateStatus("Database bağlantı bilgileri hazırlanıyor...");
                InitPreparingConnection();
                
                if (v.active_DB.managerMSSQLConn == null)
                {
                    MessageBox.Show("Database bağlantısı başlatılamadı. Lütfen sistem yöneticinize başvurun.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_ApplicationExit = true;
                    ms_WebViewSplash.CloseSplash();
                    return;
                }
                
                ms_WebViewSplash.UpdateStatus("ManagerDB bağlantısı gerçekleşiyor...");
                bool dbOpened = Db_Open(v.active_DB.managerMSSQLConn);
                if (!dbOpened || v.active_DB.managerMSSQLConn.State != System.Data.ConnectionState.Open)
                {
                    MessageBox.Show("Database bağlantısı açılamadı. Lütfen sistem yöneticinize başvurun.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_ApplicationExit = true;
                    ms_WebViewSplash.CloseSplash();
                    return;
                }
                System.Diagnostics.Debug.WriteLine($"✓ ManagerDB connection verified: State={v.active_DB.managerMSSQLConn.State}, Database={v.active_DB.managerDBName}");
            }

            /// Mesaj formu nedense kayboluyor
            /// onun açılması için burada bunlar false yapılıyor
            v.IsWaitOpen = false;
            v.SP_OpenApplication = false;
            ms_WebViewSplash.UpdateStatus("İşlemler devam ediyor...");

            ms_WebViewSplash.UpdateStatus("Kullanıcı teması hazırlanıyor...");
            setLoginSkins();

            ms_WebViewSplash.UpdateStatus("Bilgisayar hakkındaki bilgi sorgulaması...");
            InitLoginComputer();

            ms_WebViewSplash.UpdateStatus("Ekran çözünürlüğünün tespiti...");
            Screen_Sizes_Get();

            // önce yeni dosya varsa onla download olması gerekiyor
            ms_WebViewSplash.UpdateStatus("FileUpdates işlemleri yapılıyor...");
            t.read_MsFileUpdates();

            ms_WebViewSplash.UpdateStatus("Data Updates işlemleri yapılıyor...");
            t.dataUpdates();

            // dosyalardan son yeni exenin download olması gerekiyor
            ms_WebViewSplash.UpdateStatus("Exe güncelleme kontrolü yapılıyor...");
            t.read_MsExeUpdates(v.SP_tUserType);

            ms_WebViewSplash.UpdateStatus("Sistem tarihleri okunuyor, hazırlanıyor...");
            t.MSSQL_Server_Tarihi();
            //t.DonemTipiYilAyRead();

            // Settings table
            ms_WebViewSplash.UpdateStatus("Settings okunuyor...");
            t.read_Settings();


            // 3S_MSGLY 
            ms_WebViewSplash.UpdateStatus("Images okunuyor...");
            //t.SYS_Glyph_Read();

            //t.TestRead();

            // 3. NOTE(@Janberk): if v.SP_UserIN = true, then initialization is complete. The main form will call tLayout.Create_Layout(), which triggers the dashboard rendering pipeline.
            // TODO(@Janberk): Extract initialization stages into Ustad.API and the endpoints called here:
            // - InitPreparingConnection()
            // - InitLoginUser()
            // - InitLoginComputer()
            // - Screen_Sizes_Get()
            // - read_MsFileUpdates()
            // - dataUpdates()
            // - read_MsExeUpdates()
            // - MSSQL_Server_Tarihi()
            // - read_Settings()
            // - SYS_Glyph_Read()
            // - TestRead()
            
            // Close splash screen after initialization completes
            ms_WebViewSplash.CloseSplash();
            v.SP_UserIN = true;
        }

        void setLoginSkins()
        {
            #region appOpenSetDefaaultSkin
            v.sp_activeSkinName = "STARTER";
            
            WindowsFormsSettings.EnableFormSkins();

            if (v.active_DB.mainManagerDbUses)
                UserLookAndFeel.Default.SetSkinStyle(SkinStyle.Whiteprint);
            else
                UserLookAndFeel.Default.SetSkinStyle(SkinSvgPalette.Office2019White.Default);//  Yale);
            v.sp_activeSkinName = "";
            #endregion
            
        }

        #region Variable Set

        /// <summary>
        /// SECURE AUTHENTICATION FLOW: Initialize database connections from API after authentication
        /// </summary>
        /// <returns>True if connections were successfully established, false otherwise</returns>
        bool InitPreparingConnectionFromApi()
        {
            try
            {
                suppressManagerConnWarning = true;
                v.SP_ConnBool_Manager = false;
                v.SP_ConnBool_Manager_Old = false;
                // Close legacy connections opened for layout rendering before rebuilding from API.
                try { v.active_DB.managerMSSQLConn?.Dispose(); } catch { }
                try { v.active_DB.ustadCrmMSSQLConn?.Dispose(); } catch { }
                /// Get API base URL and JWT key from registry configuration
                string apiBaseUrl = Tkn_UstadAPI.tApiConfig.GetApiBaseUrl();
                string jwtKey = Tkn_UstadAPI.tApiConfig.GetJwtKey();
                
                using (var apiClient = new Tkn_UstadAPI.UstadApiClient(apiBaseUrl))
                {
                    string authToken = GetStoredAuthToken();
                    if (string.IsNullOrEmpty(authToken))
                    {
                        System.Diagnostics.Debug.WriteLine("No authentication token found. User must login first.");
                        return false;
                    }
                    
                    apiClient.SetAuthToken(authToken);
                    
                    // Get database connection info from API (synchronous call using .Result)
                    // NOTE(@Janberk): Using .Result here because this is called from synchronous InitStart() method
                    var dbInfoTask = apiClient.GetDatabaseConnectionInfoAsync(jwtKey);
                    dbInfoTask.Wait(); 
                    var dbInfo = dbInfoTask.Result;
                    if (dbInfo == null || 
                        string.IsNullOrEmpty(dbInfo.UstadCrmConnectionString) ||
                        string.IsNullOrEmpty(dbInfo.ManagerConnectionString))
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to get database connection info from API.");
                        System.Diagnostics.Debug.WriteLine($"UstadCrmConnectionString: {(dbInfo?.UstadCrmConnectionString != null ? "present" : "null")}");
                        System.Diagnostics.Debug.WriteLine($"ManagerConnectionString: {(dbInfo?.ManagerConnectionString != null ? "present" : "null")}");
                        return false;
                    }
                    // Set up connection strings from API response
                    v.active_DB.managerDBType = v.dBaseType.MSSQL;
                    v.active_DB.ustadCrmDBType = v.dBaseType.MSSQL;
                    v.active_DB.projectDBType = v.dBaseType.MSSQL;
                    ParseConnectionStringFromApi(dbInfo.UstadCrmConnectionString, true); 
                    ParseConnectionStringFromApi(dbInfo.ManagerConnectionString, false);
                    
                    // Verify that manager connection was actually created
                    if (v.active_DB.managerMSSQLConn == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Error: managerMSSQLConn was not created after ParseConnectionStringFromApi.");
                        return false;
                    }
                    
                    // Initialize publishManager_DB connection (uses same connection as manager)
                    // NOTE(@Janberk): publishManager_DB is a copy of manager connection for publishing purposes
                    v.publishManager_DB.dBaseNo = v.dBaseNo.publishManager;
                    v.publishManager_DB.userName = v.active_DB.managerUserName;
                    v.publishManager_DB.serverName = v.active_DB.managerServerName;
                    v.publishManager_DB.databaseName = v.active_DB.managerDBName;
                    v.publishManager_DB.connectionText = v.active_DB.managerConnectionText;
                    v.publishManager_DB.MSSQLConn = new SqlConnection(v.publishManager_DB.connectionText);
                    v.publishManager_DB.MSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting DB connection info from API: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
            finally
            {
                suppressManagerConnWarning = false;
            }
        }
        
        /// <summary>
        /// Get stored authentication token from user context
        /// NOTE(@Janberk): Token is stored in v.tUser.JwtToken after successful login in ms_User form
        /// </summary>
        string GetStoredAuthToken()
        {
            return v.tUser.JwtToken ?? string.Empty;
        }
        
        /// <summary>
        /// Parse connection string from API response and set up database connection objects
        /// </summary>
        void ParseConnectionStringFromApi(string connectionString, bool isUstadCrm)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                string dbType = isUstadCrm ? "UstadCrm" : "Manager";
                throw new ArgumentException($"{dbType} connection string is null or empty.");
            }
            
            try
            {
                var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                
                if (isUstadCrm)
                {
                    v.active_DB.ustadCrmServerName = builder.DataSource.Split(',')[0];
                    v.active_DB.ustadCrmDBName = builder.InitialCatalog;
                    v.active_DB.ustadCrmUserName = builder.UserID;
                    v.active_DB.ustadCrmConnectionText = connectionString;
                    v.active_DB.ustadCrmMSSQLConn = new SqlConnection(v.active_DB.ustadCrmConnectionText);
                    v.active_DB.ustadCrmMSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
                }
                else
                {
                    v.active_DB.managerServerName = builder.DataSource.Split(',')[0];
                    v.active_DB.managerDBName = builder.InitialCatalog;
                    v.active_DB.managerUserName = builder.UserID;
                    v.active_DB.managerConnectionText = connectionString;
                    v.active_DB.managerMSSQLConn = new SqlConnection(v.active_DB.managerConnectionText);
                    v.active_DB.managerMSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing connection string: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// LEGACY METHOD: Initialize database connections with hardcoded passwords
        /// NOTE(@Janberk): This method is kept for local DB mode (Tabim) and backward compatibility.
        /// For secure mode, use InitPreparingConnectionFromApi() instead.
        /// TODO(@Janberk): Remove hardcoded password references when all modes use API.
        /// </summary>
        void InitPreparingConnection() 
        {
            ///
            /// ------------------------------------------------
            ///
            /// hangi database hangi databaseServer de çalışıyor  
            /// şimdilik manuel set ediyorum
            /// 
            v.active_DB.managerDBType = v.dBaseType.MSSQL;
            v.active_DB.ustadCrmDBType = v.dBaseType.MSSQL;
            v.active_DB.projectDBType = v.dBaseType.MSSQL;
                        
            v.active_DB.managerUserName = Environment.GetEnvironmentVariable("USTAD_MANAGER_DB_USER");
            v.active_DB.managerServerName = Environment.GetEnvironmentVariable("USTAD_MANAGER_DB_SERVER");
            v.active_DB.managerDBName = Environment.GetEnvironmentVariable("USTAD_MANAGER_DB_NAME");
            string managerPass = Environment.GetEnvironmentVariable("USTAD_MANAGER_DB_PASS");
            
            // Validate required environment variables
            if (string.IsNullOrWhiteSpace(v.active_DB.managerUserName) ||
                string.IsNullOrWhiteSpace(v.active_DB.managerServerName) ||
                string.IsNullOrWhiteSpace(v.active_DB.managerDBName) ||
                string.IsNullOrWhiteSpace(managerPass))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Database environment variables not set. Connection strings will not be initialized.");
                System.Diagnostics.Debug.WriteLine($"USTAD_MANAGER_DB_USER: {v.active_DB.managerUserName ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"USTAD_MANAGER_DB_SERVER: {v.active_DB.managerServerName ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"USTAD_MANAGER_DB_NAME: {v.active_DB.managerDBName ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"USTAD_MANAGER_DB_PASS: {(string.IsNullOrWhiteSpace(managerPass) ? "NULL" : "***")}");
                return; // Exit early to prevent building invalid connection strings
            }
            
            v.active_DB.managerPsw = "Password = " + managerPass + ";";
                        
            ///
            /// main Manager DB Connections
            /// 
            #region
            
            // Connection strings are already validated above, safe to build
            v.active_DB.managerConnectionText =
                string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                v.active_DB.managerServerName,
                v.active_DB.managerDBName,
                v.active_DB.managerUserName,
                v.active_DB.managerPsw);
            v.active_DB.managerMSSQLConn = new SqlConnection(v.active_DB.managerConnectionText);
            v.active_DB.managerMSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
            #endregion

            ///
            /// publish Manager DB Connections
            /// 
            #region
            v.publishManager_DB.dBaseNo = v.dBaseNo.publishManager;
            // Use already validated values from manager connection
            v.publishManager_DB.userName = v.active_DB.managerUserName;
            v.publishManager_DB.serverName = v.active_DB.managerServerName;
            v.publishManager_DB.databaseName = v.active_DB.managerDBName;
            v.publishManager_DB.psw = v.active_DB.managerPsw;
            v.publishManager_DB.connectionText =
                string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                v.publishManager_DB.serverName,
                v.publishManager_DB.databaseName,
                v.publishManager_DB.userName,
                v.publishManager_DB.psw);

            v.publishManager_DB.MSSQLConn = new SqlConnection(v.publishManager_DB.connectionText);
            v.publishManager_DB.MSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
            #endregion

            ///
            /// UstadCRM DB Connections
            /// 
            #region

            //v.active_DB.ustadCrmDBName = "UstadCRM";
            v.active_DB.ustadCrmUserName = Environment.GetEnvironmentVariable("USTAD_CRM_DB_USER");
            v.active_DB.ustadCrmServerName = Environment.GetEnvironmentVariable("USTAD_CRM_DB_SERVER");
            v.active_DB.ustadCrmDBName = Environment.GetEnvironmentVariable("USTAD_CRM_DB_NAME");
            string crmPass = Environment.GetEnvironmentVariable("USTAD_CRM_DB_PASS");
            
            // Validate CRM environment variables (optional - may not be needed for all modes)
            if (!string.IsNullOrWhiteSpace(v.active_DB.ustadCrmServerName) &&
                !string.IsNullOrWhiteSpace(v.active_DB.ustadCrmDBName) &&
                !string.IsNullOrWhiteSpace(v.active_DB.ustadCrmUserName) &&
                !string.IsNullOrWhiteSpace(crmPass))
            {
                v.active_DB.ustadCrmPsw = "Password = " + crmPass + ";";
                v.active_DB.ustadCrmConnectionText =
                    string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                    v.active_DB.ustadCrmServerName,
                    v.active_DB.ustadCrmDBName,
                    v.active_DB.ustadCrmUserName,
                    v.active_DB.ustadCrmPsw);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Warning: CRM database environment variables not set. CRM connection will not be initialized.");
                // Set empty connection text to prevent null reference
                v.active_DB.ustadCrmConnectionText = "";
            }

            // Only create connection if connection text is valid
            if (!string.IsNullOrWhiteSpace(v.active_DB.ustadCrmConnectionText))
            {
                v.active_DB.ustadCrmMSSQLConn = new SqlConnection(v.active_DB.ustadCrmConnectionText);
                v.active_DB.ustadCrmMSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
            }
            
            #endregion

            ///
            /// master DB Connections (MSSQL.master)
            /// 
            #region

            v.active_DB.masterDBName = "master";
            if (IsNotNull(v.active_DB.masterUserName) == false)
                v.active_DB.masterUserName = "sa";
            
            // Use manager server and password for master connection if not explicitly set
            if (string.IsNullOrWhiteSpace(v.active_DB.masterServerName))
                v.active_DB.masterServerName = v.active_DB.managerServerName;
            if (string.IsNullOrWhiteSpace(v.active_DB.masterPsw))
                v.active_DB.masterPsw = v.active_DB.managerPsw;
            
            // Validate master connection parameters
            if (string.IsNullOrWhiteSpace(v.active_DB.masterServerName) ||
                string.IsNullOrWhiteSpace(v.active_DB.masterPsw))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Master database connection parameters not set. Master connection will not be initialized.");
                v.active_DB.masterConnectionText = "";
            }
            else
            {
                v.active_DB.masterConnectionText =
                    string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                    v.active_DB.masterServerName,
                    v.active_DB.masterDBName,
                    v.active_DB.masterUserName,
                    v.active_DB.masterPsw);
            }

            // Only create connection if connection text is valid
            if (!string.IsNullOrWhiteSpace(v.active_DB.masterConnectionText))
            {
                v.active_DB.masterMSSQLConn = new SqlConnection(v.active_DB.masterConnectionText);
                v.active_DB.masterMSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
            }

            #endregion

            // DİKKAT : BU METODU KULLANMA MASTER-DETAIL de DETAIL kırılıyor
            // v.SP_Conn_Text_Manager_MSSQL = " Server=94.73.145.8; Database=MSV3DFTRBLT; Uid=user4601;Pwd=CanBerk98";

        }

        void InitLoginComputer()
        {
            //MessageBox.Show(v.tComputer.Network_MACAddress);

            /// burada computer hakkında bilgi toplanıyor
            /// computer hakkındaki bilgi merkez datada bulunmakta (MVS3..)
            /// her computer network ethernet macaddresiyle takip edilmekte
            /// (MSV3..) datada computer bilgisi yoksa buradan 
            /// computer register formu açılmakta.
            /// Compter hakkında toplanan bilgiler ekranda müdehale edilemeyecek durumdadır
            /// Kullanıcıdan, hangi firma için kullanacak ise firm_guid istenmektedir
            /// eğer firm_guid yok ise sadece test firmalarını görebilir
            /// Firm_Guid aldığında da bu computer bilgileri sayesinde 
            /// firma için kayıt olan computer sayısı / lisans tespit edilmiş olacak
                        
            string networkKey = v.tComputer.Network_MACAddress;
            string pcName = v.tComputer.PcName;

            /* test için
            networkKey = null;
            pcName = "VIRA-2PC";

            v.tMainFirm.FirmId = 116;
            v.tUser.UserFirmGUID = "aab68ddf-1c4c-49e6-a860-80bbd558d945";

            v.tComputer.PcName = pcName;
            v.tComputer.Network_MACAddress = null;
            v.tComputer.Processor_Name = null;
            v.tComputer.Processor_Id = null;
            v.tComputer.DiskDrive_Model = null;
            v.tComputer.DiskDrive_SerialNumber = null;
            if (IsNotNull(networkKey) == false) networkKey = "";
            if (IsNotNull(pcName) == false) pcName = "";
            */

            /// FirmGUID
            /// NetworkMacAddress
            /// SystemName
            string tSql = "";

            tSql = @" Select * from UstadComputers where ( isnull(NetworkMacAddress,'') = '" + networkKey + "' and isnull(SystemName,'') = '" + pcName + "' ) ";

            SQL_Read_Execute(v.dBaseNo.UstadCrm, v.ds_Computer, ref tSql, "UstadComputers", "InitLoginComputer");

            if (IsNotNull(v.ds_Computer))
            {
                // Birden fazla computer kaydı var ise 
                if (v.ds_Computer.Tables[0].Rows.Count > 1)
                {
                    string delete_sql = " Delete from UstadComputers where ( isnull(NetworkMacAddress,'') = '" + networkKey + "' and isnull(SystemName,'') = '" + pcName + "' ) ";

                    DataSet ds_ = new DataSet();
                    SQL_Read_Execute(v.dBaseNo.UstadCrm, ds_, ref delete_sql, "UstadComputers", "Delete");

                    // computer bilgisini yeniden kaydet
                    InitRegisterComputer();

                    // yeni kaydedilen computer bilgisini oku
                    tSql = @" Select * from UstadComputers where ( isnull(NetworkMacAddress,'') = '" + networkKey + "' and isnull(SystemName,'') = '" + pcName + "' ) ";
                    SQL_Read_Execute(v.dBaseNo.UstadCrm, v.ds_Computer, ref tSql, "UstadComputers", "InitLoginComputer");

                }

                /// yeniden okunduğu için tekrar kontrol
                if (IsNotNull(v.ds_Computer))
                    v.tComputer.UstadCrmComputerId = Convert.ToInt32(v.ds_Computer.Tables[0].Rows[0]["ComputerId"].ToString());

                /// Bazı Computer bilgileri güncelleniyor
                /// FirmId
                /// FirmGUID 
                /// LastDate
                /// OperatingSystem
                /// ExeVersion
                if (v.ds_Computer.Tables[0].Rows.Count == 1)
                {
                    tSql = " Update UstadComputers set "
                    + "   FirmId = "+ v.tMainFirm.FirmId.ToString() 
                    + " , FirmGUID = '"+ v.tUser.UserFirmGUID + "' "
                    + " , LastDate = " + TarihSaat_Formati(Convert.ToDateTime(DateTime.Now)) // v.TARIH_SAAT
                    + " , OperatingSystem = '" + v.tComputer.OperatingSystem + "' "
                    //+ " , ExeVersion = '" + v.tExeAbout.activeVersionNo.Substring(0, 8) + "' "
                    + " , ExeVersion = '20250905_standart' "
                    + " where ComputerId = " + v.tComputer.UstadCrmComputerId.ToString();
                    DataSet ds_ = new DataSet();
                    SQL_Read_Execute(v.dBaseNo.UstadCrm, ds_, ref tSql, "UstadComputers", "Update");
                }
            }
            else
            {
                // Hiç kaydı yok ise
                InitRegisterComputer();
            }

            /*
            if (IsNotNull(v.ds_Computer))
            {
                /// computer için planlanmış firma guid bilgisi
                ///
                v.tComp.SP_COMP_ISACTIVE = Convert.ToBoolean(v.ds_Computer.Tables[0].Rows[0]["IsActive"].ToString());

                // Aktif ise
                if (v.tComp.SP_COMP_ISACTIVE)
                {
                    v.tComp.SP_COMP_ID = Convert.ToInt32(v.ds_Computer.Tables[0].Rows[0]["ComputerId"].ToString());
                    v.tComp.SP_COMP_FIRM_GUID = v.ds_Computer.Tables[0].Rows[0]["FirmGUID"].ToString();
                    v.tComp.SP_COMP_SYSTEM_NAME = v.ds_Computer.Tables[0].Rows[0]["SystemName"].ToString();
                    v.tComp.SP_COMP_MACADDRESS = v.ds_Computer.Tables[0].Rows[0]["NetworkMacAddress"].ToString();
                    v.tComp.SP_COMP_PROCESSOR_ID = v.ds_Computer.Tables[0].Rows[0]["ProcessorId"].ToString();
                }

                // Pasif ise
                if (v.tComp.SP_COMP_ISACTIVE == false)
                {
                    MessageBox.Show("Bilgisayarınız PASİF durumda. \r\n\r\n Destek ekibini arayarak bilgisayarınız AKTİF ettirebilirsiniz ...", "DİKKAT : ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                / *
                // diğer modlarda ise
                if (v.tComp.SP_COMP_ISACTIVE)
                {
                    MessageBox.Show("Bilgisayarınız  ( IsAcvite : " + v.tComp.SP_COMP_ISACTIVE.ToString() + " ) durumda.", "DİKKAT : ", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                * /
            }
            else
            {
                /// computer register için form
                InitRegisterComputer();
            }
            */
        }

        void Screen_Sizes_Get()
        {
            v.Screen_Width = Screen.PrimaryScreen.Bounds.Width - (20 + v.NavBar_Width);
            v.Screen_Height = Screen.PrimaryScreen.Bounds.Height - (90 + v.Ribbon_Height);
            v.Primary_Screen_Width = Screen.PrimaryScreen.Bounds.Width;
            v.Primary_Screen_Height = Screen.PrimaryScreen.Bounds.Height - 50;
        }

        #endregion Variable Set

        #region InitRegisterComputer
        void InitRegisterComputer()
        {
            /// Computer hakkındaki verileri topla
            /// 
            //Get_ComputerAbout();
            /// Computeri, merkezdeki db ye (MSV3..) kaydedecek formu aç
            ///
            string FormName = "ms_Computer";
            string FormCode = "UST/CRM/ABO/Computer";
            OpenFormPreparing(FormName, FormCode, v.formType.Dialog);
        }
        #endregion InitRegisterComputer

        #region InitLoginUser, InitTabimLoginUser
        // 3. NOTE(@Janberk): InitLoginUser() opens the login form (ms_User) as a modal dialog.
        // FormCode "UST/CRM/ABO/UstadUserLogin" is used by tLayout.Create_Layout() to load the form's layout metadata
        // from MS_LAYOUT table. If user authenticates, the form closes and v.SP_UserLOGIN is set to true.
        void InitLoginUser()
        {
            YesiLdefter.ms_User_Standalone loginForm = null;
            try
            {
                loginForm = new YesiLdefter.ms_User_Standalone();
                loginForm.ShowDialog(v.mainForm);
                loginForm.Dispose();
            }
            catch (Exception ex)
            {
                try
                {
                    loginForm?.Dispose();
                    v.SP_ApplicationExit = true;
                }
                catch { }
                MessageBox.Show(
                    $"Giriş formu açılırken hata oluştu:\n{ex.Message}\n\nDetaylar için debug çıktısını kontrol edin.\n\nLütfen sistem yöneticinize başvurun.",
                    "Giriş Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens legacy login form (uses database-dependent layout)
        /// Only used for Tabim local database mode
        /// </summary>
        void InitLoginUserLegacy()
        {
            // LEGACY: This method uses database-dependent form layouts
            // Only used for local database mode (Tabim)
            string FormName = "ms_User";
            string FormCode = "UST/CRM/ABO/UstadUserLogin";
            OpenFormPreparing(FormName, FormCode, v.formType.Dialog);
        }

        void InitTabimLoginUser()
        {
            string FormName = "ms_TabimMtsk";
            string FormCode = "UST/MEB/TB1/TbmWelcome";
            OpenFormPreparing(FormName, FormCode, v.formType.Dialog);
        }
        #endregion InitLoginUser
    }
}
