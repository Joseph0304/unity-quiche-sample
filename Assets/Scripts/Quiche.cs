using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

using UnityEngine;

public delegate void Callback(string line, IntPtr argp);

public class Quiche : IDisposable
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

        /* quiche config */
        [DllImport("libquiche")]
        internal static extern IntPtr quiche_config_new(uint version);
        [DllImport("libquiche")]
        internal static extern int quiche_config_load_cert_chain_from_pem_file(
            IntPtr config, string path);
        [DllImport("libquiche")]
        internal static extern int quiche_config_load_priv_key_from_pem_file(
            IntPtr config, string path);
        [DllImport("libquiche")]
        internal static extern void quiche_config_verify_peer(
            IntPtr config, bool v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_grease(IntPtr config, bool v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_log_keys(IntPtr config);
        [DllImport("libquiche")]
        internal static extern void quiche_config_enable_early_data(
            IntPtr config);
        [DllImport("libquiche")]
        internal static extern int quiche_config_set_application_protos(
            IntPtr config,
            [MarshalAs(UnmanagedType.SafeArray)] byte[] protos,
            ulong/*size_t*/ protos_len);
        [DllImport("libquiche")]
        internal static extern void quiche_config_set_idle_timeout(
            IntPtr config, ulong v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_set_max_packet_size(
            IntPtr config, ulong v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_set_initial_max_data(
            IntPtr config, ulong v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_set_initial_max_stream_data_bidi_local(
            IntPtr config, ulong v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_set_initial_max_stream_data_bidi_remote(
            IntPtr config, ulong v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_set_initial_max_stream_data_uni(
            IntPtr config, ulong v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_set_initial_max_streams_bidi(
            IntPtr config, ulong v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_set_initial_max_streams_uni(
            IntPtr config, ulong v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_set_ack_delay_exponent(
            IntPtr config, ulong v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_set_max_ack_delay(
            IntPtr config, ulong v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_set_disable_active_migration(
            IntPtr config, bool v);
        [DllImport("libquiche")]
        internal static extern void quiche_config_free(IntPtr config);

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
            [MarshalAs(UnmanagedType.SafeArray)] byte[] buf,
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

    private const int MAX_DATAGRAM_SIZE = 1350;
    private const int LOCAL_CONN_ID_LEN = 16;

    private IntPtr config;
    private IntPtr conn = IntPtr.Zero;

    private UdpClient socket;

    // Track whether Dispose has been called.
    private bool _disposed = false;

    private IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

    private byte[] buff = new byte[buff_len];
    private const int buff_len = MAX_DATAGRAM_SIZE;

    public Quiche()
    {
        socket = new UdpClient();
        config = NativeMethods.quiche_config_new(0xbabababa);
        byte[] protos = Encoding.ASCII.GetBytes("\x05hq-24\x05hq-23\x08http/0.9");
        NativeMethods.quiche_config_set_application_protos(
            config, protos, (ulong)protos.Length);
        NativeMethods.quiche_config_set_idle_timeout(config, 5000);
        NativeMethods.quiche_config_set_max_packet_size(config, MAX_DATAGRAM_SIZE);
        NativeMethods.quiche_config_set_initial_max_data(config, 10000000);
        NativeMethods.quiche_config_set_initial_max_stream_data_bidi_local(config, 1000000);
        NativeMethods.quiche_config_set_initial_max_stream_data_uni(config, 1000000);
        NativeMethods.quiche_config_set_initial_max_streams_bidi(config, 100);
        NativeMethods.quiche_config_set_initial_max_streams_uni(config, 100);
        NativeMethods.quiche_config_set_disable_active_migration(config, true);
        NativeMethods.quiche_config_verify_peer(config, false);
    }

    public void Connect(string url)
    {
        var uri = new Uri(url);
        var host = uri.Host;
        var port = uri.Port;
        socket.Connect($"{host}", port);

        var scid = new byte[LOCAL_CONN_ID_LEN];
        new System.Random().NextBytes(scid);

        // Create a QUIC connection and initiate handshake.
        conn = NativeMethods.quiche_connect(
            uri.Authority, scid, (ulong)scid.Length, config);

        Debug.Log(
            $"connecting to {uri.Authority} from {socket.Client.LocalEndPoint} with scid {string.Join(",",scid)}");
        // initial send
        int write = (int)NativeMethods.quiche_conn_send(
            conn, buff, (ulong)buff_len);
        socket.Send(buff, write);
        Debug.Log($"written {write}");
    }

    private IAsyncResult receiveResult = null;
    private void Receive()
    {
        if(receiveResult != null)
        {
            if(receiveResult.IsCompleted)
            {
                receiveResult = null;
                return;
            }
            return;
        }
        receiveResult = socket.BeginReceive((res) => {
            var recvBytes = socket.EndReceive(res, ref RemoteIpEndPoint);
            var read = NativeMethods.quiche_conn_recv(
                conn, recvBytes, (ulong)recvBytes.Length);
            if(read == -1)
            {
                Debug.Log("done reading");
                return;
            }
            if(read < 0)
            {
                Debug.LogError($"recv failed {read}");
                throw new Exception();
            }
        }, null);
    }

    private void Send()
    {
        int write = (int)NativeMethods.quiche_conn_send(
            conn, buff, (ulong)buff_len);
        if(write == -1)
        {
            write = buff_len;
            Debug.Log("done writing");
            return;
        }
        else if(write < 0)
        {
            Debug.LogError($"send failed {write}");
            throw new Exception();
        }
        socket.Send(buff, write);
    }

    private bool IsClosed
    {
        get {return NativeMethods.quiche_conn_is_closed(conn);}
    }

    private bool IsEstablished
    {
        get {return NativeMethods.quiche_conn_is_established(conn);}
    }

    public void Poll()
    {
        Receive();
        if(IsClosed)
        {
            Debug.Log("connection closed");
            return;
        }
        if(IsEstablished)
        {
            // TODO
            return;
        }
        Send();
        if(IsClosed)
        {
            Debug.Log("connection closed");
            return;
        }
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
            if (disposing)
            {
                socket.Dispose();
            }

            if(conn != IntPtr.Zero)
            {
                NativeMethods.quiche_conn_free(conn);
                conn = IntPtr.Zero;
            }
            if(config != IntPtr.Zero)
            {
                NativeMethods.quiche_config_free(config);
                config = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    ~Quiche()
    {
        Dispose(false);
    }
}
