using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Services.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public UserHelper(
            IAuthorizationService authorizationService,
            IHttpContextAccessor contextAccessor,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _authorizationService = authorizationService;
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher ?? throw new ArgumentNullException(nameof(cosmosDbQueryDispatcher));
        }

        public bool CheckUserLoggedIn()
        {
            return _contextAccessor.HttpContext.User.Identity.IsAuthenticated;
        }

        public async Task<bool> IsUserAuthorised(string policy)
        {
            if(!CheckUserLoggedIn())
            {
                return false;
            }

            var authorised = await _authorizationService.AuthorizeAsync(_contextAccessor.HttpContext.User, policy);

            return authorised.Succeeded;
        }

        public async Task<AuthUserDetails> GetUserDetailsFromClaims(IEnumerable<Claim> claims, int? UKPRN)
        {
            var provider = UKPRN.HasValue
                ? await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn { Ukprn = UKPRN.Value })
                : null;

            return new AuthUserDetails(
                userId: Guid.TryParse(GetClaim(claims, "user_id"), out Guid userId) ? userId : Guid.Empty,
                email: GetClaim(claims, "email"),
                nameOfUser: $"{GetClaim(claims, "given_name")} {GetClaim(claims, "family_Name")}",
                roleId: Guid.Empty,
                roleName: string.Empty,
                ukPrn: UKPRN.HasValue ? UKPRN.ToString() : GetClaim(claims, "UKPRN"),
                userName: GetClaim(claims, "email"),
                providerId: provider?.Id ?? Guid.Empty
            );
        }

        private string GetClaim(IEnumerable<Claim> claims, string type)
        {
            return claims.FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.CurrentCultureIgnoreCase))?.Value ?? String.Empty;
        }
    }
}
