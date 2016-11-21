using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphQL.DataLoader
{
    public class DataLoaderContext : IDisposable
    {
        /// <summary>
        /// Context representing the current batching operation.
        /// </summary>
        public static DataLoaderContext Current { get; private set; }

        /// <summary>
        /// Runs code within a new <see cref="DataLoaderContext"/> before executing each batch load.
        /// </summary>
        public static T Run<T>(Func<DataLoaderContext, T> func)
        {
            if (Current != null)
                throw new InvalidOperationException($"An active {nameof(DataLoaderContext)} already exists");

            var ctx = new DataLoaderContext();
            Current = ctx;

            try
            {
                var result = func(ctx);
                ctx.Flush();
                return result;
            }
            finally
            {
                ctx.Dispose();
                Current = null;
            }
        }

        /// <summary>
        /// Runs code within a new <see cref="DataLoaderContext"/> before executing each batch load.
        /// </summary>
        public static void Run(Action<DataLoaderContext> action)
        {
            Run<object>(ctx =>
            {
                action(ctx);
                return null;
            });
        }

        /// <summary>
        /// Runs code within a new <see cref="DataLoaderContext"/> before executing each batch load.
        /// </summary>
        public static T Run<T>(Func<T> func)
        {
            return Run(ctx => func());
        }

        /// <summary>
        /// Runs code within a new <see cref="DataLoaderContext"/> before executing each batch load.
        /// </summary>
        public static void Run(Action action)
        {
            Run(ctx => action());
        }

        private Queue<Task> _fetchQueue;

        /// <summary>
        /// Creates a new DataLoaderContext.
        /// </summary>
        /// <remarks>
        /// Provides a root to which <see cref="DataLoader"/> instances may register themselves
        ///
        /// defines the boundaries for the batch and fetch operation caller's responsibility to specify when to execute the context's fetch queue.
        /// </remarks>
        public DataLoaderContext()
        {
            _fetchQueue = new Queue<Task>();
        }

        /// <summary>
        /// Executes each registered callback sequentially until there are no more to process.
        /// </summary>
        public void Flush()
        {
            while (_fetchQueue.Count > 0)
            {
                _fetchQueue.Dequeue().RunSynchronously();
            }
        }

        /// <summary>
        /// Registers the specified batch fetch callback for later execution.
        /// </summary>
        internal async Task<T> Register<T>(Func<T> fetch)
        {
            if (_fetchQueue == null)
                throw new ObjectDisposedException(nameof(DataLoaderContext));

            var task = new Task<T>(fetch);
            _fetchQueue.Enqueue(task);
            return await task;
        }

        /// <summary>
        /// Disposes of the DataLoaderContext.
        /// </summary>
        public void Dispose()
        {
            _fetchQueue = null;
        }
    }
}