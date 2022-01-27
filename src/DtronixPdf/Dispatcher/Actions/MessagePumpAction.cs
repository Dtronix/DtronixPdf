using System;
using System.Threading;
using System.Threading.Tasks;

namespace DtronixPdf.Dispatcher.Actions
{
    public abstract class MessagePumpAction : MessagePumpActionBase
    {
        private readonly TaskCompletionSource _completionSource = new();

        public Task Result => _completionSource.Task;

        internal override void SetFailed(Exception e)
        {
            _completionSource.TrySetException(e);
        }

        internal override void SetCanceled()
        {
            _completionSource.TrySetCanceled();
        }

        protected override void Execute(CancellationToken cancellationToken)
        {
            OnExecute(cancellationToken);
            _completionSource.TrySetResult();
        }

        protected abstract void OnExecute(CancellationToken cancellationToken);
    }
}