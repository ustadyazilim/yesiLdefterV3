using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars.Navigation;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Tkn_Events;
using Tkn_Menu;
using Tkn_ToolBox;
using Tkn_Variable;

namespace YesiLdefter
{
    /// <summary>
    /// WebView2 wrapper for TileNavPane that renders EntranceTemplate.html dynamically.
    /// Keeps DevExpress controls in-memory (not rendered) for event handling.
    /// Bridges WebView2 clicks to existing tNavButton_ElementClick handlers.
    /// </summary>
    public class ms_TileNavWebView
    {
        private TileNavPane _tileNavPane;  // Exists in memory but not rendered
        private WebView2 _webView;         // Only visible UI
        private Control _parentControl;
        private string _menuCode;
        private string _formName;
        private bool _enableParityLogging = true; // For demo/validation
        private bool _isDisposed = false; // Track disposal state to prevent re-initialization
        private static System.Collections.Generic.HashSet<string> _initializingMenus = new System.Collections.Generic.HashSet<string>();
        private static readonly object _initializationLock = new object();

        public ms_TileNavWebView(Control parentControl, string menuCode, DataSet ds_Items, 
            string fieldName, bool dontReport, bool dontEDI, bool dontExit, string reportTableIPCode)
        {
            System.Diagnostics.Debug.WriteLine($"[WebView2] ms_TileNavWebView constructor called. MenuCode={menuCode}");
            _parentControl = parentControl;
            _menuCode = menuCode;

            // Get form name
            Form tForm = parentControl.FindForm();
            if (tForm != null)
            {
                _formName = tForm.Name ?? "";
            }

            // Create TileNavPane in-memory (NOT added to form)
            _tileNavPane = new TileNavPane();
            _tileNavPane.Name = "MENU_" + menuCode;
            _tileNavPane.Visible = false;  // Safety measure
            // DO NOT add to parentControl.Controls - it exists only in memory

            // Build menu structure using existing Create_TileNavPane
            // Use Tkn_Menu.tMenu to avoid circular reference issues
            Tkn_Menu.tMenu menu = new Tkn_Menu.tMenu();
            menu.Create_TileNavPane(_tileNavPane, ds_Items, fieldName, dontReport, dontEDI, dontExit, reportTableIPCode);

            // All controls now exist in memory with events wired
            // But they're never rendered

            // If a WebView for this menu already exists on the parent, reuse it to prevent re-render loops
            string webViewName = "WEBVIEW_" + menuCode;
            var existing = _parentControl.Controls.Find(webViewName, false).FirstOrDefault();
            if (existing != null)
            {
                System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ WebView2 already exists for {menuCode}, reusing existing control to avoid re-render");
                _webView = existing as WebView2;
                if (_webView != null)
                {
                    _webView.Visible = true;
                    _webView.BringToFront();
                    if (_parentControl != null && !_parentControl.IsDisposed) _parentControl.Visible = true;
                }
                return;
            }

            // Create WebView2 (only visible UI)
            _webView = new WebView2();
            _webView.Dock = DockStyle.Fill;
            _webView.Name = webViewName;
            _parentControl.Controls.Add(_webView);
            System.Diagnostics.Debug.WriteLine($"[WebView2] WebView2 control added to parent. Parent={_parentControl.Name}, WebView Name={_webView.Name}");

            // Prevent multiple initializations for the same menu
            string initKey = $"{_parentControl?.Name ?? "null"}_{menuCode}";
            lock (_initializationLock)
            {
                if (_initializingMenus.Contains(initKey))
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ Menu {menuCode} is already initializing, skipping to prevent infinite loop");
                    return;
                }
                _initializingMenus.Add(initKey);
            }
            
            // Initialize WebView2 and load template
            _ = InitializeWebViewAsync(ds_Items).ContinueWith(t =>
            {
                // Remove from initialization set when done (success or failure)
                lock (_initializationLock)
                {
                    _initializingMenus.Remove(initKey);
                }
            });
        }

        // Shared static environment for all WebView2 instances (prevents E_ABORT from too many concurrent initializations)
        private static CoreWebView2Environment _sharedEnvironment = null;
        private static readonly object _environmentLock = new object();
        private static System.Threading.Tasks.Task<CoreWebView2Environment> _environmentCreationTask = null;
        
        // Global lock to serialize WebView2 initializations (only one at a time)
        private static readonly System.Threading.SemaphoreSlim _globalInitSemaphore = new System.Threading.SemaphoreSlim(1, 1);

        private async System.Threading.Tasks.Task InitializeWebViewAsync(DataSet ds_Items)
        {
            if (_isDisposed)
            {
                System.Diagnostics.Debug.WriteLine($"[WebView2] InitializeWebViewAsync skipped - instance is disposed for {_menuCode}");
                return;
            }
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"[WebView2] Starting initialization for MenuCode={_menuCode}");
                
                // Get or create shared environment (only one environment for all WebView2 instances)
                CoreWebView2Environment env = await GetSharedEnvironmentAsync();
                if (env == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ Failed to get shared environment for {_menuCode}");
                    return;
                }
                
                // Serialize WebView2 initializations globally (only one at a time to prevent E_ABORT)
                await _globalInitSemaphore.WaitAsync();
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] Acquired initialization lock for {_menuCode}");
                    await _webView.EnsureCoreWebView2Async(env);
                }
                finally
                {
                    _globalInitSemaphore.Release();
                    System.Diagnostics.Debug.WriteLine($"[WebView2] Released initialization lock for {_menuCode}");
                }

                // Verify CoreWebView2 is ready
                if (_webView.CoreWebView2 == null)
                {
                    throw new Exception("CoreWebView2 is null after EnsureCoreWebView2Async");
                }

                System.Diagnostics.Debug.WriteLine($"[WebView2] CoreWebView2 initialized successfully");

                // Wire message handler
                _webView.CoreWebView2.WebMessageReceived += WebView_WebMessageReceived;

                // Extract menu structure to JSON
                // Use Tkn_Menu.tMenu to avoid circular reference issues
                Tkn_Menu.tMenu menu = new Tkn_Menu.tMenu();
                
                // Try extracting from TileNavPane first (more reliable - uses already-built structure)
                // Fallback to DataSet extraction if needed
                string menuJson = "";
                try
                {
                    menuJson = menu.ExtractMenuStructureToJson(_tileNavPane);
                    System.Diagnostics.Debug.WriteLine($"[WebView2] Extracted from TileNavPane. JSON length: {menuJson?.Length ?? 0}");
                    
                    // Check if we got any cards
                    if (menuJson.Contains("\"cards\":[]") || menuJson.Contains("\"cards\": []"))
                    {
                        System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ WARNING: No cards found in menu structure!");
                        System.Diagnostics.Debug.WriteLine($"[WebView2]    This menu might not be the entrance screen (MEBBIS İşlem Paneli).");
                        System.Diagnostics.Debug.WriteLine($"[WebView2]    Entrance screen should have ItemType 201 categories.");
                        System.Diagnostics.Debug.WriteLine($"[WebView2]    Current menu has {_tileNavPane.Categories.Count} categories, {_tileNavPane.Buttons.Count} buttons.");
                        System.Diagnostics.Debug.WriteLine($"[WebView2]    MenuCode: {_menuCode}");
                        System.Diagnostics.Debug.WriteLine($"[WebView2]    To find the correct menu, look for a menu with ItemType 106 that has 9 categories (ItemType 201).");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ExtractMenuStructureToJson failed, using DataSet: {ex.Message}");
                    menuJson = menu.ExtractMenuStructureFromDataSet(ds_Items, _tileNavPane.Name);
                }
                
                System.Diagnostics.Debug.WriteLine($"[WebView2] Menu JSON extracted. Length: {menuJson?.Length ?? 0}");

                // Validate JSON
                if (string.IsNullOrEmpty(menuJson))
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] WARNING: menuJson is empty or null");
                    menuJson = "{\"cards\":[],\"menuName\":\"" + _menuCode + "\"}";
                }

                // Build HTML template
                string html = BuildHtmlTemplate(menuJson);
                
                if (string.IsNullOrEmpty(html))
                {
                    throw new Exception("HTML template is empty after BuildHtmlTemplate");
                }

                System.Diagnostics.Debug.WriteLine($"[WebView2] HTML template built. Length: {html.Length}");

                // Validate HTML length (WebView2 NavigateToString has ~2MB limit)
                // If HTML is too large, save to temp file and use Navigate() instead
                if (html.Length > 2 * 1024 * 1024) // 2MB limit
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] HTML is very large ({html.Length} bytes). Using temp file approach.");
                    
                    // Save to temporary file and navigate to it
                    string tempDir = Path.Combine(Path.GetTempPath(), "YesiLdefter_WebView2");
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                    
                    string tempFile = Path.Combine(tempDir, $"menu_{_menuCode.Replace("/", "_").Replace("\\", "_")}_{DateTime.Now.Ticks}.html");
                    File.WriteAllText(tempFile, html, Encoding.UTF8);
                    
                    // Navigate to file using file:// protocol
                    string fileUri = new Uri(tempFile).AbsoluteUri;
                    _webView.CoreWebView2.Navigate(fileUri);
                    
                    System.Diagnostics.Debug.WriteLine($"[WebView2] Navigated to temp file: {tempFile}");
                }
                else
                {
                    // Use NavigateToString for smaller files (faster)
                    try
                    {
                        _webView.NavigateToString(html);
                        System.Diagnostics.Debug.WriteLine($"[WebView2] NavigateToString called successfully");
                    }
                    catch (ArgumentException argEx)
                    {
                        // Fallback to file approach if NavigateToString fails
                        System.Diagnostics.Debug.WriteLine($"[WebView2] NavigateToString failed, falling back to file approach: {argEx.Message}");
                        
                        string tempDir = Path.Combine(Path.GetTempPath(), "YesiLdefter_WebView2");
                        if (!Directory.Exists(tempDir))
                        {
                            Directory.CreateDirectory(tempDir);
                        }
                        
                        string tempFile = Path.Combine(tempDir, $"menu_{_menuCode.Replace("/", "_").Replace("\\", "_")}_{DateTime.Now.Ticks}.html");
                        File.WriteAllText(tempFile, html, Encoding.UTF8);
                        
                        string fileUri = new Uri(tempFile).AbsoluteUri;
                        _webView.CoreWebView2.Navigate(fileUri);
                        
                        System.Diagnostics.Debug.WriteLine($"[WebView2] Navigated to temp file (fallback): {tempFile}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[WebView2] HTML template loaded. MenuCode={_menuCode}, Cards in JSON: {menuJson.Contains("\"cards\"")}");

                // Optional: Log inventory of elements for validation
                if (_enableParityLogging)
                {
                    LogMenuInventory();
                }
            }
            catch (System.Runtime.InteropServices.COMException comEx)
            {
                // HRESULT errors (like E_ABORT 0x80004004) - often caused by too many simultaneous initializations
                System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ COMException (HRESULT) during initialization for {_menuCode}: {comEx.Message}");
                System.Diagnostics.Debug.WriteLine($"[WebView2] HRESULT: 0x{comEx.ErrorCode:X8}");
                // Report failure to tMenu.cs to track and potentially disable WebView2
                Tkn_Menu.tMenu.ReportWebView2Failure();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebView2] InitializeWebViewAsync error for {_menuCode}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[WebView2] Stack trace: {ex.StackTrace}");
                // Report failure to tMenu.cs to track and potentially disable WebView2
                Tkn_Menu.tMenu.ReportWebView2Failure();
                // Don't show MessageBox - it might trigger more events and cause infinite loops
            }
        }
        
        // Get or create shared CoreWebView2Environment (singleton pattern)
        private static async System.Threading.Tasks.Task<CoreWebView2Environment> GetSharedEnvironmentAsync()
        {
            // Check if already created
            if (_sharedEnvironment != null)
            {
                return _sharedEnvironment;
            }
            
            System.Threading.Tasks.Task<CoreWebView2Environment> creationTask = null;
            
            // Lock to prevent multiple simultaneous creations
            lock (_environmentLock)
            {
                // Double-check after acquiring lock
                if (_sharedEnvironment != null)
                {
                    return _sharedEnvironment;
                }
                
                // If creation is in progress, use it
                if (_environmentCreationTask != null)
                {
                    creationTask = _environmentCreationTask;
                }
                else
                {
                    // Start creation
                    _environmentCreationTask = CreateEnvironmentAsync();
                    creationTask = _environmentCreationTask;
                }
            }
            
            // Wait for creation to complete (outside lock to avoid deadlock)
            try
            {
                _sharedEnvironment = await creationTask;
                return _sharedEnvironment;
            }
            catch
            {
                // If creation failed, clear the task so we can retry
                lock (_environmentLock)
                {
                    if (_environmentCreationTask == creationTask)
                    {
                        _environmentCreationTask = null;
                    }
                }
                throw;
            }
        }
        
        private static async System.Threading.Tasks.Task<CoreWebView2Environment> CreateEnvironmentAsync()
        {
            try
            {
                var env = await CoreWebView2Environment.CreateAsync();
                lock (_environmentLock)
                {
                    _sharedEnvironment = env;
                    System.Diagnostics.Debug.WriteLine($"[WebView2] Shared environment created successfully");
                }
                return env;
            }
            catch (Exception ex)
            {
                lock (_environmentLock)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ Failed to create shared environment: {ex.Message}");
                    _environmentCreationTask = null; // Allow retry
                }
                throw;
            }
        }

        private string BuildHtmlTemplate(string menuJson)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                System.Diagnostics.Debug.WriteLine($"[WebView2] Looking for EntranceTemplate.html. BaseDir: {baseDir}");
                
                // Try multiple possible locations
                string[] possiblePaths = new string[]
                {
                    Path.Combine(baseDir, "Forms", "Templates", "EntranceTemplate.html"),
                    Path.Combine(baseDir, "Templates", "EntranceTemplate.html"),
                    Path.Combine(Application.StartupPath, "Forms", "Templates", "EntranceTemplate.html"),
                    Path.Combine(Application.StartupPath, "Templates", "EntranceTemplate.html")
                };

                string templatePath = null;
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        templatePath = path;
                        System.Diagnostics.Debug.WriteLine($"[WebView2] Found EntranceTemplate.html at: {path}");
                        break;
                    }
                }

                if (string.IsNullOrEmpty(templatePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] EntranceTemplate.html not found. Searched paths:");
                    foreach (string path in possiblePaths)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {path}");
                    }
                    return GetFallbackHtml();
                }

                string template = File.ReadAllText(templatePath, Encoding.UTF8);

                // Inject menu JSON - properly escape for JavaScript string context
                // Since the template uses: const menuJsonStr = '{{cardsJson}}';
                // We need to escape single quotes, backslashes, and control characters
                System.Text.StringBuilder escapedJson = new System.Text.StringBuilder(menuJson.Length * 2);
                foreach (char c in menuJson)
                {
                    switch (c)
                    {
                        case '\\':
                            escapedJson.Append("\\\\");
                            break;
                        case '\'':
                            escapedJson.Append("\\'");
                            break;
                        case '\r':
                            escapedJson.Append("\\r");
                            break;
                        case '\n':
                            escapedJson.Append("\\n");
                            break;
                        case '\t':
                            escapedJson.Append("\\t");
                            break;
                        case '\0':
                            escapedJson.Append("\\0");
                            break;
                        default:
                            // Only add printable characters (avoid control chars that might break HTML)
                            if (c >= 32 || c == '\t' || c == '\n' || c == '\r')
                            {
                                escapedJson.Append(c);
                            }
                            break;
                    }
                }
                
                string escapedJsonStr = escapedJson.ToString();
                template = template.Replace("{{cardsJson}}", escapedJsonStr);
                System.Diagnostics.Debug.WriteLine($"[WebView2] JSON injected into template. Original length: {menuJson.Length}, Escaped length: {escapedJsonStr.Length}");

                // Inject asset paths and logo (similar to ms_UserFirmSelect)
                string assetBase = "";
                string logoBase64 = "";
                try
                {
                    // Try Forms/Templates/public first
                    string assets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Forms", "Templates", "public") + Path.DirectorySeparatorChar;
                    if (!Directory.Exists(assets))
                    {
                        assets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "public") + Path.DirectorySeparatorChar;
                    }
                    assetBase = new Uri(assets).AbsoluteUri;

                    string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Forms", "Templates", "public", "yesildefter_horizontal.png");
                    if (!File.Exists(logoPath))
                    {
                        logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "public", "yesildefter_horizontal.png");
                    }
                    if (File.Exists(logoPath))
                    {
                        byte[] logoBytes = File.ReadAllBytes(logoPath);
                        logoBase64 = "data:image/png;base64," + Convert.ToBase64String(logoBytes);
                    }
                }
                catch
                {
                    assetBase = "";
                }

                template = template
                    .Replace("{{asset-base}}", assetBase)
                    .Replace("{{logo-base64}}", logoBase64)
                    .Replace("{{formName}}", _formName);

                return template;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BuildHtmlTemplate error: {ex.Message}");
                return GetFallbackHtml();
            }
        }

        private string GetFallbackHtml()
        {
            return @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Menu Loading...</title>
    <style>
        body { font-family: Arial; padding: 20px; background: #f8f9fa; }
        .error { color: #dc3545; }
        .warning { color: #856404; background: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0; }
    </style>
</head>
<body>
    <div class=""error"">EntranceTemplate.html not found. Please check Templates folder.</div>
    <div class=""warning"">
        <strong>Note:</strong> If you see this message, the HTML template file is missing.
        <br>Expected location: Forms\Templates\EntranceTemplate.html
    </div>
    <div id=""menu-container""></div>
    <script>
        const menuData = JSON.parse('{{cardsJson}}');
        console.log('Menu data:', menuData);
    </script>
</body>
</html>";
        }

        private void WebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var raw = e.TryGetWebMessageAsString();
                if (string.IsNullOrWhiteSpace(raw)) return;

                var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);
                if (message == null || !message.ContainsKey("action")) return;

                var action = message["action"]?.ToString();

                if (action == "tile-click")
                {
                    string buttonName = message.ContainsKey("buttonName") ? message["buttonName"]?.ToString() : "";
                    string tag = message.ContainsKey("tag") ? message["tag"]?.ToString() : "";

                    if (string.IsNullOrEmpty(buttonName))
                    {
                        System.Diagnostics.Debug.WriteLine("tile-click: buttonName is empty");
                        return;
                    }

                    // Find DevExpress control by Name
                    var element = FindElementByName(_tileNavPane, buttonName);

                    if (element != null)
                    {
                        // Parity logging
                        if (_enableParityLogging)
                        {
                            System.Diagnostics.Debug.WriteLine($"[PARITY] WebView click: buttonName={buttonName}, tag={tag}");
                            System.Diagnostics.Debug.WriteLine($"[PARITY] Found element: {element.GetType().Name}, Name={element.Name}");
                        }

                        // Call the EXACT SAME event handler that DevExpress would call
                        // Note: The handler accepts null for NavElementEventArgs (as seen in EventsMenu.cs)
                        // The handler gets the element from the sender parameter
                        tEventsMenu evm = new tEventsMenu();
                        evm.tNavButton_ElementClick(element, null);

                        // This triggers:
                        // → Tag parsing (PROP_Navigator extraction)
                        // → commonMenuClick()
                        // → Form opening
                        // → All existing logic unchanged
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] Element not found: buttonName={buttonName}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView_WebMessageReceived error: {ex.Message}");
            }
        }

        private NavElement FindElementByName(TileNavPane pane, string name)
        {
            // Search Categories
            foreach (TileNavCategory category in pane.Categories)
            {
                if (category.Name == name) return category;

                foreach (TileNavItem item in category.Items)
                {
                    if (item.Name == name) return item;

                    // Search SubItems
                    foreach (TileNavSubItem subItem in item.SubItems)
                    {
                        if (subItem.Name == name) return subItem;
                    }
                }
            }

            // Search Buttons
            // Buttons is a collection that can be accessed by index
            for (int i = 0; i < pane.Buttons.Count; i++)
            {
                NavElement btnElement = pane.Buttons[i].Element;
                if (btnElement.Name == name) return btnElement;

                // If button is a category, search its items
                if (btnElement is TileNavCategory cat)
                {
                    foreach (TileNavItem item in cat.Items)
                    {
                        if (item.Name == name) return item;
                    }
                }
            }

            return null;
        }

        private void LogMenuInventory()
        {
            System.Diagnostics.Debug.WriteLine("=== MENU INVENTORY (for validation) ===");
            System.Diagnostics.Debug.WriteLine($"Menu: {_tileNavPane.Name}, Form: {_formName}");

            int categoryCount = 0;
            int itemCount = 0;
            int buttonCount = 0;

            foreach (TileNavCategory category in _tileNavPane.Categories)
            {
                categoryCount++;
                System.Diagnostics.Debug.WriteLine($"  Category[{categoryCount}]: Name={category.Name}, Caption={category.Caption}, Tag={category.Tag}, Visible={category.Visible}");

                foreach (TileNavItem item in category.Items)
                {
                    itemCount++;
                    System.Diagnostics.Debug.WriteLine($"    Item[{itemCount}]: Name={item.Name}, Caption={item.Caption}, Tag={item.Tag}");
                }
            }

            for (int i = 0; i < _tileNavPane.Buttons.Count; i++)
            {
                buttonCount++;
                NavElement btnElement = _tileNavPane.Buttons[i].Element;
                System.Diagnostics.Debug.WriteLine($"  Button[{buttonCount}]: Name={btnElement.Name}, Caption={btnElement.Caption}, Tag={btnElement.Tag}");
            }

            System.Diagnostics.Debug.WriteLine($"Total: {categoryCount} categories, {itemCount} items, {buttonCount} buttons");
            
            // Warning if no categories found (cards won't display)
            if (categoryCount == 0)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ WARNING: No categories found! This menu may not be the entrance screen.");
                System.Diagnostics.Debug.WriteLine($"   Entrance screen should have ItemType 201 categories (top-level menu cards).");
                System.Diagnostics.Debug.WriteLine($"   This menu might be a sub-menu or different menu type.");
            }
            
            System.Diagnostics.Debug.WriteLine("=== END INVENTORY ===");
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            
            try
            {
                _webView?.Dispose();
            }
            catch { }
            
            try
            {
                _tileNavPane?.Dispose();
            }
            catch { }
            
            // Remove from initialization set
            string initKey = $"{_parentControl?.Name ?? "null"}_{_menuCode}";
            lock (_initializationLock)
            {
                _initializingMenus.Remove(initKey);
            }
        }
    }
}

