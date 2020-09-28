using System;

namespace DtronixPdf.Renderer.Dispatcher
{
    public abstract class ThreadDispatcherVoidAction : ThreadDispatcherAction
    {
        internal override void SetFailed(Exception e)
        {
        }
    }
}