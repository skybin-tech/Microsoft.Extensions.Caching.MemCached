using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Caching.MemCached
{
    public class MemCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;

        public MemCacheMiddleware(RequestDelegate next, IDistributedCache cache)
        {
            _next = next;
            _cache = cache;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            // Retrieve cache attribute from endpoint metadata
            var endpoint = context.GetEndpoint();
            var cacheAttribute = endpoint?.Metadata.GetMetadata<MemCacheAttribute>();

            if (cacheAttribute != null)
            {
                // Generate a hash of the request URL
                string requestUrl = context.Request.GetDisplayUrl();
                string cacheKey = ComputeHash(requestUrl);

                // Check if the cache contains the item
                var cachedData = await _cache.GetStringAsync(cacheKey);

                if (cachedData == null)
                {
                    // If item not found in cache, cache the response output
                    var responseStream = context.Response.Body;

                    try
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            context.Response.Body = memoryStream;

                            // Allow the response to be written to the memory stream
                            await _next(context);

                            // Reset the memory stream position to read from the beginning
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            // Copy the response content to the original response stream
                            await memoryStream.CopyToAsync(responseStream);

                            // Get the response content as a string
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            using (var reader = new StreamReader(memoryStream))
                            {
                                var responseContent = await reader.ReadToEndAsync();

                                // Set the response content in the cache with absolute expiration
                                var options = new DistributedCacheEntryOptions();
                                if (cacheAttribute.AbsoluteExpirationInSeconds > 0)
                                {
                                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheAttribute.AbsoluteExpirationInSeconds);
                                }

                                await _cache.SetStringAsync(cacheKey, responseContent, options);
                            }
                        }
                    }
                    finally
                    {
                        context.Response.Body = responseStream;
                    }
                }
                else
                {
                    // If item found in cache, write cached response to the original response stream
                    context.Response.ContentType = "text/html"; // Set appropriate content type
                    await context.Response.WriteAsync(cachedData);
                }
            }
            else
            {
                // Proceed with the next middleware in the pipeline
                await _next(context);
            }
        }

        private string ComputeHash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
