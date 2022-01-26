using System;

namespace DtronixPdf.Dispatcher
{
    public abstract class ThreadDispatcherVoidAction : ThreadDispatcherAction
    {
        internal override void SetFailed(Exception e)
        {
        }
    }
}