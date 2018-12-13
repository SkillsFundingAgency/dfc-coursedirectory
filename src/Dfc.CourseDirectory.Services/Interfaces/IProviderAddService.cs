using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IProviderAddService
    {
        Task<IResult<IProvider>> AddProviderAsync(IProviderAdd provider);
    }
}
