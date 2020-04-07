using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Helpers.Interfaces
{
    public interface IUkrlpSyncHelper
    {
        Task SyncProviderData(Guid providerId, int ukprn, string updatedBy);
    }
}
