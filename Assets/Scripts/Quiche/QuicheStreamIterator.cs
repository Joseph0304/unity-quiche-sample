using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Quiche
{
    public class QuicheStreamIterator : IEnumerable, IDisposable
    {
        private static class NativeMethods
        {
            [DllImport("libquiche")]
            internal static extern bool quiche_stream_iter_next(
                IntPtr iter,
                ref ulong /* *mut u64 */ stream_id);

            [DllImport("libquiche")]
            internal static extern void quiche_stream_iter_free(IntPtr iter);
        }

        private IntPtr Iterator { get; set; }

        private bool _disposed = false;

        public QuicheStreamIterator(IntPtr iter)
        {
            Iterator = iter;
        }

        public IEnumerator GetEnumerator()
        {
            ulong streamId = 0;
            while(NativeMethods.quiche_stream_iter_next(Iterator, ref streamId))
            {
                yield return streamId;
            }
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
                if(Iterator != IntPtr.Zero)
                {
                    NativeMethods.quiche_stream_iter_free(Iterator);
                    Iterator = IntPtr.Zero;
                }
                _disposed = true;
            }
        }

        ~QuicheStreamIterator()
        {
            Dispose(false);
        }
    }
}
