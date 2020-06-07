using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Quiche
{
    public class Config : IDisposable
    {
        private static class NativeMethods
        {
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
        }

        private IntPtr config;

        public IntPtr RawConfig { get {return config;} }


        // Track whether Dispose has been called.
        private bool _disposed = false;

        public Config(uint version)
        {
            config = NativeMethods.quiche_config_new(version);
        }

        public void SetApplicationProtos(byte[] protos)
        {
            NativeMethods.quiche_config_set_application_protos(
                config, protos, (ulong)protos.Length);
        }

        public void SetIdleTimeout(ulong timeout)
        {
            NativeMethods.quiche_config_set_idle_timeout(config, timeout);
        }

        public void SetMaxPacketSize(ulong maxPacketSize)
        {
            NativeMethods.quiche_config_set_max_packet_size(config, maxPacketSize);
        }

        public void SetInitialMaxData(ulong maxData)
        {
            NativeMethods.quiche_config_set_initial_max_data(config, maxData);
        }

        public void SetInitialMaxStreamDataBidiLocal(ulong maxData)
        {
            NativeMethods.quiche_config_set_initial_max_stream_data_bidi_local(config, maxData);
        }

        public void SetInitialMaxStreamDataUni(ulong maxData)
        {
            NativeMethods.quiche_config_set_initial_max_stream_data_uni(config, maxData);
        }

        public void SetInitialMaxStreamsBidi(ulong maxStreams)
        {
            NativeMethods.quiche_config_set_initial_max_streams_bidi(config, maxStreams);
        }

        public void SetInitialMaxStreamsUni(ulong maxStreams)
        {
            NativeMethods.quiche_config_set_initial_max_streams_uni(config, maxStreams);
        }

        public void SetDisableActiveMigration(bool v)
        {
            NativeMethods.quiche_config_set_disable_active_migration(config, v);
        }

        public void VerifyPeer(bool v)
        {
            NativeMethods.quiche_config_verify_peer(config, v);
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
                    NativeMethods.quiche_config_free(config);
                    config = IntPtr.Zero;
                }

                _disposed = true;
            }
        }

        ~Config()
        {
            Dispose(false);
        }
    }
}
