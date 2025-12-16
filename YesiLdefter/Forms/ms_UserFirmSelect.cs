using System;
using System.Collections.Generic;
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
    /// WebView2-based firm selection dialog mirroring legacy ms_UserFirmList logic.
    /// Uses shared design tokens (green/gradient) and renders a card grid for tenant choice.
    /// </summary>
    public class ms_UserFirmSelect : Form
    {
        private readonly IList<UstadApiClient.FirmInfo> _firms;
        private readonly WebView2 _webView;
        public UstadApiClient.FirmInfo SelectedFirm { get; private set; }

        public ms_UserFirmSelect(IList<UstadApiClient.FirmInfo> firms)
        {
            _firms = firms ?? Array.Empty<UstadApiClient.FirmInfo>();

            Text = "Firma Seçimi";
            Width = 960;
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
            var firmsJson = JsonConvert.SerializeObject(_firms.Select(f => new
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
            }));

            var sb = new StringBuilder();
            sb.Append($@"
<!DOCTYPE html>
<html lang='tr'>
<head>
  <meta charset='UTF-8' />
  <meta name='viewport' content='width=device-width, initial-scale=1.0' />
  <link rel='preconnect' href='https://fonts.googleapis.com'>
  <link rel='preconnect' href='https://fonts.gstatic.com' crossorigin>
  <link href='https://fonts.googleapis.com/css2?family=Inter+Tight:wght@400;500;600;700&display=swap' rel='stylesheet'>
  <style>
    :root {{
      --primary: #295c00;
      --primary-dark: #3a4a0e;
      --bg: #0f172a;
      --card: rgba(255,255,255,0.96);
      --text: #0f172a;
      --muted: #6b7280;
      --radius: 16px;
      --shadow: 0 20px 50px rgba(0,0,0,0.12);
      --border: rgba(0,0,0,0.08);
      --gradient: linear-gradient(180deg, #e0eadf 20%, #eff2ef 100%);
    }}
    * {{ box-sizing: border-box; }}
    body {{
      margin:0; padding:0;
      font-family: 'Inter Tight', 'Segoe UI', system-ui, -apple-system, sans-serif;
      background: var(--gradient);
      color: var(--text);
      display:flex; flex-direction:column; min-height:100vh;
    }}
    .page {{
      max-width: 1100px;
      margin: 0 auto;
      width: 100%;
      padding: 32px 24px 24px;
      display:flex;
      flex-direction:column;
      gap:16px;
    }}
    .header {{
      display:flex; flex-direction:column; gap:6px;
    }}
    .title {{
      font-size: 24px;
      font-weight: 700;
      color: var(--primary-dark);
      margin: 0;
    }}
    .subtitle {{
      font-size: 14px;
      color: var(--muted);
      margin: 0;
    }}
    .grid {{
      display:grid;
      grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
      gap: 12px;
      width:100%;
      max-height: calc(100vh - 210px);
      overflow: auto;
      padding: 4px;
    }}
    .card {{
      background: var(--card);
      border: 1px solid var(--border);
      border-radius: var(--radius);
      box-shadow: var(--shadow);
      padding: 16px;
      display:flex;
      flex-direction:column;
      gap:8px;
      cursor:pointer;
      transition: all .2s ease;
    }}
    .card:hover {{
      box-shadow: 0 12px 30px rgba(0,0,0,0.16);
      transform: translateY(-1px);
    }}
    .card.selected {{
      border-color: var(--primary);
      box-shadow: 0 14px 32px rgba(41,92,0,0.18);
    }}
    .card-title {{
      font-weight: 700;
      font-size: 16px;
      color: var(--primary-dark);
      margin:0;
      line-height: 1.4;
    }}
    .row {{
      display:flex;
      justify-content:space-between;
      align-items:center;
      gap:8px;
      font-size: 12px;
      color: var(--muted);
    }}
    .badge {{
      padding: 4px 8px;
      border-radius: 9999px;
      font-size: 11px;
      font-weight: 600;
      background: rgba(41,92,0,0.1);
      color: var(--primary-dark);
      text-transform: uppercase;
      letter-spacing: .4px;
    }}
    .muted {{
      color: var(--muted);
      font-size: 12px;
      margin:0;
      line-height:1.5;
      word-break: break-word;
    }}
    .footer {{
      display:flex;
      justify-content:flex-end;
      gap:10px;
      padding-top: 8px;
    }}
    button {{
      border:none;
      border-radius: 10px;
      padding: 10px 16px;
      font-weight: 700;
      cursor:pointer;
      transition: all .18s ease;
      font-size: 14px;
    }}
    .ghost {{
      background: rgba(255,255,255,0.2);
      border: 1px solid var(--border);
      color: var(--muted);
    }}
    .primary {{
      background: linear-gradient(135deg, var(--primary) 0%, #8bc34a 100%);
      color: #0b1727;
      box-shadow: 0 10px 25px rgba(34,197,94,0.25);
    }}
    .primary:disabled {{
      opacity: 0.5;
      cursor: not-allowed;
      box-shadow:none;
    }}
  </style>
</head>
<body>
  <div class='page'>
    <div class='header'>
      <h1 class='title'>Firma Seçimi</h1>
      <p class='subtitle'>Hangi firma ile devam etmek istediğinizi seçin.</p>
    </div>
    <div id='grid' class='grid'></div>
    <div class='footer'>
      <button class='ghost' id='cancelBtn'>Vazgeç</button>
      <button class='primary' id='confirmBtn' disabled>Seç ve Devam Et</button>
    </div>
  </div>
  <script>
    const firms = {firmsJson};
    let selectedId = null;

    function render() {{
      const grid = document.getElementById('grid');
      grid.innerHTML = '';
      if (!firms || firms.length === 0) {{
        grid.innerHTML = '<p class=""muted"">Firma bulunamadı.</p>';
        return;
      }}
      firms.forEach(f => {{
        const card = document.createElement('div');
        card.className = 'card' + (selectedId === f.FirmId ? ' selected' : '');
        card.onclick = () => selectFirm(f.FirmId);
        card.innerHTML = `
          <h3 class='card-title'>${{f.FirmLongName || f.FirmShortName || 'Firma'}} </h3>
          <div class='row'>
            <span class='muted'>Kod: ${{f.MenuCode || '-'}} </span>
            <span class='badge'>${{f.IsActive ? 'AKTİF' : 'PASİF'}}</span>
          </div>
          <p class='muted'>DB: ${{f.DatabaseName || '-'}} • Sunucu: ${{f.ServerNameIP || '-'}} </p>
        `;
        grid.appendChild(card);
      }});
    }}

    function selectFirm(id) {{
      selectedId = id;
      const confirmBtn = document.getElementById('confirmBtn');
      confirmBtn.disabled = !selectedId;
      render();
    }}

    function post(msg) {{
      if (window.chrome && window.chrome.webview && window.chrome.webview.postMessage) {{
        window.chrome.webview.postMessage(JSON.stringify(msg));
      }}
    }}

    document.getElementById('confirmBtn')?.addEventListener('click', () => {{
      if (!selectedId) return;
      post({{ action: 'confirm', firmId: selectedId }});
    }});
    document.getElementById('cancelBtn')?.addEventListener('click', () => {{
      post({{ action: 'cancel' }});
    }});

    render();
  </script>
</body>
</html>");

            return sb.ToString();
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
                if (action == "cancel")
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

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


