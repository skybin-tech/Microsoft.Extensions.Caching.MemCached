using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Caching.MemCached
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MemCacheAttribute : Attribute
    {
        public int AbsoluteExpirationInSeconds { get; set; } // Absolute expiration in minutes

        public MemCacheAttribute(int absoluteExpirationMinutes = 0)
        {
            AbsoluteExpirationInSeconds = absoluteExpirationMinutes;
        }
    }
}
