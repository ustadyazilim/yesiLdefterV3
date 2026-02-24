using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Tkn_Web2Checked
{

    public class WebView2RuntimeHelper
    {
        // 1. Evergreen kontrolü
        public static bool IsEvergreenInstalled()
        {
            try
            {
                string version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                //MessageBox.Show("GetAvailableBrowserVersionString() Version : " + version);
                return !string.IsNullOrEmpty(version);
            }
            catch (Exception ex) 
            {
                MessageBox.Show("IsEvergreenInstalled : false " + ex.Message.ToString());
                return false;
            }
        }

        // 2. Fixed Version klasörü kontrolü
        public static bool IsFixedVersionAvailable()
        {
            string fixedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YesiLdefter.exe.WebView2");
            return Directory.Exists(fixedPath);
        }

        // 3. Fixed Version environment oluştur
        public static async Task<CoreWebView2Environment> CreateFixedVersionEnvironmentAsync()
        {
            string fixedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YesiLdefter.exe.WebView2");
            MessageBox.Show("CreateFixedVersionEnvironmentAsync : " + fixedPath);
            return await CoreWebView2Environment.CreateAsync(fixedPath);
        }

        // 4. Evergreen installer indir ve kur
        public static async Task<bool> InstallEvergreenAsync()
        {
            MessageBox.Show("WebView2 Runtime yüklü değil. Yükleme işlemi başlayacak.", "WebView2 Runtime", MessageBoxButtons.OK, MessageBoxIcon.Information);

            bool is64 = Environment.Is64BitOperatingSystem;
            string installerUrl = is64
                ? "https://go.microsoft.com/fwlink/p/?LinkId=2124703"
                : "https://go.microsoft.com/fwlink/p/?LinkId=2124704";

            string installerFile = is64
                ? "MicrosoftEdgeWebView2RuntimeInstallerX64.exe"
                : "MicrosoftEdgeWebView2RuntimeInstallerX86.exe";

            string installerPath = Path.Combine(Path.GetTempPath(), installerFile);

            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(installerUrl, installerPath);
            }

            var process = new Process();
            process.StartInfo.FileName = installerPath;
            process.StartInfo.Arguments = "/silent /install";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas"; // yönetici yetkisi
            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }
    }
    


    /*    
    public class WebView2RuntimeHelper
    {
           
           public bool IsWebView2RuntimeInstalled()
           {
               try
               {
                   MessageBox.Show("WebView2 Runtime kontrol ediliyor...", "WebView2 Runtime", MessageBoxButtons.OK, MessageBoxIcon.Information);
                   string version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                   MessageBox.Show($"WebView2 Runtime sürümü: {version}", "WebView2 Runtime", MessageBoxButtons.OK, MessageBoxIcon.Information);
                   return !string.IsNullOrEmpty(version);
               }
               catch (Exception ex)
               {
                   MessageBox.Show($"WebView2 Runtime kontrol edilirken bir hata oluştu: {ex.Message}", "WebView2 Runtime", MessageBoxButtons.OK, MessageBoxIcon.Error);
                   return false;
               }

           }

           public async Task<bool> EnsureWebView2RuntimeAsync()
           {
               MessageBox.Show("WebView2 Runtime kontrol ediliyor...", "WebView2 Runtime", MessageBoxButtons.OK, MessageBoxIcon.Information);

               if (IsWebView2RuntimeInstalled())
                   return true;


               MessageBox.Show("WebView2 Runtime yüklü değil. Yükleme işlemi başlayacak.", "WebView2 Runtime", MessageBoxButtons.OK, MessageBoxIcon.Information);

               bool is64 = Environment.Is64BitOperatingSystem;
               string installerUrl = is64
                   ? "https://go.microsoft.com/fwlink/p/?LinkId=2124703"
                   : "https://go.microsoft.com/fwlink/p/?LinkId=2124704";

               string installerFile = is64
                   ? "MicrosoftEdgeWebView2RuntimeInstallerX64.exe"
                   : "MicrosoftEdgeWebView2RuntimeInstallerX86.exe";

               string installerPath = Path.Combine(Path.GetTempPath(), installerFile);

               using (var client = new WebClient())
               {
                   await client.DownloadFileTaskAsync(installerUrl, installerPath);
               }

               var process = new Process();
               process.StartInfo.FileName = installerPath;
               process.StartInfo.Arguments = "/silent /install";
               process.StartInfo.UseShellExecute = true;
               process.StartInfo.Verb = "runas"; // yönetici yetkisi
               process.Start();
               process.WaitForExit();

               return process.ExitCode == 0;
           }

    }
    */
}



