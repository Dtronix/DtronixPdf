using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DtronixPdf.Dispatcher;
using NUnit.Framework;
using SixLabors.ImageSharp;

namespace DtronixPdf.Tests.Dispatcher
{
    public class ThreadDispatcherTests
    {
        [Test]
        public async Task SingleThread_Blocks()
        {
            var dispatcher = new ThreadDispatcher(1);
            dispatcher.Start();
            bool complete = false;
            var tasks = new Task[2];
            tasks[0] = dispatcher.QueueForCompletion(async() =>
            {
                await Task.Delay(100);
                complete = true;
            });

            tasks[1] = dispatcher.QueueForCompletion(() =>
            {
                Assert.IsTrue(complete);
            });
            await Task.WhenAll(tasks);
        }

        [Test]
        public async Task SingleThread_SequentiallyExecutesActions()
        {
            var dispatcher = new ThreadDispatcher(1);
            dispatcher.Start();
            var counter = 0;
            var tasks = new Task[1000];
            for (int i = 0; i < tasks.Length; i++)
            {
                var i1 = i;
                tasks[i] = dispatcher.QueueForCompletion(() =>
                {
                    Assert.AreEqual(i1, counter++, "Executed tasks out of order.");
                });
            }

            await Task.WhenAll(tasks);
        }
        [Test]
        public async Task MultipleThread_ConcurrentlyExecutesActions()
        {
            var dispatcher = new ThreadDispatcher(2);
            dispatcher.Start();
            var counter = 0;
            var tasks = new Task[1000];
            var executedOutOfOrder = false;
            for (int i = 0; i < tasks.Length; i++)
            {
                var i1 = i;
                tasks[i] = dispatcher.QueueForCompletion(() =>
                {
                    if(i1 != counter++)
                        executedOutOfOrder = true;
                });
            }

            await Task.WhenAll(tasks);

            Assert.IsTrue(executedOutOfOrder, "Tasks were not executed concurrently");
        }

        [Test]
        public async Task DispatcherStopsAllThreads()
        {
            var count = 5;
            var dispatcher = new ThreadDispatcher(count);
            dispatcher.Start();
            var threads = dispatcher.Threads;


            foreach (var dispatcherThread in threads)
                Assert.IsTrue(dispatcherThread.ThreadState.HasFlag(ThreadState.Background));

            dispatcher.Stop();

            foreach (var dispatcherThread in threads)
                Assert.IsFalse(dispatcherThread.IsAlive);

        }

    }
}