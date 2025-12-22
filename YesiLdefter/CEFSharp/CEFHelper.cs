using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CefSharp;
using CefSharp.WinForms;
using CefSharp.SchemeHandler;
using System.Reflection;

namespace YesiLdefter.CEFSharp
{
    public static class CEFHelper
    {
        
        private static ChromiumWebBrowser _cefBrowser = null;
        private static bool _isInitialized = false;
        private static int _initializedThreadId = -1;
        private static readonly object _lockObject = new object();

        public static ChromiumWebBrowser CreateBrowser
        {
            get
            {
                if (_cefBrowser != null)
                {
                    return _cefBrowser;
                }

                lock (_lockObject)
                {
                    if (_cefBrowser != null)
                    {
                        return _cefBrowser;
                    }

                    // CRITICAL: Disable automatic shutdown on Application.Exit
                    // This prevents CefSharp from calling Cef.Shutdown() from the wrong thread
                    // (e.g., DevExpress SplashScreen thread instead of UI thread)
                    // We will call Shutdown() manually from CEFHelper.Shutdown()
                    CefSharpSettings.ShutdownOnExit = false;
                    
                    CefSettings settings = new CefSettings() { RemoteDebuggingPort = 8090 };
                    settings.CefCommandLineArgs.Add("remote-allow-origins", "*");

                    /* local html kullanacığın zaman açacaksın
                     * 
                    settings.RegisterScheme(new CefCustomScheme
                    {
                        SchemeName = CustomProtocolSchemeHandlerFactory.SchemeName,
                        SchemeHandlerFactory = new CustomProtocolSchemeHandlerFactory()
                    });

                    // yüklemek için

                    chromiumWebBrowser.LoadUrl("resource://ui/index.html");

                    /// unutma index.html sayfasını projenin kaynak kodlarına ekledikten sonra (yesiLdefterV3.sln)
                    /// index.html dosyasının properties ini açarak
                    /// Build Action : Content
                    /// Build Action : Embedded Resource  
                    /// şeklinde değiştirmen gerekiyor
                    /// Eğer index.html içinde çağırdığın resim dosyaları var ise onlara aynısı yapacaksın

                    */

                    Cef.Initialize(settings);
                    _isInitialized = true;
                    _initializedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    _cefBrowser = new ChromiumWebBrowser();

                    return _cefBrowser;
                }
            }
        }

        /// <summary>
        /// Safely shutdown CefSharp on the same thread it was initialized on
        /// </summary>
        public static void Shutdown()
        {
            lock (_lockObject)
            {
                if (!_isInitialized)
                {
                    return;
                }

                try
                {
                    // Dispose browser first
                    if (_cefBrowser != null)
                    {
                        _cefBrowser.Dispose();
                        _cefBrowser = null;
                    }

                    // Check if we're on the correct thread
                    int currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    if (currentThreadId != _initializedThreadId)
                    {
                        // If not on UI thread, invoke on UI thread
                        if (System.Windows.Forms.Application.OpenForms.Count > 0)
                        {
                            var mainForm = System.Windows.Forms.Application.OpenForms[0];
                            if (mainForm != null && mainForm.InvokeRequired)
                            {
                                mainForm.Invoke(new System.Action(() =>
                                {
                                    if (Cef.IsInitialized == true)
                                    {
                                        Cef.Shutdown();
                                    }
                                }));
                                _isInitialized = false;
                                _initializedThreadId = -1;
                                return;
                            }
                        }
                    }

                    // Shutdown on current thread (should be UI thread)
                    if (Cef.IsInitialized == true)
                    {
                        Cef.Shutdown();
                    }
                    _isInitialized = false;
                    _initializedThreadId = -1;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CEFHelper] Shutdown error: {ex.Message}");
                }
            }
        }

        /// proje içinde localdeki bir html sayfasını açmak istediğinde kullanırsın
        /// 

        public class ResourceSchemeHandler : ResourceHandler
        {
            public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
            {
                Uri u = new Uri(request.Url);
                string file = u.Authority + u.AbsolutePath;

                Assembly ass = Assembly.GetExecutingAssembly();
                string resourcePath = ass.GetName().Name + "." + file.Replace("/", ".");

                if (ass.GetManifestResourceStream(resourcePath) != null)
                {
                    Stream = ass.GetManifestResourceStream(resourcePath);

                    switch (Path.GetExtension(file))
                    {
                        case ".html":
                            MimeType = "text/html";
                            break;
                        case ".js":
                            MimeType = "text/javascript";
                            break;
                        case ".png":
                            MimeType = "image/png";
                            break;
                        case ".jpg":
                        case ".jpeg":
                            MimeType = "image/jpeg";
                            break;
                        case ".gif":
                            MimeType = "image/gif";
                            break;
                        case ".appchache":
                        case ".manifest":
                            MimeType = "text/cache-manifest";
                            break;
                        default:
                            MimeType = "application/octet-stream";
                            break;
                    }

                    callback.Continue();
                    return CefReturnValue.Continue;
                }

                callback.Dispose();
                return CefReturnValue.Cancel;
            }
        }
    
        public class CustomProtocolSchemeHandlerFactory : ISchemeHandlerFactory
        {
            public const string SchemeName = "resource";
            public IResourceHandler Create (IBrowser browser, IFrame frame, string schemeName, IRequest request)
            {
                return new ResourceSchemeHandler();
            }
        }
                
    }
}
