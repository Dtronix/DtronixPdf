using System;

namespace DtronixPdf.Renderer.Dispatcher
{
    public class ActionThreadDispatcherAction : ThreadMessagePumpAction<bool>
    {
        private readonly Action _action;

        public ActionThreadDispatcherAction(Action action)
        {
            _action = action;
        }

        protected override bool OnExecute()
        {
            _action();
            return true;
        }
    }
}