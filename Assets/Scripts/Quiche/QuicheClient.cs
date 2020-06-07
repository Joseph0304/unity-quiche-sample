using System;
using System.Runtime.InteropServices;

namespace Quiche
{
    public delegate void Callback(string line, IntPtr argp);

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
            /* version */
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_version();

            /* debug logging */
            [DllImport("libquiche")]
            internal static extern int quiche_enable_debug_logging(
                Callback cb, IntPtr argp);

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

            /* accept */
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_accept(
                IntPtr /* *const u8 */ scid,
                ulong /* size_t */ scid_len,
                IntPtr /* *const u8 */ odcid,
                ulong /* size_t */ odcid_len,
                IntPtr config);

            /* connect */
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_connect(
                string server_name,
                [MarshalAs(UnmanagedType.SafeArray)] byte[] scid,
                ulong /* size_t */ scid_len,
                IntPtr config);

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

            /* connection */
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_conn_new_with_tls(
                IntPtr /* *const u8 */ scid,
                ulong /* size_t */ scid_len,
                IntPtr /* *const u8 */ odcid,
                ulong /* size_t */ odcid_len,
                IntPtr config,
                IntPtr /* *mut c_void */ ssl,
                bool is_server);

            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_conn_recv(
                IntPtr conn,
                byte[] buf,
                ulong /* size_t */ buf_len);

            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_conn_send(
                IntPtr conn,
                byte[] buf,
                ulong /* size_t */ buf_len);

            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_conn_stream_recv(
                IntPtr conn,
                ulong stream_id,
                IntPtr /* *mut u8 */ buf,
                ulong /* size_t */ buf_len,
                IntPtr /* &mut bool */ fin);

            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_conn_stream_send(
                IntPtr conn,
                ulong stream_id,
                IntPtr /* *const u8 */ buf,
                ulong /* size_t */ buf_len,
                bool fin);

            [DllImport("libquiche")]
            internal static extern int quiche_conn_stream_shutdown(
                IntPtr conn,
                ulong stream_id,
                IntPtr /* Shutdown */ direction,
                ulong err);

            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_conn_stream_capacity(
                IntPtr conn,
                ulong stream_id);

            [DllImport("libquiche")]
            internal static extern bool quiche_conn_stream_finished(
                IntPtr conn,
                ulong stream_id);

            [DllImport("libquiche")]
            internal static extern IntPtr /* *mut StreamIter */ quiche_conn_readable(
                IntPtr conn);

            [DllImport("libquiche")]
            internal static extern IntPtr /* *mut StreamIter */ quiche_conn_writable(
                IntPtr conn);

            [DllImport("libquiche")]
            internal static extern int quiche_conn_stream_init_application_data(
                IntPtr conn,
                ulong stream_id,
                IntPtr /* *mut c_void */ data);

            [DllImport("libquiche")]
            internal static extern IntPtr /* *mut c_void */ quiche_conn_stream_application_data(
                IntPtr conn,
                ulong stream_id);

            [DllImport("libquiche")]
            internal static extern int quiche_conn_close(
                IntPtr conn,
                bool app,
                ulong err,
                IntPtr /* *const u8 */ reason,
                ulong /* size_t */ reason_len);

            [DllImport("libquiche")]
            internal static extern ulong quiche_conn_timeout_as_nanos(IntPtr conn);
            [DllImport("libquiche")]
            internal static extern ulong quiche_conn_timeout_as_millis(IntPtr conn);
            [DllImport("libquiche")]
            internal static extern void quiche_conn_on_timeout(IntPtr conn);

            [DllImport("libquiche")]
            internal static extern void quiche_conn_application_proto(
                IntPtr conn,
                IntPtr /* *const u8 */ _out,
                ulong /* size_t */ out_len);

            [DllImport("libquiche")]
            internal static extern bool quiche_conn_is_established(IntPtr conn);
            [DllImport("libquiche")]
            internal static extern bool quiche_conn_is_in_early_data(IntPtr conn);
            [DllImport("libquiche")]
            internal static extern bool quiche_conn_is_closed(IntPtr conn);

            [DllImport("libquiche")]
            internal static extern bool quiche_stream_iter_next(
                IntPtr iter,
                IntPtr /* *mut u64 */ stream_id);

            [DllImport("libquiche")]
            internal static extern void quiche_stream_iter_free(IntPtr iter);

            [DllImport("libquiche")]
            internal static extern void quiche_conn_stats(
                IntPtr conn,
                IntPtr /* &mut Stats */ _out);

            [DllImport("libquiche")]
            internal static extern void quiche_conn_free(IntPtr conn);
        }

        public static string GetVersion()
        {
            // quiche_versionが返すのはスタック領域のメモリなので、直接stringに変換できない
            // そのため、一度IntPtrを介してからstringに変換する
            var version = NativeMethods.quiche_version();
            return Marshal.PtrToStringAnsi(version);
        }

        public static void DebugLog(Callback cb) {
            NativeMethods.quiche_enable_debug_logging(cb, IntPtr.Zero);
        }

        public const int MAX_DATAGRAM_SIZE = 1350;
        private const int LOCAL_CONN_ID_LEN = 16;

        private QuicheConfig Config { get; set; }
        private IntPtr conn = IntPtr.Zero;

        private byte[] scid = Array.Empty<byte>();

        // Track whether Dispose has been called.
        private bool _disposed = false;

        public QuicheClient(QuicheConfig config)
        {
            Config = config;
        }

        public void Connect(string serverName)
        {
            scid = new byte[LOCAL_CONN_ID_LEN];
            new System.Random().NextBytes(scid);

            // Create a QUIC connection and initiate handshake.
            conn = NativeMethods.quiche_connect(
                serverName, scid, (ulong)scid.Length, Config.RawConfig);
        }

        public int Receive(byte[] buf)
        {
            return (int)NativeMethods.quiche_conn_recv(
                conn, buf, (ulong)buf.Length);
        }

        public int Send(byte[] buf)
        {
            return (int)NativeMethods.quiche_conn_send(
                conn, buf, (ulong)MAX_DATAGRAM_SIZE);
        }

        public bool IsClosed
        {
            get {return NativeMethods.quiche_conn_is_closed(conn);}
        }

        public bool IsEstablished
        {
            get {return NativeMethods.quiche_conn_is_established(conn);}
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
                    if(Config != null)
                    {
                        Config.Dispose();
                        Config = null;
                    }
                }

                if(conn != IntPtr.Zero)
                {
                    NativeMethods.quiche_conn_free(conn);
                    conn = IntPtr.Zero;
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
