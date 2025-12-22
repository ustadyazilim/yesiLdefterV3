using CefSharp;
using DevExpress.XtraWaitForm;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YesiLdefter
{
    static class Program
    {
        /// <summary>
        /// Uygulamanın ana girdi noktası.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // CRITICAL: Disable CefSharp auto-shutdown BEFORE any CefSharp code is loaded
            // This prevents the DevExpress SplashScreen thread from triggering Cef.Shutdown()
            // on the wrong thread when calling Application.RaiseExit()
            CefSharpSettings.ShutdownOnExit = false;
            
            // DPI farkındalığını sistem seviyesinde ayarla
            SetProcessDPIAware();

            // veya daha modern bir yaklaşım (Windows 10 ve üzeri)
            SetProcessDpiAwarenessContext((int)DpiAwarenessContext.PerMonitorAwareV2);

            DevExpress.UserSkins.BonusSkins.Register();
                        
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Create and run main form
            try
            {
                Application.Run(new main(args));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Program] Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Program] Stack: {ex.StackTrace}");
                throw;
            }
            finally
            {
                // Shutdown CefSharp on the UI thread after Application.Run() completes
                // This is the correct place because we're still on the main UI thread (Thread 1)
                try
                {
                    YesiLdefter.CEFSharp.CEFHelper.Shutdown();
                    System.Diagnostics.Debug.WriteLine("[Program] CefSharp shutdown completed successfully");
                }
                catch (Exception cefEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[Program] CefSharp shutdown error: {cefEx.Message}");
                }
            }
        }
        // DPI farkındalığı için P/Invoke tanımları
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

        private enum DpiAwarenessContext
        {
            Unaware = -1,
            SystemAware = -2,
            PerMonitorAware = -3,
            PerMonitorAwareV2 = -4
        }

    }
}
