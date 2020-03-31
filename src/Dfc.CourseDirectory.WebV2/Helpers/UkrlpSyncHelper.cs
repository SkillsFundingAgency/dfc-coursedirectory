using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Helpers.Interfaces;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Helpers
{
    public class UkrlpSyncHelper : IUkrlpSyncHelper
    {
        private readonly IUkrlpWcfService _ukrlpWcfService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IClock _clock;
        private readonly ICurrentUserProvider _currentUserProvider;

        public UkrlpSyncHelper(IUkrlpWcfService ukrlpWcfService, 
                                ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, 
                                IClock clock,
                                ICurrentUserProvider currentUserProvider)
        {
            _ukrlpWcfService = ukrlpWcfService;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _clock = clock;
            _currentUserProvider = currentUserProvider;
        }

        public async Task SyncProviderData(Guid providerId, int ukprn)
        {
            var providerData = this._ukrlpWcfService.GetProviderData(ukprn);

            if(providerData != null)
            {
                var updateCommand = new UpdateProviderUkrlpData()
                {
                    ProviderId = providerId,
                    UpdatedOn = _clock.UtcNow,
                    UpdatedBy = _currentUserProvider.GetCurrentUser().Email,
                };

                await _cosmosDbQueryDispatcher.ExecuteQuery(updateCommand);
            }
        }
    }
}
