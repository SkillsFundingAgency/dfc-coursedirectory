
using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;


namespace Dfc.CourseDirectory.Services
{
    public class ProviderSearchFacets : ValueObject<ProviderSearchFacets>, IProviderSearchFacets
    {
        public IEnumerable<SearchFacet> Town { get; }
        public string TownODataType { get; }
        //public IEnumerable<SearchFacet> Region { get; }
        //public string RegionODataType { get; }

        public ProviderSearchFacets(
            IEnumerable<SearchFacet> town,
            string townODataType //,
            //IEnumerable<SearchFacet> region,
            //string regionODataType
            )
        {
            Throw.IfNullOrEmpty(town, nameof(town));
            Throw.IfNullOrWhiteSpace(townODataType, nameof(townODataType));
            //Throw.IfNullOrEmpty(region, nameof(region));
            //Throw.IfNullOrWhiteSpace(regionODataType, nameof(regionODataType));

            Town = town;
            TownODataType = townODataType;
            //Region = region;
            //RegionODataType = regionODataType;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Town;
            yield return TownODataType;
            //yield return Region;
            //yield return RegionODataType;
        }
    }
}