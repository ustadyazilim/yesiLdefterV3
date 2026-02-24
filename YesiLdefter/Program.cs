using DevExpress.XtraWaitForm;
using Microsoft.Web.WebView2.Core;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Tkn_Web2Checked;

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
            RunApp(args);
            return;
            // Bu kontrollere gerek kalmadan WebView2Loader.dll  yüklemeyi otomatik yapıyor

            // 1. Evergreen kontrolü
            if (WebView2RuntimeHelper.IsEvergreenInstalled())
            {
                RunApp(args);
                return;
            }

            // 2. Fixed Version kontrolü
            if (WebView2RuntimeHelper.IsFixedVersionAvailable())
            {
                RunApp(args);
                return;
            }
            
            // 3. Hiçbiri yoksa → installer indir
            var result = WebView2RuntimeHelper.InstallEvergreenAsync().Result;
            if (result)
            {
                Application.Restart();
                Environment.Exit(0);
            }
            else
            {
                MessageBox.Show("WebView2 Runtime kurulumu başarısız oldu. Lütfen manuel yükleyin:\nhttps://developer.microsoft.com/en-us/microsoft-edge/webview2/");
                return;
            }
            
        }
        static void RunApp(string[] args)
        {
            // DPI farkındalığını sistem seviyesinde ayarla
            SetProcessDPIAware();

            // veya daha modern bir yaklaşım (Windows 10 ve üzeri)
            SetProcessDpiAwarenessContext((int)DpiAwarenessContext.PerMonitorAwareV2);

            DevExpress.UserSkins.BonusSkins.Register();
                        
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new main(args));
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
