using System;
using System.Threading;

namespace DtronixPdf.Dispatcher.Actions
{
    public abstract class MessagePumpActionBase
    {
        internal CancellationToken CancellationToken;
        internal abstract void SetFailed(Exception e);
        internal abstract void SetCanceled();

        internal void ExecuteCore()
        {
            Execute(CancellationToken);
        }
        protected abstract void Execute(CancellationToken cancellationToken);
    }
}