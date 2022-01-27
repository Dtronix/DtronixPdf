using System;
using System.Threading;

namespace DtronixPdf.Dispatcher.Actions
{
    public abstract class MessagePumpActionVoid : MessagePumpActionBase
    {
        internal override void SetFailed(Exception e)
        {
        }

        protected override void Execute(CancellationToken cancellationToken)
        {
        }

        internal override void SetCanceled()
        {
        }
    }
}