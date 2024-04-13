using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Skyb.Extensions.Caching.MemCached
{
    public class ResponseCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ResponseCacheOption _options;

        public ResponseCachingMiddleware(RequestDelegate next, IDistributedCache cache, IHttpContextAccessor httpContextAccessor, IOptions<ResponseCacheOption> options)
        {
            _next = next;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string cacheKey = ComputeCacheKey(context.Request.Path);

            // Check if the response is cached
            byte[] cachedResponse = await _cache.GetAsync(cacheKey);
            if (cachedResponse != null)
            {
                await WriteCachedResponseAsync(context.Response, cachedResponse);
            }
            else
            {
                // Cache the response if not already cached
                var originalBodyStream = context.Response.Body;
                using (var memoryStream = new MemoryStream())
                {
                    context.Response.Body = memoryStream;

                    await _next(context);

                    // Store response in cache
                    byte[] responseBytes = memoryStream.ToArray();
                    await _cache.SetAsync(cacheKey, responseBytes, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _options.CacheTime.GetValueOrDefault(TimeSpan.FromSeconds(1)) // Set expiration time
                    });

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(originalBodyStream);
                }
            }
        }

        private async Task WriteCachedResponseAsync(HttpResponse response, byte[] cachedResponse)
        {
            response.ContentType = "text/html"; // Set appropriate content type
            await response.Body.WriteAsync(cachedResponse, 0, cachedResponse.Length);
        }

        private string ComputeCacheKey(string requestPath)
        {
            // You can customize how you generate cache keys based on your application requirements
            return "ResponseCache:" + requestPath;
        }
    }

}
