using System;
using GraphQL.Types;

namespace GraphQL.DataLoader
{
    public class DataLoaderBuilder<TSource>
    {
        private readonly FieldType _fieldType;
        private readonly Func<TSource, int> _identityFunc;

        public DataLoaderBuilder(FieldType fieldType, Func<TSource, int> identityFunc)
        {
            _fieldType = fieldType;
            _identityFunc = identityFunc;
        }

        public void Resolve<TReturn>(FetchDelegate<TReturn> fetchFunc)
        {
            _fieldType.Resolver = new DataLoaderResolver<TSource, TReturn>(_identityFunc, fetchFunc);
        }
    }

    public class DataLoaderBuilder<TSource, TReturn> : DataLoaderBuilder<TSource>
    {
        public DataLoaderBuilder(FieldType fieldType, Func<TSource, int> identityFunc)
            : base(fieldType, identityFunc)
        {
        }

        public void Resolve(FetchDelegate<TReturn>  fetchFunc)
        {
            base.Resolve(fetchFunc);
        }
    }
}