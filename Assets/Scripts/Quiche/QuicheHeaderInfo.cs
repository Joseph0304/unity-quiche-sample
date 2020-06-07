using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Quiche
{
    public class QuicheHeaderInfo
    {
        private static class NativeMethods
        {
            /* header */
            [DllImport("libquiche")]
            internal static extern int quiche_header_info(
                byte[] /* *mut u8 */ buf,
                ulong /* size_t */ buf_len,
                ulong /* size_t */ dcil,
                ref uint /* *mut u32 */ version,
                ref byte /* *mut u8 */ ty,
                byte[] /* *mut u8 */ scid,
                ref ulong /* *mut size_t */ scid_len,
                byte[] /* *mut u8 */ dcid,
                ref ulong /* *mut size_t */ dcid_len,
                byte[] /* *mut u8 */ token,
                ref ulong /* *mut size_t */ token_len);
        }

        public static QuicheHeaderInfo Construct(byte[] buf)
        {
            byte type = 0;
            uint version = 0;

            var scid = new byte[QuicheConnection.QUICHE_MAX_CONN_ID_LEN];
            ulong scidLength = (ulong)scid.Length;
            var dcid = new byte[QuicheConnection.QUICHE_MAX_CONN_ID_LEN];
            ulong dcidLength = (ulong)dcid.Length;
            var token = new byte[65535];
            ulong tokenLength = (ulong)token.Length;

            var err = NativeMethods.quiche_header_info(
                buf, (ulong)buf.Length,
                QuicheClient.LOCAL_CONN_ID_LEN,
                ref version,
                ref type,
                scid, ref scidLength,
                dcid, ref dcidLength,
                token, ref tokenLength);
            if(err < 0)
            {
                throw new Exception();
            }
            return new QuicheHeaderInfo(
                version,
                type,
                scid.Take((int)scidLength).ToArray(),
                dcid.Take((int)dcidLength).ToArray(),
                token.Take((int)tokenLength).ToArray());
        }

        public uint Version { get; }
        public byte Type { get; }
        public byte[] Scid { get; }
        public byte[] Dcid { get; }
        public byte[] Token { get; }

        private QuicheHeaderInfo(
            uint version,
            byte type,
            byte[] scid,
            byte[] dcid,
            byte[] token)
        {
            Version = version;
            Type = type;
            Scid = scid;
            Dcid = dcid;
            Token = token;
        }
    }
}
