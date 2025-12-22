using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            _userGUID = userGUID ?? "";
            _apiBaseUrl = apiBaseUrl ?? "";

            Text = "Firma Seçimi";
            // Golden ratio-inspired dimensions (approximately 1.618:1)
            // Width: 1200px, Height: 740px (ratio ~1.62:1)
            // This provides ample space for firm selection grid (left) and QR panel (right: 420px)
            Width = 1200;
            Height = 740;
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
                    f.IsActive
                }),
                lastFirmId = _firms.FirstOrDefault(f => f.IsActive)?.FirmId,
                userGUID = _userGUID
            };

            string initialJson = JsonConvert.SerializeObject(initialPayload);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string templatePath = Path.Combine(baseDir, "Templates", "FirmSelectTemplate.html");
            var template = File.ReadAllText(templatePath, Encoding.UTF8);

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


