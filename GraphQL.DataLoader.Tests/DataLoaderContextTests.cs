using System.Collections.Generic;
using System.Threading;
using Shouldly;
using Xunit;

namespace GraphQL.DataLoader.Tests
{
    public class DataLoaderContextTests
    {
        [Fact]
        public void DataLoaderContext_Run_ExecutesInNewContext()
        {
            DataLoaderContext.Current.ShouldBeNull();

            DataLoaderContext.Run(() =>
            {
                DataLoaderContext.Current.ShouldNotBeNull();
            });

            DataLoaderContext.Current.ShouldBeNull();
        }

        [Fact]
        public void DataLoaderContext_Run_IsThreadSafe()
        {
            var contexts = new List<DataLoaderContext>();
            var signal = new AutoResetEvent(false);

            ThreadStart action = () =>
            {
                DataLoaderContext.Run(() =>
                {
                    contexts.Add(DataLoaderContext.Current);
                    signal.Set();
                });
            };

            var thread1 = new Thread(action);
            var thread2 = new Thread(action);

            thread1.Start();
            signal.WaitOne();

            thread2.Start();
            signal.WaitOne();

            thread1.Join();
            thread2.Join();

            contexts.ShouldBeUnique();
        }
    }
}