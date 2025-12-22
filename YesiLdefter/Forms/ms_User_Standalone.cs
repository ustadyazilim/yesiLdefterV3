/* Core Namespace */
using DevExpress.XtraEditors;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
/* Internal Namespaces */
using Tkn_Registry;
using Tkn_ToolBox;
using Tkn_UserFirms;
using Tkn_UstadAPI;
using Tkn_Variable;
using Tkn_UserFirms;

namespace YesiLdefter
{
    /// <summary>
    /// Standalone login form that does NOT require database connection.
    /// Self-contained UI; authentication happens via API, then firm selection and DB handoff occur after success.
    /// Mirrors the legacy ms_User flow (user → firm selection) without DB-dependent layout.
    /// </summary>
    public partial class ms_User_Standalone : XtraForm
    {
        #region Fields
        tToolBox t = new tToolBox();

        private UstadApiClient apiClient = null;
        private tRegistry reg = new tRegistry();
        tUserFirms userFirms = new tUserFirms();
        private TaskCompletionSource<UstadApiClient.FirmInfo?> firmSelectionTcs;
        private Dictionary<string, UstadApiClient.FirmInfo> firmSelectionMap;
        private bool webViewDomReady = false;

        // UI Controls
        private LabelControl lblTitle;
        private LabelControl lblEmail;
        private ComboBoxEdit cmbEmail;
        private LabelControl lblPassword;
        private TextEdit txtPassword;
        private CheckEdit chkRemember;
        private SimpleButton btnLogin;
        private SimpleButton btnForgotPassword;
        private LabelControl lblStatus;
        private PictureEdit picLogo;
        private WebView2 htmlLayout;

        private string regPath = v.registryPath;

        #endregion

        #region Constructor

        public ms_User_Standalone()
        {
            System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] Constructor called");
            try
            {
                // NOTE(@Janberk): InitializeStandaloneComponents() sets up all form properties and controls
                // We don't need InitializeComponent() from Designer.cs as we're doing everything programmatically
                System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] Initializing standalone components...");
                InitializeStandaloneComponents();
                System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] Components initialized");
                
                System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] Initializing API client...");
                InitializeApiClient();
                System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] API client initialized");
                
                System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] Loading user registry...");
                LoadUserRegistry();
                System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] User registry loaded");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] ❌ Exception in constructor: {ex.Message}\n{ex.StackTrace}");
                throw; // Re-throw to let InitLoginUser handle it
            }
        }

        #endregion

        #region Initialize Components (Programmatic - No DB Required)

        private void InitializeStandaloneComponents()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Giriş Yap";
            this.Size = new Size(960, 720);
            this.MinimumSize = new Size(960, 720);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScroll = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;

            // HTML/CSS layout (background + container) via WebView2
            htmlLayout = new WebView2();
            htmlLayout.Dock = DockStyle.Fill;
            // We'll initialize and load template in Load event (InitializeWebViewAsync)
            this.Controls.Add(htmlLayout);

            // UI elements kept for logic binding (not added to Controls to avoid legacy layout)
            picLogo = new PictureEdit();
            lblTitle = new LabelControl();
            lblEmail = new LabelControl();
            cmbEmail = new ComboBoxEdit();
            cmbEmail.EditValueChanged += CmbEmail_EditValueChanged;
            lblPassword = new LabelControl();
            txtPassword = new TextEdit();
            txtPassword.Properties.PasswordChar = '●';
            txtPassword.KeyDown += TxtPassword_KeyDown;
            chkRemember = new CheckEdit();
            btnLogin = new SimpleButton();
            btnLogin.Click += BtnLogin_Click;
            btnForgotPassword = new SimpleButton();
            btnForgotPassword.Click += BtnForgotPassword_Click;
            lblStatus = new LabelControl();

            // Form events
            this.Load += Ms_User_Standalone_Load;
            this.FormClosing += Ms_User_Standalone_FormClosing;

            this.ResumeLayout(false);
        }

        #endregion

        #region Event Handlers

        private void Ms_User_Standalone_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] Load event fired");
            try
            {
                // Initialize auth state
                v.SP_UserLOGIN = false;

                // Initialize WebView2 and load HTML template
                System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] Starting WebView2 initialization...");
                _ = InitializeWebViewAsync();
                
                // Focus on email field (will work once WebView2 is ready)
                // cmbEmail.Focus(); // Commented out - WebView2 handles focus
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] ❌ Exception in Load event: {ex.Message}\n{ex.StackTrace}");
                // Don't re-throw - let the form show even if WebView2 fails
            }
        }

        private void Ms_User_Standalone_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((v.SP_UserLOGIN == false) && (v.SP_UserIN == false))
            {
                v.SP_ApplicationExit = true;
            }

            // Cleanup
            apiClient?.Dispose();
            apiClient = null;
        }

        // Handle messages from the HTML template (buttons)
        private void HtmlLayout_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var raw = e.TryGetWebMessageAsString();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return;
                }

                // Support simple string messages and JSON payloads
                if (string.Equals(raw, "login", StringComparison.OrdinalIgnoreCase))
                {
                    BtnLogin_Click(sender, EventArgs.Empty);
                    return;
                }
                if (string.Equals(raw, "forgot", StringComparison.OrdinalIgnoreCase))
                {
                    BtnForgotPassword_Click(sender, EventArgs.Empty);
                    return;
                }

                // JSON payload expected: { action: "login"|"forgot", email, password, remember }
                JObject obj = JObject.Parse(raw);
                string action = (obj["action"] ?? "").ToString();
                string email = (obj["email"] ?? "").ToString();
                string password = (obj["password"] ?? "").ToString();
                bool remember = obj["remember"] != null && bool.TryParse(obj["remember"].ToString(), out bool rem) && rem;

                if (!string.IsNullOrEmpty(email))
                {
                    cmbEmail.EditValue = email;
                }
                if (!string.IsNullOrEmpty(password))
                {
                    txtPassword.Text = password;
                }
                chkRemember.Checked = remember;

                if (string.Equals(action, "login", StringComparison.OrdinalIgnoreCase))
                {
                    BtnLogin_Click(sender, EventArgs.Empty);
                }
                else if (string.Equals(action, "forgot", StringComparison.OrdinalIgnoreCase))
                {
                    BtnForgotPassword_Click(sender, EventArgs.Empty);
                }
                else if (string.Equals(action, "rememberChanged", StringComparison.OrdinalIgnoreCase))
                {
                    chkRemember.Checked = remember;
                }
                else if (string.Equals(action, "firm-select", StringComparison.OrdinalIgnoreCase))
                {
                    string firmGuid = (obj["firmGUID"] ?? "").ToString();
                    HandleFirmSelectionFromWeb(firmGuid, confirmSelection: false);
                }
                else if (string.Equals(action, "firm-confirm", StringComparison.OrdinalIgnoreCase))
                {
                    string firmGuid = (obj["firmGUID"] ?? "").ToString();
                    HandleFirmSelectionFromWeb(firmGuid, confirmSelection: true);
                }
                else if (string.Equals(action, "firm-cancel", StringComparison.OrdinalIgnoreCase))
                {
                    firmSelectionTcs?.TrySetResult(null);
                    ShowStatus("Firma seçimi iptal edildi.", true);
                    SetControlsEnabled(true);
                    _ = ShowLoginViewAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HtmlLayout_WebMessageReceived parse error: {ex.Message}");
            }
        }


        private void CmbEmail_EditValueChanged(object sender, EventArgs e)
        {
            txtPassword.Text = "";
            lblStatus.Text = "";
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                BtnLogin_Click(sender, e);
            }
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            await AuthenticateAsync();
        }

        private async void BtnForgotPassword_Click(object sender, EventArgs e)
        {
            string email = cmbEmail.EditValue?.ToString()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowStatus("Lütfen e-posta adresinizi girin.", true);
                return;
            }

            if (apiClient == null)
            {
                ShowStatus("API bağlantısı kurulamadı.", true);
                return;
            }

            try
            {
                ShowStatus("Şifre sıfırlama talebi gönderiliyor...", false);
                btnForgotPassword.Enabled = false;
                btnLogin.Enabled = false;

                await apiClient.RequestPasswordResetAsync(email);

                MessageBox.Show(
                    "Şifre sıfırlama talebi gönderildi.\nLütfen e-postanızı kontrol edin.",
                    "Şifre Sıfırlama",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ShowStatus("", false);
            }
            catch (Exception ex)
            {
                ShowStatus($"Hata: {ex.Message}", true);
            }
            finally
            {
                btnForgotPassword.Enabled = true;
                btnLogin.Enabled = true;
            }
        }

        #endregion

        #region Authentication Logic

        private async Task AuthenticateAsync()
        {
            string email = cmbEmail.EditValue?.ToString()?.Trim() ?? "";
            string password = txtPassword.Text?.Trim() ?? "";

            // Validation
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowStatus("Lütfen e-posta adresinizi girin.", true);
                cmbEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowStatus("Lütfen şifrenizi girin.", true);
                txtPassword.Focus();
                return;
            }

            if (apiClient == null)
            {
                ShowStatus("API bağlantısı kurulamadı. Lütfen sistem yöneticinize başvurun.", true);
                return;
            }

            try
            {
                // Disable controls during auth
                SetControlsEnabled(false);
                ShowStatus("Giriş yapılıyor...", false);

                // Attempt login
                var loginResponse = await ExecuteWithRetryAsync(
                    () => apiClient.LoginAsync(email, password),
                    maxRetries: 3,
                    operationName: "Giriş"
                );

                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    // Store user info
                    v.tUser.UserId = (loginResponse.UserId != 0) ? loginResponse.UserId : loginResponse.OperatorId;
                    v.tUser.UserGUID = loginResponse.UserGUID;
                    v.tUser.FullName = loginResponse.FullName;
                    v.tUser.UserDbTypeId = loginResponse.DbTypeId;
                    v.tUser.eMail = email;
                    // NOTE(@Janberk): API should return both UserId and UserGUID; OperatorId maps legacy UserId usage.
                    // NOTE(@Janberk): Store JWT for subsequent DB-connection-info calls.
                    v.tUser.JwtToken = loginResponse.Token;
                    // TODO(@Janberk): Add refresh-token support and persist token securely with expiry tracking.

                    // Set auth token for subsequent API calls
                    apiClient.SetAuthToken(loginResponse.Token);

                    // Save registry
                    SaveUserRegistry(email, password);

                    ShowStatus("Firma bilgileri alınıyor...", false);
                    Application.DoEvents();

                    // Get user firms
                    var userFirmsList = await ExecuteWithRetryAsync(
                        () => apiClient.GetUserFirmsAsync(loginResponse.UserGUID),
                        maxRetries: 2,
                        operationName: "Firma bilgileri"
                    );

                    if (userFirmsList != null && userFirmsList.Count > 0)
                    {
                        if (userFirmsList.Count == 1)
                        {
                            // Single firm - auto-select
                            ShowStatus("Firma seçiliyor...", false);
                            Application.DoEvents();
                            await SelectFirmAsync(userFirmsList[0]);
                        }
                        else
                        {
                            // Multiple firms - open WebView2-based firm selector (falls back to dialog if unavailable)
                            ShowStatus("Firma seçimi bekleniyor...", false);
                            Application.DoEvents();
                            var selected = await PromptFirmSelectionAsync(userFirmsList);
                            if (selected != null)
                            {
                                ShowStatus("Firma seçiliyor...", false);
                                Application.DoEvents();
                                await SelectFirmAsync(selected);
                            }
                            else
                            {
                                ShowStatus("Firma seçimi iptal edildi.", true);
                                v.SP_UserLOGIN = false;
                                SetControlsEnabled(true);
                            }
                        }
                    }
                    else
                    {
                        ShowStatus("Kullanıcıya atanmış firma bulunamadı.", true);
                        v.SP_UserLOGIN = false;
                        SetControlsEnabled(true);
                    }
                }
                else
                {
                    ShowStatus("Giriş başarısız. Lütfen bilgilerinizi kontrol edin.", true);
                    v.SP_UserLOGIN = false;
                    SetControlsEnabled(true);
                }
            }
            catch (Exception ex)
            {
                HandleAuthException(ex, email);
                SetControlsEnabled(true);
            }
        }

        private async Task SelectFirmAsync(UstadApiClient.FirmInfo firm)
        {
            try
            {
                // NOTE(@Janberk): Step 1 - Select firm via API to get new token with firm claim
                ShowStatus("Firma seçiliyor...", false);
                Application.DoEvents();

                var selectFirmResponse = await ExecuteWithRetryAsync(
                    () => apiClient.SelectFirmAsync(firm.FirmGUID),
                    maxRetries: 2,
                    operationName: "Firma seçimi"
                );

                if (selectFirmResponse == null || string.IsNullOrEmpty(selectFirmResponse.Token))
                {
                    ShowStatus("Firma seçimi başarısız oldu.", true);
                    v.SP_UserLOGIN = false;
                    SetControlsEnabled(true);
                    return;
                }

                // NOTE(@Janberk): Update stored JWT token with firm claim
                v.tUser.JwtToken = selectFirmResponse.Token;
                if (selectFirmResponse.FirmId > 0)
                {
                    v.tUser.MainFirmId = selectFirmResponse.FirmId;
                }

                // NOTE(@Janberk): Step 2 - Fetch firm details for UI population
                ShowStatus("Firma bilgileri alınıyor...", false);
                Application.DoEvents();

                var firmDetails = await ExecuteWithRetryAsync(
                    () => apiClient.GetFirmDetailsAsync(firm.FirmGUID),
                    maxRetries: 2,
                    operationName: "Firma bilgileri"
                );

                if (firmDetails?.Firm != null)
                {
                    // Populate firm info (DB connections are opened later by tStarter after login closes)
                    v.tMainFirm.FirmId = firm.FirmId;
                    v.tMainFirm.FirmLongName = firm.FirmLongName ?? firmDetails.Firm.FirmLongName ?? "";
                    v.tMainFirm.FirmShortName = firm.FirmShortName ?? "";
                    v.tMainFirm.FirmGuid = firm.FirmGUID ?? firmDetails.Firm.FirmGUID ?? "";
                    v.tMainFirm.IlKodu = firm.CityTypeId?.ToString() ?? "";
                    v.tMainFirm.IlceKodu = firm.DistrictTypeId?.ToString() ?? "";
                    v.tMainFirm.MenuCode = firm.MenuCode ?? "";
                    v.tMainFirm.SectorTypeId = firm.SectorTypeId ?? 0;
                    v.tMainFirm.DatabaseType = "1"; // MSSQL
                    v.tMainFirm.DatabaseName = firm.DatabaseName ?? "";
                    v.tMainFirm.ServerNameIP = firm.ServerNameIP ?? "";
                    v.tMainFirm.DbLoginName = firm.DbLoginName ?? "";
                    v.tMainFirm.DbPassword = firm.DbPass ?? "";
                    v.tMainFirm.DbTypeId = firm.DbTypeId ?? 0;
                    v.tMainFirm.FirmMebbisCode = firm.MebbisCode ?? "";
                    v.tMainFirm.FirmMebbisPass = firm.MebbisPass ?? "";

                    // Save to registry
                    reg.SetUstadRegistry("userFirm" + v.tUser.UserId.ToString(), firm.FirmId.ToString());
                    reg.SetUstadRegistry("userLastFirm", firm.FirmId.ToString());
                    v.tUserRegister.UserLastFirmId = firm.FirmId;

                    // Mark login successful
                    v.SP_UserLOGIN = true;

                    t.setSelectFirm(v.tMainFirm); 

                    ShowStatus("Giriş başarılı! Yükleniyor...", false);
                    Application.DoEvents();

                    // Small delay to show success message
                    await Task.Delay(500);

                    // Show splash again before closing login form (main app is loading)
                    var splash = ms_WebViewSplash.ShowSplash();
                    Application.DoEvents();
                    
                    // Wait for splash to be ready (WebView2 initialized)
                    await ms_WebViewSplash.WaitForSplashReady(3000);
                    
                    ms_WebViewSplash.UpdateStatus("Uygulama yükleniyor...");
                    System.Diagnostics.Debug.WriteLine("[Login] Splash shown again - main app is loading");

                    // Close form and continue to main app
                    this.Close();
                }
                else
                {
                    ShowStatus("Firma bilgileri alınamadı.", true);
                    v.SP_UserLOGIN = false;
                    SetControlsEnabled(true);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Firma seçimi sırasında hata: {ex.Message}", true);
                v.SP_UserLOGIN = false;
                SetControlsEnabled(true);
            }
        }

        private void HandleAuthException(Exception ex, string email)
        {
            string errorMsg = ex.Message;
            bool isAuthError = false;

            if (ex.Data.Contains("StatusCode"))
            {
                int? statusCode = (int?)ex.Data["StatusCode"];
                isAuthError = statusCode == 401;
            }

            if (isAuthError || errorMsg.Contains("401") || errorMsg.Contains("Unauthorized") ||
                errorMsg.Contains("Şifre hatalı") || errorMsg.Contains("Kullanıcı bulunamadı"))
            {
                ShowStatus("E-posta veya şifre hatalı. Lütfen tekrar deneyin.", true);
            }
            else if (errorMsg.Contains("connection") || errorMsg.Contains("timeout") ||
                     errorMsg.Contains("network") || errorMsg.Contains("API connection error"))
            {
                ShowStatus("API bağlantısı kurulamadı. Lütfen internet bağlantınızı kontrol edin.", true);
            }
            else
            {
                ShowStatus($"Hata: {errorMsg}", true);
            }

            v.SP_UserLOGIN = false;
        }

        #endregion

        #region Helper Methods

        private void InitializeApiClient()
        {
            try
            {
                string apiBaseUrl = Tkn_UstadAPI.tApiConfig.GetApiBaseUrl();
                apiClient = new UstadApiClient(apiBaseUrl);
            }
            catch (Exception ex)
            {
                ShowStatus($"API istemcisi başlatılamadı: {ex.Message}", true);
            }
        }

        private void LoadUserRegistry()
        {
            try
            {
                userFirms.GetUserRegistry(regPath);

                // Load email list from registry
                v.tUserRegister.UserLastLoginEMail = reg.getRegistryValue("userLastLoginEMail")?.ToString() ?? "";
                v.tUserRegister.UserRemember = reg.getRegistryValue("userRemember")?.ToString() == "True";

                // Load email history
                string emailList = reg.getRegistryValue("userEmailList")?.ToString() ?? "";
                if (!string.IsNullOrEmpty(emailList))
                {
                    string[] emails = emailList.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    cmbEmail.Properties.Items.AddRange(emails);
                }

                // Set last email
                if (!string.IsNullOrEmpty(v.tUserRegister.UserLastLoginEMail))
                {
                    cmbEmail.EditValue = v.tUserRegister.UserLastLoginEMail;
                }

                // Set remember checkbox
                chkRemember.Checked = v.tUserRegister.UserRemember;

                // Load last password if remember is checked
                if (v.tUserRegister.UserRemember)
                {
                    string lastKey = reg.getRegistryValue("userLastKey")?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(lastKey))
                    {
                        txtPassword.Text = lastKey;
                    }
                }

                // Load last firm ID
                string lastFirmId = reg.getRegistryValue("userLastFirm")?.ToString() ?? "";
                if (!string.IsNullOrEmpty(lastFirmId) && int.TryParse(lastFirmId, out int firmId))
                {
                    v.tUserRegister.UserLastFirmId = firmId;
                }

                if (webViewDomReady)
                {
                    _ = PushFormStateToWebViewAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user registry: {ex.Message}");
            }
        }

        private void SaveUserRegistry(string email, string password)
        {
            try
            {
                // Save last email
                reg.SetUstadRegistry("userLastLoginEMail", email);
                v.tUserRegister.UserLastLoginEMail = email;

                // Save remember preference
                reg.SetUstadRegistry("userRemember", chkRemember.Checked.ToString());
                v.tUserRegister.UserRemember = chkRemember.Checked;

                // Save password if remember is checked
                if (chkRemember.Checked)
                {
                    reg.SetUstadRegistry("userLastKey", password);
                    v.tUserRegister.UserLastKey = password;
                }
                else
                {
                    reg.SetUstadRegistry("userLastKey", "");
                    v.tUserRegister.UserLastKey = "";
                }

                // Update email list
                if (!cmbEmail.Properties.Items.Contains(email))
                {
                    cmbEmail.Properties.Items.Add(email);

                    // Save updated email list
                    // Build a string from items
                    var items = new List<string>();
                    foreach (var it in cmbEmail.Properties.Items)
                        items.Add(it?.ToString() ?? "");
                    string emailList = string.Join("|", items);
                    reg.SetUstadRegistry("userEmailList", emailList);
                }

                if (webViewDomReady)
                {
                    _ = PushFormStateToWebViewAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving user registry: {ex.Message}");
            }
        }

        private void ShowStatus(string message, bool isError)
        {
            lblStatus.Text = message;
            lblStatus.Appearance.ForeColor = isError ? Color.Red : Color.Green;
            Application.DoEvents();
            UpdateWebStatusInView(message, isError);
        }

        private async Task<UstadApiClient.FirmInfo?> PromptFirmSelectionAsync(IList<UstadApiClient.FirmInfo> firms)
        {
            if (firms == null || firms.Count == 0)
            {
                return null;
            }

            if (htmlLayout?.CoreWebView2 == null || !webViewDomReady)
            {
                return ShowFirmSelectionDialog(firms);
            }

            try
            {
                firmSelectionMap = firms
                    .Where(f => !string.IsNullOrWhiteSpace(f.FirmGUID))
                    .GroupBy(f => f.FirmGUID)
                    .Select(g => g.First())
                    .ToDictionary(f => f.FirmGUID, f => f);

                firmSelectionTcs = new TaskCompletionSource<UstadApiClient.FirmInfo?>();
                await PushFirmSelectionToWebView(firms);
                var selectedFirm = await firmSelectionTcs.Task;
                return selectedFirm;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PromptFirmSelectionAsync fallback: {ex.Message}");
                return ShowFirmSelectionDialog(firms);
            }
            finally
            {
                firmSelectionTcs = null;
            }
        }

        private async Task PushFirmSelectionToWebView(IList<UstadApiClient.FirmInfo> firms)
        {
            if (htmlLayout?.CoreWebView2 == null)
            {
                return;
            }

            var slimFirms = firms.Select(f => new
            {
                f.FirmId,
                f.FirmGUID,
                f.FirmLongName,
                f.FirmShortName,
                f.MenuCode,
                f.UserFullName,
                f.IsActive,
                f.DatabaseName,
                f.ServerNameIP
            }).ToList();

            var payload = new
            {
                firms = slimFirms,
                lastFirmId = v.tUserRegister.UserLastFirmId,
                userGUID = v.tUser.UserGUID ?? ""
            };

            string json = JsonConvert.SerializeObject(payload);
            string js = $"window.__ustadShowFirmSelection && window.__ustadShowFirmSelection({json});";
            await htmlLayout.CoreWebView2.ExecuteScriptAsync(js);
        }

        private void HandleFirmSelectionFromWeb(string firmGuid, bool confirmSelection)
        {
            if (string.IsNullOrWhiteSpace(firmGuid))
            {
                if (confirmSelection)
                {
                    firmSelectionTcs?.TrySetResult(null);
                }
                return;
            }

            if (firmSelectionMap != null && firmSelectionMap.TryGetValue(firmGuid, out var firm))
            {
                if (confirmSelection)
                {
                    firmSelectionTcs?.TrySetResult(firm);
                }
                else
                {
                    _ = HighlightSelectedFirmAsync(firmGuid);
                }
            }
        }

        private async Task HighlightSelectedFirmAsync(string firmGuid)
        {
            if (htmlLayout?.CoreWebView2 == null)
            {
                return;
            }

            string guidEscaped = JavaScriptStringEncode(firmGuid, true);
            string js = $"window.__ustadHighlightFirm && window.__ustadHighlightFirm({guidEscaped});";
            await htmlLayout.CoreWebView2.ExecuteScriptAsync(js);
        }

        private UstadApiClient.FirmInfo ShowFirmSelectionDialog(IList<UstadApiClient.FirmInfo> firms)
        {
            string apiBaseUrl = "";
            try
            {
                apiBaseUrl = Tkn_UstadAPI.tApiConfig.GetApiBaseUrl();
            }
            catch { }
            
            using (var dlg = new ms_UserFirmSelect(firms, v.tUser.UserGUID, apiBaseUrl))
            {
                var result = dlg.ShowDialog(this);
                if (result == DialogResult.OK && dlg.SelectedFirm != null)
                {
                    return dlg.SelectedFirm;
                }
            }
            return null;
        }

        private void SetControlsEnabled(bool enabled)
        {
            cmbEmail.Enabled = enabled;
            txtPassword.Enabled = enabled;
            chkRemember.Enabled = enabled;
            btnLogin.Enabled = enabled;
            btnForgotPassword.Enabled = enabled;
            Application.DoEvents();
        }

        private async Task ShowLoginViewAsync()
        {
            if (htmlLayout?.CoreWebView2 == null)
            {
                return;
            }

            await htmlLayout.CoreWebView2.ExecuteScriptAsync("window.__ustadShowLogin && window.__ustadShowLogin();");
            await PushFormStateToWebViewAsync();
        }

        private async Task PushFormStateToWebViewAsync()
        {
            if (htmlLayout?.CoreWebView2 == null || !webViewDomReady)
            {
                return;
            }

            string email = cmbEmail.EditValue?.ToString() ?? "";
            string password = chkRemember.Checked ? (txtPassword.Text ?? "") : "";
            string lastFirm = v.tUserRegister.UserLastFirmId > 0 ? v.tUserRegister.UserLastFirmId.ToString() : "";

            string js = $"window.__ustadSetFormState && window.__ustadSetFormState({JavaScriptStringEncode(email, true)}, {JavaScriptStringEncode(password, true)}, {(chkRemember.Checked ? "true" : "false")}, {JavaScriptStringEncode(lastFirm, true)});";
            await htmlLayout.CoreWebView2.ExecuteScriptAsync(js);
        }

        private async void UpdateWebStatusInView(string message, bool isError)
        {
            try
            {
                if (htmlLayout?.CoreWebView2 == null)
                {
                    return;
                }
                string encodedMsg = JavaScriptStringEncode(message, true);
                string js = $"window.__ustadSetStatus && window.__ustadSetStatus({encodedMsg}, {(isError ? "true" : "false")});";
                await htmlLayout.CoreWebView2.ExecuteScriptAsync(js);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateWebStatusInView failed: {ex.Message}");
            }
        }

        // Minimal JS string encoder (avoids System.Web dependency)
        private static string JavaScriptStringEncode(string value, bool addDoubleQuotes)
        {
            if (value == null)
                return addDoubleQuotes ? "\"\"" : string.Empty;

            var sb = new StringBuilder();
            if (addDoubleQuotes) sb.Append('\"');

            foreach (char c in value)
            {
                switch (c)
                {
                    case '\"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32 || c > 127)
                            sb.Append("\\u" + ((int)c).ToString("x4"));
                        else
                            sb.Append(c);
                        break;
                }
            }

            if (addDoubleQuotes) sb.Append('\"');
            return sb.ToString();
        }

        // Common retry helper (copied from ms_User)
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

                    // Don't retry on authentication/validation errors (common API 4xx responses)
                    if (ex.Data.Contains("StatusCode"))
                    {
                        int? statusCode = (int?)ex.Data["StatusCode"];
                        if (statusCode == 400 || statusCode == 401 || statusCode == 403 || statusCode == 404)
                        {
                            throw;
                        }
                    }

                    // Don't retry on validation errors
                    if (ex.Message.Contains("Eksik Bilgi") ||
                        ex.Message.Contains("geçersiz") ||
                        ex.Message.Contains("invalid"))
                    {
                        throw;
                    }

                    // If this is the last attempt, throw
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

            throw new Exception(
                $"{operationName} {maxRetries} deneme sonrasında başarısız oldu: {lastException?.Message}",
                lastException);
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                // Ensure CoreWebView2 initialized
                var env = await CoreWebView2Environment.CreateAsync();
                await htmlLayout.EnsureCoreWebView2Async(env);

                // Wire message handler
                htmlLayout.CoreWebView2.WebMessageReceived += HtmlLayout_WebMessageReceived;
                htmlLayout.CoreWebView2.DOMContentLoaded += async (_, __) =>
                {
                    webViewDomReady = true;
                    await PushFormStateToWebViewAsync();
                    
                    // Close splash screen when login form is ready (rendered)
                    ms_WebViewSplash.CloseSplash();
                    System.Diagnostics.Debug.WriteLine("[Login] Splash closed - login form is ready");
                };

                // Load template
                string html = LoadHtmlTemplateWithTokens();
                if (string.IsNullOrWhiteSpace(html))
                {
                    System.Diagnostics.Debug.WriteLine("HTML template was empty after loading. Check embedded resource or Templates\\LoginTemplate.html on disk.");
                    MessageBox.Show(
                        "WebView2 template not found or empty.\n" +
                        "Check that the embedded resource 'YesiLdefter.Forms.Templates.LoginTemplate.html' exists (Build Action = Embedded Resource)\n" +
                        "or place a copy at <appFolder>\\Templates\\LoginTemplate.html.",
                        "Template not found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    // Minimal fallback so UI is not completely empty
                    html = "<!doctype html><html><body style='background:#0f172a;color:#e5e7eb;font-family:Segoe UI;padding:20px;'>" +
                           "<h2>Template not loaded</h2><p>Check embedded resource or Templates\\LoginTemplate.html</p></body></html>";
                }

                htmlLayout.NavigateToString(html);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView2 init failed: {ex.Message}\n{ex.StackTrace}");
                // NOTE(@Janberk): Don't show error dialog - let the form show with fallback UI
                // The form will still be visible even if WebView2 fails, allowing user to see the error
                // MessageBox.Show(
                //     "WebView2 initialization failed: " + ex.Message + "\n\n" +
                //     "Ensure the WebView2 Runtime is installed on this machine and the Microsoft.Web.WebView2 NuGet package is compatible.",
                //     "WebView2 init error",
                //     MessageBoxButtons.OK,
                //     MessageBoxIcon.Error);
                
                // Show minimal fallback UI so form is still usable
                string fallbackHtml = "<!doctype html><html><head><meta charset='UTF-8'><style>body{font-family:Segoe UI;padding:40px;background:#f8f9fa;color:#111827;}h2{color:#295c00;}.error{color:#ea4335;margin-top:20px;}</style></head><body><h2>YesiLdefter Giriş</h2><div class='error'><strong>WebView2 yüklenemedi:</strong><br>" + 
                    System.Security.SecurityElement.Escape(ex.Message) + 
                    "<br><br>Lütfen WebView2 Runtime'ın yüklü olduğundan emin olun.</div></body></html>";
                try
                {
                    htmlLayout?.NavigateToString(fallbackHtml);
                }
                catch
                {
                    // If even fallback fails, form will still be visible (just empty)
                }
            }
        }

        // Load HTML/CSS template from embedded resource and inject tokens mirroring UstadDesignTokens.scss
        private string LoadHtmlTemplateWithTokens()
        {
            var asm = Assembly.GetExecutingAssembly();
            const string resourceName = "YesiLdefter.Forms.Templates.LoginTemplate.html";
            string matchedResource = null;

            try
            {
                var available = asm.GetManifestResourceNames();
                System.Diagnostics.Debug.WriteLine("Available embedded resources: " + string.Join(", ", available));

                // Try exact name first, then try to find by suffix (helps when default namespace changed)
                matchedResource = Array.Find(available, r => string.Equals(r, resourceName, StringComparison.OrdinalIgnoreCase))
                                  ?? Array.Find(available, r => r.EndsWith(".Templates.LoginTemplate.html", StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(matchedResource))
                {
                    System.Diagnostics.Debug.WriteLine($"Html template resource not found: {resourceName}");
                    // Try disk fallback: <app>\Templates\LoginTemplate.html
                    string fallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "LoginTemplate.html");
                    if (File.Exists(fallback))
                    {
                        System.Diagnostics.Debug.WriteLine($"Loading template from disk fallback: {fallback}");
                        string templateDisk = File.ReadAllText(fallback, Encoding.UTF8);
                        return ResolveTokens(templateDisk);
                    }

                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enumerating resources: {ex.Message}");
            }

            // Load embedded resource (use matchedResource if found, else try the constant name)
            using (var stream = asm.GetManifestResourceStream(matchedResource ?? resourceName))
            {
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine($"GetManifestResourceStream returned null for {(matchedResource ?? resourceName)}");
                    return string.Empty;
                }
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var template = reader.ReadToEnd();
                    return ResolveTokens(template);
                }
            }
        }

        private string ResolveTokens(string template)
        {
            string assetBase = "";
            string logoBase64 = "";
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string assetPath = Path.Combine(baseDir, "Templates", "public");
                // Ensure directory exists
                if (!Directory.Exists(assetPath))
                {
                    Directory.CreateDirectory(assetPath);
                }
                // Use file:// protocol for local files
                assetBase = new Uri(assetPath + Path.DirectorySeparatorChar).AbsoluteUri;

                // Try multiple paths for logo
                string[] logoPaths = new[]
                {
                    Path.Combine(assetPath, "yesildefter_horizontal_color.png"),
                    Path.Combine(assetPath, "yesildefter_horizontal.png"),
                    Path.Combine(baseDir, "yesildefter_horizontal_color.png"),
                    Path.Combine(baseDir, "yesildefter_horizontal.png"),
                    // Try relative to executable
                    Path.Combine(Application.StartupPath, "Templates", "public", "yesildefter_horizontal_color.png"),
                    Path.Combine(Application.StartupPath, "Templates", "public", "yesildefter_horizontal.png"),
                    Path.Combine(Application.StartupPath, "yesildefter_horizontal_color.png"),
                    Path.Combine(Application.StartupPath, "yesildefter_horizontal.png")
                };

                bool logoFound = false;
                foreach (string logoPath in logoPaths)
                {
                    if (File.Exists(logoPath))
                    {
                        try
                        {
                            byte[] logoBytes = File.ReadAllBytes(logoPath);
                            logoBase64 = "data:image/png;base64," + Convert.ToBase64String(logoBytes);
                            System.Diagnostics.Debug.WriteLine($"✓ Logo loaded successfully from: {logoPath}");
                            logoFound = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"✗ Error reading logo from {logoPath}: {ex.Message}");
                            continue;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✗ Logo not found at: {logoPath}");
                    }
                }

                if (!logoFound)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠ Logo file not found in any expected location. Expected locations:");
                    foreach (string path in logoPaths)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {path}");
                    }
                }

                // If still no logo, try embedded resource as last resort
                if (string.IsNullOrEmpty(logoBase64))
                {
                    logoBase64 = LoadLogoFromEmbeddedResource();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resolving asset-base: {ex.Message}");
                assetBase = "";
                // Try embedded resource as fallback
                if (string.IsNullOrEmpty(logoBase64))
                {
                    logoBase64 = LoadLogoFromEmbeddedResource();
                }
            }

            // Get API base URL from configuration
            string apiBaseUrl = Tkn_UstadAPI.tApiConfig.GetApiBaseUrl();

            return template
                .Replace("{{color-bg}}", UiTokens.ColorBg)
                .Replace("{{color-gradient-start}}", UiTokens.ColorGradientStart)
                .Replace("{{color-gradient-end}}", UiTokens.ColorGradientEnd)
                .Replace("{{color-card}}", UiTokens.ColorCard)
                .Replace("{{color-accent}}", UiTokens.ColorAccent)
                .Replace("{{color-accent2}}", UiTokens.ColorAccent2)
                .Replace("{{color-text}}", UiTokens.ColorText)
                .Replace("{{color-muted}}", UiTokens.ColorMuted)
                .Replace("{{radius}}", UiTokens.Radius)
                .Replace("{{shadow}}", UiTokens.Shadow)
                .Replace("{{font}}", UiTokens.Font)
                .Replace("{{asset-base}}", assetBase)
                .Replace("{{logo-base64}}", logoBase64)
                .Replace("{{api-base-url}}", apiBaseUrl);
        }

        private string LoadLogoFromEmbeddedResource()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                string[] resourceNames = new[]
                {
                    "YesiLdefter.Forms.Templates.public.yesildefter_horizontal_color.png",
                    "YesiLdefter.Forms.Templates.public.yesildefter_horizontal.png",
                    "YesiLdefter.Resources.yesildefter_horizontal_color.png",
                    "YesiLdefter.Resources.yesildefter_horizontal.png"
                };

                foreach (string resourceName in resourceNames)
                {
                    using (var stream = asm.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            byte[] buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);
                            string base64 = "data:image/png;base64," + Convert.ToBase64String(buffer);
                            System.Diagnostics.Debug.WriteLine($"Logo loaded from embedded resource: {resourceName}");
                            return base64;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading logo from embedded resource: {ex.Message}");
            }
            return "";
        }

        private static class UiTokens
        {
            // Mirroring key values from UstadDesignTokens.scss
            public const string ColorBg = "#0f172a";
            public const string ColorGradientStart = "#e0eadf";
            public const string ColorGradientEnd = "#eff2ef";
            public const string ColorCard = "rgba(255,255,255,0.06)";
            public const string ColorAccent = "#295c00";
            public const string ColorAccent2 = "#8bc34a";
            public const string ColorText = "#e5e7eb";
            public const string ColorMuted = "#94a3b8";
            public const string Radius = "16px";
            public const string Shadow = "0 20px 50px rgba(0,0,0,0.35)";
            public const string Font = "'Inter Tight', 'Segoe UI', sans-serif";
        }

        #endregion

    }
}