using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public UserHelper(
            IAuthorizationService authorizationService,
            IHttpContextAccessor contextAccessor)
        {
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            _contextAccessor = contextAccessor;
            _authorizationService = authorizationService;

        }
        public bool CheckUserLoggedIn()
        {
            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return true;
            }
            return false;
            
        }
        public async Task<bool> IsUserAuthorised(string policy)
        {
            if(!CheckUserLoggedIn())
            {
                return false;
            }

            var authorised = await _authorizationService.AuthorizeAsync(_contextAccessor.HttpContext.User, policy);
            if (authorised.Succeeded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
