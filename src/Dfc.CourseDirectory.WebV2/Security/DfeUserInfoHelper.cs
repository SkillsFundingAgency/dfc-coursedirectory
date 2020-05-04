using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Helpers.Interfaces;
using JWT.Algorithms;
using JWT.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class DfeUserInfoHelper : ISignInAction, IDisposable
    {
        private readonly DfeSignInSettings _settings;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly HttpClient _httpClient;
        private readonly IUkrlpSyncHelper _ukrlpSyncHelper;

        public DfeUserInfoHelper(
            DfeSignInSettings settings,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IUkrlpSyncHelper ukrlpSyncHelper)
        {
            _settings = settings;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _ukrlpSyncHelper = ukrlpSyncHelper;

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

            var roleNames = new HashSet<string>(userOrgDetails.Roles.Select(r => r.Name));
            var normalizedRoles = NormalizeRoles(roleNames, out var isProviderScoped);

            if (normalizedRoles.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No recognised roles found. " +
                    $"Received: ${(userOrgDetails.Roles.Count > 0 ? string.Join(", ", roleNames) : "<none>")}.");
            }
            else if (normalizedRoles.Count > 1)
            {
                throw new NotSupportedException(
                        $"Too many roles: " +
                        $"{string.Join(", ", normalizedRoles)}.");
            }

            var role = normalizedRoles.Single();

            if (isProviderScoped && !ukprn.HasValue)
            {
                throw new InvalidOperationException(
                    $"Expected a UKPRN for user with role: {role} organisation: {organisationId}.");
            }

            context.UserInfo.Role = role;
            context.DfeSignInOrganisationId = organisationId;
            context.ProviderUkprn = ukprn;

            if (ukprn.HasValue)
            {
                // Sync UKRLP data
                await _ukrlpSyncHelper.SyncProviderData(ukprn.Value, context.UserInfo.Email);

                var provider = await GetProvider(ukprn.Value);

                // TODO Handle this nicely
                if (provider == null)
                {
                    throw new Exception($"Cannot find provider with UKPRN: {ukprn.Value}.");
                }

                context.Provider = provider;
                context.UserInfo.CurrentProviderId = provider.Id;
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

        private IReadOnlyCollection<string> NormalizeRoles(
            IReadOnlyCollection<string> roleNames,
            out bool isProviderScoped)
        {
            if (roleNames.Contains("Developer") ||
                roleNames.Contains("Admin User") ||
                roleNames.Contains("Admin Superuser"))
            {
                isProviderScoped = false;
                return new[] { RoleNames.Developer };
            }

            if (roleNames.Contains("Helpdesk"))
            {
                isProviderScoped = false;
                return new[] { RoleNames.Helpdesk };
            }

            if (roleNames.Contains("Provider Superuser"))
            {
                isProviderScoped = true;
                return new[] { RoleNames.ProviderSuperUser };
            }

            if (roleNames.Contains("Provider User"))
            {
                isProviderScoped = true;
                return new[] { RoleNames.ProviderUser };
            }

            // No valid roles...
            isProviderScoped = default;
            return Array.Empty<string>();
        }

        private class DFEUserInfo
        {
            public Guid ServiceId { get; set; }
            public Guid OrganisationId { get; set; }
            public Guid UserId { get; set; }
            public IReadOnlyCollection<Role> Roles { get; set; }
        }

        private class Role
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}
