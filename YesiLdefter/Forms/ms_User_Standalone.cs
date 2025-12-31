/* Core Namespace */
using DevExpress.XtraEditors;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        
        // WebView2 state
        private bool webViewDomReady = false;
        
        // Remember me data
        private string rememberedEmail = "";
        private string rememberedPassword = "";
        private bool rememberedRemember = false;
        
        // Firm selection from web
        private System.Threading.Tasks.TaskCompletionSource<UstadApiClient.FirmInfo> firmSelectionTcs = null;
        private IList<UstadApiClient.FirmInfo> userFirmsList = null;

        #endregion

        #region Constructor

        public ms_User_Standalone()
        {
            InitializeStandaloneComponents();
            InitializeApiClient();
            LoadUserRegistry();
        }

        #endregion

        #region Initialize Components (Programmatic - No DB Required)

        private void InitializeStandaloneComponents()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Giriş Yap";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(650, 500); 
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScroll = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;
            this.ShowInTaskbar = false; // Don't show in taskbar when shown as dialog

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
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Load error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Stack: {ex.StackTrace}");
            }
        }

        private void Ms_User_Standalone_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cleanup WebView2 resources
            if (htmlLayout != null && !htmlLayout.IsDisposed)
            {
                try
                {
                    htmlLayout.Dispose();
                }
                catch { }
            }
        }

        #endregion

        #region API Client Initialization

        private void InitializeApiClient()
        {
            try
            {
                string apiBaseUrl = Tkn_UstadAPI.tApiConfig.GetApiBaseUrl();
                apiClient = new UstadApiClient(apiBaseUrl);
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] API client initialized with base URL: {apiBaseUrl}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] API client initialization error: {ex.Message}");
                // Continue without API client - user will see error when trying to login
            }
        }

        #endregion

        #region User Registry (Remember Me)

        private void LoadUserRegistry()
        {
            try
            {
                object emailObj = reg.getRegistryValue("Email");
                string email = emailObj?.ToString() ?? "";
                object passwordObj = reg.getRegistryValue("Password");
                string password = passwordObj?.ToString() ?? "";
                object rememberObj = reg.getRegistryValue("Remember");
                bool remember = rememberObj != null && (rememberObj.ToString() == "True" || rememberObj.ToString() == "1");
                
                if (!string.IsNullOrEmpty(email) && remember)
                {
                    rememberedEmail = email;
                    rememberedPassword = password; // Load password if remember is true
                    rememberedRemember = remember;
                    System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Loaded remembered email: {email}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Registry load error: {ex.Message}");
            }
        }

        private void SaveUserRegistry(string email, string password)
        {
            try
            {
                bool remember = chkRemember?.Checked ?? false;
                reg.SetUstadRegistry("Email", remember ? email : "");
                reg.SetUstadRegistry("Password", remember ? password : ""); // Save password if remember is true
                reg.SetUstadRegistry("Remember", remember ? "True" : "False");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Registry save error: {ex.Message}");
            }
        }

        #endregion

        #region WebView2 Initialization and HTML Template Loading

        private async Task InitializeWebViewAsync()
        {
            try
            {
                await htmlLayout.EnsureCoreWebView2Async(null);
                
                System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] WebView2 CoreWebView2 ready");

                // Set up message handler for communication from HTML/JS to C#
                htmlLayout.CoreWebView2.WebMessageReceived += HtmlLayout_WebMessageReceived;
                
                // Set up DOM content loaded handler
                htmlLayout.CoreWebView2.DOMContentLoaded += async (s, e) =>
                {
                    webViewDomReady = true;
                    System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] WebView2 DOM content loaded");
                    
                    // Populate form with remembered credentials if available
                    if (!string.IsNullOrEmpty(rememberedEmail) && rememberedRemember)
                    {
                        await PopulateRememberedCredentialsAsync();
                    }
                };

                // Load HTML template
                string html = LoadLoginTemplate();
                htmlLayout.CoreWebView2.NavigateToString(html);
                
                System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] HTML template loaded into WebView2");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] WebView2 initialization error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Stack: {ex.StackTrace}");
                ShowStatus($"WebView2 başlatma hatası: {ex.Message}", true);
            }
        }

        #endregion

        #region WebView2 Message Handler (HTML/JS → C#)

        private void HtmlLayout_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message = e.TryGetWebMessageAsString();
                if (string.IsNullOrEmpty(message))
                    return;

                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] WebMessage received: {message}");

                var payload = JObject.Parse(message);
                string action = payload["action"]?.ToString();

                switch (action)
                {
                    case "login":
                        string email = payload["email"]?.ToString();
                        string password = payload["password"]?.ToString();
                        bool remember = payload["remember"]?.ToObject<bool>() ?? false;
                        _ = HandleLoginAsync(email, password, remember);
                        break;

                    case "rememberChanged":
                        bool rem = payload["remember"]?.ToObject<bool>() ?? false;
                        if (chkRemember != null)
                        {
                            chkRemember.Checked = rem;
                        }
                        break;

                    case "firm-select":
                        string firmGUID = payload["firmGUID"]?.ToString();
                        HandleFirmSelectionFromWeb(firmGUID, false);
                        break;

                    case "firm-confirm":
                        string confirmFirmGUID = payload["firmGUID"]?.ToString();
                        HandleFirmSelectionFromWeb(confirmFirmGUID, true);
                        break;

                    case "firm-cancel":
                        ShowStatus("Firma seçimi iptal edildi.", true);
                        _ = ShowLoginViewAsync();
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Unknown action: {action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] WebMessage handler error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Stack: {ex.StackTrace}");
            }
        }

        #endregion

        #region Login Flow

        private async Task HandleLoginAsync(string email, string password, bool remember)
        {
            if (apiClient == null)
            {
                ShowStatus("API bağlantısı kurulamadı. Lütfen tekrar deneyin.", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowStatus("E-posta ve şifre gereklidir.", true);
                return;
            }

            SetControlsEnabled(false);
            ShowStatus("Giriş yapılıyor...", false);

            try
            {
                var loginResponse = await ExecuteWithRetryAsync(
                    () => apiClient.LoginAsync(email, password),
                    maxRetries: 2,
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
                    userFirmsList = await ExecuteWithRetryAsync(
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
                        }
                    }
                    else
                    {
                        ShowStatus("Bu kullanıcı için firma bulunamadı.", true);
                        SetControlsEnabled(true);
                        _ = ShowLoginViewAsync();
                    }
                }
                else
                {
                    ShowStatus("Giriş başarısız. Lütfen bilgilerinizi kontrol edin.", true);
                    SetControlsEnabled(true);
                    _ = ShowLoginViewAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Login error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Stack: {ex.StackTrace}");
                ShowStatus($"Giriş hatası: {ex.Message}", true);
                SetControlsEnabled(true);
                _ = ShowLoginViewAsync();
            }
        }

        private async Task SelectFirmAsync(UstadApiClient.FirmInfo firm)
        {
            try
            {
                ShowStatus("Firma seçiliyor...", false);
                SetControlsEnabled(false);

                var selectFirmResponse = await ExecuteWithRetryAsync(
                    () => apiClient.SelectFirmAsync(firm.FirmGUID),
                    maxRetries: 2,
                    operationName: "Firma seçimi"
                );

                if (selectFirmResponse != null && !string.IsNullOrEmpty(selectFirmResponse.Token))
                {
                    // Update token
                    v.tUser.JwtToken = selectFirmResponse.Token;
                    apiClient.SetAuthToken(selectFirmResponse.Token);

                    // Store firm info in v
                    v.SP_FIRM_ID = selectFirmResponse.FirmId;
                    v.tMainFirm.FirmId = selectFirmResponse.FirmId;
                    v.tMainFirm.FirmGuid = firm.FirmGUID ?? "";
                    v.tMainFirm.FirmLongName = firm.FirmLongName ?? "";
                    v.tMainFirm.FirmShortName = firm.FirmShortName ?? "";
                    v.tMainFirm.DatabaseName = firm.DatabaseName ?? "";
                    v.tMainFirm.ServerNameIP = firm.ServerNameIP ?? "";
                    v.tMainFirm.DbLoginName = firm.DbLoginName ?? "";
                    v.tMainFirm.DbPassword = firm.DbPass ?? "";
                    v.tMainFirm.DbTypeId = firm.DbTypeId ?? (short)0;
                    v.tMainFirm.SectorTypeId = firm.SectorTypeId ?? (short)0;
                    v.SP_Firm_SectorTypeId = firm.SectorTypeId ?? (short)0;

                    // Initialize database connection using firm info
                    t.setSelectFirm(v.tMainFirm);
                    
                    // Save firm selection to registry
                    reg.SetUstadRegistry("userFirm" + v.tUser.UserId.ToString(), v.tMainFirm.FirmId.ToString());
                    reg.SetUstadRegistry("userLastFirm", v.tMainFirm.FirmId.ToString());
                    v.tUserRegister.UserLastFirmId = v.tMainFirm.FirmId;

                    // Mark login as successful
                    v.SP_UserLOGIN = true;

                    // Close this form and return DialogResult.OK
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowStatus("Firma seçimi başarısız.", true);
                    SetControlsEnabled(true);
                    _ = ShowLoginViewAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] SelectFirm error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Stack: {ex.StackTrace}");
                ShowStatus($"Firma seçimi hatası: {ex.Message}", true);
                SetControlsEnabled(true);
                _ = ShowLoginViewAsync();
            }
        }

        #endregion

        #region Firm Selection Dialog

        private UstadApiClient.FirmInfo ShowFirmSelectionDialog(IList<UstadApiClient.FirmInfo> firms)
        {
            string apiBaseUrl = "";
            try
            {
                apiBaseUrl = Tkn_UstadAPI.tApiConfig.GetApiBaseUrl();
            }
            catch { }
            
            // Get userGUID from v.tUser (should be set after login)
            string userGUID = v.tUser?.UserGUID ?? string.Empty;
            
            using (var dlg = new ms_UserFirmSelect(firms, userGUID, apiBaseUrl))
            {
                var result = dlg.ShowDialog(this);
                if (result == DialogResult.OK && dlg.SelectedFirm != null)
                {
                    return dlg.SelectedFirm;
                }
            }
            return null;
        }

        private async void HandleFirmSelectionFromWeb(string firmGUID, bool confirmSelection)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(firmGUID))
                {
                    ShowStatus("Geçersiz firma GUID.", true);
                    return;
                }

                if (userFirmsList == null || userFirmsList.Count == 0)
                {
                    ShowStatus("Firma listesi yüklenmedi. Lütfen tekrar giriş yapın.", true);
                    return;
                }

                // Find firm by GUID
                var firm = userFirmsList.FirstOrDefault(f => 
                    string.Equals(f.FirmGUID, firmGUID, StringComparison.OrdinalIgnoreCase));

                if (firm == null)
                {
                    ShowStatus("Seçilen firma bulunamadı.", true);
                    return;
                }

                if (confirmSelection)
                {
                    // User confirmed - proceed with firm selection
                    await SelectFirmAsync(firm);
                    firmSelectionTcs?.TrySetResult(firm); // Resolve the TCS
                }
                else
                {
                    // User just selected (not confirmed yet) - just update UI state
                    // The web UI will handle the visual selection
                    System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Firm selected (not confirmed): {firm.FirmLongName}");
                    // Optionally, push a message back to the webview to highlight the firm
                    await htmlLayout.CoreWebView2.ExecuteScriptAsync($"window.__ustadHighlightFirm && window.__ustadHighlightFirm('{firm.FirmGUID}');");
                    firmSelectionTcs?.TrySetResult(null); // Keep waiting or reset if needed
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] HandleFirmSelectionFromWeb error: {ex.Message}");
                ShowStatus($"Firma seçimi sırasında hata: {ex.Message}", true);
                SetControlsEnabled(true);
                firmSelectionTcs?.TrySetException(ex);
            }
        }

        #endregion

        #region UI State Management

        private void SetControlsEnabled(bool enabled)
        {
            if (btnLogin != null) btnLogin.Enabled = enabled;
            if (cmbEmail != null) cmbEmail.Enabled = enabled;
            if (txtPassword != null) txtPassword.Enabled = enabled;
            if (chkRemember != null) chkRemember.Enabled = enabled;
        }

        private void ShowStatus(string message, bool isError)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = isError ? Color.Red : Color.Black;
            }

            // Also send status to WebView2
            if (htmlLayout != null && htmlLayout.CoreWebView2 != null && webViewDomReady)
            {
                _ = htmlLayout.CoreWebView2.ExecuteScriptAsync($"window.__ustadUpdateStatus && window.__ustadUpdateStatus({Newtonsoft.Json.JsonConvert.SerializeObject(new { message = message, isError = isError })});");
            }

            System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Status: {message}");
        }

        private async Task ShowLoginViewAsync()
        {
            if (htmlLayout != null && htmlLayout.CoreWebView2 != null)
            {
                await htmlLayout.CoreWebView2.ExecuteScriptAsync("window.__ustadShowLogin && window.__ustadShowLogin();");
            }
        }

        private async Task PopulateRememberedCredentialsAsync()
        {
            if (htmlLayout != null && htmlLayout.CoreWebView2 != null && webViewDomReady)
            {
                try
                {
                    string script = $@"
                        if (window.__ustadSetFormState) {{
                            window.__ustadSetFormState(
                                {Newtonsoft.Json.JsonConvert.SerializeObject(rememberedEmail)},
                                {Newtonsoft.Json.JsonConvert.SerializeObject(rememberedPassword)},
                                {Newtonsoft.Json.JsonConvert.SerializeObject(rememberedRemember)},
                                null
                            );
                        }}
                    ";
                    await htmlLayout.CoreWebView2.ExecuteScriptAsync(script);
                    System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Populated form with remembered credentials: {rememberedEmail}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Error populating remembered credentials: {ex.Message}");
                }
            }
        }

        #endregion

        #region HTML Template Loading

        private string LoadLoginTemplate()
        {
            // Try embedded resource first
            var asm = Assembly.GetExecutingAssembly();
            const string resourceName = "YesiLdefter.Forms.Templates.LoginTemplate.html";
            string matchedResource = null;

            try
            {
                var available = asm.GetManifestResourceNames();
                System.Diagnostics.Debug.WriteLine("[ms_User_Standalone] Available embedded resources: " + string.Join(", ", available));

                matchedResource = Array.Find(available, r => string.Equals(r, resourceName, StringComparison.OrdinalIgnoreCase))
                                  ?? Array.Find(available, r => r.EndsWith(".Templates.LoginTemplate.html", StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(matchedResource))
                {
                    System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Html template resource not found: {resourceName}");
                    // Fallback to file system
                    string fallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "LoginTemplate.html");
                    if (File.Exists(fallback))
                    {
                        System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Loading template from disk fallback: {fallback}");
                        return File.ReadAllText(fallback, Encoding.UTF8);
                    }
                    return "<!doctype html><html><body style='background:#f0f2f5;color:#333;font-family:Segoe UI;padding:20px;'><h2>Template not loaded</h2><p>Login template not found. Please contact support.</p></body></html>";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Error enumerating resources: {ex.Message}");
            }

            using (var stream = asm.GetManifestResourceStream(matchedResource ?? resourceName))
            {
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] GetManifestResourceStream returned null for {(matchedResource ?? resourceName)}");
                    // Fallback to file system
                    string fallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "LoginTemplate.html");
                    if (File.Exists(fallback))
                    {
                        System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] Loading template from disk fallback: {fallback}");
                        return File.ReadAllText(fallback, Encoding.UTF8);
                    }
                    return "<!doctype html><html><body style='background:#f0f2f5;color:#333;font-family:Segoe UI;padding:20px;'><h2>Template not loaded</h2><p>Login template not found. Please contact support.</p></body></html>";
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

                // Try multiple paths for logo (including Forms/Templates/public where source files are)
                string[] logoPaths = new string[]
                {
                    Path.Combine(baseDir, "Forms", "Templates", "public", "yesildefter_horizontal.png"),
                    Path.Combine(baseDir, "Templates", "public", "yesildefter_horizontal.png"),
                    Path.Combine(baseDir, "yesildefter_horizontal.png"),
                    Path.Combine(baseDir, "Forms", "Templates", "public", "yesildefter_horizontal_color.png"),
                    Path.Combine(baseDir, "Templates", "public", "yesildefter_horizontal_color.png"),
                    Path.Combine(baseDir, "yesildefter_horizontal_color.png")
                };

                foreach (string logoPath in logoPaths)
                {
                    if (File.Exists(logoPath))
                    {
                        byte[] logoBytes = File.ReadAllBytes(logoPath);
                        logoBase64 = "data:image/png;base64," + Convert.ToBase64String(logoBytes);
                        System.Diagnostics.Debug.WriteLine($"[Logo] Logo loaded from: {logoPath}");
                        break;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✗ Logo not found at: {logoPath}");
                    }
                }

                // If still no logo, try embedded resource
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
                .Replace("{{color-bg}}", "#f0f2f5")
                .Replace("{{color-gradient-start}}", "#f8fafc")
                .Replace("{{color-gradient-end}}", "#e5e7eb")
                .Replace("{{color-card}}", "#ffffff")
                .Replace("{{color-accent}}", "#295c00")
                .Replace("{{color-accent2}}", "#5a7323")
                .Replace("{{color-text}}", "#1f2937")
                .Replace("{{color-muted}}", "#6b7280")
                .Replace("{{radius}}", "8px")
                .Replace("{{shadow}}", "0 2px 8px rgba(0,0,0,0.1)")
                .Replace("{{font}}", "Segoe UI, -apple-system, BlinkMacSystemFont, sans-serif")
                .Replace("{{asset-base}}", assetBase)
                .Replace("{{logo-base64}}", logoBase64)
                .Replace("{{api-base-url}}", apiBaseUrl);
        }

        private string LoadLogoFromEmbeddedResource()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var resources = asm.GetManifestResourceNames();
                System.Diagnostics.Debug.WriteLine("[Logo] Available embedded resources: " + string.Join(", ", resources));

                // Try to find logo in embedded resources
                var logoResource = Array.Find(resources, r => r.Contains("yesildefter") && r.Contains("horizontal") && r.EndsWith(".png"));
                if (logoResource == null)
                {
                    // Try alternative: look for any .png in g.resources
                    logoResource = Array.Find(resources, r => r.Contains("g.resources"));
                }

                if (logoResource != null)
                {
                    using (var stream = asm.GetManifestResourceStream(logoResource))
                    {
                        if (stream != null)
                        {
                            // For .resx/.resources files, we need ResourceManager
                            var resourceManager = new System.Resources.ResourceManager("YesiLdefter.Properties.Resources", asm);
                            try
                            {
                                var logoObj = resourceManager.GetObject("yesildefter_horizontal");
                                if (logoObj is System.Drawing.Bitmap bitmap)
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                        byte[] logoBytes = ms.ToArray();
                                        string base64 = "data:image/png;base64," + Convert.ToBase64String(logoBytes);
                                        System.Diagnostics.Debug.WriteLine($"✓ Logo loaded from embedded resource: {logoResource}");
                                        return base64;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[Logo] Error loading from ResourceManager: {ex.Message}");
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("[Logo] ⚠ Logo file not found in any expected location.");
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Logo] Error loading logo from embedded resource: {ex.Message}");
                return "";
            }
        }

        #endregion

        #region Helper Methods

        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, string operationName = "Operation")
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
                    System.Diagnostics.Debug.WriteLine($"[ms_User_Standalone] {operationName} attempt {attempt}/{maxRetries} failed: {ex.Message}");
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(1000 * attempt); // Exponential backoff
                    }
                }
            }
            throw lastException ?? new Exception($"{operationName} failed after {maxRetries} attempts");
        }

        #endregion

        #region Event Handlers (Legacy UI - Not Used in WebView2 Mode)

        private void CmbEmail_EditValueChanged(object sender, EventArgs e)
        {
            // Handled by WebView2 HTML/JS
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            // Handled by WebView2 HTML/JS
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // Handled by WebView2 HTML/JS
        }

        private void BtnForgotPassword_Click(object sender, EventArgs e)
        {
            // Handled by WebView2 HTML/JS
        }

        #endregion
    }
}
