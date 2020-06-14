namespace Quiche.H3
{
    enum H3Error
    {
        /// There is no error or no work to do
        QUICHE_H3_DONE = -1,

        /// The provided buffer is too short.
        QUICHE_H3_BUFFER_TOO_SHORT = -2,

        /// Peer violated protocol requirements in a way which doesn’t match a
        /// more specific error code, or endpoint declines to use the more
        /// specific error code.
        QUICHE_H3_GENERAL_PROTOCOL_ERROR = -3,

        /// Internal error in the HTTP/3 stack.
        QUICHE_H3_INTERNAL_ERROR = -5,

        /// The client no longer needs the requested data.
        QUICHE_H3_REQUEST_CANCELLED = -7,

        /// The request stream terminated before completing the request.
        QUICHE_H3_REQUEST_INCOMPLETE = -8,

        /// Forward connection failure for CONNECT target.
        QUICHE_H3_CONNECT_ERROR = -9,

        /// Endpoint detected that the peer is exhibiting behavior that causes.
        /// excessive load.
        QUICHE_H3_EXCESSIVE_LOAD = -10,

        /// Operation cannot be served over HTTP/3. Retry over HTTP/1.1.
        QUICHE_H3_VERSION_FALLBACK = -11,

        /// Stream ID or Push ID greater that current maximum was
        /// used incorrectly, such as exceeding a limit, reducing a limit,
        /// or being reused.
        QUICHE_H3_ID_ERROR = -13,

        /// The endpoint detected that its peer created a stream that it will not
        /// accept.
        QUICHE_H3_STREAM_CREATION_ERROR = -15,

        /// A required critical stream was closed.
        QUICHE_H3_CLOSED_CRITICAL_STREAM = -17,

        /// Inform client that remainder of request is not needed. Used in
        /// STOP_SENDING only.
        QUICHE_H3_EARLY_RESPONSE = -19,

        /// No SETTINGS frame at beginning of control stream.
        QUICHE_H3_MISSING_SETTINGS = -20,

        /// A frame was received which is not permitted in the current state.
        QUICHE_H3_FRAME_UNEXPECTED = -21,

        /// Server rejected request without performing any application processing.
        QUICHE_H3_REQUEST_REJECTED = -22,

        /// An endpoint detected an error in the payload of a SETTINGS frame:
        /// a duplicate setting was detected, a client-only setting was sent by a
        /// server, or a server-only setting by a client.
        QUICHE_H3_SETTINGS_ERROR = -23,

        /// Frame violated layout or size rules.
        QUICHE_H3_FRAME_ERROR = -24,

        /// QPACK Header block decompression failure.
        QUICHE_H3_QPACK_DECOMPRESSION_FAILED = -25,

        /// QPACK encoder stream error.
        QUICHE_H3_QPACK_ENCODER_STREAM_ERROR = -26,

        /// QPACK decoder stream error.
        QUICHE_H3_QPACK_DECODER_STREAM_ERROR = -27,

        /// Error originated from the transport layer.
        QUICHE_H3_TRANSPORT_ERROR = -28,
    }
}
