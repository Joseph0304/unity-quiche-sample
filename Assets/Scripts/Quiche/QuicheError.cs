namespace Quiche
{
    enum QuicheError
    {
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
    }
}
