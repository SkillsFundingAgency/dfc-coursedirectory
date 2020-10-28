using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Jobs
{
    public class SyncUkrlp
    {
        private readonly UkrlpSyncHelper _ukrlpSyncHelper;

        public SyncUkrlp(UkrlpSyncHelper ukrlpSyncHelper)
        {
            _ukrlpSyncHelper = ukrlpSyncHelper;
        }

        [FunctionName("SyncUkrlpChanges")]
        public async Task RunNightly([TimerTrigger("0 0 5 * * *")] TimerInfo timer)
        {
            // Only get records updated in the past week.
            // It times out if you try to pull the world back and doesn't support paging.
            // We run every day but this gives some buffer to allow for any errors.
            var updatedSince = DateTime.Today.AddDays(-7);

            await _ukrlpSyncHelper.SyncAllProviderData(updatedSince);
        }
    }
}
