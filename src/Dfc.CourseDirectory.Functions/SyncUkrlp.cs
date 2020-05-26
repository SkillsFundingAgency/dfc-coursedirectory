using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Helpers;
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
        [NoAutomaticTrigger]
        public Task Run(string input) => _ukrlpSyncHelper.SyncAllProviderData();
    }
}
