using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class ClearableMemoryCache : MemoryCache
    {
        public ClearableMemoryCache(IOptions<MemoryCacheOptions> optionsAccessor)
            : base(optionsAccessor)
        {
        }

        public void Clear()
        {
            // HACK

            var entriesDict = typeof(MemoryCache)
                .GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(this);

            entriesDict.GetType().GetMethod("Clear").Invoke(entriesDict, new object[0]);
        }
    }
}
