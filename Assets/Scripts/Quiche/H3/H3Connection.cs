using System;
using System.Runtime.InteropServices;

namespace Quiche.H3
{
    class H3Connection : IDisposable
    {
        private static class NativeMethods
        {
            /* h3 connection */
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_h3_conn_new_with_transport(
                IntPtr quic_conn, IntPtr h3_config);
            [DllImport("libquiche")]
            internal static extern long quiche_h3_conn_poll(
                IntPtr h3_conn,
                IntPtr quic_conn,
                ref IntPtr ev /* *mut *const h3::Event */);
            [DllImport("libquiche")]
            internal static extern long quiche_h3_send_request(
                IntPtr h3_conn,
                IntPtr quic_conn,
                H3Header[] headers,
                ulong /* size_t */ headers_len,
                bool fin);
            [DllImport("libquiche")]
            internal static extern int quiche_h3_send_response(
                IntPtr h3_conn,
                IntPtr quic_conn,
                ulong stream_id,
                H3Header[] headers,
                ulong /* size_t */ headers_len,
                bool fin);
            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_h3_send_body(
                IntPtr h3_conn,
                IntPtr quic_conn,
                ulong stream_id,
                byte[] body,
                ulong /* size_t */ body_len,
                bool fin);
            [DllImport("libquiche")]
            internal static extern long /* ssize_t */ quiche_h3_recv_body(
                IntPtr h3_conn,
                IntPtr quic_conn,
                ulong stream_id,
                byte[] _out,
                ulong /* size_t */ out_len);
            [DllImport("libquiche")]
            internal static extern void quiche_h3_conn_free(
                IntPtr h3_conn);
        }

        private QuicheConnection quicConnection;
        private IntPtr conn;

        // Track whether Dispose has been called.
        private bool _disposed = false;

        public H3Connection(QuicheConnection quicConn, H3Config config)
        {
            quicConnection = quicConn;
            conn = NativeMethods.quiche_h3_conn_new_with_transport(
                quicConnection.Connection, config.Config);
        }

        public long Poll(ref H3Event h3_event)
        {
            IntPtr ev = IntPtr.Zero;
            var ret = NativeMethods.quiche_h3_conn_poll(
                conn, quicConnection.Connection, ref ev);
            h3_event = new H3Event(ev);
            return ret;
        }

        public long SendRequest(
            H3Header[] headers,
            bool fin)
        {
            return NativeMethods.quiche_h3_send_request(
                conn,
                quicConnection.Connection,
                headers,
                (ulong)headers.Length,
                fin);
        }

        public int SendResponse(
            ulong streamId,
            H3Header[] headers,
            bool fin)
        {
            return NativeMethods.quiche_h3_send_response(
                conn,
                quicConnection.Connection,
                streamId,
                headers,
                (ulong)headers.Length,
                fin);
        }

        public long SendBody(ulong streamId, byte[] body, bool fin)
        {
            return NativeMethods.quiche_h3_send_body(
                conn,
                quicConnection.Connection,
                streamId,
                body,
                (ulong)body.Length,
                fin);
        }

        public long ReceiveBody(ulong streamId, byte[] _out)
        {
            return NativeMethods.quiche_h3_recv_body(
                conn,
                quicConnection.Connection,
                streamId,
                _out,
                (ulong)_out.Length);
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
                if(conn != IntPtr.Zero)
                {
                    NativeMethods.quiche_h3_conn_free(conn);
                    conn = IntPtr.Zero;
                }

                _disposed = true;
            }
        }

        ~H3Connection()
        {
            Dispose(false);
        }
    }
}
