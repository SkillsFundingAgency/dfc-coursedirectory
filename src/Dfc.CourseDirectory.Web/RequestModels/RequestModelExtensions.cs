using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public static class RequestModelExtensions
    {
        public static ILarsSearchCriteria ToLarsSearchCriteria(
            this LarsSearchRequestModel extendee,
            int currentPageNo,
            int itemsPerPage,
            IEnumerable<LarsSearchFacet> facets)
        {
            var searchTerm = extendee.SearchTerm;
            var notionalNVQLevelv2FilterCount = extendee.NotionalNVQLevelv2Filter.Count();
            var awardOrgCodeFilterCount = extendee.AwardOrgCodeFilter.Count();
            var sb = new StringBuilder();

            for (var i = 0; i < notionalNVQLevelv2FilterCount; i++)
            {
                if (i == 0 && notionalNVQLevelv2FilterCount > 1)
                {
                    new LarsSearchFilterBuilder(sb)
                        .Field("NotionalNVQLevelv2")
                        .EqualTo(extendee.NotionalNVQLevelv2Filter[i])
                        .Or();
                }
                else
                {
                    new LarsSearchFilterBuilder(sb)
                        .Field("NotionalNVQLevelv2")
                        .EqualTo(extendee.NotionalNVQLevelv2Filter[i]);
                }
            }

            if (sb.Length > 0 && extendee.AwardOrgCodeFilter.Any())
            {
                new LarsSearchFilterBuilder(sb).And();
            }

            for (var i = 0; i < awardOrgCodeFilterCount; i++)
            {
                if (awardOrgCodeFilterCount > 1)
                {
                    new LarsSearchFilterBuilder(sb)
                        .Field("AwardOrgCode")
                        .EqualTo(extendee.AwardOrgCodeFilter[i])
                        .Or();
                }
                else
                {
                    new LarsSearchFilterBuilder(sb)
                        .Field("AwardOrgCode")
                        .EqualTo(extendee.AwardOrgCodeFilter[i]);
                }
            }

            var skip = currentPageNo == 1 ? 0 : itemsPerPage * (currentPageNo - 1);

            var criteria = new LarsSearchCriteria(
                string.Join("+", searchTerm.Split(' ').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x))),
                itemsPerPage,
                skip,
                new LarsSearchFilterBuilder(sb).Build(),
                facets);

            return criteria;
        }

        public static bool IsFilterSelected(
            this LarsSearchRequestModel extendee,
            string filterName,
            string value)
        {
            if (filterName == nameof(extendee.AwardOrgCodeFilter)
                && extendee.AwardOrgCodeFilter.Any())
            {
                return extendee.AwardOrgCodeFilter.Contains(value);
            }

            if (filterName == nameof(extendee.NotionalNVQLevelv2Filter)
                && extendee.NotionalNVQLevelv2Filter.Any())
            {
                return extendee.NotionalNVQLevelv2Filter.Contains(value);
            }

            return false;
        }
    }
}