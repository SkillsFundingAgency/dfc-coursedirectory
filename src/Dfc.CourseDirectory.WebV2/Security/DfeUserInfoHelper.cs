using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class DfeUserInfoHelper : ISignInAction, IDisposable
    {
        private static readonly ISet<string> _knownRoles = new HashSet<string>(new[]
        {
            RoleNames.Developer,
            RoleNames.Helpdesk,
            RoleNames.ProviderSuperUser,
            RoleNames.ProviderUser
        });

        private readonly DfeSignInSettings _settings;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly HttpClient _httpClient;

        public DfeUserInfoHelper(
            DfeSignInSettings settings,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _settings = settings;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;

            _httpClient = new HttpClient();  // TODO Use HttpClientFactory
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {CreateApiToken(settings)}");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task OnUserSignedIn(SignInContext context)
        {
            var organisation = JObject.Parse(context.OriginalPrincipal.FindFirst("Organisation").Value);
            var organisationId = organisation["id"].ToString();
            var ukprn = organisation["ukprn"].ToObject<int?>();

            var userOrgDetails = await GetUserOrganisationDetails(organisationId, context.UserInfo.UserId);

            var filteredRoles = userOrgDetails.Roles.Where(r => _knownRoles.Contains(r.Name)).ToList();

            if (filteredRoles.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No recognised roles found. " +
                    $"Received: ${(userOrgDetails.Roles.Count > 0 ? string.Join(", ", userOrgDetails.Roles.Select(r => r.Name)) : "<none>")}.");
            }
            else if (filteredRoles.Count > 1)
            {
                throw new NotSupportedException(
                    $"Too many roles: " +
                    $"{string.Join(", ", filteredRoles.Select(r => r.Name))}.");
            }

            context.UserInfo.Role = userOrgDetails.Roles.Single().Name;
            context.DfeSignInOrganisationId = organisationId;
            context.ProviderUkprn = ukprn;

            if (ukprn.HasValue)
            {
                var provider = await GetProvider(ukprn.Value);

                // TODO Handle this nicely
                if (provider != null)
                {
                    throw new Exception("Unknown provider");
                }

                context.Provider = provider;
            }
        }

        private static string CreateApiToken(DfeSignInSettings settings) =>
            new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .Issuer(settings.Issuer)
                .Audience(settings.Audience)
                .WithSecret(settings.ApiSecret)
                .Build();

        private Task<Provider> GetProvider(int ukprn) =>
            _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn() { Ukprn = ukprn });

        private async Task<DFEUserInfo> GetUserOrganisationDetails(string organisationId, string userId)
        {
            var endpoint = $"{_settings.ApiUri}/organisations/{organisationId}/users/{userId}";

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<DFEUserInfo>(responseJson);
        }

        private class DFEUserInfo
        {
            public Guid ServiceId { get; set; }
            public Guid OrganisationId { get; set; }
            public Guid UserId { get; set; }
            public IList<Role> Roles { get; set; }
        }

        private class Role
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}
