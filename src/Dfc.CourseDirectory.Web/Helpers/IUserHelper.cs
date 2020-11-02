using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Auth;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IUserHelper
    {
        bool CheckUserLoggedIn();
        Task<bool> IsUserAuthorised(string policy);
        AuthUserDetails GetUserDetailsFromClaims(IEnumerable<Claim> claims, int? UKPRN);
    }
}
