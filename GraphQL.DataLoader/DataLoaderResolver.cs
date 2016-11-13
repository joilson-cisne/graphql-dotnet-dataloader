using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace GraphQL.DataLoader
{
    /// <summary>
    /// Collects each item into a batch to be fetched or queried in one call.d.
    /// </summary>
    public class DataLoaderResolver<TParent, TChild> : IFieldResolver<Task<IEnumerable<TChild>>>
    {
        private readonly Func<TParent, int> _keySelector;
        private readonly IDataLoader<TChild> _loader;

        public DataLoaderResolver(Func<TParent, int> keySelector, IDataLoader<TChild> loader)
        {
            _keySelector = keySelector;
            _loader = loader;
        }

        public DataLoaderResolver(Func<TParent, int> keySelector, FetchDelegate<TChild> fetch)
            : this(keySelector, new DataLoader<TChild>(fetch))
        {
        }

        public Task<IEnumerable<TChild>> Resolve(ResolveFieldContext context)
        {
            var source = (TParent)context.Source;
            var key = _keySelector(source);
            return _loader.LoadAsync(key);
        }

        object IFieldResolver.Resolve(ResolveFieldContext context)
        {
            return Resolve(context);
        }
    }
}
