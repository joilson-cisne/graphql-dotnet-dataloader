using System;
using System.Runtime.CompilerServices;
using GraphQL.Builders;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQL.DataLoader
{
    public static class DataLoaderExtensions
    {
        public static DataLoaderBuilder<TSource, TReturn> Batch<TSource, TReturn>(this FieldBuilder<TSource, TReturn> fieldBuilder, Func<TSource, int> identityFunc)
        {
            return new DataLoaderBuilder<TSource, TReturn>(fieldBuilder.FieldType, identityFunc);
        }

        private static readonly ConditionalWeakTable<Field, object> Loaders = new ConditionalWeakTable<Field, object>();

        public static IDataLoader<TReturn> GetBatchLoader<TSource, TReturn>(this ResolveFieldContext<TSource> source,
            FetchDelegate<TReturn> fetch)
        {
            return (IDataLoader<TReturn>) Loaders.GetValue(source.FieldAst, field => new DataLoader<TReturn>(fetch));
        }
    }
}