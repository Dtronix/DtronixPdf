using System;
using System.Threading.Tasks;

namespace DtronixPdf.Renderer.Dispatcher
{
    public abstract class ThreadDispatcherAction
    {
        internal abstract void SetFailed(Exception e);
        public abstract void Execute();
    }

    public abstract class ThreadMessagePumpAction<TResult> : ThreadDispatcherAction
    {
        private readonly TaskCompletionSource<TResult> _completionSource
            = new TaskCompletionSource<TResult>();

        public Task<TResult> Result => _completionSource.Task;

        internal override void SetFailed(Exception e)
        {
            _completionSource.TrySetException(e);
        }

        public override void Execute()
        {
            _completionSource.TrySetResult(OnExecute());
        }

        protected abstract TResult OnExecute();
    }
}