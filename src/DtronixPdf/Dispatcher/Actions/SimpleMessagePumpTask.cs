using System;
using System.Threading;
using System.Threading.Tasks;

namespace DtronixPdf.Dispatcher.Actions
{
    public class SimpleMessagePumpTask : MessagePumpAction
    {
        private readonly Func<CancellationToken, Task> _cancellableAction;
        private readonly Func<Task> _action;

        public SimpleMessagePumpTask(Func<Task> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public SimpleMessagePumpTask(Func<CancellationToken, Task> cancellableAction)
        {
            _cancellableAction = cancellableAction;
        }

        protected override void OnExecute(CancellationToken cancellationToken)
        {
            if (_action != null)
            {
                _action.Invoke().Wait(cancellationToken);
                return;
            }

            _cancellableAction?.Invoke(cancellationToken).Wait(cancellationToken);
        }
    }
}