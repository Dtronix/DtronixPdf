using System;
using System.Threading;
using System.Threading.Tasks;

namespace DtronixPdf.Dispatcher.Actions
{
    public abstract class MessagePumpActionResult<TResult> : MessagePumpActionBase
    {
        private readonly TaskCompletionSource<TResult> _completionSource = new();

        public Task<TResult> Result => _completionSource.Task;

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
            _completionSource.TrySetResult(OnExecute(cancellationToken));
        }

        protected abstract TResult OnExecute(CancellationToken cancellationToken);
    }
}