using System;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Auth;

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

        public AuthUserDetails GetUserDetailsFromClaims(IEnumerable<Claim> claims, int? UKPRN)
        {
            return new AuthUserDetails(
            
                userId: Guid.TryParse(GetClaim(claims, "userId"), out Guid userId) ? userId : Guid.Empty,
                email : GetClaim(claims, "email"),
                nameOfUser : $"{GetClaim(claims, "firstName")} {GetClaim(claims, "familyName")}",
                providerType : GetClaim(claims, "ProviderType"),
                roleId : Guid.TryParse(GetClaim(claims, "role_id"), out Guid roleId) ? roleId : Guid.Empty,
                roleName : GetClaim(claims, "rolename"),
                ukPrn : UKPRN.HasValue ? UKPRN.ToString() : GetClaim(claims, "UKPRN"),
                userName : GetClaim(claims, "email")
            );
        }

        internal string GetClaim(IEnumerable<Claim> claims, string type)
        {
            return claims.FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.CurrentCultureIgnoreCase))?.Value ?? String.Empty;
        }
    }
}
