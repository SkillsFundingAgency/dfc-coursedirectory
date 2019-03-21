using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.Areas.Identity.Data;
using Dfc.CourseDirectory.Web.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;

        public UserHelper(
            SignInManager<User> signInManager, 
            ILogger<LoginModel> logger, 
            UserManager<User> userManager, 
            IHttpContextAccessor contextAccessor)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            _contextAccessor = contextAccessor;

        }
        public bool CheckUserLoggedIn()
        {
            if (_signInManager.IsSignedIn(_contextAccessor.HttpContext.User))
            {
                return true;
            }
            return false;
            
        }
    }
}
