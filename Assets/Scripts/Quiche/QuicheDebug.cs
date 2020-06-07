using System;
using System.Runtime.InteropServices;

namespace Quiche
{
    public delegate void Callback(string line, IntPtr argp);

    public class QuicheDebug
    {
        private static class NativeMethods
        {
            /* debug logging */
            [DllImport("libquiche")]
            internal static extern int quiche_enable_debug_logging(
                Callback cb, IntPtr argp);
        }

        public static void EnableDebugLogging(Callback cb)
        {
            NativeMethods.quiche_enable_debug_logging(cb, IntPtr.Zero);
        }
    }
}
