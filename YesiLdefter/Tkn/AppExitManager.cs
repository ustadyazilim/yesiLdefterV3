using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tkn
{
    public static class AppExitManager
    {
        // 0 = not exiting, 1 = exit requested
        static int s_exiting;

        public static bool ExitRequested => s_exiting == 1;

        public static void RequestExit(Form owner = null)
        {
            if (Interlocked.Exchange(ref s_exiting, 1) == 1) return;

            try
            {
                Form target = owner ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);

                if (target != null && target.IsHandleCreated)
                {
                    target.BeginInvoke((MethodInvoker)(() => SafeExit()));
                    return;
                }

                if (Application.OpenForms.Count > 0)
                {
                    var f = Application.OpenForms[0];
                    if (f.IsHandleCreated)
                        f.BeginInvoke((MethodInvoker)(() => SafeExit()));
                    else
                        Task.Run(() => { Thread.Sleep(10); SafeExit(); });
                    return;
                }

                Task.Run(() => { Thread.Sleep(10); SafeExit(); });
            }
            catch
            {
                try { SafeExit(); } catch { }
            }
        }

        static void SafeExit()
        {
            try { Application.Exit(); }
            catch { }
        }
    }
}
