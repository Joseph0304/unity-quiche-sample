using System;
using System.Runtime.InteropServices;

namespace Quiche
{
    public class QuicheClient : IDisposable
    {
        private static class NativeMethods
        {
            /* negotiate version */
            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_negotiate_version(
                IntPtr /* *const u8 */ scid,
                ulong /* size_t */ scid_len,
                IntPtr /* *const u8 */ dcid,
                ulong /* size_t */ dcid_len,
                IntPtr /* *mut u8 */ _out,
                ulong /* size_t */ out_len);

            /* version supported */
            [DllImport("libquiche")]
            internal static extern bool quiche_version_is_supported(
                uint version);

            /* retry */
            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_retry(
                IntPtr /* *const u8 */ scid,
                ulong /* size_t */ scid_len,
                IntPtr /* *const u8 */ dcid,
                ulong /* size_t */ dcid_len,
                IntPtr /* *const u8 */ new_scid,
                ulong /* size_t */ new_scid_len,
                IntPtr /* *const u8 */ token,
                ulong /* size_t */ tolen_len,
                IntPtr /* *mut u8 */ _out,
                ulong /* size_t */ out_len);
        }

        // The minimum length of Initial packets sent by a client.
        public const int QUICHE_MIN_CLIENT_INITIAL_LEN = 1200;

        public const int MAX_DATAGRAM_SIZE = 1350;
        public const int LOCAL_CONN_ID_LEN = 16;

        private QuicheConfig Config { get; set; }
        private QuicheConnection Connection { get; set; }

        private byte[] scid = Array.Empty<byte>();

        // Track whether Dispose has been called.
        private bool _disposed = false;

        public bool IsClosed
        {
            get { return Connection != null ? Connection.IsClosed : true; }
        }

        public bool IsEstablished
        {
            get { return Connection != null ? Connection.IsEstablished : false; }
        }

        public QuicheClient(QuicheConfig config)
        {
            Config = config;
        }

        public void Connect(string serverName)
        {
            scid = new byte[LOCAL_CONN_ID_LEN];
            new System.Random().NextBytes(scid);

            // Create a QUIC connection and initiate handshake.
            Connection = QuicheConnection.Connect(
                serverName, scid, Config);
        }

        public int Close(byte[] reason)
        {
            return Connection.Close(true, 0, reason);
        }

        public int Receive(byte[] buf)
        {
            return Connection.Receive(buf);
        }

        public int Send(byte[] buf)
        {
            return Connection.Send(buf);
        }

        public int StreamReceive(ulong streamId, byte[] _out, ref bool fin)
        {
            return Connection.StreamReceive(streamId, _out, ref fin);
        }

        public int StreamSend(ulong streamId, byte[] buf, bool fin)
        {
            return Connection.StreamSend(streamId, buf, fin);
        }

        public QuicheStreamIterator Readable()
        {
            return Connection.Readable();
        }

        public string HexDump
        {
            get {return string.Join(",", scid);}
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
                    if(Connection != null)
                    {
                        Connection.Dispose();
                        Connection = null;
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

        ~QuicheClient()
        {
            Dispose(false);
        }
    }
}
