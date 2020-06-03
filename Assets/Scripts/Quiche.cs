using System.Runtime.InteropServices;

public class Quiche
{
    private static class NativeMethods
    {
        [DllImport("libquiche")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        internal static extern string quiche_version();
    }
}
