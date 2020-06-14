using System;
using System.Runtime.InteropServices;

namespace Quiche.H3
{
    public delegate void Callback(
        string name,
        ulong /* size_t */ name_len,
        string value,
        ulong /* size_t */ value_len,
        IntPtr argp);

    class H3Event : IDisposable
    {
        private static class NativeMethods
        {
            [DllImport("libquiche")]
            internal static extern uint quiche_h3_event_type(
                IntPtr ev /* &h3::Event */);
            [DllImport("libquiche")]
            internal static extern int quiche_h3_event_for_each_header(
                IntPtr ev /* &h3::Event */,
                Callback cb,
                IntPtr argp);
            [DllImport("libquiche")]
            internal static extern bool quiche_h3_event_headers_has_body(
                IntPtr ev /* &h3::Event */);
            [DllImport("libquiche")]
            internal static extern void quiche_h3_event_free(
                IntPtr ev /* &h3::Event */);
        }

        private IntPtr ev;

        // Track whether Dispose has been called.
        private bool _disposed = false;

        public H3Event(IntPtr ev)
        {
            this.ev = ev;
        }

        public uint EventType
        {
            get { return NativeMethods.quiche_h3_event_type(ev); }
        }

        public int ForEachHeader(Callback cb)
        {
            return NativeMethods.quiche_h3_event_for_each_header(
                ev, cb, IntPtr.Zero);
        }

        public bool HeadersHasBody
        {
            get { return NativeMethods.quiche_h3_event_headers_has_body(ev); }
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
                if(ev != IntPtr.Zero)
                {
                    NativeMethods.quiche_h3_event_free(ev);
                    ev = IntPtr.Zero;
                }

                _disposed = true;
            }
        }

        ~H3Event()
        {
            Dispose(false);
        }
    }
}
