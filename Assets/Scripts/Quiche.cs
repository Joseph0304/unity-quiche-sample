using System;
using System.Runtime.InteropServices;
using System.Text;

delegate void Callback(string line, IntPtr argp);

public class Quiche
{
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
            IntPtr config, string protos, ulong/*size_t*/ protos_len);
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
            IntPtr /* *const u8 */ scid,
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
    }

    public static string GetVersion()
    {
        // quiche_versionが返すのはスタック領域のメモリなので、直接stringに変換できない
        // そのため、一度IntPtrを介してからstringに変換する
        var version = NativeMethods.quiche_version();
        return Marshal.PtrToStringAnsi(version);
    }

    private IntPtr config;

    public Quiche()
    {
        config = NativeMethods.quiche_config_new(0xbababa);
    }
}
