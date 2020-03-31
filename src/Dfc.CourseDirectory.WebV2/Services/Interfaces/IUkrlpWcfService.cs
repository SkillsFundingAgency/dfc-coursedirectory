using System;
using System.Collections.Generic;
using System.Text;
using UkrlpService;

namespace Dfc.CourseDirectory.WebV2.Services.Interfaces
{
    public interface IUkrlpWcfService
    {
        ProviderRecordStructure GetProviderData(int ukprn);
    }
}
