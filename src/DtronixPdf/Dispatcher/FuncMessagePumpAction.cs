using System;

namespace DtronixPdf.Dispatcher
{
    public class FuncMessagePumpAction<TResult> : ThreadMessagePumpAction<TResult>
    {
        private readonly Func<TResult> _func;

        public FuncMessagePumpAction(Func<TResult> func)
        {
            _func = func;
        }

        protected override TResult OnExecute()
        {
            return _func();
        }
    }
}