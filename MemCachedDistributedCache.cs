using Enyim.Caching.Memcached;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skyb.Extensions.Caching.MemCached
{
    public class MemCachedDistributedCache : IDistributedCache
    {
        private readonly IMemcachedClient _client;

        private static readonly TimeSpan Default = TimeSpan.FromSeconds(30);
        public MemCachedDistributedCache(IMemcachedClient client)
        {
            _client = client;
        }

        public byte[] Get(string key)
        {
            return RunSynchronously(GetAsync(key));
        }


        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            return await _client.GetAsync<byte[]>(key);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var expiration = options?.AbsoluteExpirationRelativeToNow ?? Default;
            RunSynchronously(_client.SetAsync(key, value, expiration));
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var expiration = options?.AbsoluteExpirationRelativeToNow ?? Default;
            await _client.SetAsync(key, value, expiration);
        }

        public void Refresh(string key)
        {
            // Memcache does not support explicit refresh, so no action needed here
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            // Memcache does not support explicit refresh, so no action needed here
            return Task.CompletedTask;
        }

        public void Remove(string key, CancellationToken token = default)
        {
            RunSynchronously(_client.DeleteAsync(key));
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            await _client.DeleteAsync(key);
        }

        static T RunSynchronously<T>(Task<T> task, CancellationToken token = default)
        {
            task.Wait(token); // Blocking the main thread until the asynchronous operation completes
            return task.Result;
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }
    }
}
