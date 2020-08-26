#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dfc.CourseDirectory.Web
{
    /// <summary>
    /// WARNING: This code is for development only and must not be used in production.
    /// Allows cached state, including session, to be persisted across rebuilds to facilitate development.
    /// A file called "cache.json" is written to the bin folder. To clear the cache, simply delete the file or clean the solution.
    /// </summary>
    public class FileDistributedCache : IDistributedCache
    {
        private static readonly string CacheFilePath = Path.Combine(AppContext.BaseDirectory, "cache.json");

        private readonly object _syncLock = new object();
        private readonly JsonSerializer _jsonSerializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver(), Formatting = Formatting.Indented };
        private readonly Func<DateTimeOffset> utcNow;

        public FileDistributedCache(Func<DateTimeOffset> utcNow)
        {
            this.utcNow = utcNow ?? throw new ArgumentNullException(nameof(utcNow));
        }

        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var now = utcNow();
            var value = default(byte[]);

            CacheAction(cache =>
            {
                if (!cache.TryGetValue(key, out var item))
                {
                    return;
                }

                if (item.AbsoluteExpiration.HasValue && item.AbsoluteExpiration <= now)
                {
                    cache.Remove(key);
                    return;
                }

                if (item.SlidingExpiration.HasValue && (now - item.LastAccessed) >= item.SlidingExpiration)
                {
                    cache.Remove(key);
                    return;
                }

                value = item.Value;

                item.LastAccessed = now;
            });

            return value;
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Get(key));
        }

        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Get(key);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            Refresh(key);
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            CacheAction(c => c.Remove(key));
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var now = utcNow();

            var absoluteExpiration = options.AbsoluteExpiration;

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                var absoluteExpirationRelativeToNow = now + options.AbsoluteExpirationRelativeToNow;
                absoluteExpiration = absoluteExpiration.HasValue && absoluteExpiration < absoluteExpirationRelativeToNow ? absoluteExpiration : absoluteExpirationRelativeToNow;
            }

            CacheAction(c => c[key] = new CacheItem
            {
                Value = value,
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = options.SlidingExpiration,
                LastAccessed = now
            });
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        private void CacheAction(Action<IDictionary<string, CacheItem>> action)
        {
            lock (_syncLock)
            {
                using (var stream = File.Open(CacheFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    var cache = default(IDictionary<string, CacheItem>);

                    using (var reader = new StreamReader(stream, leaveOpen: true))
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        cache = _jsonSerializer.Deserialize<IDictionary<string, CacheItem>>(jsonReader);
                    }

                    if (cache == null)
                    {
                        cache = new Dictionary<string, CacheItem>();
                    }

                    action(cache);

                    stream.Seek(0, SeekOrigin.Begin);

                    using (var writer = new StreamWriter(stream))
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        _jsonSerializer.Serialize(writer, cache);
                        jsonWriter.Flush();
                    }
                }
            }
        }

        private class CacheItem
        {
            public byte[] Value { get; set; }

            public DateTimeOffset? AbsoluteExpiration { get; set; }

            public TimeSpan? SlidingExpiration { get; set; }

            public DateTimeOffset? LastAccessed { get; set; }
        }
    }
}
#endif