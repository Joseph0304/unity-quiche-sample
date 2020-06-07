using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Quiche
{
    public class QuicheConfig : IDisposable
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
                byte[] protos,
                ulong /*size_t*/ protos_len);
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

        public IntPtr Config { get; private set; }

        // Track whether Dispose has been called.
        private bool _disposed = false;

        public QuicheConfig(uint version)
        {
            Config = NativeMethods.quiche_config_new(version);
        }

        public int LoadCertChainFromPemFile(string path)
        {
            return NativeMethods.quiche_config_load_cert_chain_from_pem_file(
                Config, path);
        }

        public int LoadPrivKeyFromPemFile(string path)
        {
            return NativeMethods.quiche_config_load_priv_key_from_pem_file(
                Config, path);
        }

        public void VerifyPeer(bool v)
        {
            NativeMethods.quiche_config_verify_peer(Config, v);
        }

        public void Grease(bool v)
        {
            NativeMethods.quiche_config_grease(Config, v);
        }

        public void LogKeys()
        {
            NativeMethods.quiche_config_log_keys(Config);
        }

        public void EnableEarlyData()
        {
            NativeMethods.quiche_config_enable_early_data(Config);
        }

        public void SetApplicationProtos(byte[] protos)
        {
            NativeMethods.quiche_config_set_application_protos(
                Config, protos, (ulong)protos.Length);
        }

        public void SetIdleTimeout(ulong timeout)
        {
            NativeMethods.quiche_config_set_idle_timeout(Config, timeout);
        }

        public void SetMaxPacketSize(ulong maxPacketSize)
        {
            NativeMethods.quiche_config_set_max_packet_size(Config, maxPacketSize);
        }

        public void SetInitialMaxData(ulong maxData)
        {
            NativeMethods.quiche_config_set_initial_max_data(Config, maxData);
        }

        public void SetInitialMaxStreamDataBidiLocal(ulong maxData)
        {
            NativeMethods.quiche_config_set_initial_max_stream_data_bidi_local(
                Config, maxData);
        }

        public void SetInitialMaxStreamDataBidiRemote(ulong maxData)
        {
            NativeMethods.quiche_config_set_initial_max_stream_data_bidi_remote(
                Config, maxData);
        }

        public void SetInitialMaxStreamDataUni(ulong maxData)
        {
            NativeMethods.quiche_config_set_initial_max_stream_data_uni(
                Config, maxData);
        }

        public void SetInitialMaxStreamsBidi(ulong maxStreams)
        {
            NativeMethods.quiche_config_set_initial_max_streams_bidi(
                Config, maxStreams);
        }

        public void SetInitialMaxStreamsUni(ulong maxStreams)
        {
            NativeMethods.quiche_config_set_initial_max_streams_uni(
                Config, maxStreams);
        }

        public void SetAckDelayExponent(ulong v)
        {
            NativeMethods.quiche_config_set_ack_delay_exponent(Config, v);
        }

        public void SetMaxAckDelay(ulong v)
        {
            NativeMethods.quiche_config_set_max_ack_delay(Config, v);
        }

        public void SetDisableActiveMigration(bool v)
        {
            NativeMethods.quiche_config_set_disable_active_migration(Config, v);
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
                if(Config != IntPtr.Zero)
                {
                    NativeMethods.quiche_config_free(Config);
                    Config = IntPtr.Zero;
                }

                _disposed = true;
            }
        }

        ~QuicheConfig()
        {
            Dispose(false);
        }
    }
}
