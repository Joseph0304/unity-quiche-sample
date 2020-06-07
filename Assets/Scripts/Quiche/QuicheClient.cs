using System;
using System.Runtime.InteropServices;

namespace Quiche
{
    public class QuicheClient : IDisposable
    {
        enum QuicError {
            // There is no more work to do.
            QUICHE_ERR_DONE = -1,

            // The provided buffer is too short.
            QUICHE_ERR_BUFFER_TOO_SHORT = -2,

            // The provided packet cannot be parsed because its version is unknown.
            QUICHE_ERR_UNKNOWN_VERSION = -3,

            // The provided packet cannot be parsed because it contains an invalid
            // frame.
            QUICHE_ERR_INVALID_FRAME = -4,

            // The provided packet cannot be parsed.
            QUICHE_ERR_INVALID_PACKET = -5,

            // The operation cannot be completed because the connection is in an
            // invalid state.
            QUICHE_ERR_INVALID_STATE = -6,

            // The operation cannot be completed because the stream is in an
            // invalid state.
            QUICHE_ERR_INVALID_STREAM_STATE = -7,

            // The peer's transport params cannot be parsed.
            QUICHE_ERR_INVALID_TRANSPORT_PARAM = -8,

            // A cryptographic operation failed.
            QUICHE_ERR_CRYPTO_FAIL = -9,

            // The TLS handshake failed.
            QUICHE_ERR_TLS_FAIL = -10,

            // The peer violated the local flow control limits.
            QUICHE_ERR_FLOW_CONTROL = -11,

            // The peer violated the local stream limits.
            QUICHE_ERR_STREAM_LIMIT = -12,

            // The received data exceeds the stream's final size.
            QUICHE_ERR_FINAL_SIZE = -13,
        };
        private static class NativeMethods
        {
            /* header */
            [DllImport("libquiche")]
            internal static extern int quiche_header_info(
                IntPtr /* *mut u8 */ buf,
                ulong /* size_t */ buf_len,
                ulong /* size_t */ dcil,
                IntPtr /* *mut u32 */ version,
                IntPtr /* *mut u8 */ ty,
                IntPtr /* *mut u8 */ scid,
                IntPtr /* *mut size_t */ scid_len,
                IntPtr /* *mut u8 */ dcid,
                IntPtr /* *mut size_t */ dcid_len,
                IntPtr /* *mut u8 */ token,
                IntPtr /* *mut size_t */ token_len);

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
            internal static extern bool quiche_negotiate_version(uint version);

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
        public const int MAX_DATAGRAM_SIZE = 1350;
        private const int LOCAL_CONN_ID_LEN = 16;

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

        public int Receive(byte[] buf)
        {
            return Connection.Receive(buf);
        }

        public int Send(byte[] buf)
        {
            return Connection.Send(buf);
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
