/* Core Namespace */
using DevExpress.XtraEditors;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
/* Internal Namespaces */
using Tkn_Registry;
using Tkn_UstadAPI;
using Tkn_Variable;

namespace YesiLdefter
{
    /// <summary>
    /// Standalone login form that does NOT require database connection
    /// This form is self-contained and creates all controls programmatically
    /// Used for secure authentication flow where DB is only accessed AFTER successful login
    /// </summary>
    public partial class ms_User_Standalone : XtraForm
    {
        #region Fields
        
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
            
            // Logo (top center)
            picLogo = new PictureEdit();
            picLogo.Location = new Point(175, 20);
            picLogo.Size = new Size(100, 60);
            picLogo.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Never;
            picLogo.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            // Note: Logo image can be loaded from resources if available
            
            // Title
            lblTitle = new LabelControl();
            lblTitle.Text = "Hoş Geldiniz";
            lblTitle.Location = new Point(150, 90);
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle.AutoSizeMode = LabelAutoSizeMode.None;
            lblTitle.Size = new Size(150, 30);
            lblTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            
            // Email label
            lblEmail = new LabelControl();
            lblEmail.Text = "E-posta / TC No / Telefon";
            lblEmail.Location = new Point(50, 140);
            lblEmail.AutoSizeMode = LabelAutoSizeMode.None;
            lblEmail.Size = new Size(350, 20);
            lblEmail.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblEmail.Appearance.ForeColor = Color.FromArgb(100, 100, 100);
            
            // Email combobox
            cmbEmail = new ComboBoxEdit();
            cmbEmail.Location = new Point(50, 162);
            cmbEmail.Size = new Size(350, 32);
            cmbEmail.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
            cmbEmail.Properties.Appearance.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            cmbEmail.TabIndex = 0;
            cmbEmail.EditValueChanged += CmbEmail_EditValueChanged;
            
            // Password label
            lblPassword = new LabelControl();
            lblPassword.Text = "Şifre";
            lblPassword.Location = new Point(50, 204);
            lblPassword.AutoSizeMode = LabelAutoSizeMode.None;
            lblPassword.Size = new Size(350, 20);
            lblPassword.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblPassword.Appearance.ForeColor = Color.FromArgb(100, 100, 100);
            
            // Password textbox
            txtPassword = new TextEdit();
            txtPassword.Location = new Point(50, 226);
            txtPassword.Size = new Size(350, 32);
            txtPassword.Properties.PasswordChar = '●';
            txtPassword.Properties.Appearance.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            txtPassword.TabIndex = 1;
            txtPassword.KeyDown += TxtPassword_KeyDown;
            
            // Remember checkbox
            chkRemember = new CheckEdit();
            chkRemember.Text = "Beni Hatırla";
            chkRemember.Location = new Point(50, 268);
            chkRemember.Size = new Size(150, 24);
            chkRemember.TabIndex = 2;
            chkRemember.Properties.Appearance.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            
            // Login button
            btnLogin = new SimpleButton();
            btnLogin.Text = "Giriş Yap";
            btnLogin.Location = new Point(250, 302);
            btnLogin.Size = new Size(150, 36);
            btnLogin.TabIndex = 3;
            btnLogin.Click += BtnLogin_Click;
            btnLogin.Appearance.BackColor = Color.FromArgb(34, 139, 34);
            btnLogin.Appearance.ForeColor = Color.White;
            btnLogin.Appearance.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnLogin.Appearance.Options.UseBackColor = true;
            btnLogin.Appearance.Options.UseForeColor = true;
            btnLogin.Appearance.Options.UseFont = true;
            
            // Forgot password button
            btnForgotPassword = new SimpleButton();
            btnForgotPassword.Text = "Şifremi Unuttum";
            btnForgotPassword.Location = new Point(50, 302);
            btnForgotPassword.Size = new Size(150, 36);
            btnForgotPassword.TabIndex = 4;
            btnForgotPassword.Click += BtnForgotPassword_Click;
            btnForgotPassword.Appearance.BackColor = Color.Transparent;
            btnForgotPassword.Appearance.ForeColor = Color.FromArgb(100, 100, 100);
            btnForgotPassword.Appearance.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            btnForgotPassword.Appearance.Options.UseBackColor = true;
            btnForgotPassword.Appearance.Options.UseForeColor = true;
            btnForgotPassword.Appearance.Options.UseFont = true;
            btnForgotPassword.Appearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnForgotPassword.Appearance.Options.UseBorderColor = true;
            
            // Status label
            lblStatus = new LabelControl();
            lblStatus.Text = "";
            lblStatus.Location = new Point(50, 348);
            lblStatus.AutoSizeMode = LabelAutoSizeMode.Vertical;
            lblStatus.Size = new Size(350, 30);
            lblStatus.Appearance.ForeColor = Color.Red;
            lblStatus.Appearance.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblStatus.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lblStatus.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            
            // Add controls to form
            this.Controls.Add(picLogo);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblEmail);
            this.Controls.Add(cmbEmail);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(chkRemember);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnForgotPassword);
            this.Controls.Add(lblStatus);
            
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
                    v.tUser.JwtToken = loginResponse.Token;
                    
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
                            // Multiple firms - show selection (to be implemented with standalone firm selection)
                            ShowStatus("Firma seçiliyor...", false);
                            Application.DoEvents();
                            // TODO: Implement standalone firm selection dialog
                            // For now, auto-select first firm
                            await SelectFirmAsync(userFirmsList[0]);
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
                    // Populate firm info
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
                    string emailList = string.Join("|", cmbEmail.Properties.Items.GetEnumerator());
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
                    
                    // Don't retry on authentication errors
                    if (ex.Data.Contains("StatusCode"))
                    {
                        int? statusCode = (int?)ex.Data["StatusCode"];
                        if (statusCode == 401 || statusCode == 403)
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
        
        #endregion
    }
}

