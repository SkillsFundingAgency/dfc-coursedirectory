using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.WebV2
{
    public class StandardsCache : IStandardsCache
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        // These fields are lazily initialized
        private Task<IReadOnlyDictionary<(int standardCode, int version), Standard>> _standards;

        public StandardsCache(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public void Clear()
        {
            _standards = null;
        }

        public async Task<IReadOnlyCollection<Standard>> GetAllStandards()
        {
            EnsureStandardsCacheInitialized();

            return (await _standards).Values.ToList();
        }

        public async Task<Standard> GetStandard(int standardCode, int version)
        {
            EnsureStandardsCacheInitialized();

            return (await _standards).GetValueOrDefault((standardCode, version));
        }

        private void EnsureStandardsCacheInitialized()
        {
            LazyInitializer.EnsureInitialized(ref _standards, LoadStandards);
        }

        private async Task<IReadOnlyDictionary<(int standardCode, int version), Standard>> LoadStandards()
        {
            var allStandards = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetAllStandards());

            return allStandards.ToDictionary(
                s => (s.StandardCode, s.Version),
                s => new Standard()
                {
                    StandardCode = s.StandardCode,
                    Version = s.Version,
                    StandardName = s.StandardName,
                    NotionalNVQLevelv2 = s.NotionalEndLevel,
                    OtherBodyApprovalRequired = s.OtherBodyApprovalRequired?.Equals("y", StringComparison.OrdinalIgnoreCase) ?? false
                });
        }
    }
}
