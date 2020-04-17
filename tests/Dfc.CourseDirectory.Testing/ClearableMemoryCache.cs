using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Testing
{
    public class ClearableMemoryCache : MemoryDistributedCache, IDistributedCache
    {
        private readonly HashSet<string> _keys;

        public ClearableMemoryCache(IOptions<MemoryDistributedCacheOptions> optionsAccessor, ILoggerFactory loggerFactory)
            : base(optionsAccessor, loggerFactory)
        {
            _keys = new HashSet<string>();
        }

        public void Clear()
        {
            foreach (var key in _keys)
            {
                Remove(key);
            }

            _keys.Clear();
        }

        void IDistributedCache.Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            Set(key, value, options);

            _keys.Add(key);
        }

        async Task IDistributedCache.SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token)
        {
            await SetAsync(key, value, options, token);

            _keys.Add(key);
        }
    }
}
