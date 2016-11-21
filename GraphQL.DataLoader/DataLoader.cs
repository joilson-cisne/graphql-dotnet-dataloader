using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.DataLoader
{
    public interface IDataLoader
    {
        Task<object> LoadAsync(object key);
    }

    public interface IDataLoader<in TKey, TValue>
    {
        Task<TValue> LoadAsync(TKey key);
    }

    public class DataLoader<TKey, TValue> : IDataLoader<TKey, TValue>, IDataLoader
    {
        private readonly Func<IEnumerable<TKey>, IDictionary<TKey, TValue>> _fetchFunc;
        private HashSet<TKey> _keys = new HashSet<TKey>();
        private Task<IDictionary<TKey, TValue>> _future;

        /// <summary>
        /// The context this loader is attached to.
        /// </summary>
        public DataLoaderContext Context { get; }

        /// <summary>
        /// The keys to be sent in the next batch.
        /// </summary>
        public IEnumerable<TKey> Keys => _keys.Select(key => key);

        /// <summary>
        /// Initializes a new <see cref="DataLoader"/> attached to the specified context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fetchFunc"></param>
        public DataLoader(DataLoaderContext context, Func<IEnumerable<TKey>, IDictionary<TKey, TValue>> fetchFunc)
        {
            Context = context;
            _fetchFunc = fetchFunc;
        }

        /// <summary>
        /// Initializes a new <see cref="DataLoader"/> using the default ambient context.
        /// </summary>
        /// <param name="fetchFunc"></param>
        public DataLoader(Func<IEnumerable<TKey>, IDictionary<TKey, TValue>> fetchFunc) : this(DataLoaderContext.Current, fetchFunc)
        {
        }

        /// <summary>
        /// Calls the configured fetch function, passing it the batch of keys.
        /// </summary>
        private IDictionary<TKey, TValue> Fetch() => _fetchFunc(Interlocked.Exchange(ref _keys, new HashSet<TKey>()));

        /// <summary>
        /// Retrieves a <typeparamref name="TValue"/> for the given <typeparamref name="TKey"/>
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<TValue> LoadAsync(TKey key)
        {
            if (Context == null)
                throw new InvalidOperationException($"No DataLoaderContext is set");

            if (_keys.Count == 0)
                _future = Context.Register(Fetch);

            _keys.Add(key);

            var batchResult = await _future.ConfigureAwait(false);

            return batchResult[key];
        }

        async Task<object> IDataLoader.LoadAsync(object key)
        {
            return await LoadAsync((TKey)key).ConfigureAwait(false);
        }
    }
}