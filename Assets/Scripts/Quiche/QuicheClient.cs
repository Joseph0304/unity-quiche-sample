using System;
using System.Runtime.InteropServices;

namespace Quiche
{
    public class QuicheClient : IDisposable
    {
        // The minimum length of Initial packets sent by a client.
        public const int QUICHE_MIN_CLIENT_INITIAL_LEN = 1200;

        public const int MAX_DATAGRAM_SIZE = 1350;
        public const int LOCAL_CONN_ID_LEN = 16;

        private QuicheConfig Config { get; set; }

        private byte[] scid = Array.Empty<byte>();

        // Track whether Dispose has been called.
        private bool _disposed = false;

        public QuicheClient(QuicheConfig config)
        {
            Config = config;
        }

        public QuicheConnection Connect(string serverName)
        {
            scid = new byte[LOCAL_CONN_ID_LEN];
            new System.Random().NextBytes(scid);

            // Create a QUIC connection and initiate handshake.
            return QuicheConnection.Connect(
                serverName, scid, Config);
        }

        public string HexDump
        {
            get {return string.Join(",", scid);}
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
                // dispose managed resource
                if (disposing)
                {
                    if(Config != null)
                    {
                        Config.Dispose();
                        Config = null;
                    }
                }

                _disposed = true;
            }
        }

        ~QuicheClient()
        {
            Dispose(false);
        }
    }
}
