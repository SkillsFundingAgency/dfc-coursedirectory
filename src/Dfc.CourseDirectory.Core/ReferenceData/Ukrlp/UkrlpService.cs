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

        private static readonly string[] _statuses = new[]
        {
            "A", // Active
            //"V", // Verified
            "PD1", // Deactivation in process
            "PD2" // Deactivation complete
        };

        public async Task<ProviderRecordStructure> GetProviderData(int ukprn)
        {
            using var client = new ProviderQueryPortTypeClient();

            foreach (var status in _statuses)
            {
                var request = CreateRequestForProvider(ukprn, status);

                var result = await client.retrieveAllProvidersAsync(request);
                var response = result.ProviderQueryResponse;

                if (response.MatchingProviderRecords.Any())
                {
                    return response.MatchingProviderRecords.Single();
                }
            }

            return null;
        }

        private static ProviderQueryStructure CreateRequestForProvider(int ukprn, string status) => new ProviderQueryStructure()
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
