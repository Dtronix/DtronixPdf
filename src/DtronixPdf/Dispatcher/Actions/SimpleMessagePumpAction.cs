using System;
using System.Threading;

namespace DtronixPdf.Dispatcher.Actions
{
    public class SimpleMessagePumpAction : MessagePumpAction
    {
        private readonly Action<CancellationToken> _cancellableAction;
        private readonly Action _action;

        public SimpleMessagePumpAction(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public SimpleMessagePumpAction(Action<CancellationToken> cancellableAction)
        {
            _cancellableAction = cancellableAction;
        }

        protected override void OnExecute(CancellationToken cancellationToken)
        {
            if (_action != null)
            {
                _action.Invoke();
                return;
            }

            _cancellableAction?.Invoke(cancellationToken);
        }
    }
}