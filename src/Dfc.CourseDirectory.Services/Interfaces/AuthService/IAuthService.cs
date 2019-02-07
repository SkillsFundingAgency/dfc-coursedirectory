using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.Interfaces.AuthService
{
    public interface IAuthService
    {
        Task<IResult<IAuthUserDetails>> GetDetailsByEmail(IAuthUserDetails authUserDetails);
    }
}
