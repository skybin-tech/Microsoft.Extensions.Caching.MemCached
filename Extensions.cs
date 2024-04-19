using Enyim.Caching.Memcached;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
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

        private const string DataProtectionKeysName = "DataProtection-Keys";
        public static IMemcachedClient AddMemCache(this IServiceCollection services, TimeSpan? cacheTime = null, params string[] servers)
        {
            var cluster = new MemcachedCluster(string.Join(",", servers));
            cluster.Start();
            var client = cluster.GetClient();
            services.AddSingleton(client);
            services.AddSingleton<IDistributedCache, MemCachedDistributedCache>();
            services.AddSingleton<MemCachedDistributedCache>();
            services.Configure<ResponseCacheOption>(configure =>
            {
                configure.CacheTime = cacheTime;
            });
            return client;
        }

        public static IDataProtectionBuilder PersistKeysToMemCached(this IDataProtectionBuilder builder, IMemcachedClient memCached)
        {
            return PersistKeysToMemCached(builder, memCached, DataProtectionKeysName);
        }

        public static IDataProtectionBuilder PersistKeysToMemCached(this IDataProtectionBuilder builder, IMemcachedClient memCached, string keyName)
        {
            builder.Services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new MemCachedXmlRepository(memCached, keyName);
            });
            return builder;
        }

        //public static IApplicationBuilder UseMemCacheResponseMiddleware(
        //this IApplicationBuilder builder)
        //{
        //    return builder.UseMiddleware<MemCacheMiddleware>();
        //}
    }
}
