using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class SyncUkrlp
    {
        // Do a 'full sync' going back to just before Provider migration happened
        private static readonly DateTime _fullSyncFrom = new DateTime(2020, 3, 1);

        private readonly UkrlpSyncHelper _ukrlpSyncHelper;

        public SyncUkrlp(UkrlpSyncHelper ukrlpSyncHelper)
        {
            _ukrlpSyncHelper = ukrlpSyncHelper;
        }

        [FunctionName("SyncUkrlpChanges")]
        public async Task RunNightly([TimerTrigger("0 * 5 * * *")] TimerInfo timer)
        {
            // Only get records updated in the past week
            // We run every day but this gives some buffer to allow for any errors
            var updatedSince = DateTime.Today.AddDays(-7);

            await _ukrlpSyncHelper.SyncAllProviderData(updatedSince);
        }

        [FunctionName("SyncUkrlp")]
        [NoAutomaticTrigger]
        public Task RunFull(string input) => _ukrlpSyncHelper.SyncAllProviderData(_fullSyncFrom);
    }
}
