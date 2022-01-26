using System;

namespace DtronixPdf.Dispatcher
{
    public class ThreadDispatcherExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public ThreadDispatcherExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}