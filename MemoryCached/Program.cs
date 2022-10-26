using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryCached
{
    internal class Program
    {
        
        static void Main(string[] args)
        {
            var tmp = new MemoryCacheDemo();
            Console.ReadLine();
        }
    }

    public class MemoryCacheDemo
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheDemo(IMemoryCache memoryCache) => _memoryCache = memoryCache;

        public string GetCacheValue()
        {
            var cachedValue = _memoryCache.GetOrCreate(
               CacheKeys.Entry,
               cacheEntry =>
               {
                   cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                   return "10001";
               });
            return cachedValue;
        }
    }

    public static class CacheKeys
    {
        public const string Entry = "_Entry";

        public const string CallbackEntry = "_CallbackEntry";

        public const string CallbackMessage = "_CallbackMessage";

        public const string Parent = "_Parent";

        public const string Child = "_Child";

        public const string DependentCancellationTokenSource = "_DependentCancellationTokenSource";
    }
}
