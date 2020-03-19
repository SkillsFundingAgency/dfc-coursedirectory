﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2
{
    public class StandardsAndFrameworksCache : IStandardsAndFrameworksCache
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        // These fields are lazily initialized
        private Task<IReadOnlyDictionary<(int standardCode, int version), Standard>> _standards;
        private Task<IReadOnlyDictionary<(int frameworkCode, int progType, int pathwayCode), Framework>> _frameworks;

        public StandardsAndFrameworksCache(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<IReadOnlyCollection<Framework>> GetAllFrameworks()
        {
            EnsureFrameworksCacheInitialized();

            return (await _frameworks).Values.ToList();
        }

        public async Task<IReadOnlyCollection<Standard>> GetAllStandards()
        {
            EnsureStandardsCacheInitialized();

            return (await _standards).Values.ToList();
        }

        public async Task<Framework> GetFramework(int frameworkCode, int progType, int pathwayCode)
        {
            EnsureFrameworksCacheInitialized();

            return (await _frameworks).GetValueOrDefault((frameworkCode, progType, pathwayCode));
        }

        public async Task<Standard> GetStandard(int standardCode, int version)
        {
            EnsureStandardsCacheInitialized();

            return (await _standards).GetValueOrDefault((standardCode, version));
        }

        private void EnsureFrameworksCacheInitialized()
        {
            LazyInitializer.EnsureInitialized(ref _frameworks, LoadFrameworks);
        }

        private void EnsureStandardsCacheInitialized()
        {
            LazyInitializer.EnsureInitialized(ref _standards, LoadStandards);
        }

        private async Task<IReadOnlyDictionary<(int frameworkCode, int progType, int pathwayCode), Framework>> LoadFrameworks()
        {
            var allFrameworks = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetAllFrameworks());

            return allFrameworks.ToDictionary(
                f => (f.FrameworkCode, f.ProgType, f.PathwayCode),
                f => new Framework()
                {
                    FrameworkCode = f.FrameworkCode,
                    ProgType = f.ProgType,
                    PathwayCode = f.PathwayCode,
                    NasTitle = f.NasTitle
                });
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
