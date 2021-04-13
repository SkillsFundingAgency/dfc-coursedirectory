using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore
{
    public class RegionCache : IRegionCache
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;

        private Task<IReadOnlyCollection<Region>> _regions;

        public RegionCache(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
        }

        public Task<IReadOnlyCollection<Region>> GetAllRegions()
        {
            EnsureCacheIsPopulated();

            return _regions;
        }

        private void EnsureCacheIsPopulated()
        {
            LazyInitializer.EnsureInitialized(ref _regions, LoadRegions);
        }

        private async Task<IReadOnlyCollection<Region>> LoadRegions()
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();
            return await dispatcher.ExecuteQuery(new GetAllRegions());
        }
    }
}
