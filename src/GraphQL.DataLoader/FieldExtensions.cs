using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GraphQL.Builders;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQL.DataLoader
{
    public static class FieldExtensions
    {
        public static FieldBuilder<TSource, IEnumerable<TReturn>> Resolve<TSource, TReturn>(this FieldBuilder<TSource, object> fieldBuilder, Func<TSource, int> identityFunc, FetchDelegate<TReturn> fetchFunc)
        {
            return fieldBuilder
                .Returns<IEnumerable<TReturn>>()
                .Resolve(new DataLoaderResolver<TSource, TReturn>(identityFunc, fetchFunc));
        }

        private static readonly ConditionalWeakTable<Field, object> Loaders = new ConditionalWeakTable<Field, object>();
        public static IDataLoader<TReturn> GetBatchLoader<TSource, TReturn>(this ResolveFieldContext<TSource> source,
            FetchDelegate<TReturn> fetch)
        {
            return (IDataLoader<TReturn>) Loaders.GetValue(source.FieldAst, field => new DataLoader<TReturn>(fetch));
        }
    }
}