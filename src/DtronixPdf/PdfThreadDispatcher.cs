using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DtronixCommon.Threading.Dispatcher;

namespace DtronixPdf
{
    public class PdfThreadDispatcher : ThreadDispatcher
    {
        public readonly SemaphoreSlim Semaphore;

        public PdfThreadDispatcher(int threadCount) : this(new ThreadDispatcherConfiguration()
        {
            ThreadCount = threadCount
        })
        {

        }

        public PdfThreadDispatcher(ThreadDispatcherConfiguration configs) 
            : base(configs)
        {
            Semaphore = new SemaphoreSlim(configs.ThreadCount);
            this.DispatcherExecutionWrapper = SyncExec;
        }

        public void SyncExec(Action action)
        {
            try
            {
                Semaphore.Wait();
                action();
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public T SyncExec<T>(Func<T> function)
        {
            try
            {
                Semaphore.Wait();
                return function();
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}
