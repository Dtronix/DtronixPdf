using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DtronixPdf.Dispatcher.Actions;

namespace DtronixPdf.Dispatcher
{
    public class ThreadDispatcher : IDisposable
    {
        private class StopLoop : MessagePumpActionVoid
        {
        }

        private bool _isDisposed = false;
        public EventHandler<ThreadDispatcherExceptionEventArgs> Exception;

        private Thread _thread;
        protected BlockingCollection<MessagePumpActionBase> InternalPriorityQueue;
        protected BlockingCollection<MessagePumpActionBase> HighPriorityQueue;
        protected BlockingCollection<MessagePumpActionBase> NormalPriorityQueue;
        protected BlockingCollection<MessagePumpActionBase> LowPriorityQueue;
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
                    var queueId = BlockingCollection<MessagePumpActionBase>.TakeFromAny(queueList, out var action);

                    // Check if this is a command.
                    if (queueId == 0)
                    {
                        if (action is StopLoop)
                            return;

                        continue;
                    }

                    if (action == null)
                        continue;

                    try
                    {
                        if (action.CancellationToken.IsCancellationRequested)
                        {
                            action.SetCanceled();
                            continue;
                        }

                        action.ExecuteCore();
                    }
                    catch (TaskCanceledException)
                    {
                        action.SetCanceled();
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

            InternalPriorityQueue = new BlockingCollection<MessagePumpActionBase>();
            HighPriorityQueue = new BlockingCollection<MessagePumpActionBase>();
            NormalPriorityQueue = new BlockingCollection<MessagePumpActionBase>();
            LowPriorityQueue = new BlockingCollection<MessagePumpActionBase>();

            _cancellationTokenSource = new CancellationTokenSource();
            _thread.Start();
        }
        public Task QueueForCompletion(
            Action action,
            DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            return QueueForCompletion(
                new SimpleMessagePumpAction(action),
                priority,
                cancellationToken);
        }

        public Task QueueForCompletion(
            Action<CancellationToken> action,
            DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            return QueueForCompletion(
                new SimpleMessagePumpAction(action),
                priority,
                cancellationToken);
        }

        public Task QueueForCompletion(
            MessagePumpAction action,
            DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            action.CancellationToken = cancellationToken;
            GetQueue(priority).Add(action, cancellationToken);
            return action.Result;
        }

        public Task<TResult> QueueWithResult<TResult>(
            Func<TResult> action,
            DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            return QueueWithResult(
                new SimpleMessagePumpActionResult<TResult>(action),
                priority,
                cancellationToken);
        }

        public Task<TResult> QueueWithResult<TResult>(
            Func<CancellationToken, TResult> action,
            DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            return QueueWithResult(
                new SimpleMessagePumpActionResult<TResult>(action),
                priority,
                cancellationToken);
        }

        public Task<TResult> QueueWithResult<TResult>(
            MessagePumpActionResult<TResult> action,
            DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            action.CancellationToken = cancellationToken;
            GetQueue(priority).Add(action, cancellationToken);
            return action.Result;
        }

        private BlockingCollection<MessagePumpActionBase> GetQueue(DispatcherPriority priority)
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