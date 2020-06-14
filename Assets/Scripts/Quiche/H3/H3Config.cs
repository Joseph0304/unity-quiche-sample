using System;
using System.Runtime.InteropServices;

namespace Quiche.H3
{
    public class H3Config : IDisposable
    {
        private static class NativeMethods
        {
            /* h3 config */
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_h3_config_new();
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_h3_config_set_max_header_list_size(
                IntPtr config, ulong v);
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_h3_config_set_qpack_max_table_capacity(
                IntPtr config, ulong v);
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_h3_config_set_qpack_blocked_streams(
                IntPtr config, ulong v);
            [DllImport("libquiche")]
            internal static extern IntPtr quiche_h3_config_free(IntPtr config);
        }

        public IntPtr Config { get { return config; } }
        private IntPtr config;

        // Track whether Dispose has been called.
        private bool _disposed = false;

        public H3Config()
        {
            config = NativeMethods.quiche_h3_config_new();
        }

        public void SetMaxHeaderListSize(ulong v)
        {
            NativeMethods.quiche_h3_config_set_max_header_list_size(config, v);
        }

        public void SetQpackMaxTableCapacity(ulong v)
        {
            NativeMethods.quiche_h3_config_set_qpack_max_table_capacity(config, v);
        }

        public void SetQpackBlockedStream(ulong v)
        {
            NativeMethods.quiche_h3_config_set_qpack_blocked_streams(config, v);
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
                if(config != IntPtr.Zero)
                {
                    NativeMethods.quiche_h3_config_free(config);
                    config = IntPtr.Zero;
                }

                _disposed = true;
            }
        }

        ~H3Config()
        {
            Dispose(false);
        }
    }
}
