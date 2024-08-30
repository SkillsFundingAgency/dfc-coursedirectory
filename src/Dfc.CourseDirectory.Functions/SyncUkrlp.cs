using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

namespace Dfc.CourseDirectory.Functions
{
    public class SyncUkrlp
    {
        private readonly UkrlpSyncHelper _ukrlpSyncHelper;

        public SyncUkrlp(UkrlpSyncHelper ukrlpSyncHelper)
        {
            _ukrlpSyncHelper = ukrlpSyncHelper;
        }

        [Function("SyncUkrlpChanges")]        
        public async Task RunNightly([TimerTrigger("0 0 3 * * *")] TimerInfo timer, ILogger logger)
        {
            // Only get records updated in the past week.
            // It times out if you try to pull the world back and doesn't support paging.
            // We run every day but this gives some buffer to allow for any errors.
            var updatedSince = DateTime.Today.AddDays(-7);
            logger.LogInformation("Starting UKRLP Sync using cutoff date of {0}", updatedSince);

            await _ukrlpSyncHelper.SyncAllProviderData(updatedSince);

            logger.LogInformation("Function App completed successfully");
        }

        [Function("SyncKnownProviders")]        
        public Task SyncKnownProviders(string input) => _ukrlpSyncHelper.SyncProviderData(int.Parse(input));

        [Function("SyncAllKnownProvidersData")]
        public Task SyncAllKnownProvidersData(string input) => _ukrlpSyncHelper.SyncAllKnownProvidersData();
    }
}
