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
            // DPI farkındalığını sistem seviyesinde ayarla
            SetProcessDPIAware();

            // veya daha modern bir yaklaşım (Windows 10 ve üzeri)
            SetProcessDpiAwarenessContext((int)DpiAwarenessContext.PerMonitorAwareV2);

            DevExpress.UserSkins.BonusSkins.Register();
                        
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // NOTE(@Janberk): WebView2 splash enabled for initial app startup
            // Create and run main form
            try
            {
                Application.Run(new main(args));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Program] Error: {ex.Message}");
                throw;
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
