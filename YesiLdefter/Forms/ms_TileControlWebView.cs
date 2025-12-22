using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using DevExpress.XtraEditors;

namespace YesiLdefter
{
    /// <summary>
    /// WebView2 wrapper for TileControl that renders EntranceTemplate.html dynamically.
    /// Similar to ms_TileNavWebView but for TileControl (ItemType 105) - the entrance screen.
    /// </summary>
    public class ms_TileControlWebView : IDisposable
    {
        private Control _parentControl;
        private string _menuCode;
        private TileControl _tileControl;
        private WebView2 _webView;
        private string _formName;
        private bool _enableParityLogging = true;
        
        // Constructor-level re-entrancy guard (prevents Create_Menu_IN_Control loops creating the same overlay repeatedly)
        private static readonly object _constructLock = new object();
        private static readonly HashSet<string> _constructing = new HashSet<string>();

        public ms_TileControlWebView(Control parentControl, string menuCode, DataSet ds_Items,
            string fieldName, bool dontReport, bool dontEDI, bool dontExit, string reportTableIPCode)
        {
            System.Diagnostics.Debug.WriteLine($"[WebView2] ms_TileControlWebView constructor called. MenuCode={menuCode}");
            _parentControl = parentControl;
            _menuCode = menuCode;
            
            // Prevent re-entrant construction for the same parent instance + menu
            string constructKey = $"{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(parentControl)}_{menuCode}";
            lock (_constructLock)
            {
                if (_constructing.Contains(constructKey))
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ ms_TileControlWebView is already constructing for {menuCode}. Reusing existing WebView2 if present.");
                    string reuseName = "WEBVIEW_" + menuCode;
                    Control existing = _parentControl?.Controls.Find(reuseName, false).FirstOrDefault();
                    if (existing is WebView2 wv)
                    {
                        wv.Visible = true;
                        wv.BringToFront();
                        if (_parentControl != null && !_parentControl.IsDisposed) _parentControl.Visible = true;
                    }
                    return;
                }
                _constructing.Add(constructKey);
            }

            // Get form name
            Form tForm = parentControl.FindForm();
            if (tForm != null)
            {
                _formName = tForm.Name ?? "";
            }

            // Create TileControl in-memory (NOT added to form)
            _tileControl = new TileControl();
            _tileControl.Name = "MENU_" + menuCode;
            _tileControl.Visible = false;  // Safety measure
            // DO NOT add to parentControl.Controls - it exists only in memory

            // Build menu structure using existing Create_TileControl
            Tkn_Menu.tMenu menu = new Tkn_Menu.tMenu();
            menu.Create_TileControl(_tileControl, ds_Items);

            // All controls now exist in memory with events wired
            // But they're never rendered

            // Check if WebView2 already exists for this menu (prevent duplicates/infinite loops)
            string webViewName = "WEBVIEW_" + menuCode;
            Control existingWebView = _parentControl.Controls.Find(webViewName, false).FirstOrDefault();
            if (existingWebView != null)
            {
                System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ WebView2 already exists for {menuCode}, reusing existing control to prevent infinite loop");
                _webView = existingWebView as WebView2;
                if (_webView != null)
                {
                    _webView.Visible = true;
                    _webView.BringToFront();
                    _parentControl.Visible = true;
                }
                
                // Release constructor guard
                lock (_constructLock)
                {
                    _constructing.Remove(constructKey);
                }
                return; // Exit constructor early
            }
            
            // Hide any existing controls in parent (like iframes, other WebViews, etc.)
            foreach (Control existingControl in _parentControl.Controls)
            {
                if (existingControl != null && !existingControl.Name.StartsWith("WEBVIEW_"))
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] Hiding existing control: {existingControl.Name} (Type: {existingControl.GetType().Name})");
                    existingControl.Visible = false;
                }
            }
               
            // Create WebView2 (only visible UI)
            _webView = new WebView2();
            _webView.Dock = DockStyle.Fill;
            _webView.Name = webViewName;
            _webView.Visible = true;
            _parentControl.Controls.Add(_webView);
            _webView.BringToFront();
            _parentControl.Visible = true;
            
            System.Diagnostics.Debug.WriteLine($"[WebView2] WebView2 control added to parent. Parent={_parentControl.Name}, Parent.Visible={_parentControl.Visible}, WebView Name={_webView.Name}, Visible={_webView.Visible}, Size={_webView.Size}, Parent.Controls.Count={_parentControl.Controls.Count}");

            // Initialize WebView2 and load template
            _ = InitializeWebViewAsync(ds_Items).ContinueWith(t =>
            {
                lock (_constructLock)
                {
                    _constructing.Remove(constructKey);
                }
            });
        }

        // Shared static environment for all WebView2 instances (prevents E_ABORT from too many concurrent initializations)
        private static CoreWebView2Environment _sharedEnvironment = null;
        private static readonly object _environmentLock = new object();
        private static System.Threading.Tasks.Task<CoreWebView2Environment> _environmentCreationTask = null;
        
        private static System.Collections.Generic.HashSet<string> _initializingWebViews = new System.Collections.Generic.HashSet<string>();
        private static readonly object _webViewInitLock = new object();
        
        // Global lock to serialize WebView2 initializations (only one at a time)
        private static readonly System.Threading.SemaphoreSlim _globalInitSemaphore = new System.Threading.SemaphoreSlim(1, 1);

        private async System.Threading.Tasks.Task InitializeWebViewAsync(DataSet ds_Items)
        {
            // Prevent multiple simultaneous initializations of the same WebView2 (per parent + menu)
            string initKey = $"{_parentControl?.Name ?? "null"}_{_menuCode}";
            lock (_webViewInitLock)
            {
                if (_initializingWebViews.Contains(initKey))
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ WebView2 for {_menuCode} is already initializing on this parent, skipping to prevent infinite loop");
                    return;
                }
                _initializingWebViews.Add(initKey);
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[WebView2] Starting initialization for TileControl MenuCode={_menuCode}");

                // Get or create shared environment (only one environment for all WebView2 instances)
                CoreWebView2Environment env = await GetSharedEnvironmentAsync();
                if (env == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ Failed to get shared environment for {_menuCode}");
                    return;
                }
                
                // Serialize WebView2 initializations globally (only one at a time to prevent E_ABORT)
                // Add retry logic for E_ABORT errors
                int maxRetries = 3;
                int retryCount = 0;
                bool initialized = false;
                
                while (retryCount < maxRetries && !initialized)
                {
                    await _globalInitSemaphore.WaitAsync();
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"[WebView2] Acquired initialization lock for {_menuCode} (attempt {retryCount + 1}/{maxRetries})");
                        
                        // Use a timeout for EnsureCoreWebView2Async to prevent infinite waiting
                        var initTask = _webView.EnsureCoreWebView2Async(env);
                        var timeoutTask = System.Threading.Tasks.Task.Delay(15000); // 15 second timeout
                        var completedTask = await System.Threading.Tasks.Task.WhenAny(initTask, timeoutTask);
                        
                        if (completedTask == timeoutTask)
                        {
                            System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ WebView2 initialization timeout for {_menuCode} (attempt {retryCount + 1})");
                            retryCount++;
                            if (retryCount < maxRetries)
                            {
                                System.Diagnostics.Debug.WriteLine($"[WebView2] Retrying initialization for {_menuCode} in 500ms...");
                                await System.Threading.Tasks.Task.Delay(500); // Wait before retry
                            }
                            continue;
                        }
                        
                        try
                        {
                            await initTask;
                            initialized = true;
                            System.Diagnostics.Debug.WriteLine($"[WebView2] ✅ Successfully initialized WebView2 for {_menuCode}");
                        }
                        catch (System.Runtime.InteropServices.COMException comEx) when (comEx.ErrorCode == unchecked((int)0x80004004)) // E_ABORT
                        {
                            System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ E_ABORT during initialization for {_menuCode} (attempt {retryCount + 1}): {comEx.Message}");
                            retryCount++;
                            if (retryCount < maxRetries)
                            {
                                System.Diagnostics.Debug.WriteLine($"[WebView2] Retrying initialization for {_menuCode} in 1000ms...");
                                await System.Threading.Tasks.Task.Delay(1000); // Wait longer before retry for E_ABORT
                            }
                            continue;
                        }
                    }
                    finally
                    {
                        _globalInitSemaphore.Release();
                        System.Diagnostics.Debug.WriteLine($"[WebView2] Released initialization lock for {_menuCode}");
                    }
                }
                
                if (!initialized)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ❌ Failed to initialize WebView2 for {_menuCode} after {maxRetries} attempts");
                    Tkn_Menu.tMenu.ReportWebView2Failure();
                    return;
                }

                // Verify CoreWebView2 is ready
                if (_webView.CoreWebView2 == null)
                {
                    throw new Exception("CoreWebView2 is null after EnsureCoreWebView2Async");
                }

                System.Diagnostics.Debug.WriteLine($"[WebView2] CoreWebView2 initialized successfully");

                // Wire message handler
                _webView.CoreWebView2.WebMessageReceived += WebView_WebMessageReceived;
                
                // Note: ConsoleMessageReceived may not be available in all WebView2 versions
                // JavaScript errors will be logged via console.log in the HTML template

                // Extract menu structure to JSON from TileControl Groups
                Tkn_Menu.tMenu menu = new Tkn_Menu.tMenu();
                string menuJson = menu.ExtractMenuStructureFromTileControl(_tileControl, _menuCode);

                System.Diagnostics.Debug.WriteLine($"[WebView2] Menu JSON extracted. Length: {menuJson?.Length ?? 0}");
                
                // Debug: Log TileControl structure
                System.Diagnostics.Debug.WriteLine($"[WebView2] TileControl structure: {_tileControl.Groups.Count} groups, {_tileControl.Groups.Cast<TileGroup>().Sum(g => g.Items.Count)} total items");
                foreach (TileGroup group in _tileControl.Groups)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2]   Group: {group.Name}, Items: {group.Items.Count}");
                    foreach (TileItem item in group.Items)
                    {
                        string itemText = item.Elements.Count > 0 ? item.Elements[0].Text : "";
                        System.Diagnostics.Debug.WriteLine($"[WebView2]     Item: {item.Name}, Text: {itemText}");
                    }
                }

                // Validate JSON
                if (string.IsNullOrEmpty(menuJson))
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] WARNING: menuJson is empty or null");
                    menuJson = "{\"cards\":[],\"menuName\":\"" + _menuCode + "\"}";
                }

                // Check if we got any cards
                try
                {
                    var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(menuJson);
                    var cardsArray = jsonObj?["cards"] as Newtonsoft.Json.Linq.JArray;
                    int cardCount = cardsArray?.Count ?? 0;
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ✅ Extracted {cardCount} cards from menu structure");
                    
                    // Log each card for debugging
                    if (cardsArray != null && cardsArray.Count > 0)
                    {
                        for (int i = 0; i < cardsArray.Count; i++)
                        {
                            var card = cardsArray[i] as Newtonsoft.Json.Linq.JObject;
                            if (card != null)
                            {
                                string cardName = card["name"]?.ToString() ?? "unknown";
                                string cardCaption = card["caption"]?.ToString() ?? "no caption";
                                bool cardVisible = card["visible"]?.ToObject<bool>() ?? true;
                                System.Diagnostics.Debug.WriteLine($"[WebView2]   Card[{i}]: Name={cardName}, Caption={cardCaption}, Visible={cardVisible}");
                            }
                        }
                    }
                    
                    if (cardCount == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ WARNING: No cards found in menu structure!");
                        System.Diagnostics.Debug.WriteLine($"[WebView2]    Current menu has {_tileControl.Groups.Count} groups");
                        System.Diagnostics.Debug.WriteLine($"[WebView2]    MenuCode: {_menuCode}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[WebView2] ✅ Cards will be rendered: {cardCount} cards found");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ Error parsing JSON to check card count: {ex.Message}");
                }

                // Build HTML template
                string html = BuildHtmlTemplate(menuJson);

                if (string.IsNullOrEmpty(html))
                {
                    throw new Exception("HTML template is empty after BuildHtmlTemplate");
                }

                System.Diagnostics.Debug.WriteLine($"[WebView2] HTML template built. Length: {html.Length}");

                // Validate HTML length (WebView2 has limits)
                if (html.Length > 2 * 1024 * 1024) // 2MB limit
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] WARNING: HTML is very large ({html.Length} bytes). This may cause issues.");
                }

                // Load template - use try-catch for NavigateToString specifically
                try
                {
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
                        _webView.NavigateToString(html);
                        System.Diagnostics.Debug.WriteLine($"[WebView2] NavigateToString called successfully");
                    }
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

                System.Diagnostics.Debug.WriteLine($"[WebView2] HTML template loaded. MenuCode={_menuCode}, Cards in JSON: {menuJson.Contains("\"cards\"")}");
                
                // Ensure WebView2 is visible after navigation (use BeginInvoke to avoid blocking)
                if (_webView != null && _webView.InvokeRequired)
                {
                    _webView.BeginInvoke(new Action(() => {
                        try
                        {
                            if (_webView != null && !_webView.IsDisposed)
                            {
                                _webView.Visible = true;
                                _webView.BringToFront();
                                if (_parentControl != null && !_parentControl.IsDisposed)
                                {
                                    _parentControl.Visible = true;
                                }
                                System.Diagnostics.Debug.WriteLine($"[WebView2] WebView2 made visible (async). Visible={_webView.Visible}, Parent.Visible={_parentControl?.Visible}");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[WebView2] Error setting visibility (async): {ex.Message}");
                        }
                    }));
                }
                else if (_webView != null)
                {
                    try
                    {
                        if (!_webView.IsDisposed)
                        {
                            _webView.Visible = true;
                            _webView.BringToFront();
                            if (_parentControl != null && !_parentControl.IsDisposed)
                            {
                                _parentControl.Visible = true;
                            }
                            System.Diagnostics.Debug.WriteLine($"[WebView2] WebView2 made visible (sync). Visible={_webView.Visible}, Parent.Visible={_parentControl?.Visible}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[WebView2] Error setting visibility (sync): {ex.Message}");
                    }
                }

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
            }
            finally
            {
                // Always remove from initializing set, even on error
                lock (_webViewInitLock)
                {
                _initializingWebViews.Remove(initKey);
                }
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
                
                // Debug: Check if placeholder exists
                bool hasPlaceholder = template.Contains("{{cardsJson}}");
                System.Diagnostics.Debug.WriteLine($"[WebView2] Template loaded. Length: {template.Length}, Contains {{cardsJson}}: {hasPlaceholder}");
                
                if (!hasPlaceholder)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ WARNING: Template does not contain {{cardsJson}} placeholder!");
                    // Try to find what placeholder it uses
                    if (template.Contains("cardsJson"))
                    {
                        int idx = template.IndexOf("cardsJson");
                        string context = template.Substring(Math.Max(0, idx - 50), Math.Min(100, template.Length - Math.Max(0, idx - 50)));
                        System.Diagnostics.Debug.WriteLine($"[WebView2] Found 'cardsJson' at position {idx}. Context: {context}");
                    }
                }

                // Validate menuJson before escaping
                System.Diagnostics.Debug.WriteLine($"[WebView2] menuJson preview (first 200 chars): {menuJson.Substring(0, Math.Min(200, menuJson.Length))}");
                
                // Try to parse JSON to validate it
                try
                {
                    var testParse = Newtonsoft.Json.JsonConvert.DeserializeObject(menuJson);
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ✅ menuJson is valid JSON");
                }
                catch (Exception jsonEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ menuJson is NOT valid JSON: {jsonEx.Message}");
                }

                // Inject menu JSON - escape for JavaScript string context (using single quotes in template)
                // Need to escape: backslashes, single quotes, and control characters
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
                        case '"':
                            // Escape double quotes for JavaScript string (single-quoted)
                            escapedJson.Append("\\\"");
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
                System.Diagnostics.Debug.WriteLine($"[WebView2] JSON escaped. Original length: {menuJson.Length}, Escaped length: {escapedJsonStr.Length}");
                System.Diagnostics.Debug.WriteLine($"[WebView2] Escaped JSON preview (first 200 chars): {escapedJsonStr.Substring(0, Math.Min(200, escapedJsonStr.Length))}");
                
                // Perform replacement
                int replaceCount = 0;
                while (template.Contains("{{cardsJson}}"))
                {
                    template = template.Replace("{{cardsJson}}", escapedJsonStr);
                    replaceCount++;
                    if (replaceCount > 10) break; // Safety limit
                }
                
                System.Diagnostics.Debug.WriteLine($"[WebView2] Replacement performed {replaceCount} times");
                
                // Verify replacement happened
                if (template.Contains("{{cardsJson}}"))
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ WARNING: Replacement failed! Template still contains {{cardsJson}}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2] ✅ Replacement successful - placeholder removed");
                    // Verify the JSON is actually in the template
                    if (template.Contains(escapedJsonStr.Substring(0, Math.Min(50, escapedJsonStr.Length))))
                    {
                        System.Diagnostics.Debug.WriteLine($"[WebView2] ✅ Verified: Escaped JSON found in template");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[WebView2] ⚠️ WARNING: Escaped JSON NOT found in template after replacement!");
                    }
                }

                // Inject asset paths and logo
                string assetBase = "";
                string logoBase64 = "";
                try
                {
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
    </style>
</head>
<body>
    <div class=""error"">EntranceTemplate.html not found. Please check Templates folder.</div>
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
                string messageStr = e.TryGetWebMessageAsString();
                var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(messageStr);

                if (message == null) return;

                string action = message.ContainsKey("action") ? message["action"]?.ToString() : "";

                if (action == "tile-click")
                {
                    string buttonName = message.ContainsKey("buttonName") ? message["buttonName"]?.ToString() : "";
                    string tag = message.ContainsKey("tag") ? message["tag"]?.ToString() : "";

                    // Find the TileItem in the in-memory TileControl
                    TileItem tileItem = FindTileItemByName(_tileControl, buttonName);

                    if (tileItem != null)
                    {
                        // Parity logging
                        if (_enableParityLogging)
                        {
                            System.Diagnostics.Debug.WriteLine($"[PARITY] WebView click: buttonName={buttonName}, tag={tag}");
                            System.Diagnostics.Debug.WriteLine($"[PARITY] Found TileItem: Name={tileItem.Name}, Tag={tileItem.Tag}, Group={tileItem.Group?.Name}");
                        }

                        // Ensure the TileItem has the correct Tag (from the message)
                        if (!string.IsNullOrEmpty(tag) && (tileItem.Tag == null || string.IsNullOrEmpty(tileItem.Tag.ToString())))
                        {
                            tileItem.Tag = tag;
                            System.Diagnostics.Debug.WriteLine($"[WebView2] Set TileItem.Tag from message: {tag.Substring(0, Math.Min(100, tag.Length))}...");
                        }

                        // Call the EXACT SAME event handler that DevExpress would call
                        // TileControl uses ItemClick event
                        try
                        {
                            Tkn_Events.tEventsMenu evm = new Tkn_Events.tEventsMenu();
                            DevExpress.XtraEditors.TileItemEventArgs args = null;
                            evm.tTileItem_ItemClick(tileItem, args);
                            System.Diagnostics.Debug.WriteLine($"[WebView2] ✅ Successfully called tTileItem_ItemClick for {buttonName}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[WebView2] ❌ Error calling tTileItem_ItemClick: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"[WebView2] Stack: {ex.StackTrace}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[WebView2] ❌ TileItem not found: {buttonName}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView_WebMessageReceived error: {ex.Message}");
            }
        }

        private TileItem FindTileItemByName(TileControl control, string name)
        {
            // Search through all groups and items
            foreach (TileGroup group in control.Groups)
            {
                foreach (TileItem item in group.Items)
                {
                    if (item.Name == name)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        private void LogMenuInventory()
        {
            System.Diagnostics.Debug.WriteLine("=== MENU INVENTORY (TileControl) ===");
            System.Diagnostics.Debug.WriteLine($"Menu: {_tileControl.Name}, Form: {_formName}");

            int groupCount = 0;
            int itemCount = 0;

            foreach (TileGroup group in _tileControl.Groups)
            {
                groupCount++;
                System.Diagnostics.Debug.WriteLine($"  Group[{groupCount}]: Name={group.Name}, Text={group.Text}, Visible={group.Visible}");

                foreach (TileItem item in group.Items)
                {
                    itemCount++;
                    System.Diagnostics.Debug.WriteLine($"    Item[{itemCount}]: Name={item.Name}, Text={item.Elements[0]?.Text}, Visible={item.Visible}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Total: {groupCount} groups, {itemCount} items");

            if (groupCount == 0)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ WARNING: No groups found! This menu may not be the entrance screen.");
            }

            System.Diagnostics.Debug.WriteLine("=== END INVENTORY ===");
        }

        public void Dispose()
        {
            _webView?.Dispose();
            _tileControl?.Dispose();
        }
    }
}

