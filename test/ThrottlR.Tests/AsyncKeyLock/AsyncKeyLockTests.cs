// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.
// Thanks to https://github.com/SixLabors/ImageSharp.Web/
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ThrottlR.Tests
{
    public class AsyncKeyLockTests
    {
        private static readonly AsyncKeyLock _asyncLock = new AsyncKeyLock();

        private const string AsyncKey = "ASYNC_KEY";
        private const string AsyncKey1 = "ASYNC_KEY1";
        private const string AsyncKey2 = "ASYNC_KEY2";

        [Fact]
        public async Task AsyncLockCanLockByKeyAsync()
        {
            var zeroEntered = false;
            var entered = false;
            var index = 0;
            var tasks = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
            {
                using (await _asyncLock.WriterLockAsync(AsyncKey))
                {
                    if (i == 0)
                    {
                        entered = true;
                        zeroEntered = true;
                        await Task.Delay(3000);
                        entered = false;
                    }
                    else if (zeroEntered)
                    {
                        Assert.False(entered);
                    }

                    index++;
                }
            })).ToArray();

            await Task.WhenAll(tasks);
            Assert.Equal(5, index);
        }

        [Fact]
        public async Task AsyncLockAllowsDifferentKeysToRunAsync()
        {
            var zeroEntered = false;
            var entered = false;
            var index = 0;
            var tasks = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
            {
                using (await _asyncLock.WriterLockAsync(i > 0 ? AsyncKey2 : AsyncKey1))
                {
                    if (i == 0)
                    {
                        entered = true;
                        zeroEntered = true;
                        await Task.Delay(2000);
                        entered = false;
                    }
                    else if (zeroEntered)
                    {
                        Assert.True(entered);
                    }

                    index++;
                }
            })).ToArray();

            await Task.WhenAll(tasks);
            Assert.Equal(5, index);
        }
    }
}
