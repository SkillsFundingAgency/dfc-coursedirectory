using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UkrlpService;

namespace Dfc.CourseDirectory.Core.ReferenceData.Ukrlp
{
    public class UkrlpService : IUkrlpService
    {
        // Magic values to make the service happy
        private const string QueryId = "0";
        private const string StakeholderId = "1";

        private static readonly TimeSpan _sendTimeout = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan _receiveTimeout = TimeSpan.FromMinutes(5);

        private static readonly string[] _statuses = new[]
        {
            "A", // Active
            "PD1", // Deactivation in process
            "PD2" // Deactivation complete
        };

        public async Task<IReadOnlyCollection<ProviderRecordStructure>> GetAllProviderData(DateTime updatedSince)
        {
            using var client = new ProviderQueryPortTypeClient();
            client.ChannelFactory.Endpoint.Binding.SendTimeout = _sendTimeout;
            client.ChannelFactory.Endpoint.Binding.ReceiveTimeout = _receiveTimeout;

            var results = new List<ProviderRecordStructure>();

            foreach (var status in _statuses)
            {
                var request = CreateRequest(status);

                var result = await client.retrieveAllProvidersAsync(request);
                var records = result.ProviderQueryResponse.MatchingProviderRecords;

                if (records != null)
                {
                   results.AddRange(records);
                }
            }

            return results;

            ProviderQueryStructure CreateRequest(string status) => new ProviderQueryStructure()
            {
                SelectionCriteria = new SelectionCriteriaStructure()
                {
                    ApprovedProvidersOnly = YesNoType.No,
                    ApprovedProvidersOnlySpecified = true,
                    CriteriaConditionSpecified = true,
                    ProviderStatus = status,
                    StakeholderId = StakeholderId,
                    ProviderUpdatedSince = updatedSince,
                    ProviderUpdatedSinceSpecified = true
                },
                QueryId = QueryId
            };
        }

        public async Task<ProviderRecordStructure> GetProviderData(int ukprn)
        {
            using var client = new ProviderQueryPortTypeClient();

            foreach (var status in _statuses)
            {
                var request = CreateRequest(ukprn, status);

                var result = await client.retrieveAllProvidersAsync(request);
                var response = result.ProviderQueryResponse;

                if (response.MatchingProviderRecords.Any())
                {
                    return response.MatchingProviderRecords.Single();
                }
            }

            return null;

            static ProviderQueryStructure CreateRequest(int ukprn, string status) => new ProviderQueryStructure()
            {
                SelectionCriteria = new SelectionCriteriaStructure()
                {
                    ApprovedProvidersOnly = YesNoType.No,
                    ApprovedProvidersOnlySpecified = true,
                    CriteriaConditionSpecified = true,
                    ProviderStatus = status,
                    StakeholderId = StakeholderId,
                    UnitedKingdomProviderReferenceNumberList = new[] { ukprn.ToString() },
                },
                QueryId = QueryId
            };
        }
    }
}
