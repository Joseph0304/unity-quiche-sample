using System;
using System.Runtime.InteropServices;

namespace Quiche
{
    public enum Shutdown {
        QUICHE_SHUTDOWN_READ = 0,
        QUICHE_SHUTDOWN_WRITE = 1,
    };
    public class QuicheConnection : IDisposable
    {
        private static class NativeMethods
        {
            /* accept */
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_accept(
                byte[] scid,
                ulong /* size_t */ scid_len,
                byte[] odcid,
                ulong /* size_t */ odcid_len,
                IntPtr config);

            /* connect */
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_connect(
                string server_name,
                byte[] scid,
                ulong /* size_t */ scid_len,
                IntPtr config);

            /* connection */
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_conn_new_with_tls(
                byte[] scid,
                ulong /* size_t */ scid_len,
                byte[] odcid,
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
                byte[] buf,
                ulong /* size_t */ buf_len,
                ref bool /* &mut bool */ fin);

            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_conn_stream_send(
                IntPtr conn,
                ulong stream_id,
                byte[] buf,
                ulong /* size_t */ buf_len,
                bool fin);

            [DllImport("libquiche")]
            internal static extern int quiche_conn_stream_shutdown(
                IntPtr conn,
                ulong stream_id,
                int /* Shutdown */ direction,
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
                byte[] reason,
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
                byte[] _out,
                ulong /* size_t */ out_len);

            [DllImport("libquiche")]
            internal static extern bool quiche_conn_is_established(IntPtr conn);
            [DllImport("libquiche")]
            internal static extern bool quiche_conn_is_in_early_data(IntPtr conn);
            [DllImport("libquiche")]
            internal static extern bool quiche_conn_is_closed(IntPtr conn);

            [DllImport("libquiche")]
            internal static extern void quiche_conn_stats(
                IntPtr conn,
                IntPtr /* &mut Stats */ _out);

            [DllImport("libquiche")]
            internal static extern void quiche_conn_free(IntPtr conn);
        }

        public static QuicheConnection Accept(
            byte[] scid, byte[] odcid, QuicheConfig config)
        {
            var conn = NativeMethods.quiche_accept(
                scid, (ulong)scid.Length, odcid, (ulong)odcid.Length,config.Config);
            return new QuicheConnection(conn);
        }

        public static QuicheConnection Connect(
            string serverName, byte[] scid, QuicheConfig config)
        {
            var conn = NativeMethods.quiche_connect(
                serverName, scid, (ulong)scid.Length, config.Config);
            return new QuicheConnection(conn);
        }

        public static QuicheConnection WithTls(
            byte[] scid,
            byte[] odcid,
            QuicheConfig config,
            byte[] ssl,
            bool isServer)
        {
            var ptr = Marshal.AllocCoTaskMem(ssl.Length);
            Marshal.Copy(ssl, 0, ptr, ssl.Length);
            var conn = NativeMethods.quiche_conn_new_with_tls(
                scid, (ulong)scid.Length,
                odcid, (ulong)odcid.Length,
                config.Config,
                ptr,
                isServer);
            Marshal.FreeCoTaskMem(ptr);
            return new QuicheConnection(conn);
        }

        // The maximum length of a connection ID.
        public const int QUICHE_MAX_CONN_ID_LEN = 20;

        public ulong TimeoutAsNanos
        {
            get { return NativeMethods.quiche_conn_timeout_as_nanos(Connection); }
        }

        public ulong TimeoutAsMillis
        {
            get { return NativeMethods.quiche_conn_timeout_as_millis(Connection); }
        }

        public bool IsEstablished
        {
            get { return NativeMethods.quiche_conn_is_established(Connection); }
        }

        public bool IsInEarlyData
        {
            get { return NativeMethods.quiche_conn_is_in_early_data(Connection); }
        }

        public bool IsClosed
        {
            get { return NativeMethods.quiche_conn_is_closed(Connection); }
        }

        private IntPtr Connection { get; set; }
        private IntPtr ReadableIter { get; set; }
        private IntPtr WritableIter { get; set; }
        private bool _disposed = false;

        private QuicheConnection(IntPtr connection)
        {
            Connection = connection;
            ReadableIter = IntPtr.Zero;
            WritableIter = IntPtr.Zero;
        }

        public int Receive(byte[] buf)
        {
            return (int)NativeMethods.quiche_conn_recv(Connection, buf, (ulong)buf.Length);
        }

        public int Send(byte[] buf)
        {
            return (int)NativeMethods.quiche_conn_send(Connection, buf, (ulong)buf.Length);
        }

        public int StreamReceive(ulong streamId, byte[] _out, ref bool fin)
        {
            return (int)NativeMethods.quiche_conn_stream_recv(
                Connection, streamId, _out, (ulong)_out.Length, ref fin);
        }

        public int StreamSend(ulong streamId, byte[] buf, bool fin)
        {
            return (int)NativeMethods.quiche_conn_stream_send(
                Connection, streamId, buf, (ulong)buf.Length, fin);
        }

        public int StreamShutdown(ulong streamId, Shutdown direction, ulong err)
        {
            return NativeMethods.quiche_conn_stream_shutdown(
                Connection, streamId, (int)direction, err);
        }

        public long StreamCapacity(ulong streamId)
        {
            return NativeMethods.quiche_conn_stream_capacity(
                Connection, streamId);
        }

        public bool StreamFinished(ulong streamId)
        {
            return NativeMethods.quiche_conn_stream_finished(
                Connection, streamId);
        }

        public QuicheStreamIterator Readable()
        {
            return new QuicheStreamIterator(
                NativeMethods.quiche_conn_readable(Connection));
        }

        public QuicheStreamIterator Writable()
        {
            return new QuicheStreamIterator(
                NativeMethods.quiche_conn_writable(Connection));
        }

        public int StreamInitApplicationData(ulong streamId, byte[] data)
        {
            var ptr = Marshal.AllocCoTaskMem(data.Length);
            Marshal.Copy(data, 0, ptr, data.Length);
            var err = NativeMethods.quiche_conn_stream_init_application_data(
               Connection, streamId, ptr);
            Marshal.FreeCoTaskMem(ptr);
            return err;
        }

        public IntPtr StreamApplicationData(ulong streamId)
        {
            return NativeMethods.quiche_conn_stream_application_data(
                Connection, streamId);
        }

        public int Close(bool app, ulong err, byte[] reason)
        {
            return NativeMethods.quiche_conn_close(
                Connection, app, err, reason, (ulong)reason.Length);
        }

        public void OnTimeout()
        {
            NativeMethods.quiche_conn_on_timeout(Connection);
        }

        public void ApplicationProto(byte[] _out)
        {
            NativeMethods.quiche_conn_application_proto(
                Connection, _out, (ulong)_out.Length);
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
                if(Connection != IntPtr.Zero)
                {
                    NativeMethods.quiche_conn_free(Connection);
                    Connection = IntPtr.Zero;
                }
                _disposed = true;
            }
        }

        ~QuicheConnection()
        {
            Dispose(false);
        }
    }
}
