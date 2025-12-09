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
            //
            t.WaitFormOpen(v.mainForm, "Ini dosyalar okunuyor...");
            // NOTE(@Janberk): INI read hydrates legacy DB endpoints (manager/CRM/publish/local). This is the only source before API handoff.
            t.ftpDownloadIniFile();

            // Initialize API configuration defaults
            // NOTE(@Janberk): Ensures API base URL and JWT key are set in registry if not already configured
            Tkn_UstadAPI.tApiConfig.InitializeDefaults();

            // SECURE FLOW: No database connection before authentication
            // Form layouts will be loaded AFTER successful authentication and DB connection establishment
            // Note: We do NOT call InitPreparingConnection() or Db_Open() here - connections are established after auth

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
            // The login form (ms_User) uses checkedInputApi() which calls /auth/login endpoint.
            // After successful authentication, we get database connection info from API.
            t.WaitFormOpen(v.mainForm, "Kullanıcı Girişi...");
            if (v.active_DB.localDbUses == false)
            {
                InitLoginUser(); // Ustad YesiLdester user girişi - NO DB CONNECTION NEEDED
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
            // Connection strings are retrieved from API and decrypted using JWT key.
            // This prevents hardcoded passwords from being compiled into the DLL.
            if (v.SP_UserLOGIN == true && v.active_DB.localDbUses == false)
            {
                // NOTE(@Janberk): DB handoff point — triggered after ms_User closes with a selected firm; uses API-provided encrypted strings.
                t.WaitFormOpen(v.mainForm, "Database bağlantı bilgileri API'den alınıyor...");
                bool dbConnectionsEstablished = InitPreparingConnectionFromApi();
                
                if (!dbConnectionsEstablished)
                {
                    MessageBox.Show("Database bağlantı bilgileri alınamadı. Lütfen sistem yöneticinize başvurun.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_ApplicationExit = true;
                    return;
                }

                // Open ManagerDB connection (only if connection object was created)
                if (v.active_DB.managerMSSQLConn != null)
                {
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
                    MessageBox.Show("ManagerDB bağlantısı açılamadı (local). Lütfen bağlantı bilgilerini kontrol edin.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_ApplicationExit = true;
                    return;
                }
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
            t.WaitFormOpen(v.mainForm, "Settings okunuyor...");
            t.read_Settings();


            // 3S_MSGLY 
            t.WaitFormOpen(v.mainForm, "Images okunuyor...");
            //t.SYS_Glyph_Read();

            //t.TestRead();

            // 3. NOTE(@Janberk): if v.SP_UserIN = true, then initialization is complete.
            // The main form will call tLayout.Create_Layout(), which triggers the dashboard rendering pipeline.
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
        /// SECURE AUTHENTICATION FLOW: Initialize database connections from API after authentication
        /// NOTE(@Janberk): This method retrieves encrypted connection strings from API and decrypts them.
        /// No hardcoded passwords are used - all credentials come from API environment variables.
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

                // Get API base URL from registry configuration
                // NOTE(@Janberk): API base URL is now stored in registry for runtime configuration
                string apiBaseUrl = Tkn_UstadAPI.tApiConfig.GetApiBaseUrl();
                
                // Get JWT key for decryption from registry configuration
                // NOTE(@Janberk): JWT key is stored in registry - must match API's JWT key for encryption/decryption
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
                    dbInfoTask.Wait(); // Wait for async operation to complete
                    var dbInfo = dbInfoTask.Result;
                    
                    // Validate that decrypted connection strings exist
                    if (dbInfo == null)
                    {
                        System.Diagnostics.Debug.WriteLine("InitPreparingConnectionFromApi: dbInfo is null.");
                        return false;
                    }

                    bool hasCrm = !string.IsNullOrWhiteSpace(dbInfo.UstadCrmConnectionString);
                    bool hasMgr = !string.IsNullOrWhiteSpace(dbInfo.ManagerConnectionString);

                    if (!hasCrm || !hasMgr)
                    {
                        // If decrypted values are missing, log lengths of encrypted payloads for diagnosis
                        System.Diagnostics.Debug.WriteLine($"InitPreparingConnectionFromApi: Missing decrypted strings. EncryptedCRM len: {dbInfo.EncryptedUstadCrmConnectionString?.Length ?? 0}, EncryptedMgr len: {dbInfo.EncryptedManagerConnectionString?.Length ?? 0}");
                        return false;
                    }
                    
                    // Set up connection strings from API response
                    v.active_DB.managerDBType = v.dBaseType.MSSQL;
                    v.active_DB.ustadCrmDBType = v.dBaseType.MSSQL;
                    v.active_DB.projectDBType = v.dBaseType.MSSQL;
                    
                    // Parse connection strings
                    ParseConnectionStringFromApi(dbInfo.UstadCrmConnectionString, true); // UstadCRM
                    ParseConnectionStringFromApi(dbInfo.ManagerConnectionString, false); // Manager

                    // Ensure connections were created
                    if (v.active_DB.managerMSSQLConn == null || v.active_DB.ustadCrmMSSQLConn == null)
                    {
                        System.Diagnostics.Debug.WriteLine("InitPreparingConnectionFromApi: Connection objects were not created after parsing connection strings.");
                        return false;
                    }
                    
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
            // Token is stored in v.tUser.JwtToken after successful login
            return v.tUser.JwtToken ?? string.Empty;
        }
        
        /// <summary>
        /// Parse connection string from API response and set up database connection objects
        /// </summary>
        void ParseConnectionStringFromApi(string connectionString, bool isUstadCrm)
        {
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
                        
            ///
            /// main Manager DB Connections
            /// 
            #region
            
            v.active_DB.managerUserName = "sa";
            // SECURE: Get password from environment variable only - NO HARDCODED FALLBACK
            // This method is now only used for local DB mode (Tabim) - not for cloud/API mode
            string managerPassword = Environment.GetEnvironmentVariable("USTAD_MANAGER_DB_PASS");
            if (string.IsNullOrWhiteSpace(managerPassword))
            {
                throw new InvalidOperationException(
                    "USTAD_MANAGER_DB_PASS environment variable is required for local database mode. " +
                    "For cloud mode with API authentication, this is not needed. " +
                    "Please set this environment variable or use API authentication mode.");
            }
            
            v.active_DB.managerPsw = "Password = " + managerPassword + ";";

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
            v.publishManager_DB.userName = "sa";
            // Use same legacy password for publish manager unless overridden by environment.
            v.publishManager_DB.psw = "Password = " + managerPassword + ";";
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
            v.active_DB.ustadCrmUserName = "sa";
            
            // Use CRM password override if provided, else legacy fallback.
            string ustadCrmPassword = Environment.GetEnvironmentVariable("USTAD_CRM_DB_PASS");
            if (string.IsNullOrWhiteSpace(ustadCrmPassword))
            {
                ustadCrmPassword = managerPassword;
            }
            v.active_DB.ustadCrmPsw = "Password = " + ustadCrmPassword + ";";

            v.active_DB.ustadCrmConnectionText =
                string.Format(" Data Source = {0}; Initial Catalog = {1}; User ID = {2}; {3} MultipleActiveResultSets = True ",
                v.active_DB.ustadCrmServerName,
                v.active_DB.ustadCrmDBName,
                v.active_DB.ustadCrmUserName,
                v.active_DB.ustadCrmPsw);

            v.active_DB.ustadCrmMSSQLConn = new SqlConnection(v.active_DB.ustadCrmConnectionText);
            v.active_DB.ustadCrmMSSQLConn.StateChange += new StateChangeEventHandler(DBConnectStateManager);
            
            #endregion

            ///
            /// master DB Connections (MSSQL.master)
            /// 
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
        /// This is the SECURE authentication flow - DB connections are only established AFTER successful login
        /// </summary>
        void InitLoginUser()
        {
            try
            {
                // Use standalone login form that doesn't require database for layout
                YesiLdefter.ms_User_Standalone loginForm = new YesiLdefter.ms_User_Standalone();
                loginForm.ShowDialog(v.mainForm);
                loginForm.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Giriş formu açılırken hata oluştu:\n{ex.Message}\n\nLütfen sistem yöneticinize başvurun.",
                    "Giriş Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                v.SP_ApplicationExit = true;
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

        #region orders


        #endregion orders
    }
}
