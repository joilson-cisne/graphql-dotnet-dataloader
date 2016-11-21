using System;
using System.Collections.Generic;
using GraphQL.Builders;

namespace GraphQL.DataLoader
{
    public static class DataLoaderExtensions
    {
        public static FieldBuilder<TSource, IEnumerable<TValue>> Resolve<TSource, TKey, TValue>(this FieldBuilder<TSource, object> fieldBuilder, Func<TSource, TKey> identityFunc, Func<IEnumerable<TKey>, IDictionary<TKey, TValue>> fetchFunc)
        {
            return fieldBuilder
                .Returns<IEnumerable<TValue>>()
                .Resolve(new DataLoaderResolver<TSource, TKey, TValue>(identityFunc, fetchFunc));
        }
    }
}