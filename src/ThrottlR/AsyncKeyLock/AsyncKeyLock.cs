// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.
// Thanks to https://github.com/SixLabors/ImageSharp.Web/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlR
{
    /// <summary>
    /// The async key lock prevents multiple asynchronous threads acting upon the same object with the given key at the same time.
    /// It is designed so that it does not block unique requests allowing a high throughput.
    /// </summary>
    internal sealed class AsyncKeyLock
    {
        /// <summary>
        /// A collection of doorman counters used for tracking references to the same key.
        /// </summary>
        private static readonly Dictionary<string, Doorman> _keys = new Dictionary<string, Doorman>();

        /// <summary>
        /// A pool of unused doorman counters that can be re-used to avoid allocations.
        /// </summary>
        private static readonly Stack<Doorman> _pool = new Stack<Doorman>(MaxPoolSize);

        /// <summary>
        /// SpinLock used to protect access to the Keys and Pool collections.
        /// </summary>
        private static SpinLock _spinLock = new SpinLock(false);

        /// <summary>
        /// Maximum size of the doorman pool. If the pool is already full when releasing
        /// a doorman, it is simply left for garbage collection.
        /// </summary>
        private const int MaxPoolSize = 20;

        /// <summary>
        /// Locks the current thread in read mode asynchronously.
        /// </summary>
        /// <param name="key">The key identifying the specific object to lock against.</param>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        public async Task<IDisposable> ReaderLockAsync(string key)
        {
            var doorman = GetDoorman(key);

            return await doorman.ReaderLockAsync();
        }

        /// <summary>
        /// Locks the current thread in write mode asynchronously.
        /// </summary>
        /// <param name="key">The key identifying the specific object to lock against.</param>
        /// <returns>
        /// The <see cref="Task{IDisposable}"/> that will release the lock.
        /// </returns>
        public async Task<IDisposable> WriterLockAsync(string key)
        {
            var doorman = GetDoorman(key);

            return await doorman.WriterLockAsync();
        }

        /// <summary>
        /// Gets the doorman for the specified key. If no such doorman exists, an unused doorman
        /// is obtained from the pool (or a new one is allocated if the pool is empty), and it's
        /// assigned to the requested key.
        /// </summary>
        /// <param name="key">The key for the desired doorman.</param>
        /// <returns>The <see cref="Doorman"/>.</returns>
        private static Doorman GetDoorman(string key)
        {
            Doorman doorman;
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (!_keys.TryGetValue(key, out doorman))
                {
                    doorman = (_pool.Count > 0) ? _pool.Pop() : new Doorman(ReleaseDoorman);
                    doorman.Key = key;
                    _keys.Add(key, doorman);
                }

                doorman.RefCount++;
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit();
                }
            }

            return doorman;
        }

        /// <summary>
        /// Releases a reference to a doorman. If the ref-count hits zero, then the doorman is
        /// returned to the pool (or is simply left for the garbage collector to cleanup if the
        /// pool is already full).
        /// </summary>
        /// <param name="doorman">The <see cref="Doorman"/>.</param>
        private static void ReleaseDoorman(Doorman doorman)
        {
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (--doorman.RefCount == 0)
                {
                    _keys.Remove(doorman.Key);
                    if (_pool.Count < MaxPoolSize)
                    {
                        doorman.Key = null;
                        _pool.Push(doorman);
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit();
                }
            }
        }
    }
}
