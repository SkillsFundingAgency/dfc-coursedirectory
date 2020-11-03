using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Models.Models.Auth;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IProviderService _providerService;

        public UserHelper(
            IAuthorizationService authorizationService,
            IHttpContextAccessor contextAccessor,
            IProviderService providerService)
        {
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(providerService, nameof(providerService));
            _contextAccessor = contextAccessor;
            _authorizationService = authorizationService;
            _providerService = providerService;

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
            
                userId: Guid.TryParse(GetClaim(claims, "user_id"), out Guid userId) ? userId : Guid.Empty,
                email : GetClaim(claims, "email"),
                nameOfUser : $"{GetClaim(claims, "given_name")} {GetClaim(claims, "family_Name")}",
                roleId : Guid.Empty,
                roleName : string.Empty,
                ukPrn : UKPRN.HasValue ? UKPRN.ToString() : GetClaim(claims, "UKPRN"),
                userName : GetClaim(claims, "email"),
                providerId: UKPRN.HasValue ? GetProviderDetails(UKPRN.Value) : Guid.Empty

            );
        }

        internal Guid GetProviderDetails(int ukprn)
        {
            var result = _providerService.GetProviderByPRNAsync(ukprn.ToString()).Result;
            if (result.IsSuccess && result.HasValue)
            {
                return result.Value.Select(x => x.id).FirstOrDefault();
            }
            else return Guid.Empty;
        }
        internal string GetClaim(IEnumerable<Claim> claims, string type)
        {
            return claims.FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.CurrentCultureIgnoreCase))?.Value ?? String.Empty;
        }
    }
}
