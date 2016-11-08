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
    public class DataLoaderResolver<TParent,TChild> : IFieldResolver<Task<IEnumerable<TChild>>>
    {
        private readonly Func<TParent,int> _keySelector;
        private readonly IDataLoader<TChild> _loader;

        public DataLoaderResolver(IDataLoader<TChild> loader)
        {
            _loader = loader;
        }

        public DataLoaderResolver(Func<TParent,int> keySelector, FetchDelegate<TChild> fetch)
        {
            _keySelector = keySelector;
            _loader = new DataLoader<TChild>(fetch);
        }

        public Task<IEnumerable<TChild>> Resolve(ResolveFieldContext<TParent> context)
        {
            var key = _keySelector(context.Source);
            return _loader.LoadAsync(key);
        }

        public Task<IEnumerable<TChild>> Resolve(ResolveFieldContext context)
        {
            var typedContext = context as ResolveFieldContext<TParent>;
            return Resolve(typedContext ?? new ResolveFieldContext<TParent>(context));
        }

        object IFieldResolver.Resolve(ResolveFieldContext context)
        {
            return Resolve(context);
        }
    }
}
