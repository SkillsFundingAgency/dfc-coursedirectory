using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UkrlpService;

namespace Dfc.CourseDirectory.WebV2.Services.Interfaces
{
    public interface IUkrlpWcfService
    {
       Task<ProviderRecordStructure> GetProviderData(int ukprn);
    }
}
