using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
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
    public class DfeUserInfoHelper : IDisposable
    {
        private readonly DfeSignInSettings _settings;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IHostEnvironment _environment;
        private readonly HttpClient _httpClient;

        public DfeUserInfoHelper(
            DfeSignInSettings settings,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IHostEnvironment environment)
        {
            _settings = settings;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _environment = environment;

            _httpClient = new HttpClient();  // TODO Use HttpClientFactory
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {CreateApiToken(settings)}");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task AppendAdditionalClaims(ClaimsPrincipal principal)
        {
            var userId = principal.FindFirst("sub").Value;

            var organisation = JObject.Parse(principal.FindFirst("Organisation").Value);
            var organisationId = organisation["id"].ToString();
            var ukprn = organisation["ukprn"].ToObject<int?>();

            var userOrgDetails = await GetUserOrganisationDetails(organisationId, userId);

            var additionalClaims = new List<Claim>()
            {
                new Claim("OrganisationId", organisationId)
            };

            additionalClaims.AddRange(userOrgDetails.Roles.Select(r => new Claim(ClaimTypes.Role, r.Name)));

            if (ukprn.HasValue)
            {
                var provider = await GetProvider(ukprn.Value);

                // TODO Handle this nicely
                if (provider != null)
                {
                    throw new Exception("Unknown provider");
                }

                additionalClaims.Add(new Claim("ProviderId", provider.Id.ToString()));
                additionalClaims.Add(new Claim("ProviderType", provider.ProviderType.ToString()));
                additionalClaims.Add(new Claim("provider_status", provider.ProviderStatus));

                // This claim is kept around to keep the old UI bits working.
                // New bits should use ProviderId instead
                if (!_environment.IsTesting())
                {
                    additionalClaims.Add(new Claim("UKPRN", ukprn.Value.ToString()));
                }
            }
            else
            {
                // TODO: Old UI uses this to figure out what UI to show so we need to keep it around for now.
                // New UI shouldn't depend on this so we should omit this after we've migrated all the UI
                if (!_environment.IsTesting())
                {
                    additionalClaims.Add(new Claim("ProviderType", "Both"));
                }
            }

            principal.AddIdentity(new ClaimsIdentity(additionalClaims));
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
            public IEnumerable<Role> Roles { get; set; }
        }

        private class Role
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}
