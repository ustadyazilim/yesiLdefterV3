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

namespace Tkn_Starter
{
    public class tStarter : tToolBox
    {
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
            // NOTE(@Janberk): INI read hydrates legacy DB endpoints (manager/CRM/publish/local). 
            // This is the only source before API handoff.
            //
            t.WaitFormOpen(v.mainForm, "Ini dosyalar okunuyor...");
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
            // NOTE(@Janberk): Authentication happens via API - no database connection required for login.
            t.WaitFormOpen(v.mainForm, "Kullanıcı Girişi...");
            if (v.active_DB.localDbUses == false)
            {
                //InitLoginUser(); // WebView2-based login with LoginTemplate.html

                InitPreparingConnection();// mecburen burada connection hazırlanıyor
                InitLoginUserLegacy(); // Legacy form - not used when WebView2 is available
            }
            else
            {
                /// exe ilk çalıştığında [] args ile userId / ExternalUserId ile çalıştırılabilir
                /// 
                if (v.tUser.UserId != 0)
                {
                    /// Kullanıcı hakkındaki bilgileri oku
                    /// 
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

            // 2. SECURE AUTHENTICATION FLOW: After successful authentication, get database connection info from API
            // NOTE(@Janberk): Database connections are established ONLY after user authentication.
            System.Diagnostics.Debug.WriteLine($"[tStarter.InitStart] SP_UserLOGIN={v.SP_UserLOGIN}, localDbUses={v.active_DB.localDbUses}, managerMSSQLConn={(v.active_DB.managerMSSQLConn != null ? "exists" : "null")}");
            if (v.SP_UserLOGIN == true && v.active_DB.localDbUses == false)
            {
                t.WaitFormOpen(v.mainForm, "Database bağlantı bilgileri API'den alınıyor...");
                System.Diagnostics.Debug.WriteLine("[tStarter.InitStart] Calling InitPreparingConnectionFromApi()...");
                bool dbConnectionsEstablished = InitPreparingConnectionFromApi();
                System.Diagnostics.Debug.WriteLine($"[tStarter.InitStart] InitPreparingConnectionFromApi() returned: {dbConnectionsEstablished}");
                if (!dbConnectionsEstablished)
                {
                    MessageBox.Show("Database bağlantı bilgileri alınamadı. Lütfen sistem yöneticinize başvurun.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_ApplicationExit = true;
                    return;
                }
                if (v.active_DB.managerMSSQLConn != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[tStarter.InitStart] managerMSSQLConn exists, State={v.active_DB.managerMSSQLConn.State}, ConnectionString length={v.active_DB.managerMSSQLConn.ConnectionString?.Length ?? 0}");
                    t.WaitFormOpen(v.mainForm, "ManagerDB bağlantısı gerçekleşiyor...");
                    bool dbOpened = Db_Open(v.active_DB.managerMSSQLConn);
                    System.Diagnostics.Debug.WriteLine($"[tStarter.InitStart] Db_Open(managerMSSQLConn) returned: {dbOpened}, State={v.active_DB.managerMSSQLConn?.State}");
                    if (!dbOpened)
                    {
                        MessageBox.Show("ManagerDB bağlantısı açılamadı. Lütfen bağlantı bilgilerini kontrol edin.",
                            "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        v.SP_ApplicationExit = true;
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("ManagerDB bağlantı nesnesi oluşturulamadı. API'den bağlantı bilgileri alınamadı.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_ApplicationExit = true;
                    return;
                }
            }
            else if (v.active_DB.localDbUses == true)
            {
                // Local DB mode - use existing connection setup (for Tabim local database)
                t.WaitFormOpen(v.mainForm, "Database bağlantı bilgileri hazırlanıyor...");
                InitPreparingConnection();
                t.WaitFormOpen(v.mainForm, "ManagerDB bağlantısı gerçekleşiyor...");
                if (!Db_Open(v.active_DB.managerMSSQLConn))
                {
                    MessageBox.Show("ManagerDB bağlantısı açılamadı. Lütfen bağlantı bilgilerini kontrol edin.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_ApplicationExit = true;
                    return;
                }
            }
            else
            {
                // User did not login - should not reach here, but handle gracefully
                MessageBox.Show("Kullanıcı girişi yapılmadı. Lütfen tekrar deneyin.",
                    "Giriş Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                v.SP_ApplicationExit = true;
                return;
            }

            /// Mesaj formu nedense kayboluyor
            /// onun açılması için burada bunlar false yapılıyor
            v.IsWaitOpen = false;
            v.SP_OpenApplication = false;
            t.WaitFormOpen(v.mainForm, "İşlemler devam ediyor...");

            t.WaitFormOpen(v.mainForm, "Kullanıcı teması hazırlanıyor...");
            setLoginSkins();

            t.WaitFormOpen(v.mainForm, "Bilgisayar hakkındaki bilgi sorgulaması...");
            InitLoginComputer();

            t.WaitFormOpen(v.mainForm, "Ekran çözünürlüğünün tespiti...");
            Screen_Sizes_Get();

            // önce yeni dosya varsa onla download olması gerekiyor
            t.WaitFormOpen(v.mainForm, "FileUpdates işlemleri yapılıyor...");
            t.read_MsFileUpdates();

            t.WaitFormOpen(v.mainForm, "Data Updates işlemleri yapılıyor...");
            t.dataUpdates();

            // dosyalardan son yeni exenin download olması gerekiyor
            t.WaitFormOpen(v.mainForm, "Exe güncelleme kontrolü yapılıyor...");
            t.read_MsExeUpdates(v.SP_tUserType);

            t.WaitFormOpen(v.mainForm, "Sistem tarihleri okunuyor, hazırlanıyor...");
            t.MSSQL_Server_Tarihi();
            //t.DonemTipiYilAyRead();

            // Settings table
            t.WaitFormOpen(v.mainForm, "Settings read...");
            t.read_Settings();


            // 3S_MSGLY 
            t.WaitFormOpen(v.mainForm, "Images read...");
            Task task1 = new Task(() =>
            {
                t.SYS_Glyph_Read();
            });
            task1.Start();

            //t.TestRead();

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
        /// This method retrieves encrypted connection strings from API and decrypts them.
        /// </summary>
        /// <returns>True if connections were successfully established, false otherwise</returns>
        bool InitPreparingConnectionFromApi()
        {
            try
            {
                suppressManagerConnWarning = true;
                v.SP_ConnBool_Manager = false;
                v.SP_ConnBool_Manager_Old = false;
                try { v.active_DB.managerMSSQLConn?.Dispose(); } catch { }
                try { v.active_DB.ustadCrmMSSQLConn?.Dispose(); } catch { }
                string apiBaseUrl = Tkn_UstadAPI.tApiConfig.GetApiBaseUrl();
                string jwtKey = Tkn_UstadAPI.tApiConfig.GetJwtKey();
                using (var apiClient = new Tkn_UstadAPI.UstadApiClient(apiBaseUrl))
                {
                    // Get JWT token stored after successful login
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
                    
                    if (dbInfo == null)
                    {
                        System.Diagnostics.Debug.WriteLine("InitPreparingConnectionFromApi: dbInfo is null.");
                        return false;
                    }
                    bool hasCrm = !string.IsNullOrWhiteSpace(dbInfo.UstadCrmConnectionString);
                    bool hasMgr = !string.IsNullOrWhiteSpace(dbInfo.ManagerConnectionString);
                    if (!hasCrm || !hasMgr)
                    {
                        System.Diagnostics.Debug.WriteLine($"InitPreparingConnectionFromApi: Missing decrypted strings. EncryptedCRM len: {dbInfo.EncryptedUstadCrmConnectionString?.Length ?? 0}, EncryptedMgr len: {dbInfo.EncryptedManagerConnectionString?.Length ?? 0}");
                        return false;
                    }
                    v.active_DB.managerDBType = v.dBaseType.MSSQL;
                    v.active_DB.ustadCrmDBType = v.dBaseType.MSSQL;
                    v.active_DB.projectDBType = v.dBaseType.MSSQL;
                    ParseConnectionStringFromApi(dbInfo.UstadCrmConnectionString, true); 
                    ParseConnectionStringFromApi(dbInfo.ManagerConnectionString, false); 
                    if (v.active_DB.managerMSSQLConn == null || v.active_DB.ustadCrmMSSQLConn == null)
                    {
                        System.Diagnostics.Debug.WriteLine("InitPreparingConnectionFromApi: Connection objects were not created after parsing connection strings.");
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
                    
                    System.Diagnostics.Debug.WriteLine($"[tStarter.InitPreparingConnectionFromApi] Connections established: managerMSSQLConn={(v.active_DB.managerMSSQLConn != null ? "exists" : "null")}, ustadCrmMSSQLConn={(v.active_DB.ustadCrmMSSQLConn != null ? "exists" : "null")}, publishManager_MSSQLConn={(v.publishManager_DB.MSSQLConn != null ? "exists" : "null")}");
                    
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
            /// buradki set işlemleri yanlış
            try
            {
                var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                var dataSource = builder.DataSource?.Split(',')[0] ?? string.Empty;
                var initialCatalog = builder.InitialCatalog;
                var userId = builder.UserID;
                var conn = new SqlConnection(connectionString);
                conn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
                if (isUstadCrm)
                {
                    v.active_DB.ustadCrmServerName = dataSource;
                    v.active_DB.ustadCrmDBName = initialCatalog;
                    v.active_DB.ustadCrmUserName = userId;
                    v.active_DB.ustadCrmConnectionText = connectionString;
                    v.active_DB.ustadCrmMSSQLConn = conn;
                }
                else
                {
                    v.active_DB.managerServerName = dataSource;
                    v.active_DB.managerDBName = initialCatalog;
                    v.active_DB.managerUserName = userId;
                    v.active_DB.managerConnectionText = connectionString;
                    v.active_DB.managerMSSQLConn = conn;
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
        /// </summary>
        private void InitPreparingConnection() 
        {

            bool IsDebugMode = v.active_DB.mainManagerDbUses;   //false;// true;
            string _user = "sa";

            string _managerDbName = "MainManagerV3";
            string _managerDbPass = v.db_PASSWORD;
            string _managerServerName = v.db_SERVERIP;

            string _crmDbName = "UstadCRMV1";
            string _crmDbPass = v.db_PASSWORD;
            string _crmServerName = v.db_SERVERIP;

            string _publishDbName = "UstadManagerV3";
            string _publishDbPass = v.db_PASSWORD;
            string _publishServerName = v.db_SERVERIP;

            /// NOT : Project burada kullanılmıyor
            /// kullanıcı bir firma seçtiğinde hangi database  kullanılacak ise 
            /// o bilgi user&firms tablosundaki alınacak
            /// bu örnek : 
            string _projectDbName = "";
            _projectDbName = "Mts00000011";  // vaya
            _projectDbName = "Src00004204";  // 
            string _projectDbPass = v.db_PASSWORD; /// şimdillik ortak pass kullanılıyor

            ///
            /// ------------------------------------------------
            ///
            /// hangi database hangi databaseServer de çalışıyor  
            /// şimdilik manuel set ediyorum
            /// 
            v.active_DB.managerDBType = v.dBaseType.MSSQL;
            v.active_DB.ustadCrmDBType = v.dBaseType.MSSQL;
            v.active_DB.projectDBType = v.dBaseType.MSSQL;


            if (IsDebugMode)
            {
                /// birşey yapma     
            }
            else
            {
                _managerDbName = _publishDbName;
                _managerDbPass = _publishDbPass;
            }

            /// main Manager DB Connections
            #region
            v.active_DB.managerUserName = _user;
            v.active_DB.managerServerName = _managerServerName;
            v.active_DB.managerDBName = _managerDbName;
            v.active_DB.managerPsw = "Password = " + _managerDbPass + ";";
            v.active_DB.managerConnectionText =
                string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                v.active_DB.managerServerName,
                v.active_DB.managerDBName,
                v.active_DB.managerUserName,
                v.active_DB.managerPsw);
            v.active_DB.managerMSSQLConn = new SqlConnection(v.active_DB.managerConnectionText);
            v.active_DB.managerMSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
            #endregion

            /// publish Manager DB Connections
            #region
            v.publishManager_DB.dBaseNo = v.dBaseNo.publishManager;
            v.publishManager_DB.serverName = _publishServerName;
            v.publishManager_DB.databaseName = _publishDbName;
            v.publishManager_DB.userName = _user;
            v.publishManager_DB.psw = "Password = " + _publishDbPass + ";";
            v.publishManager_DB.connectionText =
                string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                v.publishManager_DB.serverName,
                v.publishManager_DB.databaseName,
                v.publishManager_DB.userName,
                v.publishManager_DB.psw);
            v.publishManager_DB.MSSQLConn = new SqlConnection(v.publishManager_DB.connectionText);
            v.publishManager_DB.MSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
            #endregion

            /// UstadCRM DB Connections
            #region

            //v.active_DB.ustadCrmDBName = "UstadCRM";
            v.active_DB.ustadCrmUserName = _user;
            v.active_DB.ustadCrmServerName = _crmServerName;
            v.active_DB.ustadCrmDBName = _crmDbName;
            v.active_DB.ustadCrmPsw = "Password = " + _crmDbPass + ";";
            v.active_DB.ustadCrmConnectionText =
                string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                v.active_DB.ustadCrmServerName,
                v.active_DB.ustadCrmDBName,
                v.active_DB.ustadCrmUserName,
                v.active_DB.ustadCrmPsw);
            v.active_DB.ustadCrmMSSQLConn = new SqlConnection(v.active_DB.ustadCrmConnectionText);
            v.active_DB.ustadCrmMSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
            #endregion

            /// master DB Connections (MSSQL.master)
            #region

            v.active_DB.masterDBName = "master";
            if (IsNotNull(v.active_DB.masterUserName) == false)
                v.active_DB.masterUserName = "sa";

            v.active_DB.masterConnectionText =
                string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                v.active_DB.masterServerName,
                v.active_DB.masterDBName,
                v.active_DB.masterUserName,
                v.active_DB.masterPsw);

            v.active_DB.masterMSSQLConn = new SqlConnection(v.active_DB.masterConnectionText);
            v.active_DB.masterMSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);

            #endregion

            // DİKKAT : BU METODU KULLANMA MASTER-DETAIL de DETAIL kırılıyor
            // v.SP_Conn_Text_Manager_MSSQL = " Server=94.73.145.8; Database=MSV3DFTRBLT; Uid=user4601;Pwd=CanBerk98";
            tToolBox t = new tToolBox();
            if (v.active_DB.managerMSSQLConn != null)
            {
                t.Db_Open(v.active_DB.managerMSSQLConn);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[InitPreparingConnection] WARNING: managerMSSQLConn is null, cannot open connection");
            }

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
        
        /// <summary>
        /// Opens standalone login form (no database required)
        /// </summary>
        /// <summary>
        /// Opens standalone login form (no database required)
        /// Uses WebView2-based login with LoginTemplate.html
        /// </summary>
        void InitLoginUser()
        {
            System.Diagnostics.Debug.WriteLine($"[InitLoginUser] Starting login. v.mainForm={(v.mainForm != null ? v.mainForm.Name : "null")}, v.mainForm.IsMdiContainer={v.mainForm?.IsMdiContainer}, v.mainForm.IsDisposed={v.mainForm?.IsDisposed}");
            YesiLdefter.ms_User_Standalone loginForm = null;
            try
            {
                loginForm = new YesiLdefter.ms_User_Standalone();
                System.Diagnostics.Debug.WriteLine($"[InitLoginUser] Created loginForm: {loginForm.Name}, calling ShowDialog with parent: {(v.mainForm != null ? v.mainForm.Name : "null")}");
                loginForm.ShowDialog(v.mainForm);
                System.Diagnostics.Debug.WriteLine($"[InitLoginUser] Login dialog closed. DialogResult={loginForm.DialogResult}, SP_UserLOGIN={v.SP_UserLOGIN}");
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
                    "Giriş Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
