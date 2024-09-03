using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class SyncUkrlp
    {
        private readonly UkrlpSyncHelper _ukrlpSyncHelper;
        private readonly ILogger _logger;

        public SyncUkrlp(UkrlpSyncHelper ukrlpSyncHelper, ILogger<SyncUkrlp> logger)
        {
            _ukrlpSyncHelper = ukrlpSyncHelper;
            _logger = logger;
        }

        [Function("SyncUkrlpChanges")]
        public async Task RunNightly([TimerTrigger("0 0 3 * * *")] TimerInfo timer)
        {
            // Only get records updated in the past week.
            // It times out if you try to pull the world back and doesn't support paging.
            // We run every day but this gives some buffer to allow for any errors.
            var updatedSince = DateTime.Today.AddDays(-7);
            _logger.LogInformation("Starting UKRLP Sync using cutoff date of {0}", updatedSince);

            await _ukrlpSyncHelper.SyncAllProviderData(updatedSince);

            _logger.LogInformation("Function App completed successfully");
        }

        [Function("SyncKnownProviders")]
        public Task SyncKnownProviders(string input) => _ukrlpSyncHelper.SyncProviderData(int.Parse(input));

        [Function("SyncAllKnownProvidersData")]
        public Task SyncAllKnownProvidersData(string input) => _ukrlpSyncHelper.SyncAllKnownProvidersData();
    }
}
