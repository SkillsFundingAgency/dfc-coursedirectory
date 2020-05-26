using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class SyncUkrlp
    {
        private readonly UkrlpSyncHelper _ukrlpSyncHelper;

        public SyncUkrlp(UkrlpSyncHelper ukrlpSyncHelper)
        {
            _ukrlpSyncHelper = ukrlpSyncHelper;
        }

        [FunctionName(nameof(SyncUkrlp))]
        public Task Run([TimerTrigger("0 0 5 * * *")] TimerInfo timer) => _ukrlpSyncHelper.SyncAllProviderData();
    }
}
