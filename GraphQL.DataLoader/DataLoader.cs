using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.DataLoader
{
    public delegate ILookup<int, T> FetchDelegate<T>(IEnumerable<int> keys);

    public interface IDataLoader<T>
    {
        Task<IEnumerable<T>> LoadAsync(int key);
    }

    public class DataLoader<T> : IDataLoader<T>
    {
        private readonly FetchDelegate<T> _fetchFunc;
        private HashSet<int> _keys = new HashSet<int>();
        private Task<ILookup<int, T>> _future;

        public DataLoader(FetchDelegate<T> fetchFunc)
        {
            _fetchFunc = fetchFunc;
        }

        public async Task<IEnumerable<T>> LoadAsync(int key)
        {
            if (DataLoaderContext.Current == null)
                throw new InvalidOperationException($"{nameof(LoadAsync)} must be called within an active DataLoaderContext" +
                                                    $" - use {nameof(DataLoaderContext)}.{nameof(DataLoaderContext.Run)}");

            if (_keys.Count == 0)
                _future = DataLoaderContext.Current.Enqueue(Fetch);

            _keys.Add(key);
            var batchResult = await _future.ConfigureAwait(false);
            return batchResult[key];
        }

        private ILookup<int, T> Fetch() => _fetchFunc(Interlocked.Exchange(ref _keys, new HashSet<int>()));
    }
}