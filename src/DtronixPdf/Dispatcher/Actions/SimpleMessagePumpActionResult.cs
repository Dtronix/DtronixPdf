using System;
using System.Threading;

namespace DtronixPdf.Dispatcher.Actions
{
    public class SimpleMessagePumpActionResult<TResult> : MessagePumpActionResult<TResult>
    {
        private readonly Func<CancellationToken, TResult> _cancellableFunction;
        private readonly Func<TResult> _function;

        public SimpleMessagePumpActionResult(Func<TResult> function)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
        }

        public SimpleMessagePumpActionResult(Func<CancellationToken, TResult> cancellableFunction)
        {
            _cancellableFunction = cancellableFunction;
        }
        protected override TResult OnExecute(CancellationToken cancellationToken)
        {
            if (_function != null)
                return _function.Invoke();

            return _cancellableFunction.Invoke(cancellationToken);
        }
    }
}