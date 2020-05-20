using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UkrlpService;

namespace Dfc.CourseDirectory.Core.ReferenceData.Ukrlp
{
    public class UkrlpWcfService : IUkrlpWcfService
    {
        public async Task<ProviderRecordStructure> GetProviderData(int ukprn)
        {
            var statusesToFetch = new[]
            {
                "A", // Active
            };

            List<ProviderRecordStructure> results = new List<ProviderRecordStructure>();
            foreach (var status in statusesToFetch)
            {
                var request = BuildRequest(ukprn.ToString());
                request.SelectionCriteria.ProviderStatus = status;
                request.QueryId = "1";

                var providerClient = new ProviderQueryPortTypeClient();
                providerClient.InnerChannel.OperationTimeout = new TimeSpan(0, 10, 0);
                var x = await providerClient.retrieveAllProvidersAsync(request);

                results.AddRange(x?.ProviderQueryResponse?.MatchingProviderRecords ?? new ProviderRecordStructure[] { });
            }

            if(results.Any())
            {
                return results.First();
            }
            else
            {
                return null;
            }
       }

        private ProviderQueryStructure BuildRequest(string ukprnListToFetch)
        {
            SelectionCriteriaStructure scs = new SelectionCriteriaStructure
            {
                StakeholderId = "1",
                UnitedKingdomProviderReferenceNumberList = new[] { ukprnListToFetch },
                ApprovedProvidersOnly = YesNoType.No,
                ApprovedProvidersOnlySpecified = true,
                CriteriaCondition = QueryCriteriaConditionType.OR,
                CriteriaConditionSpecified = true
            };

            return new ProviderQueryStructure { SelectionCriteria = scs };
        }
    }
}
