using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DtronixPdf.Dispatcher
{
    public class ThreadDispatcher : IDisposable
    {
        private class StopLoop : ThreadDispatcherVoidAction
        {
            public override void Execute()
            {
            }
        }

        private bool _isDisposed = false;
        public EventHandler<ThreadDispatcherExceptionEventArgs> Exception;

        private Thread _thread;
        protected BlockingCollection<ThreadDispatcherAction> InternalPriorityQueue;
        protected BlockingCollection<ThreadDispatcherAction> HighPriorityQueue;
        protected BlockingCollection<ThreadDispatcherAction> NormalPriorityQueue;
        protected BlockingCollection<ThreadDispatcherAction> LowPriorityQueue;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsRunning => _thread != null;

        private void Pump()
        {
            var queueList = new[]
            {
                InternalPriorityQueue,
                HighPriorityQueue,
                NormalPriorityQueue,
                LowPriorityQueue
            };
            try
            {
                while (true)
                {
                    var queueId = BlockingCollection<ThreadDispatcherAction>.TakeFromAny(queueList, out var action);

                    // Check if this is a command.
                    if (queueId == 0)
                    {
                        if (action is StopLoop)
                            return;

                        continue;
                    }

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
            if (_thread == null)
                throw new InvalidOperationException("Message pump is not running.");

            InternalPriorityQueue.TryAdd(new StopLoop());
            _cancellationTokenSource.Cancel();
            _thread = null;
        }

        public void Start()
        {
            if (_thread != null)
                throw new InvalidOperationException("Message pump already running.");

            _thread = new Thread(Pump);

            InternalPriorityQueue = new BlockingCollection<ThreadDispatcherAction>();
            HighPriorityQueue = new BlockingCollection<ThreadDispatcherAction>();
            NormalPriorityQueue = new BlockingCollection<ThreadDispatcherAction>();
            LowPriorityQueue = new BlockingCollection<ThreadDispatcherAction>();

            _cancellationTokenSource = new CancellationTokenSource();
            _thread.Start();
        }

        public Task<TResult> QueueWithResult<TResult>(ThreadMessagePumpAction<TResult> action)
        {
            return QueueWithResult(action, DispatcherPriority.Normal);
        }

        public Task<TResult> QueueWithResult<TResult>(ThreadMessagePumpAction<TResult> action, DispatcherPriority priority)
        {
            GetQueue(priority).Add(action);
            return action.Result;
        }

        public Task<TResult> QueueWithResult<TResult>(FuncMessagePumpAction<TResult> action)
        {
            return QueueWithResult(action, DispatcherPriority.Normal);
        }

        public Task<TResult> QueueWithResult<TResult>(FuncMessagePumpAction<TResult> action, DispatcherPriority priority)
        {
            GetQueue(priority).Add(action);
            return action.Result;
        }

        public void Queue(Action action)
        {
            Queue(action, DispatcherPriority.Normal);
        }

        public void Queue(Action action, DispatcherPriority priority)
        {
            var actionInstance = new ActionThreadDispatcherAction(action);
            GetQueue(priority).Add(actionInstance);
        }

        public Task<bool> QueueForCompletion(Action action)
        {
            return QueueForCompletion(action, DispatcherPriority.Normal);
        }

        public Task<bool> QueueForCompletion(Action action, DispatcherPriority priority)
        {
            var actionInstance = new ActionThreadDispatcherAction(action);
            GetQueue(priority).Add(actionInstance);
            return actionInstance.Result;
        }

        public Task<TResult> QueueWithResult<TResult>(Func<TResult> func)
        {
            return QueueWithResult(func, DispatcherPriority.Normal);
        }

        public Task<TResult> QueueWithResult<TResult>(Func<TResult> func, DispatcherPriority priority)
        {
            var actionInstance = new FuncMessagePumpAction<TResult>(func);
            GetQueue(priority).Add(actionInstance);
            return actionInstance.Result;
        }


        private BlockingCollection<ThreadDispatcherAction> GetQueue(DispatcherPriority priority)
        {
            return priority switch
            {
                DispatcherPriority.Low => LowPriorityQueue,
                DispatcherPriority.Normal => NormalPriorityQueue,
                DispatcherPriority.High => HighPriorityQueue,
                _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null)
            };
        }

        public virtual void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            Stop();
            InternalPriorityQueue.Dispose();
            HighPriorityQueue.Dispose();
            NormalPriorityQueue.Dispose();
            LowPriorityQueue.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}