using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YesiLdefter
{
    /// <summary>
    /// Lightweight WebView2-based splash screen that shows the branded loading template.
    /// Displays Templates/LoadingTemplate.html and replaces {{asset-base}} with the local file URI.
    /// </summary>
    public class ms_WebViewSplash : Form
    {
        private readonly WebView2 web;
        private bool _isWebViewReady = false;
        private bool _initStarted = false;
        private bool _pendingHide = false;
        private bool _webViewPermanentlyDisabled = false; // if WebView2 runtime repeatedly fails, fall back for this session

        // WinForms fallback controls (must always render status text)
        private Panel _fallbackPanel = null;
        private Label _fallbackStatusLabel = null;
        private PictureBox _fallbackLogo = null;

        private static ms_WebViewSplash _currentInstance = null;
        private static readonly object _instanceLock = new object();

        // Use a Virtual Host mapping so templates loaded via NavigateToString can still load local assets reliably.
        // This avoids file:// restrictions from about:blank origins.
        private const string AssetHostName = "ustad-assets";
        
        // Shared environment to avoid threading issues and multiple initializations
        private static CoreWebView2Environment _sharedEnvironment = null;
        private static System.Threading.Tasks.Task<CoreWebView2Environment> _environmentCreationTask = null;
        private static readonly object _environmentLock = new object();
        private static readonly System.Threading.SemaphoreSlim _globalSplashInitSemaphore = new System.Threading.SemaphoreSlim(1, 1);

        public ms_WebViewSplash()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(240, 242, 239); // Light background from design tokens (#f0f2ef)
            Width = 320;
            Height = 180;

            web = new WebView2
            {
                Dock = DockStyle.Fill,
                AllowExternalDrop = false
            };
            Controls.Add(web);

            Shown += async (_, __) =>
            {
                if (_initStarted) return;
                _initStarted = true;
                // Ensure we're on UI thread
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(async () => await InitializeAsync()));
                }
                else
                {
                    await InitializeAsync();
                }
            };
            
            // Add timeout to prevent splash from getting stuck
            FormClosing += (s, e) =>
            {
                try
                {
                    if (web != null && !web.IsDisposed)
                    {
                        web.Stop();
                    }
                }
                catch { }
            };
        }

        private async Task InitializeAsync()
        {
            try
            {
                if (_webViewPermanentlyDisabled)
                {
                    ShowWinFormsFallback(_fallbackStatusLabel?.Text ?? "Yükleniyor...");
                    return;
                }

                await _globalSplashInitSemaphore.WaitAsync();
                try
                {
                // WebView2 initialization MUST happen on UI thread (STA mode required)
                // Use shared environment to avoid multiple concurrent initializations and threading issues
                CoreWebView2Environment env = await GetSharedEnvironmentAsync();
                
                // Ensure WebView2 is initialized (must be on UI thread)
                try
                {
                    // Retry on E_ABORT to reduce flakiness during rapid show/hide
                    int maxRetries = 2;
                    int attempt = 0;
                    while (true)
                    {
                        attempt++;
                        try
                        {
                await web.EnsureCoreWebView2Async(env);
                            break;
                        }
                        catch (System.Runtime.InteropServices.COMException comEx) when (comEx.ErrorCode == unchecked((int)0x80004004) && attempt <= maxRetries)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Splash] EnsureCoreWebView2Async E_ABORT (attempt {attempt}/{maxRetries + 1}) - retrying...");
                            await Task.Delay(150);
                        }
                    }
                    
                    // Mark WebView2 as ready
                    _isWebViewReady = true;
                    System.Diagnostics.Debug.WriteLine("[Splash] WebView2 initialized and ready");
                }
                catch (Exception ensureEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[Splash] EnsureCoreWebView2Async failed: {ensureEx.Message}");
                    _isWebViewReady = false;
                    _webViewPermanentlyDisabled = true;
                    ShowWinFormsFallback(_fallbackStatusLabel?.Text ?? "Yükleniyor...");
                    return;
                }

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                
                // Search multiple locations for the template
                string[] searchPaths = new[]
                {
                    Path.Combine(baseDir, "Forms", "Templates", "LoadingTemplate.html"),
                    Path.Combine(baseDir, "Templates", "LoadingTemplate.html"),
                    Path.Combine(Application.StartupPath, "Forms", "Templates", "LoadingTemplate.html"),
                    Path.Combine(Application.StartupPath, "Templates", "LoadingTemplate.html"),
                };
                
                string templatePath = null;
                string assetBasePath = null;
                
                foreach (var path in searchPaths)
                {
                    System.Diagnostics.Debug.WriteLine($"[Splash] Checking for template at: {path}");
                    if (File.Exists(path))
                    {
                        templatePath = path;
                        // Asset path should be in the 'public' subdirectory
                        string templateDir = Path.GetDirectoryName(path);
                        assetBasePath = Path.Combine(templateDir, "public") + Path.DirectorySeparatorChar;
                        
                        // Verify public directory exists
                        if (!Directory.Exists(Path.Combine(templateDir, "public")))
                        {
                            System.Diagnostics.Debug.WriteLine($"[Splash] ⚠️ Public directory not found at: {Path.Combine(templateDir, "public")}");
                            // Try alternative location
                            assetBasePath = Path.Combine(baseDir, "Forms", "Templates", "public") + Path.DirectorySeparatorChar;
                            if (!Directory.Exists(Path.Combine(baseDir, "Forms", "Templates", "public")))
                            {
                                assetBasePath = Path.Combine(baseDir, "Templates", "public") + Path.DirectorySeparatorChar;
                            }
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"[Splash] Found template at: {path}");
                        System.Diagnostics.Debug.WriteLine($"[Splash] Asset base path: {assetBasePath}");
                        break;
                    }
                }

                if (templatePath == null)
                {
                    // Try embedded resource as fallback
                    System.Diagnostics.Debug.WriteLine($"[Splash] Template not found on disk. Trying embedded resource...");
                    try
                    {
                        var asm = System.Reflection.Assembly.GetExecutingAssembly();
                        const string resourceName = "YesiLdefter.Forms.Templates.LoadingTemplate.html";
                        var available = asm.GetManifestResourceNames();
                        string matchedResource = Array.Find(available, r => string.Equals(r, resourceName, StringComparison.OrdinalIgnoreCase))
                                          ?? Array.Find(available, r => r.EndsWith(".Templates.LoadingTemplate.html", StringComparison.OrdinalIgnoreCase));
                        
                        if (!string.IsNullOrEmpty(matchedResource))
                        {
                            using (var stream = asm.GetManifestResourceStream(matchedResource))
                            {
                                if (stream != null)
                                {
                                    using (var reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8))
                                    {
                                        string embeddedTemplate = reader.ReadToEnd();
                                        // Replace tokens - reuse existing baseDir variable
                                        string embeddedAssetPath = Path.Combine(baseDir, "Forms", "Templates", "public");
                                        if (!Directory.Exists(embeddedAssetPath))
                                        {
                                            embeddedAssetPath = Path.Combine(baseDir, "Templates", "public");
                                        }
                                        
                                        // Ensure directory exists
                                        if (!Directory.Exists(embeddedAssetPath))
                                        {
                                            Directory.CreateDirectory(embeddedAssetPath);
                                        }
                                        
                                        // Use virtual host mapping for embedded resources too
                                        string embeddedAssetBase = $"https://{AssetHostName}/";
                                        try
                                        {
                                            web.CoreWebView2.SetVirtualHostNameToFolderMapping(
                                                AssetHostName,
                                                embeddedAssetPath + Path.DirectorySeparatorChar,
                                                CoreWebView2HostResourceAccessKind.Allow);
                                            System.Diagnostics.Debug.WriteLine($"[Splash] Virtual host mapping set for embedded resource: {embeddedAssetBase} -> {embeddedAssetPath}");
                                        }
                                        catch (Exception mapEx)
                                        {
                                            // Fallback to file:// URI if virtual host mapping fails
                                            embeddedAssetBase = new Uri(embeddedAssetPath + Path.DirectorySeparatorChar).AbsoluteUri;
                                            System.Diagnostics.Debug.WriteLine($"[Splash] ⚠️ Virtual host mapping failed for embedded resource ({mapEx.Message}). Using file URI: {embeddedAssetBase}");
                                        }
                                        
                                        embeddedTemplate = embeddedTemplate.Replace("{{asset-base}}", embeddedAssetBase);
                                        
                                        System.Diagnostics.Debug.WriteLine($"[Splash] ✅ Loaded template from embedded resource: {matchedResource}");
                                        System.Diagnostics.Debug.WriteLine($"[Splash] Asset base: {embeddedAssetBase}");
                                        web.NavigateToString(embeddedTemplate);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Splash] Failed to load from embedded resource: {ex.Message}");
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[Splash] Template not found in any location. Using fallback.");
                    web.NavigateToString(GetFallbackHtml());
                    return;
                }

                // Ensure assetBasePath ends with directory separator for proper folder mapping
                if (!assetBasePath.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                    !assetBasePath.EndsWith("/") &&
                    !assetBasePath.EndsWith("\\"))
                {
                    assetBasePath += Path.DirectorySeparatorChar;
                }

                // Virtual host mapping for local assets
                string assetBase = $"https://{AssetHostName}/";
                try
                {
                    // Map host -> folder (physical path, no file://)
                    web.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        AssetHostName,
                        assetBasePath,
                        CoreWebView2HostResourceAccessKind.Allow);
                    System.Diagnostics.Debug.WriteLine($"[Splash] Virtual host mapping set: {assetBase} -> {assetBasePath}");
                }
                catch (Exception mapEx)
                {
                    // If mapping fails, fallback to file:// (may still work on some machines)
                    assetBase = new Uri(assetBasePath).AbsoluteUri;
                    System.Diagnostics.Debug.WriteLine($"[Splash] ⚠️ Virtual host mapping failed ({mapEx.Message}). Falling back to file URI: {assetBase}");
                }
                
                // Logo policy: user requested NO text and ONLY the horizontal PNG.
                // We intentionally do NOT inject base64 (it causes huge HTML and was unreliable in practice).
                // LoadingTemplate.html uses: src="{{asset-base}}yesildefter_horizontal.png"
                // So we only need a correct asset base URI pointing to the "public" folder.
                string logoBase64 = "";
                
                var template = File.ReadAllText(templatePath, Encoding.UTF8);
                
                // Debug: log logo file existence
                try
                {
                    string logoFsPath = Path.Combine(assetBasePath, "yesildefter_horizontal.png");
                    System.Diagnostics.Debug.WriteLine($"[Splash] Logo exists at {logoFsPath}: {File.Exists(logoFsPath)}");
                }
                catch { }
                
                // Perform replacements
                template = template.Replace("{{asset-base}}", assetBase ?? "");
                template = template.Replace("{{logo-base64}}", logoBase64 ?? "");
                
                // Verify replacement happened
                if (template.Contains("{{logo-base64}}"))
                {
                    System.Diagnostics.Debug.WriteLine("[Splash] ⚠️ WARNING: {{logo-base64}} placeholder still exists after replacement!");
                }

                System.Diagnostics.Debug.WriteLine($"[Splash] Template loaded. Size: {template.Length} bytes");
                
                // Navigate with size protection:
                // - NavigateToString has an approx ~2MB limit and will throw ArgumentException when exceeded.
                // - Our splash can exceed this because the horizontal logo base64 is ~2.4MB.
                if (_isWebViewReady && web?.CoreWebView2 != null)
                {
                    try
                    {
                        const int navigateToStringLimitBytes = 2 * 1024 * 1024; // ~2MB
                        if (template.Length > navigateToStringLimitBytes)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Splash] Template is large ({template.Length} chars). Using temp file navigation.");

                            string tempDir = Path.Combine(Path.GetTempPath(), "YesiLdefter_WebView2");
                            if (!Directory.Exists(tempDir))
                            {
                                Directory.CreateDirectory(tempDir);
                            }

                            string tempFile = Path.Combine(tempDir, $"splash_{DateTime.Now.Ticks}.html");
                            File.WriteAllText(tempFile, template, Encoding.UTF8);
                            string fileUri = new Uri(tempFile).AbsoluteUri;

                            web.CoreWebView2.Navigate(fileUri);
                            System.Diagnostics.Debug.WriteLine($"[Splash] Navigated to temp file: {tempFile}");
                        }
                        else
                        {
                web.NavigateToString(template);
                            System.Diagnostics.Debug.WriteLine("[Splash] Template navigation started (NavigateToString)");
                        }
                    }
                    catch (ArgumentException argEx)
                    {
                        // Fallback to temp file approach if NavigateToString fails unexpectedly
                        System.Diagnostics.Debug.WriteLine($"[Splash] NavigateToString ArgumentException, falling back to temp file: {argEx.Message}");
                        try
                        {
                            string tempDir = Path.Combine(Path.GetTempPath(), "YesiLdefter_WebView2");
                            if (!Directory.Exists(tempDir))
                            {
                                Directory.CreateDirectory(tempDir);
                            }

                            string tempFile = Path.Combine(tempDir, $"splash_{DateTime.Now.Ticks}.html");
                            File.WriteAllText(tempFile, template, Encoding.UTF8);
                            string fileUri = new Uri(tempFile).AbsoluteUri;

                            web.CoreWebView2.Navigate(fileUri);
                            System.Diagnostics.Debug.WriteLine($"[Splash] Navigated to temp file (fallback): {tempFile}");
                        }
                        catch (Exception fileNavEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Splash] Temp file navigation also failed: {fileNavEx.Message}");
                            web.NavigateToString(GetFallbackHtml());
                        }
                    }
                    catch (Exception navEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Splash] Navigation error: {navEx.Message}");
                        // Fallback to simple HTML
                        web.NavigateToString(GetFallbackHtml());
                    }
                }
                else
                {
                    // WebView2 not ready, use fallback
                    System.Diagnostics.Debug.WriteLine("[Splash] WebView2 not ready, using fallback HTML");
                    ShowWinFormsFallback(_fallbackStatusLabel?.Text ?? "Yükleniyor...");
                }

                if (_pendingHide)
                {
                    _pendingHide = false;
                    try { Hide(); } catch { }
                }
                }
                finally
                {
                    _globalSplashInitSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Splash] Init failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Splash] Stack: {ex.StackTrace}");
                
                _webViewPermanentlyDisabled = true;
                ShowWinFormsFallback(_fallbackStatusLabel?.Text ?? "Yükleniyor...");
            }
        }

        private void ShowWinFormsFallback(string message)
        {
            try
            {
                BackColor = Color.FromArgb(240, 242, 239);

                if (_fallbackPanel == null || _fallbackPanel.IsDisposed)
                {
                    // Replace WebView2 view with a reliable native fallback.
                    Controls.Clear();

                    _fallbackPanel = new Panel
                    {
                        Dock = DockStyle.Fill,
                        BackColor = BackColor
                    };

                    _fallbackLogo = new PictureBox
                    {
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Width = 220,
                        Height = 42,
                        BackColor = Color.Transparent
                    };

                    // Load logo from disk or embedded resource (no text).
                    try
                    {
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        string logoPath = Path.Combine(baseDir, "Forms", "Templates", "public", "yesildefter_horizontal.png");
                        if (!File.Exists(logoPath))
                            logoPath = Path.Combine(baseDir, "Templates", "public", "yesildefter_horizontal.png");

                        if (File.Exists(logoPath))
                        {
                            _fallbackLogo.Image = Image.FromFile(logoPath);
                        }
                        else
                        {
                            using (var s = System.Reflection.Assembly.GetExecutingAssembly()
                                .GetManifestResourceStream("YesiLdefter.Forms.Templates.public.yesildefter_horizontal.png"))
                            {
                                if (s != null) _fallbackLogo.Image = Image.FromStream(s);
                            }
                        }
                    }
                    catch { }

                    var spinner = new ProgressBar
                    {
                        Style = ProgressBarStyle.Marquee,
                        MarqueeAnimationSpeed = 30,
                        Width = 200,
                        Height = 12
                    };

                    _fallbackStatusLabel = new Label
                    {
                        AutoSize = true,
                        Text = "Yükleniyor...",
                        ForeColor = Color.FromArgb(55, 65, 81),
                        Font = new Font("Segoe UI", 9f, FontStyle.Regular)
                    };

                    _fallbackPanel.Controls.Add(_fallbackLogo);
                    _fallbackPanel.Controls.Add(spinner);
                    _fallbackPanel.Controls.Add(_fallbackStatusLabel);
                    Controls.Add(_fallbackPanel);

                    _fallbackPanel.Resize += (s, e) =>
                    {
                        int cx = _fallbackPanel.ClientSize.Width / 2;
                        int cy = _fallbackPanel.ClientSize.Height / 2;

                        _fallbackLogo.Left = cx - (_fallbackLogo.Width / 2);
                        _fallbackLogo.Top = cy - 55;

                        spinner.Left = cx - (spinner.Width / 2);
                        spinner.Top = _fallbackLogo.Bottom + 14;

                        _fallbackStatusLabel.Left = cx - (_fallbackStatusLabel.Width / 2);
                        _fallbackStatusLabel.Top = spinner.Bottom + 10;
                    };
                }

                if (_fallbackStatusLabel != null && !_fallbackStatusLabel.IsDisposed)
                {
                    _fallbackStatusLabel.Text = message ?? "Yükleniyor...";
                }

                // Ensure we never appear blank.
                _fallbackPanel?.PerformLayout();
                _fallbackPanel?.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Splash] WinForms fallback failed: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine("[Splash] Creating CoreWebView2Environment...");
                var env = await CoreWebView2Environment.CreateAsync();
                System.Diagnostics.Debug.WriteLine("[Splash] CoreWebView2Environment created successfully");
                lock (_environmentLock)
                {
                    _sharedEnvironment = env;
                }
                return env;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Splash] Failed to create CoreWebView2Environment: {ex.Message}");
                throw;
            }
        }
        
        private string LoadLogoFromEmbeddedResource()
        {
            try
            {
                // Try multiple embedded resource names
                string[] resourceNames = new[]
                {
                    "YesiLdefter.Forms.Templates.public.yesildefter_horizontal.png",
                    "YesiLdefter.Forms.Templates.public.yesildefter_horizontal_color.png"
                };
                
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                
                foreach (var resourceName in resourceNames)
                {
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            byte[] imageBytes = new byte[stream.Length];
                            stream.Read(imageBytes, 0, imageBytes.Length);
                            string base64 = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
                            System.Diagnostics.Debug.WriteLine($"[Splash] ✅ Logo loaded from embedded resource: {resourceName}, Size: {imageBytes.Length} bytes");
                            return base64;
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("[Splash] ⚠️ No logo found in embedded resources");
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Splash] ⚠️ Error loading logo from embedded resource: {ex.Message}");
                return "";
            }
        }
        
        private string GetFallbackHtml()
        {
            return @"
                <html>
                <body style='margin:0;background:linear-gradient(180deg, #e0eadf 24.5%, #eff2ef 64.5%, #ffffff 100%);color:#111827;font-family:Inter Tight,sans-serif;display:flex;align-items:center;justify-content:center;height:100vh;'>
                    <div style='text-align:center;'>
                        <div style='width:40px;height:40px;border:4px solid rgba(41,92,0,0.1);border-top-color:#295c00;border-radius:50%;margin:0 auto 16px;animation:spin 1s linear infinite;'></div>
                        <div style='font-size:13px;color:#6b7280;'>Yükleniyor...</div>
                    </div>
                    <style>@keyframes spin{to{transform:rotate(360deg);}}</style>
                </body>
                </html>";
        }

        public void SafeClose()
        {
            try
            {
                if (!IsDisposed)
                {
                    // Try to close gracefully first
                    if (InvokeRequired)
                    {
                        BeginInvoke(new Action(() => { try { Close(); } catch { } }));
                    }
                    else
                {
                    Close();
                }
            }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Splash] SafeClose error: {ex.Message}");
                // Force dispose if close fails
                try
                {
                    if (!IsDisposed)
                    {
                        Dispose();
                    }
                }
                catch { }
            }
        }
        
        public void ForceClose()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Splash] Force closing splash screen");
                
                // Stop WebView2 navigation if in progress
                try
                {
                    if (web?.CoreWebView2 != null)
                    {
                        web.Stop();
                    }
                }
                catch { }
                
                // Force dispose WebView2
                try
                {
                    if (web != null && !web.IsDisposed)
                    {
                        web.Dispose();
                    }
                }
            catch { }
                
                // Force close form
                if (!IsDisposed)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke(new Action(() => { try { Close(); Dispose(); } catch { } }));
                    }
                    else
                    {
                        try { Close(); } catch { }
                        try { Dispose(); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Splash] ForceClose error: {ex.Message}");
            }
        }

        public static ms_WebViewSplash ShowSplash()
        {
            // Always marshal splash creation to the main UI thread to avoid WebView2 COM errors.
            Form ui = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
            if (ui != null && ui.InvokeRequired)
            {
                return (ms_WebViewSplash)ui.Invoke(new Func<ms_WebViewSplash>(() => ShowSplash()));
            }

            lock (_instanceLock)
            {
                // Close any existing DevExpress splash screens
                try
                {
                    if (DevExpress.XtraSplashScreen.SplashScreenManager.Default != null)
                    {
                        DevExpress.XtraSplashScreen.SplashScreenManager.CloseForm(false);
                        System.Diagnostics.Debug.WriteLine("[Splash] Closed existing DevExpress splash screen");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Splash] Error closing DevExpress splash: {ex.Message}");
                }

                if (_currentInstance != null && !_currentInstance.IsDisposed)
                {
                    // Bring existing splash to front
                    _currentInstance.BringToFront();
                    _currentInstance.TopMost = true;
                    _currentInstance.Visible = true;
                    System.Diagnostics.Debug.WriteLine("[Splash] Existing splash brought to front");
                    Application.DoEvents();
                    return _currentInstance;
                }

                _currentInstance = new ms_WebViewSplash();
                _currentInstance.Show();
                Application.DoEvents();
                System.Diagnostics.Debug.WriteLine("[Splash] New splash created and shown");
                return _currentInstance;
            }
        }
        
        public static bool IsSplashReady()
        {
            lock (_instanceLock)
            {
                try
                {
                    return _currentInstance != null && 
                           !_currentInstance.IsDisposed && 
                           _currentInstance.Visible &&
                           _currentInstance._isWebViewReady &&
                           _currentInstance.web?.CoreWebView2 != null;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        public static bool IsSplashVisible()
        {
            lock (_instanceLock)
            {
                try
                {
                    return _currentInstance != null && 
                           !_currentInstance.IsDisposed && 
                           _currentInstance.Visible;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        public static async Task WaitForSplashReady(int maxWaitMs = 5000)
        {
            int waited = 0;
            int checkInterval = 100;
            
            while (waited < maxWaitMs)
            {
                if (IsSplashReady())
                {
                    System.Diagnostics.Debug.WriteLine($"[Splash] Splash ready after {waited}ms");
                    return;
                }
                await Task.Delay(checkInterval);
                waited += checkInterval;
            }
            
            System.Diagnostics.Debug.WriteLine($"[Splash] ⚠️ Splash not ready after {maxWaitMs}ms timeout");
        }

        public static void CloseSplash()
        {
            CloseSplashWithRetry(maxRetries: 3, retryDelayMs: 100);
        }
        
        public static void CloseSplashWithRetry(int maxRetries = 3, int retryDelayMs = 100)
        {
            // Marshal to UI thread to avoid cross-thread form close issues.
            Form ui = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
            if (ui != null && ui.InvokeRequired)
            {
                ui.BeginInvoke(new Action(() => CloseSplashWithRetry(maxRetries, retryDelayMs)));
                return;
            }

            lock (_instanceLock)
            {
                if (_currentInstance == null || _currentInstance.IsDisposed)
                {
                    // Also ensure DevExpress splash is closed
                    try
                    {
                        if (DevExpress.XtraSplashScreen.SplashScreenManager.Default != null)
                        {
                            DevExpress.XtraSplashScreen.SplashScreenManager.CloseForm(false);
                        }
                    }
                    catch { }
                    return;
                }

                // Reliability change:
                // Do NOT Dispose/Close the splash on every WaitFormClose. Just Hide it.
                // This avoids repeated WebView2 initialization and prevents intermittent E_ABORT failures.
                try
                {
                    System.Diagnostics.Debug.WriteLine("[Splash] Hiding splash (WaitFormClose)");
                    if (_currentInstance._initStarted && !_currentInstance._isWebViewReady)
                    {
                        _currentInstance._pendingHide = true;
                        System.Diagnostics.Debug.WriteLine("[Splash] Close requested during init; will hide after init completes.");
                    }
                    else
                    {
                        _currentInstance.Hide();
                    }
                    Application.DoEvents();
                }
                catch { }
                
                // Also ensure DevExpress splash is closed
                try
                {
                    if (DevExpress.XtraSplashScreen.SplashScreenManager.Default != null)
                    {
                        DevExpress.XtraSplashScreen.SplashScreenManager.CloseForm(false);
                    }
                }
                catch { }

                System.Diagnostics.Debug.WriteLine("[Splash] Splash close operation completed (hidden)");
            }
        }
        
        public static void ForceCloseSplash()
        {
            lock (_instanceLock)
            {
                System.Diagnostics.Debug.WriteLine("[Splash] Force closing splash (emergency)");
                
                if (_currentInstance != null && !_currentInstance.IsDisposed)
                {
                    _currentInstance.ForceClose();
                    Application.DoEvents();
                }
                
                // Also force close DevExpress splash
                try
                {
                    if (DevExpress.XtraSplashScreen.SplashScreenManager.Default != null)
                    {
                        DevExpress.XtraSplashScreen.SplashScreenManager.CloseForm(false);
                    }
                }
                catch { }
                
                _currentInstance = null;
            }
        }
        
        /// <summary>
        /// Emergency cleanup - call this on application exit or critical errors
        /// Ensures splash is terminated even if normal close fails
        /// </summary>
        public static void EmergencyCleanup()
        {
            lock (_instanceLock)
            {
                System.Diagnostics.Debug.WriteLine("[Splash] Emergency cleanup initiated");
                
                try
                {
                    ForceCloseSplash();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Splash] Emergency cleanup error: {ex.Message}");
                }
                
                // Final attempt - dispose everything
                try
                {
                    if (_currentInstance != null)
                    {
                        _currentInstance.Dispose();
                        _currentInstance = null;
                    }
                }
                catch { }
            }
        }

        public static void UpdateStatus(string message)
        {
            lock (_instanceLock)
            {
                if (_currentInstance != null && !_currentInstance.IsDisposed)
                {
                    try
                    {
                        if (_currentInstance.InvokeRequired)
                        {
                            _currentInstance.BeginInvoke(new Action(() => UpdateStatus(message)));
                            return;
                        }

                        // Always update WinForms fallback label (requirement: status must never disappear).
                        if (_currentInstance._fallbackStatusLabel != null && !_currentInstance._fallbackStatusLabel.IsDisposed)
                        {
                            _currentInstance._fallbackStatusLabel.Text = message ?? "Yükleniyor...";
                        }

                        // If WebView2 isn't ready, stop here (fallback still shows status).
                        if (_currentInstance.web?.CoreWebView2 == null)
                        {
                            return;
                        }
                        // Simple JavaScript string encoding
                        string encodedMsg = JavaScriptStringEncode(message ?? "");
                        string js = $"window.__ustadSetLoadingStatus && window.__ustadSetLoadingStatus({encodedMsg});";
                        _ = _currentInstance.web.CoreWebView2.ExecuteScriptAsync(js);
                        System.Diagnostics.Debug.WriteLine($"[Splash] Status updated: {message}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Splash] Failed to update status: {ex.Message}");
                    }
                }
            }
        }

        private static string JavaScriptStringEncode(string value)
        {
            if (value == null) return "\"\"";

            var sb = new StringBuilder();
            sb.Append('\"');

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

            sb.Append('\"');
            return sb.ToString();
        }
    }
}

