using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Tkn_UstadAPI;

namespace YesiLdefter
{
    /// <summary>
    /// WebView2-based firm selection dialog aligned with the inline WebView2 contract
    /// (actions: firm-select, firm-confirm, firm-cancel; helpers: __ustadShowFirmSelection, __ustadHighlightFirm).
    /// </summary>
    public class ms_UserFirmSelect : Form
    {
        private readonly IList<UstadApiClient.FirmInfo> _firms;
        private readonly WebView2 _webView;
        private readonly string _userGUID;
        private readonly string _apiBaseUrl;
        public UstadApiClient.FirmInfo SelectedFirm { get; private set; }

      public ms_UserFirmSelect(IList<UstadApiClient.FirmInfo> firms, string userGUID = null, string apiBaseUrl = null)
      {
            _firms = firms ?? Array.Empty<UstadApiClient.FirmInfo>();
            _userGUID = userGUID ?? string.Empty;
            _apiBaseUrl = apiBaseUrl ?? string.Empty;
            
            Text = "Firma Seçimi";
            Width = 800;
            Height = 720;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;

            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(_webView);
            Load += OnLoadAsync;
        }

        private async void OnLoadAsync(object sender, EventArgs e)
        {
            try
            {
                var env = await CoreWebView2Environment.CreateAsync();
                await _webView.EnsureCoreWebView2Async(env);
                _webView.CoreWebView2.WebMessageReceived += WebMessageReceived;

                var html = BuildHtml();
                _webView.NavigateToString(html);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 başlatılırken hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Abort;
                Close();
            }
        }

        private string BuildHtml()
        {
               var initialPayload = new
            {
                firms = _firms.Select(f => new
                {
                    f.FirmId,
                    f.FirmGUID,
                    f.FirmLongName,
                    f.FirmShortName,
                    f.MenuCode,
                    f.DatabaseName,
                    f.ServerNameIP,
                    f.DbLoginName,
                    f.DbPass,
                    f.DbTypeId,
                    f.MebbisCode,
                    f.MebbisPass,
                    f.CityTypeId,
                    f.DistrictTypeId,
                    f.SectorTypeId,
                    f.IsActive
                }),
                lastFirmId = _firms.FirstOrDefault(f => f.IsActive)?.FirmId,
                userGUID = _userGUID
            };

            string initialJson = JsonConvert.SerializeObject(initialPayload);

            // Load template from embedded resource first, then fallback to disk
            string template = LoadFirmSelectTemplate();
            if (string.IsNullOrWhiteSpace(template))
            {
                System.Diagnostics.Debug.WriteLine("[ms_UserFirmSelect] ⚠️ Template not found, using minimal fallback");
                // Minimal fallback HTML
                template = @"<!DOCTYPE html>
<html lang='tr'>
<head>
  <meta charset='UTF-8' />
  <title>Firma Seçimi</title>
  <style>
    body { font-family: 'Segoe UI', sans-serif; padding: 40px; background: #f0f0f0; }
    .error { color: #d32f2f; background: #fff; padding: 20px; border-radius: 8px; }
  </style>
</head>
<body>
  <div class='error'>
    <h2>Template Yüklenemedi</h2>
    <p>FirmSelectTemplate.html dosyası bulunamadı.</p>
    <p>Lütfen embedded resource veya Templates\FirmSelectTemplate.html dosyasını kontrol edin.</p>
  </div>
</body>
</html>";
            }

            string assetBase = "";
            string logoBase64 = "";
            try
            {
                string assets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "public") + Path.DirectorySeparatorChar;
                assetBase = new Uri(assets).AbsoluteUri;

                // Convert logo to base64 data URI to avoid WebView2 file:// blocking
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "public", "yesildefter_horizontal.png");
                if (File.Exists(logoPath))
                {
                    byte[] logoBytes = File.ReadAllBytes(logoPath);
                    logoBase64 = "data:image/png;base64," + Convert.ToBase64String(logoBytes);
                }
                else
                {
                    // Fallback to leaf-only
                    string logoFallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "public", "yesildefter-logo-leaf.png");
                    if (File.Exists(logoFallback))
                    {
                        byte[] logoBytes = File.ReadAllBytes(logoFallback);
                        logoBase64 = "data:image/png;base64," + Convert.ToBase64String(logoBytes);
                    }
                }
            }
            catch
            {
                assetBase = "";
            }

            // Get API base URL from configuration if not provided
            string apiBaseUrl = _apiBaseUrl;
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                apiBaseUrl = Tkn_UstadAPI.tApiConfig.GetApiBaseUrl();
            }

            return template
                .Replace("{{initialPayload}}", initialJson)
                .Replace("{{asset-base}}", assetBase)
                .Replace("{{logo-base64}}", logoBase64)
                .Replace("{{api-base-url}}", apiBaseUrl);
        }

        private void WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var raw = e.TryGetWebMessageAsString();
                if (string.IsNullOrWhiteSpace(raw)) return;
                var payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);
                if (payload == null || !payload.ContainsKey("action")) return;

                var action = payload["action"]?.ToString();
                if (action == "firm-cancel" || action == "cancel") 
                {                
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                if (action == "firm-confirm")
                {
                    var firmGuid = payload.ContainsKey("firmGUID") ? payload["firmGUID"]?.ToString() : null;
                    if (!string.IsNullOrWhiteSpace(firmGuid))
                    {
                        var firm = _firms.FirstOrDefault(f => string.Equals(f.FirmGUID, firmGuid, StringComparison.OrdinalIgnoreCase));
                        if (firm != null)
                        {
                            SelectedFirm = firm;
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                    }
                    return;
                }

                // Legacy compatibility: confirm by firmId
                if (action == "confirm" && payload.ContainsKey("firmId"))
                {
                    if (int.TryParse(payload["firmId"]?.ToString(), out int firmId))
                    {
                        var firm = _firms.FirstOrDefault(f => f.FirmId == firmId);
                        if (firm != null)
                        {
                            SelectedFirm = firm;
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Firma seçimi okunurken hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load FirmSelectTemplate.html from embedded resource first, then fallback to disk
        /// </summary>
        private string LoadFirmSelectTemplate()
        {
            var asm = Assembly.GetExecutingAssembly();
            const string resourceName = "YesiLdefter.Forms.Templates.FirmSelectTemplate.html";
            string matchedResource = null;

            try
            {
                var available = asm.GetManifestResourceNames();
                System.Diagnostics.Debug.WriteLine("[ms_UserFirmSelect] Available embedded resources: " + string.Join(", ", available));

                // Try exact name first, then try to find by suffix (helps when default namespace changed)
                matchedResource = Array.Find(available, r => string.Equals(r, resourceName, StringComparison.OrdinalIgnoreCase))
                                  ?? Array.Find(available, r => r.EndsWith(".Templates.FirmSelectTemplate.html", StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(matchedResource))
                {
                    System.Diagnostics.Debug.WriteLine($"[ms_UserFirmSelect] Template resource not found: {resourceName}");
                    // Try disk fallback: <app>\Templates\FirmSelectTemplate.html
                    string fallback = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "FirmSelectTemplate.html");
                    if (File.Exists(fallback))
                    {
                        System.Diagnostics.Debug.WriteLine($"[ms_UserFirmSelect] Loading template from disk fallback: {fallback}");
                        return File.ReadAllText(fallback, Encoding.UTF8);
                    }

                    System.Diagnostics.Debug.WriteLine($"[ms_UserFirmSelect] ⚠️ Template not found in embedded resources or disk: {fallback}");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_UserFirmSelect] Error enumerating resources: {ex.Message}");
            }

            // Load embedded resource (use matchedResource if found, else try the constant name)
            try
            {
                using (var stream = asm.GetManifestResourceStream(matchedResource ?? resourceName))
                {
                    if (stream == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ms_UserFirmSelect] GetManifestResourceStream returned null for {(matchedResource ?? resourceName)}");
                        return string.Empty;
                    }
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string template = reader.ReadToEnd();
                        System.Diagnostics.Debug.WriteLine($"[ms_UserFirmSelect] ✅ Loaded template from embedded resource: {matchedResource ?? resourceName}");
                        return template;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ms_UserFirmSelect] Error loading embedded resource: {ex.Message}");
                return string.Empty;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ms_UserFirmSelect
            // 
            this.ClientSize = new System.Drawing.Size(542, 490);
            this.Name = "ms_UserFirmSelect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }
    }
}


