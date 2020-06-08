using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Quiche
{
    public class QuicheListener : IDisposable
    {
        private static class NativeMethods
        {
            /* negotiate version */
            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_negotiate_version(
                byte[] /* *const u8 */ scid,
                ulong /* size_t */ scid_len,
                byte[] /* *const u8 */ dcid,
                ulong /* size_t */ dcid_len,
                byte[] /* *mut u8 */ _out,
                ulong /* size_t */ out_len);

            /* version supported */
            [DllImport("libquiche")]
            internal static extern bool quiche_version_is_supported(
                uint version);

            /* retry */
            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_retry(
                byte[] /* *const u8 */ scid,
                ulong /* size_t */ scid_len,
                byte[] /* *const u8 */ dcid,
                ulong /* size_t */ dcid_len,
                byte[] /* *const u8 */ new_scid,
                ulong /* size_t */ new_scid_len,
                byte[] /* *const u8 */ token,
                ulong /* size_t */ tolen_len,
                byte[] /* *mut u8 */ _out,
                ulong /* size_t */ out_len);
        }

        private QuicheConfig Config { get; set; }
        private Dictionary<string, QuicheConnection> Connections { get; set; }

        // Track whether Dispose has been called.
        private bool _disposed = false;

        public static string HexDump(byte[] buf)
        {
            return string.Join("", buf.Select(x => x.ToString("x2")));
        }

        public QuicheListener(QuicheConfig config)
        {
            Config = config;
            Connections = new Dictionary<string, QuicheConnection>();
        }

        public byte[] Accept(byte[] odcid)
        {
            byte[] scid = new byte[QuicheClient.LOCAL_CONN_ID_LEN];
            new System.Random().NextBytes(scid);
            var conn = QuicheConnection.Accept(scid, odcid, Config);
            Connections.Add(HexDump(scid), conn);
            return scid;
        }

        public int Receive(byte[] scid, byte[] buf)
        {
            var conn = Connections[HexDump(scid)];
            return conn.Receive(buf);
        }

        public int Send(byte[] scid, byte[] buf)
        {
            var conn = Connections[HexDump(scid)];
            return conn.Send(buf);
        }

        public long NegotiateVersion(byte[] scid, byte[] dcid, byte[] _out)
        {
            return NativeMethods.quiche_negotiate_version(
                scid, (ulong)scid.Length,
                dcid, (ulong)dcid.Length,
                _out, (ulong)_out.Length);
        }

        public bool VersionIsSupported(uint version)
        {
            return NativeMethods.quiche_version_is_supported(version);
        }

        public long Retry(
            byte[] scid,
            byte[] dcid,
            byte[] new_scid,
            byte[] token,
            byte[] _out)
        {
            return NativeMethods.quiche_retry(
                scid, (ulong)scid.Length,
                dcid, (ulong)dcid.Length,
                new_scid, (ulong)new_scid.Length,
                token, (ulong)token.Length,
                _out, (ulong)_out.Length);
        }

        public void Close(byte[] scid)
        {
            string key = HexDump(scid);
            QuicheConnection conn;
            if(Connections.TryGetValue(key, out conn))
            {
                if(!conn.IsClosed)
                {
                    conn.Close(true, 0, null);
                }
            }
            Connections.Remove(key);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // dispose managed resource
                if (disposing)
                {
                    if(Connections != null)
                    {
                        foreach(var conn in Connections.Values)
                        {
                            conn.Dispose();
                        }
                        Connections = null;
                    }
                    if(Config != null)
                    {
                        Config.Dispose();
                        Config = null;
                    }
                }

                _disposed = true;
            }
        }

        ~QuicheListener()
        {
            Dispose(false);
        }
    }
}
