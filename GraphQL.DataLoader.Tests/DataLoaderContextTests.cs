﻿using System;
using System.Collections.Generic;
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
            Should.NotThrow(() =>
            {
                DataLoaderContext.Run(async () =>
                {
                    var loader = new DataLoader<int, string>(ints => ints.ToDictionary(x => x, x => $"SomeResult{x}"));
                    await loader.LoadAsync(1);
                    await loader.LoadAsync(2);
                    await loader.LoadAsync(3);
                    await loader.LoadAsync(4);
                });
            });
        }

        [Fact]
        public void DataLoaderContext_IsAsyncSafe()
        {

        }
    }
}