using System;
using System.Runtime.InteropServices;

namespace Quiche
{
    public class QuicheVersion
    {
        private static class NativeMethods
        {
            /* version */
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_version();
        }

        // The current QUIC wire version.
        public const ulong QUICHE_PROTOCOL_VERSION = 0xff000018;

        public static string GetVersion()
        {
            // quiche_versionが返すのはスタック領域のメモリなので、直接stringに変換できない
            // そのため、一度IntPtrを介してからstringに変換する
            var version = NativeMethods.quiche_version();
            return Marshal.PtrToStringAnsi(version);
        }
    }
}
