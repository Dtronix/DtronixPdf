using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DtronixCommon.Threading.Dispatcher;

namespace DtronixPdf
{
    public class PdfActionSynchronizer
    {
        public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        public void SyncExec(Action action)
        {
            try
            {
                Semaphore.Wait();
                action();
                Semaphore.Release();
            }
            catch
            {
                Semaphore.Release();
                throw;
            }
        }

        public T SyncExec<T>(Func<T> function)
        {
            try
            {
                Semaphore.Wait();
                var result = function();
                Semaphore.Release();
                return result;
            }
            catch
            {
                Semaphore.Release();
                throw;
            }
        }
    }
}
