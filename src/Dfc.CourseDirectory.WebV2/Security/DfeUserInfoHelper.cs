using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using JWT.Algorithms;
using JWT.Builder;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public class DfeUserInfoHelper : ISignInAction
    {
        private readonly DfeSignInSettings _settings;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly HttpClient _httpClient;
        private readonly UkrlpSyncHelper _ukrlpSyncHelper;

        public DfeUserInfoHelper(
            DfeSignInSettings settings,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            UkrlpSyncHelper ukrlpSyncHelper,
            IHttpClientFactory httpClientFactory)
        {
            _settings = settings;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _ukrlpSyncHelper = ukrlpSyncHelper;
            _httpClient = httpClientFactory.CreateClient("DfeSignIn");
        }

        public static string CreateApiToken(DfeSignInSettings settings) =>
            new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .Issuer(settings.Issuer)
                .Audience(settings.Audience)
                .WithSecret(settings.ApiSecret)
                .Encode();

        public async Task OnUserSignedIn(SignInContext context)
        {
            var organisation = JObject.Parse(context.OriginalPrincipal.FindFirst("organisation").Value);
            var organisationId = organisation["id"].ToString();

            var ukprnTask = GetOrganisationUkprn(organisationId, context.UserInfo.UserId);
            var userRolesTask = GetUserRoles(organisationId, context.UserInfo.UserId);

            await Task.WhenAll(ukprnTask, userRolesTask);

            var ukprn = ukprnTask.Result;
            var userRoles = userRolesTask.Result;

            var normalizedRoles = NormalizeRoles(userRoles, out var isProviderScoped);

            if (normalizedRoles.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No recognised roles found. " +
                    $"Received: ${(userRoles.Count > 0 ? string.Join(", ", userRoles) : "<none>")}.");
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
                await _ukrlpSyncHelper.SyncProviderData(ukprn.Value);

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

        private Task<Provider> GetProvider(int ukprn) =>
            _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderByUkprn() { Ukprn = ukprn });

        private async Task<int?> GetOrganisationUkprn(string organisationId, string userId)
        {
            var endpoint = $"/users/{userId}/organisations";

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            return JArray.Parse(responseJson)
                .Single(org => org.SelectToken("id").ToString() == organisationId)
                ["ukprn"].ToObject<int?>();
        }

        private async Task<IReadOnlyCollection<string>> GetUserRoles(string organisationId, string userId)
        {
            var endpoint = $"/services/{_settings.ServiceId}/organisations/{organisationId}/users/{userId}";

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            return JObject.Parse(responseJson)
                .SelectToken("roles")
                .Select(role => role.SelectToken("name").ToString())
                .ToList();
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

        private class Role
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}
