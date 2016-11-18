using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.DataLoader
{
    public delegate ILookup<int, T> FetchDelegate<T>(IEnumerable<int> keys);

    public interface IDataLoader
    {
        Task<IEnumerable> LoadAsync(int key);
    }

    public interface IDataLoader<T>
    {
        Task<IEnumerable<T>> LoadAsync(int key);
    }

    public class DataLoader<T> : IDataLoader<T>, IDataLoader
    {
        private readonly DataLoaderContext _context;
        private readonly FetchDelegate<T> _fetchFunc;
        private HashSet<int> _keys = new HashSet<int>();
        private Task<ILookup<int, T>> _future;

        /// <summary>
        /// Initialize a new DataLoader using an explicit context.
        /// The context controls the beginning/end of each batch and fetch cycle.
        /// </summary>
        public DataLoader(DataLoaderContext context, FetchDelegate<T> fetchFunc)
        {
            _context = context;
            _fetchFunc = fetchFunc;
        }

        /// <summary>
        /// Initialize a new DataLoader using the implicit (ambient) context.
        /// The ambient context is created by the static Run method and stored
        /// in the Current property.
        /// </summary>
        /// <param name="fetchFunc"></param>
        internal DataLoader(FetchDelegate<T> fetchFunc) : this(DataLoaderContext.Current, fetchFunc)
        {
        }

        public async Task<IEnumerable<T>> LoadAsync(int key)
        {
            if (_context == null)
                throw new InvalidOperationException($"No DataLoaderContext has been set");

            if (_keys.Count == 0)
                _future = _context.Enqueue(Fetch);

            _keys.Add(key);
            var batchResult = await _future.ConfigureAwait(false);
            return batchResult[key];
        }

        async Task<IEnumerable> IDataLoader.LoadAsync(int key)
        {
            return await LoadAsync(key).ConfigureAwait(false);
        }

        private ILookup<int, T> Fetch() => _fetchFunc(Interlocked.Exchange(ref _keys, new HashSet<int>()));
    }
}