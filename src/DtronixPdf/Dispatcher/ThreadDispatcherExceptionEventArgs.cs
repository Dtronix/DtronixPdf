using System;

namespace DtronixPdf.Renderer.Dispatcher
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