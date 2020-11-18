using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Auth;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public interface IUserHelper
    {
        bool CheckUserLoggedIn();
        Task<bool> IsUserAuthorised(string policy);
        Task<AuthUserDetails> GetUserDetailsFromClaims(IEnumerable<Claim> claims, int? UKPRN);
    }
}
