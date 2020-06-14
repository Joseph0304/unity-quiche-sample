using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Quiche.H3
{
    [StructLayout(LayoutKind.Sequential)]
    public struct H3Header
    {
        [MarshalAs(UnmanagedType.LPArray)]
        public byte[] name;
        public ulong /* usize */ name_len;

        [MarshalAs(UnmanagedType.LPArray)]
        public byte[] value;
        public ulong /* usize */ value_len;

        public static string DebugString(H3Header header)
        {
            return $"Header({Encoding.ASCII.GetString(header.name)}, {Encoding.ASCII.GetString(header.value)})";
        }

        public H3Header(string name, string value)
        {
            this.name = Encoding.ASCII.GetBytes(name);
            name_len = (ulong)this.name.Length;
            this.value = Encoding.ASCII.GetBytes(value);
            value_len = (ulong)this.value.Length;
        }
    }
}
