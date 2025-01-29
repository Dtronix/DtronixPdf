using System;
using System.Threading;

namespace DtronixPdf
{
    public class PdfActionSynchronizer
    {
        public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private const int Timeout = 60_000;

        public void SyncExec(Action action)
        {
            bool acquired = false;
            try
            {
                Semaphore.Wait(Timeout);
                acquired = true;
                action();
                Semaphore.Release();
            }
            catch
            {
                if(acquired)
                    Semaphore.Release();
                throw;
            }
        }

        public T SyncExec<T>(Func<T> function)
        {
            bool acquired = false;
            try
            {
                Semaphore.Wait(Timeout);
                acquired = true;
                var result = function();
                Semaphore.Release();
                return result;
            }
            catch
            {
                if (acquired)
                    Semaphore.Release();
                throw;
            }
        }
    }
}
