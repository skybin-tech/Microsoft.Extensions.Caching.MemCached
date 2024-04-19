using Enyim.Caching.Memcached;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyb.Extensions.Caching.MemCached
{
    internal static  class MemcachedClientExtensions
    {
        public static void SetValue<T>(this IMemcachedClient memcachedClient, string key, T value, DateTime? expirationTime = null)
            where T : class, new()
        {
            if (value == null)
            {
                return;
            }
            var json = JsonConvert.SerializeObject(value);

            var buffer = Encoding.UTF8.GetBytes(json);
            if (expirationTime == null)
            {
                RunSynchronously(memcachedClient.SetAsync(key, buffer));
            }
            else
            {
                RunSynchronously(memcachedClient.SetAsync(key, buffer, expirationTime.Value));
            }
        }

        public static T? GetValue<T>(this IMemcachedClient memcachedClient, string key)
            where T : class, new()
        {            
            var buffer = RunSynchronously(memcachedClient.GetAsync<byte[]>(key));

            if(buffer != null)
            {
                var json = Encoding.UTF8.GetString(buffer);
                var obj = JsonConvert.DeserializeObject<T>(json);
                return obj;
            }

            return default(T);
        }

        private static T RunSynchronously<T>(Task<T> task, CancellationToken token = default)
        {
            task.Wait(token); // Blocking the main thread until the asynchronous operation completes
            return task.Result;
        }
    }
}
