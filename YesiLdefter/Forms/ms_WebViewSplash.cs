using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YesiLdefter
{
    /// <summary>
    /// Splash screen class - disabled. All methods are no-ops to maintain API compatibility.
    /// </summary>
    public class ms_WebViewSplash : Form
    {
        public ms_WebViewSplash()
        {
            // Constructor kept for compatibility, but splash is disabled
        }

        public static ms_WebViewSplash ShowSplash()
        {
            // Splash screen disabled - return null to prevent any splash from showing
            return null;
        }

        public static bool IsSplashReady()
        {
            // Splash screen disabled - always return false
            return false;
        }

        public static bool IsSplashVisible()
        {
            // Splash screen disabled - always return false
            return false;
        }

        public static async Task WaitForSplashReady(int maxWaitMs = 5000)
        {
            // Splash screen disabled - return immediately
            await Task.CompletedTask;
        }

        public static void CloseSplash()
        {
            // Splash screen disabled - no-op to prevent breaking existing calls
        }

        public static void CloseSplashWithRetry(int maxRetries = 3, int retryDelayMs = 100)
        {
            // Splash screen disabled - no-op to prevent breaking existing calls
        }

        public static void ForceCloseSplash()
        {
            // Splash screen disabled - no-op to prevent breaking existing calls
        }

        /// <summary>
        /// Emergency cleanup - call this on application exit or critical errors
        /// Ensures splash is terminated even if normal close fails
        /// </summary>
        public static void EmergencyCleanup()
        {
            // Splash screen disabled - no-op to prevent breaking existing calls
        }

        public static void UpdateStatus(string message)
        {
            // Splash screen disabled - no-op to prevent breaking existing calls
            // Status updates are silently ignored
        }
    }
}
