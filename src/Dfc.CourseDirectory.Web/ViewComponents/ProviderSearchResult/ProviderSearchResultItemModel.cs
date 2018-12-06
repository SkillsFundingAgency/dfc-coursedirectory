using Dfc.CourseDirectory.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderSearchResult
{
    public class ProviderSearchResultItemModel : ValueObject<ProviderSearchResultItemModel>, IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
        public string UnitedKingdomProviderReferenceNumber { get; }
        public string ProviderName { get; }
        public string ProviderStatus { get; }

        public ProviderSearchResultItemModel(
            string unitedKingdomProviderReferenceNumber,
            string providerName,
            string providerStatus)
        {
            Errors = new string[] { };
            UnitedKingdomProviderReferenceNumber = unitedKingdomProviderReferenceNumber;
            ProviderName = providerName;
            ProviderStatus = providerStatus;
        }

        public ProviderSearchResultItemModel()
        {
            Errors = new string[] { };
        }

        public ProviderSearchResultItemModel(string error)
        {
            Errors = new string[] { error };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HasErrors;
            yield return Errors;
            yield return UnitedKingdomProviderReferenceNumber;
            yield return ProviderName;
            yield return ProviderStatus;
        }
    }
}
