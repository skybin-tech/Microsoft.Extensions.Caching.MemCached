using Enyim.Caching.Memcached;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skyb.Extensions.Caching.MemCached
{
    public static class Extensions
    {
        public const string CacheName = "MemCache";
        public static IServiceCollection AddMemCache(this IServiceCollection services, TimeSpan? cacheTime = null, params string[] servers)
        {
            var cluster = new MemcachedCluster(string.Join(",", servers));
            cluster.Start();
            services.AddSingleton<IMemcachedClient>(cluster.GetClient());
            services.AddSingleton<IDistributedCache, MemCacheDistributedCache>();
            services.Configure<ResponseCacheOption>(configure =>
            {
                configure.CacheTime = cacheTime;
            });
            return services;
        }

        //public static IApplicationBuilder UseMemCacheResponseMiddleware(
        //this IApplicationBuilder builder)
        //{
        //    return builder.UseMiddleware<MemCacheMiddleware>();
        //}
    }
}
