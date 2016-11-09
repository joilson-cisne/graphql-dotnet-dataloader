using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.DataLoader
{
    public class DataLoaderContext : IDisposable
    {
        /// <summary>
        /// Context for the current batch operation.
        /// Pending fetches will be executed upon its disposal.
        /// </summary>
        public static DataLoaderContext Current { get; private set; }

        /// <summary>
        /// Runs code within a new DataLoaderContext.
        /// Loaders will collect keys during the given function's
        /// execution then fire them once it has finished.
        /// </summary>
        public static T Run<T>(Func<T> func)
        {
            if (Current != null)
                throw new InvalidOperationException($"An active {nameof(DataLoaderContext)} already exists");

            T result;
            using (Current = new DataLoaderContext())
            {
                result = func();
                Current.Flush();
            }
            Current = null;
            return result;
        }

        /// <summary>
        /// Pending fetches.
        /// </summary>
        private Queue<Action> _queue;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private DataLoaderContext()
        {
            _queue = new Queue<Action>();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        /// <summary>
        /// Triggers the pending loaders sequentially until there are none left.
        /// Note that code may run when a task completes that causes more
        /// functions to be added to the queue. This is what we want, since it
        /// allows us to set up dependent loaders and resolvers.
        /// </summary>
        private void Flush()
        {
            while (_queue.Count > 0)
            {
                _queue.Dequeue().Invoke();
            }
        }

        /// <summary>
        /// Enqueue a function to be executed later.
        /// </summary>
        public Task<T> Enqueue<T>(Func<T> fetch)
        {
            if (_queue == null)
                throw new ObjectDisposedException(nameof(DataLoaderContext));

            var tcs = new TaskCompletionSource<T>();
            var cancellation = _cancellationToken.Register(tcs.SetCanceled);
            _queue.Enqueue(() =>
            {
                cancellation.Dispose();
                tcs.SetResult(fetch());
            });
            return tcs.Task;
        }

        /// <summary>
        /// Disposes of the DataLoaderContext.
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            _queue = null;
        }
    }
}