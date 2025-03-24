using System;
using System.Threading;

namespace DtronixPdf
{
    public class PdfActionSynchronizer
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private const int Timeout = 60_000;

        public void SyncExec(Action action)
        {
            bool acquired = false;
            try
            {
                if (!_semaphore.Wait(Timeout))
                    throw new TimeoutException("Timed out acquiring exclusive lock...");
                acquired = true;
                action();
                _semaphore.Release();
            }
            catch
            {
                if(acquired)
                    _semaphore.Release();
                throw;
            }
        }

        public T SyncExec<T>(Func<T> function)
        {
            bool acquired = false;
            try
            {
                if (!_semaphore.Wait(Timeout))
                    throw new TimeoutException("Timed out acquiring exclusive lock...");
                acquired = true;
                var result = function();
                _semaphore.Release();
                return result;
            }
            catch
            {
                if (acquired)
                    _semaphore.Release();
                throw;
            }
        }
    }
}
