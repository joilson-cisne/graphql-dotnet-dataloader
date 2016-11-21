using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace GraphQL.DataLoader.Tests
{
    public class DataLoaderContextTests
    {
        [Fact]
        public void DataLoaderContext_Run_SetsContext()
        {
            DataLoaderContext.Current.ShouldBeNull();

            DataLoaderContext.Run(ctx =>
            {
                DataLoaderContext.Current.ShouldNotBeNull();
            });

            DataLoaderContext.Current.ShouldBeNull();
        }

        [Fact]
        public void DataLoaderContext_Run_ThrowsWhenCalledTwice()
        {
            DataLoaderContext.Run(() =>
            {
                Should.Throw<InvalidOperationException>(() =>
                {
                    DataLoaderContext.Run(() => { });
                });
            });
        }

        [Fact]
        public void DataLoaderContext_Flush_CanHandleMultipleLevelsOfNestedFetches()
        {
            string result1;
            string result2;
            string result3;

            var result = DataLoaderContext.Run(async () =>
            {
                var loader = new DataLoader<int, string>(ints => ints.ToDictionary(x => x, x => $"SomeResult{x}"));
                result1 = await loader.LoadAsync(1);
                result2 = await loader.LoadAsync(2);
                result3 = await loader.LoadAsync(3);
                return new[] {result1, result2, result3};
            }).Result;
        }

        [Fact]
        public void DataLoaderContext_IsAsyncSafe()
        {

        }
    }
}