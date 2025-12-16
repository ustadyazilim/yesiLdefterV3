/* Core Namespace */
using DevExpress.XtraEditors;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
using System.Text;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
/* Internal Namespaces */
using Tkn_Registry;
using Tkn_UstadAPI;
using Tkn_Variable;
using Tkn_ToolBox;

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
            InitializeStandaloneComponents();
            LoadUserRegistry();
            InitializeApiClient();
        }

        #endregion

        #region Initialize Components (Programmatic - No DB Required)

        private void InitializeStandaloneComponents()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Giriş Yap";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
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
            // Focus on email field
            cmbEmail.Focus();

            // Initialize auth state
            v.SP_UserLOGIN = false;

            // Initialize WebView2 and load HTML template
            _ = InitializeWebViewAsync();
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
                bool remember = false;
                if (obj["remember"] != null && bool.TryParse(obj["remember"].ToString(), out bool rem))
                {
                    remember = rem;
                }

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


                ///// sil
                cmbEmail.Text = "tekinucar70@hotmail.com";
                txtPassword.Text = "7470";

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
                            // Multiple firms - open WebView2-based firm selector
                            ShowStatus("Firma seçimi bekleniyor...", false);
                            Application.DoEvents();
                            var selected = ShowFirmSelectionDialog(userFirmsList);
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
                ShowStatus("Firma bilgileri alınıyor...", false);

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

        private UstadApiClient.FirmInfo ShowFirmSelectionDialog(IList<UstadApiClient.FirmInfo> firms)
        {
            using (var dlg = new ms_UserFirmSelect(firms))
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
                MessageBox.Show(
                    "WebView2 initialization failed: " + ex.Message + "\n\n" +
                    "Ensure the WebView2 Runtime is installed on this machine and the Microsoft.Web.WebView2 NuGet package is compatible.",
                    "WebView2 init error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
                .Replace("{{font}}", UiTokens.Font);
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