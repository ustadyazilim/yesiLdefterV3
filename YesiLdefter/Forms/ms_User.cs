/* Core Namespace */
using DevExpress.XtraEditors;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
/* Internal Namespaces */
using Tkn_Events;
using Tkn_Registry;
using Tkn_SQLs;
using Tkn_ToolBox;
using Tkn_UserFirms;
using Tkn_UstadAPI;
using Tkn_Variable;

namespace YesiLdefter
{
    public partial class ms_User : DevExpress.XtraEditors.XtraForm
    {
        #region Tanımlar

        tToolBox t = new tToolBox();
        tSQLs Sqls = new tSQLs();
        tRegistry reg = new tRegistry();
        tUserFirms userFirms = new tUserFirms();

        UstadApiClient apiClient = null; 
        
        DataSet ds_UL = null;
        DataNavigator dN_UL = null;

        DataSet dsUserFirmList = null;
        DataNavigator dNUserFirmList = null;

        // New User
        DataSet ds_NU = null;
        DataNavigator dN_NU = null;
        // UK = Key
        DataSet ds_UK = null;
        DataNavigator dN_UK = null;

        DataNavigator dN_Query = new DataNavigator();
        DataSet ds_Query = new DataSet();
        DataSet ds_Query2 = new DataSet();

        Control cmb_EMail = null;
        Control txt_Pass = null;
        Control btn_BHatirla = null;
        Control btn_SifremiUnuttum = null;
        Control uk_user_mail = null;
        Control uk_old_user_pass = null;
        Control uk_new_user_pass = null;
        Control uk_rpt_user_pass = null;

        int u_user_Last_FirmId = 0;
        string u_user_email = string.Empty;
        string u_user_key = string.Empty;
        string u_db_user_key = string.Empty;

        string tSql = string.Empty;
        string TableIPCode = string.Empty;

        string Login_TableIPCode = "UST/CRM/UstadUsers.Login_F01";
        string NewPass_TableIPCode = "UST/CRM/UstadUsers.NewPassword_F01";
        string NewUser_TableIPCode = "UST/CRM/UstadUsers.NewUser_F01";
        string FirmList_TableIPCode = "UST/CRM/UstadFirmsUsers.KullanicininFirmaSecimi_L01";
        //"UST/CRM/UstadFirms.UserFirmList_L01";

        string regPath = v.registryPath;//"Software\\Üstad\\YesiLdefter";

        #endregion

        public ms_User()
        {
            /// NOTE(@Tekin):
            /// comp - user - firm ilişkisi ?
            /// 
            /// prog açılışında 
            /// önce  user bilgileri
            /// sonra comp bilgileri
            // burası değişti 
            /// user için tanımlı olan user_firm_guid varsa o firma/shop 
            /// yoksa 
            /// comp kartında tanımlı olan comp_firm_guid e göre firma/shop baz alınacak
            /// 
            /// yani kullanıcının kendisi için tanımlı firmaları var ise o listeye göre çalışır
            /// eğer kullanıcı için firm_guid yok ise Comp için tanımlı olan firma çalışır
            /// 
            /// comp taki veya user daki xxxx_firm_guid hangisi olursa olsun
            /// kayıtlı bu firm_guidin kendisi ve kendisine bağlı alt firm's/shop's listelenir
            /// böyle birden fazla firma ve şube gelirse 
            /// kullanıcıya hangisinde işlem yapacağı (firma listesinden) sorulur
            /// tek firma veya shop olursa kullanıcaya sorulmadan direk program o firm_id üzerinden
            /// çalışmaya başlar.
            /// 
            /// Peki comp'unda firm_guidi yoksa o zamanda kullanıcının karşına bizim TEST firmaları listelenir
            /// 

            InitializeComponent();

            tEventsForm evf = new tEventsForm();

            this.Load += new System.EventHandler(evf.myForm_Load);
            this.Shown += new System.EventHandler(evf.myForm_Shown);
            this.Shown += new System.EventHandler(this.ms_User_Shown);
            this.Leave += new System.EventHandler(evf.myForm_Leave);

            this.KeyPreview = true;
        }

        private void ms_User_Shown(object sender, EventArgs e)
        {
            //
            // Sisteme Giriş // User Login ---------------------------------------
            //
            #region
            Application.DoEvents();

            t.Find_DataSet(this, ref ds_UL, ref dN_UL, Login_TableIPCode);

            Control cntrl = null;
            string[] controls = new string[] { };
            cntrl = t.Find_Control(this, "simpleButton_ek1", Login_TableIPCode, controls);

            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Dock = DockStyle.Right;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Width = 75;
                //((DevExpress.XtraEditors.SimpleButton)cntrl).Text = "İleri";
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Click += new System.EventHandler(btn_SistemeGiris_Ileri);
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Image = t.Find_Glyph("SIHIRBAZDEVAM16");
            }

            btn_BHatirla = t.Find_Control(this, "checkButton_ek1", Login_TableIPCode, controls);

            btn_SifremiUnuttum = t.Find_Control(this, "simpleButton_ek2", Login_TableIPCode, controls);
            if (btn_SifremiUnuttum != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)btn_SifremiUnuttum).Click += new System.EventHandler(btn_SifremiUnuttumClick);
            }

            cmb_EMail = t.Find_Control(this, "Column_UserEMail", Login_TableIPCode, controls);
            txt_Pass = t.Find_Control(this, "Column_UserKey", Login_TableIPCode, controls);

            if (cmb_EMail != null)
            {
                ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).EnterMoveNextControl = true;
                ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).EditValueChanged
                    += new System.EventHandler(cmb_EMail_EditValueChanged);
            }

            if (txt_Pass != null)
            {
                //((DevExpress.XtraEditors.TextEdit)txt_Pass).EnterMoveNextControl = true;
                ((DevExpress.XtraEditors.TextEdit)txt_Pass).KeyDown += new KeyEventHandler(txt_Pass_KeyDown);
                //((DevExpress.XtraEditors.TextEdit)txt_Pass).Properties.PasswordChar = '*';
            }
            #endregion
            //
            // Yeni Kullanıcı // New User ----------------------------------------
            // 
            #region
            t.Find_DataSet(this, ref ds_NU, ref dN_NU, NewUser_TableIPCode);

            cntrl = t.Find_Control(this, "simpleButton_ek1", NewUser_TableIPCode, controls);

            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Dock = DockStyle.Right;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Click += new System.EventHandler(btn_YeniKullanici_Kaydet);
            }
            #endregion
            //
            // Şifre Değişikliği -------------------------------------------------
            // 
            #region
            t.Find_DataSet(this, ref ds_UK, ref dN_UK, NewPass_TableIPCode);

            cntrl = t.Find_Control(this, "simpleButton_ek1", NewPass_TableIPCode, controls);

            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Dock = DockStyle.Right;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Click += new System.EventHandler(btn_Yeni_SifreClick);
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Image = t.Find_Glyph("KAYDET16");
            }

            uk_user_mail = t.Find_Control(this, "Column_UserEMail", NewPass_TableIPCode, controls);
            if (uk_user_mail != null)
            {
                ((DevExpress.XtraEditors.TextEdit)uk_user_mail).EnterMoveNextControl = true;
            }

            uk_old_user_pass = t.Find_Control(this, "Column_UserFirstName", NewPass_TableIPCode, controls);
            if (uk_old_user_pass != null)
            {
                ((DevExpress.XtraEditors.TextEdit)uk_old_user_pass).EnterMoveNextControl = true;
                //((DevExpress.XtraEditors.TextEdit)uk_old_user_pass).Properties.PasswordChar = '*';
            }

            uk_new_user_pass = t.Find_Control(this, "Column_UserLastName", NewPass_TableIPCode, controls);
            if (uk_new_user_pass != null)
            {
                ((DevExpress.XtraEditors.TextEdit)uk_new_user_pass).EnterMoveNextControl = true;
                //((DevExpress.XtraEditors.TextEdit)uk_new_user_pass).Properties.PasswordChar = '*';
            }

            uk_rpt_user_pass = t.Find_Control(this, "Column_UserKey", NewPass_TableIPCode, controls);
            if (uk_rpt_user_pass != null)
            {
                ((DevExpress.XtraEditors.TextEdit)uk_rpt_user_pass).EnterMoveNextControl = true;
                //((DevExpress.XtraEditors.TextEdit)uk_rpt_user_pass).Properties.PasswordChar = '*';
            }
            #endregion

            //
            // FirmList ----------------------------------------------------------
            //
            #region
            cntrl = t.Find_Control(this, "simpleButton_ek1", FirmList_TableIPCode, controls);

            if (cntrl != null)
            {
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Dock = DockStyle.Right;
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Click += new System.EventHandler(btn_FirmListSec_Click);
                ((DevExpress.XtraEditors.SimpleButton)cntrl).Image = t.Find_Glyph("SIHIRBAZDEVAM16");
            }
            #endregion

            GetUserRegistry();
            try
            {
                // Get API base URL from registry configuration
                // NOTE(@Janberk): API base URL is now stored in registry for runtime configuration
                string apiBaseUrl = Tkn_UstadAPI.tApiConfig.GetApiBaseUrl();
                apiClient = new UstadApiClient(apiBaseUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API client initialization failed: {ex.Message}");
            }
            v.SP_UserLOGIN = false;
            if (cmb_EMail != null)
            {
                ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).Focus();
            }
        }

        void btn_SistemeGiris_Ileri(object sender, EventArgs e)
        {
            if (apiClient == null)
            {
                string apiBaseUrl = Tkn_UstadAPI.tApiConfig.GetApiBaseUrl();
                MessageBox.Show("API bağlantısı kurulamadı. API servisinin çalıştığından emin olun.\n\n" +
                    "API URL: " + apiBaseUrl, "API Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            checkedInputApi();
        }

        void cmb_EMail_EditValueChanged(object sender, EventArgs e)
        {
            ((DevExpress.XtraEditors.TextEdit)txt_Pass).EditValue = "";
        }

        void txt_Pass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                if (apiClient == null)
                {
                    MessageBox.Show("API bağlantısı kurulamadı. Lütfen API servisinin çalıştığından emin olun.",
                        "API Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                checkedInputApi();
            }
        }

        [Obsolete("Use checkedInputApi() instead. This method contains database connection strings.", false)]
        void checkedInput()
        {
            if (t.IsNotNull(ds_UL) && (dN_UL != null))
            {
                if (dN_UL.Position > -1)
                {
                    // buradaki bilgi databaseden gelmiyor kullanıcı elle giriyor
                    //user_email = ds_UL.Tables[0].Rows[dN_UL.Position]["USER_EMAIL"].ToString().Trim();
                    //user_key = ds_UL.Tables[0].Rows[dN_UL.Position]["USER_KEY"].ToString().Trim(); ;
                    u_user_email = ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).EditValue.ToString();
                    u_user_key = ((DevExpress.XtraEditors.TextEdit)txt_Pass).EditValue.ToString();

                    v.tUserRegister.UserLastLoginEMail = u_user_email;
                    v.tUserRegister.UserLastKey = u_user_key;
                    v.tUserRegister.UserRemember = ((DevExpress.XtraEditors.CheckButton)btn_BHatirla).Checked;

                    t.TableRemove(ds_Query);

                    // şimdi [ e-mail ile şifre ] databaseden kontrol ediliyor
                    tSql = Sqls.preparingUstadUsersSql(u_user_email, u_user_key, 0);
                    t.SQL_Read_Execute(v.dBaseNo.UstadCrm, ds_Query, ref tSql, "UstadUsers", "UserLogin");

                    if (t.IsNotNull(ds_Query) == false)
                    {
                        //böyle bir email varmı diye kontrol et
                        checkedUser(u_user_email, "FIND");
                    }
                    else
                    {
                        if (ds_Query.Tables.Count > 0)
                            dN_Query.DataSource = ds_Query.Tables[0];

                        if (ds_Query.Tables[0].Rows.Count == 0)
                        {
                            //böyle bir email varmı diye kontrol et
                            checkedUser(u_user_email, "FIND");
                        }

                        if (ds_Query.Tables[0].Rows.Count == 1)
                        {
                            //e-mail ve şifre girdikte sonra gelen sonucu değerlendir
                            userFirms.UserSelectFirm(this, ds_Query, dN_Query, u_user_key,
                                ref dsUserFirmList, ref dNUserFirmList, FirmList_TableIPCode);
                        }

                        if (ds_Query.Tables[0].Rows.Count > 1)
                        {
                            /// birden fazla kayıt geliyorsa aynı emailden birden fazla mevcut 
                            /// sorun var demektir. çünki bir email, bir kayıt esasına göre çalışılacak
                            /// 
                            MessageBox.Show(u_user_email + v.ENTER2 +
                                "Veritabanında birden fazla aynı e-mail mevcut.", "Çok Ciddi Problem",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            v.SP_UserLOGIN = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// API-based authentication method.
        /// It replaces the legacy checkedInput() which used direct SQL connections.
        /// This method handles: login → token storage → firm selection → main form access.
        /// </summary>
        // TODO(@Janberk): Extract this into AuthenticationService.LoginAsync() for better separation of concerns.
        async void checkedInputApi()
        {
            if (apiClient == null)
            {
                MessageBox.Show("API bağlantısı kurulamadı.",
                    "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (t.IsNotNull(ds_UL))
            {
                if (dN_UL.Position > -1)
                {
                    try
                    {
                        u_user_email = ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).EditValue?.ToString()?.Trim() ?? "";
                        u_user_key = ((DevExpress.XtraEditors.TextEdit)txt_Pass).EditValue?.ToString()?.Trim() ?? "";

                        if (string.IsNullOrWhiteSpace(u_user_email))
                        {
                            MessageBox.Show("Lütfen e-posta adresinizi girin.", "Eksik Bilgi",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(u_user_key))
                        {
                            MessageBox.Show("Lütfen şifrenizi girin.", "Eksik Bilgi",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        v.tUserRegister.UserLastLoginEMail = u_user_email;
                        v.tUserRegister.UserLastKey = u_user_key;
                        v.tUserRegister.UserRemember = ((DevExpress.XtraEditors.CheckButton)btn_BHatirla).Checked;

                        t.WaitFormOpen(this, "Giriş yapılıyor...");
                        Application.DoEvents();

                        var loginResponse = await ExecuteWithRetryAsync(
                            () => apiClient.LoginAsync(u_user_email, u_user_key),
                            maxRetries: 3,
                            operationName: "Giriş"
                        );

                        if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                        {
                            // Prefer UserId; fall back to OperatorId for legacy responses
                            v.tUser.UserId = (loginResponse.UserId != 0) ? loginResponse.UserId : loginResponse.OperatorId;
                            v.tUser.UserGUID = loginResponse.UserGUID;
                            v.tUser.FullName = loginResponse.FullName;
                            v.tUser.UserDbTypeId = loginResponse.DbTypeId;
                            v.tUser.eMail = u_user_email;
                            // NOTE(@Janberk): API must return both UserId (numeric) and UserGUID; OperatorId currently maps to legacy UserId usage.
                            
                            // NOTE(@Janberk): Store JWT token for later use in getting DB connection info
                            // The token is needed to authenticate API calls to /auth/db-connection-info
                            v.tUser.JwtToken = loginResponse.Token;
                            // TODO(@Janberk): Add refresh-token support and persist token securely (registry/DPAPI) with expiry tracking.

                            apiClient.SetAuthToken(loginResponse.Token);
                            
                            // NOTE(@Janberk): Show loading indicator while fetching user firms
                            t.WaitFormOpen(this, "Firma bilgileri alınıyor...");
                            Application.DoEvents();
                            
                            var userFirmsList = await ExecuteWithRetryAsync(
                                () => apiClient.GetUserFirmsAsync(loginResponse.UserGUID),
                                maxRetries: 2,
                                operationName: "Firma bilgileri"
                            );

                            if (userFirmsList != null && userFirmsList.Count > 0)
                            {
                                if (userFirmsList.Count == 1)
                                {
                                    var firm = userFirmsList[0];
                                    t.WaitFormOpen(this, "Firma seçiliyor...");
                                    Application.DoEvents();
                                    await SelectFirmFromApiAsync(firm);
                                }
                                else
                                {
                                    ShowFirmSelectionFromApi(userFirmsList, ref dsUserFirmList, ref dNUserFirmList);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Kullanıcıya atanmış firma bulunamadı.", "Firma Bulunamadı",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                v.SP_UserLOGIN = false;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Giriş başarısız. Lütfen bilgilerinizi kontrol edin.", "Giriş Hatası",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            v.SP_UserLOGIN = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        // NOTE(@Janberk): Improved error handling with better user feedback
                        string errorMsg = ex.Message;
                        bool isAuthError = false;
                        bool isNetworkError = false;
                        int? statusCode = null;
                        
                        if (ex.Data.Contains("StatusCode"))
                        {
                            statusCode = (int?)ex.Data["StatusCode"];
                            isAuthError = statusCode == 401;
                        }
                        
                        // Check for network/connection errors
                        if (errorMsg.Contains("connection") || 
                            errorMsg.Contains("timeout") || 
                            errorMsg.Contains("network") ||
                            errorMsg.Contains("API connection error"))
                        {
                            isNetworkError = true;
                        }
                        
                        string apiErrorMsg = "";
                        if (ex.Data.Contains("ErrorContent"))
                        {
                            apiErrorMsg = ex.Data["ErrorContent"]?.ToString() ?? "";
                        }
                        
                        if (isAuthError || 
                            errorMsg.Contains("401") || 
                            errorMsg.Contains("Unauthorized") || 
                            errorMsg.Contains("Şifre hatalı") ||
                            errorMsg.Contains("Kullanıcı bulunamadı"))
                        {
                            checkedUserApi(u_user_email, "FIND");
                        }
                        else if (isNetworkError)
                        {
                            string displayMsg = "API bağlantısı kurulamadı.\n\n" +
                                "Lütfen internet bağlantınızı kontrol edin ve API servisinin çalıştığından emin olun.\n\n" +
                                $"Hata: {errorMsg}";
                            MessageBox.Show(displayMsg, "Bağlantı Hatası",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            v.SP_UserLOGIN = false;
                        }
                        else
                        {
                            string displayMsg = errorMsg;
                            if (errorMsg.Contains("Şifre hatalı"))
                            {
                                displayMsg = "Giriş başarısız. Şifreniz hatalı olabilir.\n\n" +
                                    "Şifrenizi unuttuysanız 'Şifremi Unuttum' butonunu kullanabilirsiniz.";
                            }
                            MessageBox.Show(displayMsg, "Giriş Hatası",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            v.SP_UserLOGIN = false;
                        }
                    }
                    finally
                    {
                        // NOTE(@Janberk): Always close loading indicator
                        t.WaitFormClose();
                    }
                }
            }
        }
        
        /// <summary>
        /// Populate v.tMainFirm from API FirmInfo and FirmDetails response
        /// Replaces SQL-based getFirmAbout() method with API-based data population
        /// </summary>
        private void PopulateFirmFromApiResponse(UstadApiClient.FirmInfo firmInfo, UstadApiClient.Firm firmDetails)
        {
            if (firmInfo == null || firmDetails == null)
            {
                throw new ArgumentNullException("FirmInfo or FirmDetails cannot be null");
            }

            // Populate from FirmInfo (has all the fields we need)
            v.tMainFirm.FirmId = firmInfo.FirmId;
            v.tMainFirm.FirmLongName = firmInfo.FirmLongName ?? firmDetails.FirmLongName ?? "";
            v.tMainFirm.FirmShortName = firmInfo.FirmShortName ?? "";
            v.tMainFirm.FirmGuid = firmInfo.FirmGUID ?? firmDetails.FirmGUID ?? "";
            v.tMainFirm.IlKodu = firmInfo.CityTypeId?.ToString() ?? "";
            v.tMainFirm.IlceKodu = firmInfo.DistrictTypeId?.ToString() ?? "";
            v.tMainFirm.MenuCode = firmInfo.MenuCode ?? "";
            v.tMainFirm.SectorTypeId = firmInfo.SectorTypeId ?? 0;
            v.tMainFirm.DatabaseType = "1"; // MSSQL
            v.tMainFirm.DatabaseName = firmInfo.DatabaseName ?? "";
            v.tMainFirm.ServerNameIP = firmInfo.ServerNameIP ?? "";
            v.tMainFirm.DbLoginName = firmInfo.DbLoginName ?? "";
            v.tMainFirm.DbPassword = firmInfo.DbPass ?? "";
            v.tMainFirm.DbTypeId = firmInfo.DbTypeId ?? 0; // 2 = Abone database (Ustad yazılım müşterileri)
            v.tMainFirm.FirmMebbisCode = firmInfo.MebbisCode ?? "";
            v.tMainFirm.FirmMebbisPass = firmInfo.MebbisPass ?? "";
        }

        /// <summary>
        /// Extract FirmInfo from DataRow (populated from firm selection list)
        /// NOTE(@Janberk): Helper method to reconstruct FirmInfo from DataRow for API-based flow
        /// </summary>
        private UstadApiClient.FirmInfo ExtractFirmInfoFromRow(DataRow row)
        {
            if (row == null)
            {
                return null;
            }

            try
            {
                return new UstadApiClient.FirmInfo
                {
                    FirmId = Convert.ToInt32(row["FirmId"] ?? 0),
                    FirmGUID = row["FirmGUID"]?.ToString() ?? "",
                    FirmLongName = row["FirmLongName"]?.ToString() ?? "",
                    FirmShortName = row["FirmShortName"]?.ToString() ?? "",
                    MenuCode = row["MenuCode"]?.ToString() ?? "",
                    SectorTypeId = row["SectorTypeId"] != DBNull.Value ? Convert.ToInt16(row["SectorTypeId"]) : (short?)null,
                    DatabaseName = row["DatabaseName"]?.ToString() ?? "",
                    ServerNameIP = row["ServerNameIP"]?.ToString() ?? "",
                    DbLoginName = row["DbLoginName"]?.ToString() ?? "",
                    DbPass = row["DbPass"]?.ToString() ?? "",
                    DbTypeId = row["DbTypeId"] != DBNull.Value ? Convert.ToInt16(row["DbTypeId"]) : (short?)null,
                    DistrictTypeId = row["DistrictTypeId"] != DBNull.Value ? Convert.ToInt32(row["DistrictTypeId"]) : (int?)null,
                    CityTypeId = row["CityTypeId"] != DBNull.Value ? Convert.ToInt32(row["CityTypeId"]) : (int?)null,
                    MebbisCode = row["MebbisCode"]?.ToString() ?? "",
                    MebbisPass = row["MebbisPass"]?.ToString() ?? "",
                    IsActive = row["IsActive"] != DBNull.Value && Convert.ToInt32(row["IsActive"]) == 1
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting FirmInfo from DataRow: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Execute async operation with retry logic
        /// TODO(@Janberk): Adds retry mechanism for transient API failures (network issues, timeouts)
        /// </summary>
        private async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxRetries = 3,
            int delayMs = 1000,
            string operationName = "İşlem")
        {
            Exception lastException = null;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    // Don't retry on authentication errors (401, 403)
                    if (ex.Data.Contains("StatusCode"))
                    {
                        int? statusCode = (int?)ex.Data["StatusCode"];
                        if (statusCode == 401 || statusCode == 403)
                        {
                            throw; // Re-throw auth errors immediately
                        }
                    }
                    
                    // Don't retry on validation errors
                    if (ex.Message.Contains("Eksik Bilgi") || 
                        ex.Message.Contains("geçersiz") ||
                        ex.Message.Contains("invalid"))
                    {
                        throw; // Re-throw validation errors immediately
                    }
                    
                    // If this is the last attempt, throw the exception
                    if (attempt == maxRetries)
                    {
                        break;
                    }
                    
                    // Wait before retrying (exponential backoff)
                    int waitTime = delayMs * attempt;
                    System.Diagnostics.Debug.WriteLine(
                        $"{operationName} başarısız (deneme {attempt}/{maxRetries}). {waitTime}ms sonra tekrar denenecek...");
                    
                    await Task.Delay(waitTime);
                }
            }
            
            // All retries failed
            throw new Exception(
                $"{operationName} {maxRetries} deneme sonrasında başarısız oldu: {lastException?.Message}",
                lastException);
        }

        [Obsolete("Use API methods instead. This method contains database connection strings.", false)]
        void read_eMail(DataSet ds, string user_EMail)
        {
            tSql = Sqls.preparingUstadUsersSql(user_EMail, "", 0);
            t.SQL_Read_Execute(v.dBaseNo.UstadCrm, ds, ref tSql, "UstadUsers", "FindUser");
        }

        [Obsolete("Use checkedUserApi() instead. This method contains database connection strings.", false)]
        void checkedUser(string user_Email, string work)
        {
            read_eMail(ds_Query, user_Email);

            if (ds_Query.Tables.Count == 1)
            {
                if (ds_Query.Tables[0].Rows.Count == 0)
                {
                    string soru = user_Email + "  böyle bir hesap bulunamadı. \r\n\r\n Yeni bir kullanıcı oluşturmak ister misiniz  ?";
                    DialogResult cevap = t.mySoru(soru);
                    if (DialogResult.Yes == cevap)
                    {
                        t.SelectPage(this, "BACKVIEW", "NEWUSER", -1);
                    }
                }

                if ((work == "FIND") & (ds_Query.Tables[0].Rows.Count == 1))
                {
                    MessageBox.Show(" Şifrenizde bir sorun olabilir.\r\n\r\n Yeniden deneyebilir veya yeni bir şifre alabilirsiniz.");
                }

                if ((work == "SEND_EMAIL") & (ds_Query.Tables[0].Rows.Count == 1))
                {
                    int Id = t.myInt32(ds_Query.Tables[0].Rows[0]["UserId"].ToString());
                    Int16 IsActive = t.myInt16(ds_Query.Tables[0].Rows[0]["IsActive"].ToString());
                    string userFullName = ds_Query.Tables[0].Rows[0]["UserFullName"].ToString();
                    u_db_user_key = ds_Query.Tables[0].Rows[0]["UserKey"].ToString();

                    // 0.pasif, 1.aktif 
                    if (IsActive < 2)
                    {
                        Send_EMail(user_Email, userFullName, u_db_user_key);
                    }
                    else
                    {

                    }
                }
            }
        }

        // 2. NOTE(@Janberk): checkedUserApi() is the API-based user check method.
        // It replaces the legacy checkedUser() which used direct SQL connections.
        // This method handles: user check → password reset → user creation.
        // TODO(@Janberk): Extract this into UserService.CheckUserExistsAsync() for better separation of concerns.
        async void checkedUserApi(string user_Email, string work)
        {
            if (apiClient == null)
            {
                MessageBox.Show("API bağlantısı kurulamadı. API servisinin çalıştığından emin olun.",
                    "API Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            user_Email = user_Email?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(user_Email))
            {
                MessageBox.Show("Lütfen geçerli bir e-posta adresi giriniz.", "Eksik Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // NOTE(@Janberk): Show loading indicator during user check
                if (work == "SEND_EMAIL")
                {
                    t.WaitFormOpen(this, "Şifre sıfırlama talebi gönderiliyor...");
                }
                else
                {
                    t.WaitFormOpen(this, "Kullanıcı kontrol ediliyor...");
                }
                Application.DoEvents();
                
                var userExists = await ExecuteWithRetryAsync(
                    () => apiClient.CheckUserExistsAsync(user_Email),
                    maxRetries: 2,
                    operationName: "Kullanıcı kontrolü"
                );

                if (!userExists.Exists)
                {
                    if (work == "FIND" || work == "SEND_EMAIL")
                    {
                        string soru = user_Email + "  böyle bir hesap bulunamadı. \r\n\r\n Yeni bir kullanıcı oluşturmak ister misiniz  ?";
                        DialogResult cevap = t.mySoru(soru);
                        if (DialogResult.Yes == cevap)
                        {
                            t.SelectPage(this, "BACKVIEW", "NEWUSER", -1);
                        }
                    }
                    return;
                }

                if (work == "FIND")
                {
                    string message = "Şifrenizde bir sorun olabilir.\r\n\r\n" +
                        "Yeniden deneyebilir veya yeni bir şifre alabilirsiniz.\r\n\r\n" +
                        "Not: Eğer şifrenizi yeni değiştirdiyseniz veya sistem yükseltmesinden sonra giriş yapamıyorsanız, " +
                        "'Şifremi Unuttum' butonunu kullanarak şifrenizi sıfırlayabilirsiniz.";
                    MessageBox.Show(message, "Şifre Sorunu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (work == "SEND_EMAIL")
                {
                    if (userExists.IsActive)
                    {
                        try
                        {
                            t.WaitFormOpen(this, "Şifre sıfırlama e-postası gönderiliyor...");
                            Application.DoEvents();
                            
                            await ExecuteWithRetryAsync(
                                () => apiClient.RequestPasswordResetAsync(user_Email),
                                maxRetries: 2,
                                operationName: "Şifre sıfırlama"
                            );
                            
                            MessageBox.Show("Şifre sıfırlama talebi gönderildi.\nLütfen e-postanızı kontrol edin.",
                                "Şifre Sıfırlama", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Şifre sıfırlama talebi gönderilemedi:\n" + ex.Message,
                                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Bu hesap aktif değil.", "Pasif Hesap",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kullanıcı kontrolü sırasında hata oluştu:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // NOTE(@Janberk): Always close loading indicator
                t.WaitFormClose();
            }
        }

        /// <summary>
        /// Desktop Application Tennant Firm Selection
        /// </summary>
        /// <param name="firm"></param>
        /// <returns></returns>
        async Task SelectFirmFromApiAsync(UstadApiClient.FirmInfo firm)
        {
            try
            {
                // NOTE(@Janberk): Step 1 - Select firm via API to get new token with firm claim
                t.WaitFormOpen(this, "Firma seçiliyor...");
                Application.DoEvents();
                
                var selectFirmResponse = await ExecuteWithRetryAsync(
                    () => apiClient.SelectFirmAsync(firm.FirmGUID),
                    maxRetries: 2,
                    operationName: "Firma seçimi"
                );

                if (selectFirmResponse == null || string.IsNullOrEmpty(selectFirmResponse.Token))
                {
                    MessageBox.Show("Firma seçimi başarısız oldu.", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_UserLOGIN = false;
                    return;
                }

                // NOTE(@Janberk): Update stored JWT token with firm claim
                v.tUser.JwtToken = selectFirmResponse.Token;
                if (selectFirmResponse.FirmId > 0)
                {
                    // Store firm ID in MainFirmId (user's current working firm)
                    v.tUser.MainFirmId = selectFirmResponse.FirmId;
                }

                // NOTE(@Janberk): Step 2 - Fetch firm details for UI population
                t.WaitFormOpen(this, "Firma bilgileri alınıyor...");
                Application.DoEvents();
                
                var firmDetails = await ExecuteWithRetryAsync(
                    () => apiClient.GetFirmDetailsAsync(firm.FirmGUID),
                    maxRetries: 2,
                    operationName: "Firma bilgileri"
                );

                if (firmDetails?.Firm != null)
                {
                    PopulateFirmFromApiResponse(firm, firmDetails.Firm);
                    
                    t.setSelectFirm(v.tMainFirm);
                    SetUserRegistryFirm(v.tUser.UserId, v.tMainFirm.FirmId);
                    v.SP_UserLOGIN = true;
                    // NOTE(@Janberk): DB connections are not opened here; InitStart() (tStarter) will open Manager/Ustad after this form closes.
                    // The token now includes firm claim, so GetDatabaseConnectionInfo will succeed.
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Firma bilgileri alınamadı.", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    v.SP_UserLOGIN = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Firma seçimi sırasında hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                v.SP_UserLOGIN = false;
            }
            finally
            {
                // NOTE(@Janberk): Always close loading indicator
                t.WaitFormClose();
            }
        }

        // 3. NOTE(@Janberk): ShowFirmSelectionFromApi() is the API-based firm selection method.
        // It replaces the legacy ShowFirmSelection() which used direct SQL connections.
        // This method handles: firm list display → firm selection → firm details retrieval.
        // TODO(@Janberk): Extract this into FirmService.GetFirmListAsync() for better separation of concerns.
        private void ShowFirmSelectionFromApi(System.Collections.Generic.List<UstadApiClient.FirmInfo> firms, ref DataSet dsUserFirmList, ref DataNavigator dNUserFirmList)
        {
            try
            {
                t.Find_DataSet(this, ref dsUserFirmList, ref dNUserFirmList, FirmList_TableIPCode);

                int i = 1;
                foreach (var firm in firms)
                {
                    DataRow row = dsUserFirmList.Tables[0].NewRow();

                    //var row = table.NewRow();
                    row["Id"] = i++;
                    row["IsActive"] = firm.IsActive ? 1 : 0;
                    row["FirmId"] = firm.FirmId;
                    row["FirmGUID"] = firm.FirmGUID ?? "";
                    row["UserId"] = v.tUser.UserId;
                    row["UserGUID"] = v.tUser.UserGUID ?? "";
                    row["Lkp_FirmLongName"] = firm.FirmLongName ?? "";
                    row["Lkp_UserFullName"] = firm.UserFullName ?? "";

                    row["FirmShortName"] = firm.FirmShortName ?? "";
                    row["FirmLongName"] = firm.FirmLongName ?? "";
                    row["MenuCode"] = firm.MenuCode ?? "";
                    row["SectorTypeId"] = firm.SectorTypeId ?? 0;
                    row["DatabaseName"] = firm.DatabaseName ?? "";
                    row["ServerNameIP"] = firm.ServerNameIP ?? "";
                    row["DbLoginName"] = firm.DbLoginName ?? "";
                    row["DbPass"] = firm.DbPass ?? "";
                    row["DbTypeId"] = firm.DbTypeId ?? 0;
                    row["DistrictTypeId"] = firm.DistrictTypeId ?? 0;
                    row["CityTypeId"] = firm.CityTypeId ?? 0;
                    row["MebbisCode"] = firm.MebbisCode ?? "";
                    row["MebbisPass"] = firm.MebbisPass ?? "";

                    dsUserFirmList.Tables[0].Rows.Add(row);
                }

                t.SelectPage(this, "BACKVIEW", "FIRMLIST", -1);

                Application.DoEvents();

                if (dsUserFirmList != null && dsUserFirmList.Tables.Count > 0 && dNUserFirmList != null)
                {
                    if (dsUserFirmList.Tables[0].Rows.Count > 0)
                    {
                        if (v.tUserRegister.UserLastFirmId > 0)
                        {
                            int pos = 0;
                            foreach (System.Data.DataRow row in dsUserFirmList.Tables[0].Rows)
                            {
                                if (Convert.ToInt32(row["FirmId"]) == v.tUserRegister.UserLastFirmId)
                                {
                                    dNUserFirmList.Position = pos;
                                    break;
                                }
                                pos++;
                            }
                        }
                        else
                        {
                            dNUserFirmList.Position = 0;
                        }
                        // Nudge bindings
                        Application.DoEvents();
                    }
                }

                Control cntrl = t.Find_Control_View(this, FirmList_TableIPCode);
                if (cntrl != null)
                {
                    t.tFormActiveControl(this, cntrl);
                }
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Firma listesi gösterilirken hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void Send_EMail(string myeMail, string myUserFullName, string myKey)
        {
            //
            eMail email = new eMail();

            email.toMailAddress = myeMail;
            email.subject = "Kullanıcı kodu ";
            email.message =
                "<p> Üstad Bilişim veritabanında kayıtlı olan, </p>" +
                "<p> Kullanıcı Adı Soyadı : " + myUserFullName + "<br>" +
                " Kullanıcı Şifreniz : " + myKey + "</p>";

            t.eMailSend3(email);
        }

        #region User giriş yaptı, firma seçti

        void btn_FirmListSec_Click(object sender, EventArgs e)
        {
            /// Buraya geldiysen firma seçimi yapılacak .
            /// kullanıcı için birden fazla firma seçeneği var demek ki
            /// kullanıcı bu butona basar ve seçilen değerler alınır lets go ... main form
            ///
            if (t.IsNotNull(dsUserFirmList))
            { 
                if (dNUserFirmList == null || dNUserFirmList.Position < 0 || dsUserFirmList.Tables.Count == 0 || dsUserFirmList.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("Firma listesi henüz hazır değil.", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataRow row = dsUserFirmList.Tables[0].Rows[dNUserFirmList.Position];

                if (apiClient != null && row.Table.Columns.Contains("FirmGUID"))
                {
                    // API-based flow
                    readUstadFirmAboutFromApi(row);
                    return;
                }
                // else: SQL-based flow
                userFirms.readUstadFirmAbout(this, row);
            }
        }

        /// <summary>
        /// API-based firm details retrieval method.
        /// It replaces the legacy readUstadFirmAbout() which used direct SQL connections.
        /// </summary>
        /// <param name="row"></param>
        // TODO(@Janberk): Extract this into FirmService.GetFirmDetailsAsync() for better separation of concerns.
        async void readUstadFirmAboutFromApi(DataRow row)
        {
            try
            {
                string firmGUID = row["FirmGUID"]?.ToString();
                if (string.IsNullOrEmpty(firmGUID))
                {
                    MessageBox.Show("Firma bilgisi bulunamadı.", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // NOTE(@Janberk): Show loading indicator while fetching firm details
                t.WaitFormOpen(this, "Firma bilgileri alınıyor...");
                Application.DoEvents();

                var firmDetails = await ExecuteWithRetryAsync(
                    () => apiClient.GetFirmDetailsAsync(firmGUID),
                    maxRetries: 2,
                    operationName: "Firma bilgileri"
                );

                if (firmDetails?.Firm != null)
                {
                    // Extract firm info from DataRow (populated from FirmInfo list)
                    UstadApiClient.FirmInfo firmInfo = ExtractFirmInfoFromRow(row);
                    if (firmInfo != null)
                    {
                        PopulateFirmFromApiResponse(firmInfo, firmDetails.Firm);
                        
                        t.setSelectFirm(v.tMainFirm);
                        SetUserRegistryFirm(v.tUser.UserId, v.tMainFirm.FirmId);
                        v.SP_UserLOGIN = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Firma bilgileri alınamadı.", "Hata",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        v.SP_UserLOGIN = false;
                    }

                    //t.setSelectFirm(v.tMainFirm);
                    //SetUserRegistryFirm(v.tUser.UserId, v.tMainFirm.FirmId);
                    //v.SP_UserLOGIN = true;
                    //this.Close();

                }
                else
                {
                    MessageBox.Show("Firma bilgileri alınamadı.", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Firma seçimi sırasında hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // NOTE(@Janberk): Always close loading indicator
                t.WaitFormClose();
            }
        }

        void SetUserRegistryFirm(int userId, int firmId)
        {
            reg.SetUstadRegistry("userFirm" + userId.ToString(), firmId.ToString());
            reg.SetUstadRegistry("userLastFirm", firmId.ToString());
            v.tUserRegister.UserLastFirmId = firmId;
        }

        #endregion User giriş yaptı, firma seçti

        [Obsolete("This method contains database connection strings.", false)]
        void SetUserIsActive(int Id)
        {
            tSql = Sqls.preparingUstadUsersSql("", "", Id);
            t.SQL_Read_Execute(v.dBaseNo.UstadCrm, ds_Query2, ref tSql, "UstadUsers", "SetUserIsActive");
        }

        void GetUserRegistry()
        {
            userFirms.GetUserRegistry(regPath);

            if (cmb_EMail != null)
            {
                // e-mail listesi combo için okunuyor
                ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).Properties.Items.AddRange(v.tUserRegister.eMailList);
                // en son giriş yapan e-mail combonun text ine atanıyor
                if (cmb_EMail != null)
                    ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).EditValue = v.tUserRegister.UserLastLoginEMail;
                if (uk_user_mail != null)
                    ((DevExpress.XtraEditors.TextEdit)uk_user_mail).EditValue = v.tUserRegister.UserLastLoginEMail;
                if (btn_BHatirla != null)
                {
                    ((DevExpress.XtraEditors.CheckButton)btn_BHatirla).Checked = v.tUserRegister.UserRemember;

                    // beni hatırla onaylıysa enson girilen key okunuyor ve şifreye atanıyor
                    if (((DevExpress.XtraEditors.CheckButton)btn_BHatirla).Checked)
                        ((DevExpress.XtraEditors.TextEdit)txt_Pass).EditValue = v.tUserRegister.UserLastKey;
                }
                u_user_Last_FirmId = v.tUserRegister.UserLastFirmId;
            }
        }

        void btn_YeniKullanici_Kaydet(object sender, EventArgs e)
        {
            if (t.IsNotNull(ds_NU) && (dN_NU != null))
            {
                if (dN_NU.Position > -1)
                {
                    string user_Email = ds_NU.Tables[0].Rows[0]["UserEMail"].ToString();

                    if (apiClient != null)
                    {
                        btn_YeniKullanici_KaydetApi(user_Email);
                    }
                    else
                    {
                        MessageBox.Show("API bağlantısı kurulamadı. Kullanıcı kaydı yapılamıyor.",
                            "API Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        async void btn_YeniKullanici_KaydetApi(string user_Email)
        {
            try
            {
                var userExists = await apiClient.CheckUserExistsAsync(user_Email);

                if (userExists.Exists)
                {
                    MessageBox.Show(user_Email + "  hesabı mevcut. Lütfen giriş sayfasına dönün.", "Kullanıcı Mevcut",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    // Optionally redirect to login
                    // t.SelectPage(this, "BACKVIEW", "USERLOGIN", -1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanıcı kontrolü sırasında hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void btn_SifremiUnuttumClick(object sender, EventArgs e)
        {
            u_user_email = ((DevExpress.XtraEditors.ComboBoxEdit)cmb_EMail).EditValue.ToString();

            if (apiClient == null)
            {
                MessageBox.Show("API bağlantısı kurulamadı. API servisinin çalıştığından emin olun.",
                    "API Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            checkedUserApi(u_user_email, "SEND_EMAIL");
        }

        void btn_Yeni_SifreClick(object sender, EventArgs e)
        {
            if (t.IsNotNull(ds_UK) && (dN_UK != null))
            {
                if (dN_UK.Position > -1)
                {
                    string user_email = ((DevExpress.XtraEditors.TextEdit)uk_user_mail).EditValue.ToString();
                    string user_old_pass = ((DevExpress.XtraEditors.TextEdit)uk_old_user_pass).EditValue.ToString();
                    string user_new_pass = ((DevExpress.XtraEditors.TextEdit)uk_new_user_pass).EditValue.ToString();
                    string user_rpt_pass = ((DevExpress.XtraEditors.TextEdit)uk_rpt_user_pass).EditValue.ToString();
                    bool user_active = Convert.ToBoolean(ds_UK.Tables[0].Rows[dN_UK.Position]["IsActive"].ToString());
                    if (user_active == false)
                    {
                        MessageBox.Show("Kullanıcı aktif değil. Lütfen kontrol edin.", "Kullanıcı Aktif Değil",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    if (apiClient != null)
                    {
                        btn_Yeni_SifreClickApi(user_email, user_old_pass, user_new_pass, user_rpt_pass);
                    }
                    else
                    {
                        btn_Yeni_SifreClickLegacy(user_email, user_old_pass, user_new_pass, user_rpt_pass);
                    }
                }
            }
        }

        async void btn_Yeni_SifreClickApi(string user_email, string user_old_pass, string user_new_pass, string user_rpt_pass)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user_email))
                {
                    MessageBox.Show("Lütfen e-posta adresinizi giriniz.", "Eksik Bilgi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(user_old_pass))
                {
                    MessageBox.Show("Lütfen mevcut şifrenizi giriniz.", "Eksik Bilgi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(user_new_pass))
                {
                    MessageBox.Show("Lütfen yeni şifrenizi giriniz. Boş geçemezsiniz!", "Eksik Bilgi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (user_new_pass != user_rpt_pass)
                {
                    MessageBox.Show("DİKKAT : Yeni şifre ile tekrar yazdığınız şifre aynı değil...", "Şifre Uyumsuz",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (user_new_pass == user_old_pass)
                {
                    MessageBox.Show("Yeni şifre ile eski şifre aynı olamaz.", "Geçersiz Şifre",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (user_new_pass.Length < 4)
                {
                    MessageBox.Show("Lütfen en az 4 karakterlik bir şifre giriniz.", "Şifre Kısa",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // NOTE(@Janberk): Show loading indicator during password change
                t.WaitFormOpen(this, "Şifre değiştiriliyor...");
                Application.DoEvents();

                bool success = await ExecuteWithRetryAsync(
                    () => apiClient.ChangePasswordAsync(user_email, user_old_pass, user_new_pass),
                    maxRetries: 2,
                    operationName: "Şifre değiştirme"
                );

                if (success)
                {
                    MessageBox.Show("Şifreniz başarıyla güncellenmiştir...", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                if (errorMsg.Contains("401") || errorMsg.Contains("Unauthorized") || errorMsg.Contains("incorrect"))
                {
                    MessageBox.Show("Mevcut şifreniz hatalı. Lütfen kontrol edin.", "Şifre Hatası",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (errorMsg.Contains("not found") || errorMsg.Contains("inactive"))
                {
                    checkedUserApi(user_email, "FIND");
                }
                else
                {
                    MessageBox.Show("Şifre değiştirme sırasında hata oluştu:\n" + errorMsg, "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                // NOTE(@Janberk): Always close loading indicator
                t.WaitFormClose();
            }
        }

        void btn_Yeni_SifreClickLegacy(string user_email, string user_old_pass, string user_new_pass, string user_rpt_pass)
        {
            // şimdi [ e-mail ile şifre ] databaseden kontrol ediliyor
            tSql = Sqls.preparingUstadUsersSql(user_email, user_old_pass, 0);
            t.SQL_Read_Execute(v.dBaseNo.UstadCrm, ds_Query, ref tSql, "UstadUser", "UserLogin");

            if (t.IsNotNull(ds_Query))
            {
                int userId = t.myInt32(ds_Query.Tables[0].Rows[0]["UserId"].ToString());
                bool IsActive = Convert.ToBoolean(ds_Query.Tables[0].Rows[0]["IsActive"].ToString());
                string userFullName = ds_Query.Tables[0].Rows[0]["UserFullName"].ToString();
                string db_user_key = ds_Query.Tables[0].Rows[0]["UserKey"].ToString();

                if (IsActive == false)
                {
                    MessageBox.Show("DİKKAT : Bu hesap AKTİF değildir.");
                    return;
                }

                if (IsActive)
                {
                    if (db_user_key == user_old_pass)
                    {
                        if ((user_new_pass == user_rpt_pass) && // yeni şifre = tekrarlanan şifre
                            (user_new_pass != "") &&            // yeni şifre boş değilse
                            (user_new_pass != user_old_pass) && // yeni şifre ile eski şifre  aynı değilse
                            (user_new_pass.Length >= 4)         // en az 4 karekter   
                            )
                        {
                            tSql = Sqls.preparingUstadUsersSql("", user_new_pass, userId);

                            string myProp = string.Empty;
                            t.MyProperties_Set(ref myProp, "DBaseNo", "3");
                            t.MyProperties_Set(ref myProp, "TableName", "UstadUsers");
                            t.MyProperties_Set(ref myProp, "SqlFirst", tSql);
                            t.MyProperties_Set(ref myProp, "SqlSecond", "null");
                            ds_Query2.Namespace = myProp;

                            bool onay = t.Data_Read_Execute(this, ds_Query2, ref tSql, "NEW_USER_KEY", null);

                            if (onay) MessageBox.Show("Şifreniz başarıyla güncellenmiştir...");
                        }

                        if (user_new_pass == "")
                            MessageBox.Show("Lütfen bir şifre giriniz. Boş geçemezsiniz!");

                        if (user_new_pass != user_rpt_pass)
                            MessageBox.Show("DİKKAT : Yeni şifre ile tekrar yazdığınız şifre aynı değil...");

                        if (user_new_pass.Length < 4)
                            MessageBox.Show("Lütfen en az 4 karekterlik bir şifre giriniz.");
                    }
                }
            }

            if (t.IsNotNull(ds_Query) == false)
            {
                //böyle bir email varmı diye kontrol et
                if (apiClient != null)
                {
                    checkedUserApi(user_email, "FIND");
                }
                else
                {
                    MessageBox.Show("API bağlantısı kurulamadı. Kullanıcı kontrolü yapılamıyor.",
                        "API Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ms_User_FormClosed(object sender, FormClosedEventArgs e)
        {
            apiClient?.Dispose();
            apiClient = null;
        }

        private void ms_User_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((v.SP_UserLOGIN == false) &&
                (v.SP_UserIN == false))
                v.SP_ApplicationExit = true;
        }
    }
}
