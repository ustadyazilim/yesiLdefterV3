using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UstadDesktop.WhatsAppIntegration.WinFormsClient.Forms
{
    /// <summary>
    /// Login form for WhatsApp operator authentication
    /// Integrates with Ustad Desktop design patterns and DevExpress components
    /// </summary>
    public partial class LoginForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        // Login response properties
        public string JwtToken { get; private set; }
        public int OperatorId { get; private set; }
        public string FullName { get; private set; }
        public string Role { get; private set; }
        public int FirmId { get; private set; }
        public string UserName { get; private set; }

        public LoginForm(string apiBaseUrl = "http://localhost:5000")
        {
            InitializeComponent();
            _apiBaseUrl = apiBaseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_apiBaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Apply Ustad Desktop styling
            ApplyUstadStyling();
            
            // Load saved credentials if available
            LoadSavedCredentials();
        }

        private void ApplyUstadStyling()
        {
            // Apply consistent styling with main Ustad Desktop application
            this.Text = "Ustad WhatsApp - Operatör Girişi";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Set Ustad color scheme
            this.BackColor = System.Drawing.ColorTranslator.FromHtml("#F5F7F9");
            
            // Configure login button
            btnLogin.Appearance.BackColor = System.Drawing.ColorTranslator.FromHtml("#295C00");
            btnLogin.Appearance.ForeColor = System.Drawing.Color.White;
            btnLogin.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            
            // Configure cancel button
            btnCancel.Appearance.BackColor = System.Drawing.ColorTranslator.FromHtml("#6B7280");
            btnCancel.Appearance.ForeColor = System.Drawing.Color.White;
            
            // Configure forgot password link
            lnkForgotPassword.Appearance.ForeColor = System.Drawing.ColorTranslator.FromHtml("#295C00");
        }

        private void LoadSavedCredentials()
        {
            try
            {
                // Load from registry or application settings
                var savedUsername = Microsoft.Win32.Registry.CurrentUser
                    .OpenSubKey(@"Software\Ustad\WhatsApp", false)
                    ?.GetValue("LastUsername") as string;

                if (!string.IsNullOrEmpty(savedUsername))
                {
                    txtUsername.Text = savedUsername;
                    chkRememberMe.Checked = true;
                    txtPassword.Focus();
                }
                else
                {
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't show to user
                System.Diagnostics.Debug.WriteLine($"Error loading saved credentials: {ex.Message}");
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            await PerformLogin();
        }

        private async void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                await PerformLogin();
            }
        }

        private async Task PerformLogin()
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;

            // Validate input
            if (string.IsNullOrEmpty(username))
            {
                ShowError("Kullanıcı adı boş olamaz.");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Şifre boş olamaz.");
                txtPassword.Focus();
                return;
            }

            // Show loading state
            SetLoadingState(true);

            try
            {
                var loginRequest = new
                {
                    UserName = username,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync("/auth/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    
                    // Store login information
                    JwtToken = loginResponse.Token;
                    OperatorId = loginResponse.OperatorId;
                    FullName = loginResponse.FullName;
                    Role = loginResponse.Role;
                    FirmId = loginResponse.FirmId;
                    UserName = username;

                    // Save credentials if remember me is checked
                    if (chkRememberMe.Checked)
                    {
                        SaveCredentials(username);
                    }

                    // Log successful login
                    LogLoginAttempt(username, true, "Başarılı giriş");

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    string errorMessage = "Giriş başarısız.";
                    
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.Unauthorized:
                            errorMessage = "Kullanıcı adı veya şifre hatalı.";
                            break;
                        case (System.Net.HttpStatusCode)423: // Locked
                            errorMessage = "Hesabınız kilitlenmiştir. Lütfen daha sonra tekrar deneyin.";
                            break;
                        case System.Net.HttpStatusCode.BadRequest:
                            var errorContent = await response.Content.ReadAsStringAsync();
                            if (errorContent.Contains("Account disabled"))
                                errorMessage = "Hesabınız devre dışı bırakılmıştır. Yöneticinizle iletişime geçin.";
                            break;
                    }

                    ShowError(errorMessage);
                    LogLoginAttempt(username, false, errorMessage);
                    
                    // Clear password on failed attempt
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (HttpRequestException ex)
            {
                ShowError($"Sunucuya bağlanılamadı. Lütfen ağ bağlantınızı kontrol edin.\n\nHata: {ex.Message}");
                LogLoginAttempt(username, false, $"Bağlantı hatası: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                ShowError("Bağlantı zaman aşımına uğradı. Lütfen tekrar deneyin.");
                LogLoginAttempt(username, false, "Bağlantı zaman aşımı");
            }
            catch (Exception ex)
            {
                ShowError($"Beklenmeyen bir hata oluştu.\n\nHata: {ex.Message}");
                LogLoginAttempt(username, false, $"Beklenmeyen hata: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool loading)
        {
            btnLogin.Enabled = !loading;
            btnCancel.Enabled = !loading;
            txtUsername.Enabled = !loading;
            txtPassword.Enabled = !loading;
            chkRememberMe.Enabled = !loading;
            lnkForgotPassword.Enabled = !loading;

            if (loading)
            {
                btnLogin.Text = "Giriş yapılıyor...";
                this.Cursor = Cursors.WaitCursor;
            }
            else
            {
                btnLogin.Text = "Giriş Yap";
                this.Cursor = Cursors.Default;
            }
        }

        private void ShowError(string message)
        {
            DevExpress.XtraEditors.XtraMessageBox.Show(
                message,
                "Giriş Hatası",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        private void SaveCredentials(string username)
        {
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Ustad\WhatsApp");
                key?.SetValue("LastUsername", username);
                key?.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving credentials: {ex.Message}");
            }
        }

        private void LogLoginAttempt(string username, bool success, string details)
        {
            try
            {
                // Log to Windows Event Log or local file
                var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Login attempt for '{username}': {(success ? "SUCCESS" : "FAILED")} - {details}";
                System.Diagnostics.Debug.WriteLine(logMessage);
                
                // TODO: Integrate with Ustad's existing logging system
                // This should use the same logging mechanism as the main application
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging login attempt: {ex.Message}");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void lnkForgotPassword_Click(object sender, EventArgs e)
        {
            // Show password reset form
            using (var resetForm = new PasswordResetForm(_apiBaseUrl))
            {
                resetForm.ShowDialog(this);
            }
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _httpClient?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            base.Dispose(disposing);
        }

        // Response model matching the API
        private class LoginResponse
        {
            public string Token { get; set; }
            public int OperatorId { get; set; }
            public string FullName { get; set; }
            public string Role { get; set; }
            public int FirmId { get; set; }
        }
    }
}
