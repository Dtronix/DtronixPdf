using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DtronixPdf.Dispatcher.Actions;

namespace DtronixPdf.Dispatcher
{
    public class ThreadDispatcher : IDisposable
    {
        private readonly int _threadCount;

        private class StopLoop : MessagePumpActionVoid
        {
        }

        private bool _isDisposed = false;
        public EventHandler<ThreadDispatcherExceptionEventArgs> Exception;

        internal Thread[] Threads;
        protected BlockingCollection<MessagePumpActionBase> InternalPriorityQueue;
        protected BlockingCollection<MessagePumpActionBase> HighPriorityQueue;
        protected BlockingCollection<MessagePumpActionBase> NormalPriorityQueue;
        protected BlockingCollection<MessagePumpActionBase> LowPriorityQueue;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsRunning => Threads != null;

        public ThreadDispatcher(int threadCount)
        {
            _threadCount = threadCount;
        }

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

        /// <summary>
        /// Stops the thread dispatcher and joins all the threads.
        /// </summary>
        /// <param name="timeout">Timeout for waiting on each thread.</param>
        /// <returns>True on successful stopping of the dispatcher threads, otherwise false.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool Stop(int timeout = 1000)
        {
            if (Threads == null)
                throw new InvalidOperationException("Message pump is not running.");

            // Send enough StopLoop commands to end all the threads.
            for (int i = 0; i < _threadCount; i++)
                InternalPriorityQueue.TryAdd(new StopLoop());
            

            _cancellationTokenSource.Cancel();
            InternalPriorityQueue.CompleteAdding();
            HighPriorityQueue.CompleteAdding();
            NormalPriorityQueue.CompleteAdding();
            LowPriorityQueue.CompleteAdding();

            var stopSuccessful = true;
            // Join all the threads back to ensure they are complete.
            foreach (var thread in Threads)
            {
                // If the thread join times out, return a failure result to the caller.
                if (!thread.Join(timeout))
                    stopSuccessful = false;
            }
            
            Threads = null;

            return stopSuccessful;
        }

        /// <summary>
        /// Starts the dispatcher and waits for the startup of each thread.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start()
        {
            if (Threads != null)
                throw new InvalidOperationException("Message pump already running.");

            InternalPriorityQueue = new BlockingCollection<MessagePumpActionBase>();
            HighPriorityQueue = new BlockingCollection<MessagePumpActionBase>();
            NormalPriorityQueue = new BlockingCollection<MessagePumpActionBase>();
            LowPriorityQueue = new BlockingCollection<MessagePumpActionBase>();

            _cancellationTokenSource = new CancellationTokenSource();

            Threads = new Thread[_threadCount];
            for (int i = 0; i < _threadCount; i++)
            {
                Threads[i] = new Thread(Pump)
                {
                    IsBackground = true
                };
                Threads[i].Start();
            }
            
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

        public Task QueueForCompletion(
            Func<Task> action,
            DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            var messageTask = new SimpleMessagePumpTask(action);
            GetQueue(priority).Add(messageTask, cancellationToken);
            return messageTask.Result;
        }

        public Task QueueForCompletion(
            Func<CancellationToken, Task> action,
            DispatcherPriority priority = DispatcherPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            var messageTask = new SimpleMessagePumpTask(action);
            GetQueue(priority).Add(messageTask, cancellationToken);
            return messageTask.Result;
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