using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchFacets : ValueObject<LarsSearchFacets>, ILarsSearchFacets
    {
        public IEnumerable<SearchFacet> AwardOrgCode { get; }
        public string AwardOrgCodeODataType { get; }
        public IEnumerable<SearchFacet> NotionalNVQLevelv2 { get; }
        public string NotionalNVQLevelv2ODataType { get; }
        public IEnumerable<SearchFacet> SectorSubjectAreaTier1 { get; }
        public string SectorSubjectAreaTier1ODataType { get; }
        public IEnumerable<SearchFacet> SectorSubjectAreaTier2 { get; }
        public string SectorSubjectAreaTier2ODataType { get; }

        public LarsSearchFacets(
            IEnumerable<SearchFacet> awardOrgCode,
            string awardOrgCodeODataType,
            IEnumerable<SearchFacet> notionalNVQLevelv2,
            string notionalNVQLevelv2ODataType
            )
        {
            Throw.IfNullOrEmpty(awardOrgCode, nameof(awardOrgCode));
            Throw.IfNullOrWhiteSpace(awardOrgCodeODataType, nameof(awardOrgCodeODataType));
            Throw.IfNullOrEmpty(notionalNVQLevelv2, nameof(notionalNVQLevelv2));
            Throw.IfNullOrWhiteSpace(notionalNVQLevelv2ODataType, nameof(notionalNVQLevelv2ODataType));

            AwardOrgCode = awardOrgCode;
            AwardOrgCodeODataType = awardOrgCodeODataType;
            NotionalNVQLevelv2 = notionalNVQLevelv2;
            NotionalNVQLevelv2ODataType = notionalNVQLevelv2ODataType;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return AwardOrgCode;
            yield return AwardOrgCodeODataType;
            yield return NotionalNVQLevelv2;
            yield return NotionalNVQLevelv2ODataType;
        }
    }
}