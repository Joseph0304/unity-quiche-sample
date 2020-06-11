using System;
using System.Runtime.InteropServices;

namespace Quiche
{
    public class QuicheListener : IDisposable
    {
        private QuicheConfig Config { get; set; }

        // Track whether Dispose has been called.
        private bool _disposed = false;

        public QuicheListener(QuicheConfig config)
        {
            Config = config;
        }

        public QuicheConnection Accept(byte[] scid, byte[] odcid)
        {
            return QuicheConnection.Accept(scid, odcid, Config);
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

        ~QuicheListener()
        {
            Dispose(false);
        }
    }
}
