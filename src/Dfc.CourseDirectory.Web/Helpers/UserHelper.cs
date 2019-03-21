using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.Areas.Identity.Data;
using Dfc.CourseDirectory.Web.Areas.Identity.Pages.Account;
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
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public UserHelper(
            SignInManager<User> signInManager, 
            ILogger<LoginModel> logger, 
            UserManager<User> userManager, 
            IAuthorizationService authorizationService,
            IHttpContextAccessor contextAccessor)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            _contextAccessor = contextAccessor;
            _authorizationService = authorizationService;

        }
        public bool CheckUserLoggedIn()
        {
            if (_signInManager.IsSignedIn(_contextAccessor.HttpContext.User))
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
