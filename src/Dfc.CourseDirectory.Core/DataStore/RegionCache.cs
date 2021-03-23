using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.DataStore
{
    public class RegionCache : IRegionCache
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private Task<IReadOnlyCollection<Region>> _regions;

        public RegionCache(IServiceScopeFactory serviceScopeFactory)
        {
            // Can't inject an ISqlQueryDispatcher directly here since this type needs to have Singleton lifetime
            // and ISqlQueryDispatcher holds onto a SqlTransaction

            _serviceScopeFactory = serviceScopeFactory;
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
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var sqlDispatcher = scope.ServiceProvider.GetRequiredService<ISqlQueryDispatcher>();

                return await sqlDispatcher.ExecuteQuery(new GetAllRegions());
            }
        }
    }
}
