using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DtronixPdf.Renderer.Dispatcher
{
    public class ThreadDispatcher : IDisposable
    {
        private class StopLoop : ThreadDispatcherVoidAction
        {
            public override void Execute()
            {
            }
        }

        public EventHandler<ThreadDispatcherExceptionEventArgs> Exception;

        private Thread _thread;
        protected BlockingCollection<ThreadDispatcherAction> Actions;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsRunning => _thread != null;

        private void Pump()
        {
            try
            {
                foreach (var action in Actions.GetConsumingEnumerable(_cancellationTokenSource.Token))
                {
                    if (action is StopLoop)
                        return;

                    try
                    {
                        action.Execute();
                    }
                    catch (Exception e)
                    {
                        Exception?.Invoke(this, new ThreadDispatcherExceptionEventArgs(e));
                        action.SetFailed(e);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                Exception?.Invoke(this, new ThreadDispatcherExceptionEventArgs(e));
            }

        }

        public void Stop()
        {
            if(_thread == null)
                throw new InvalidOperationException("Message pump is not running.");

            Actions.TryAdd(new StopLoop());
            _cancellationTokenSource.Cancel();
            _thread = null;
        }

        public void Start()
        {
            if(_thread != null)
                throw new InvalidOperationException("Message pump already running.");

            _thread = new Thread(Pump);
            Actions = new BlockingCollection<ThreadDispatcherAction>();
            _cancellationTokenSource = new CancellationTokenSource();
            _thread.Start();
        }

        public Task<TResult> QueueWithResult<TResult>(ThreadMessagePumpAction<TResult> action)
        {
            Actions.Add(action);
            return action.Result;
        }

        public Task<TResult> QueueWithResult<TResult>(FuncMessagePumpAction<TResult> action)
        {
            Actions.Add(action);
            return action.Result;
        }

        
        public void Queue(Action action)
        {
            var actionInstance = new ActionThreadDispatcherAction(action);
            Actions.Add(actionInstance);
        }

        public Task<bool> QueueForCompletion(Action action)
        {
            var actionInstance = new ActionThreadDispatcherAction(action);
            Actions.Add(actionInstance);
            return actionInstance.Result;
        }

        public Task<TResult> QueueWithResult<TResult>(Func<TResult> func)
        {
            var actionInstance = new FuncMessagePumpAction<TResult>(func);
            Actions.Add(actionInstance);
            return actionInstance.Result;
        }

        public virtual void Dispose()
        {
            Stop();
            Actions?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
