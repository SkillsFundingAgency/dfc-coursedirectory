using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class LarsSearchHelper : ILarsSearchHelper
    {
        public ILarsSearchCriteria GetLarsSearchCriteria(
            LarsSearchRequestModel larsSearchRequestModel,
            int currentPageNo,
            int itemsPerPage,
            IEnumerable<LarsSearchFacet> facets = null)
        {
            Throw.IfNull(larsSearchRequestModel, nameof(larsSearchRequestModel));
            Throw.IfLessThan(1, currentPageNo, nameof(currentPageNo));
            Throw.IfLessThan(1, itemsPerPage, nameof(itemsPerPage));

            var sb = new StringBuilder();

            sb = BuildUpFilterStringBuilder(sb, "NotionalNVQLevelv2", larsSearchRequestModel.NotionalNVQLevelv2Filter);
            sb = BuildUpFilterStringBuilder(sb, "AwardOrgCode", larsSearchRequestModel.AwardOrgCodeFilter);
            sb = BuildUpFilterStringBuilder(sb, "SectorSubjectAreaTier1", larsSearchRequestModel.SectorSubjectAreaTier1Filter);
            sb = BuildUpFilterStringBuilder(sb, "SectorSubjectAreaTier2", larsSearchRequestModel.SectorSubjectAreaTier2Filter);

            var skip = currentPageNo == 1 ? 0 : itemsPerPage * (currentPageNo - 1);

            var criteria = new LarsSearchCriteria(
                FormatSearchTerm(larsSearchRequestModel.SearchTerm),
                itemsPerPage,
                skip,
                new LarsSearchFilterBuilder(sb).Build(),
                facets);

            return criteria;
        }

        public IEnumerable<LarsSearchFilterModel> GetLarsSearchFilterModels(
            LarsSearchFacets larsSearchFacets,
            LarsSearchRequestModel larsSearchRequestModel)
        {
            Throw.IfNull(larsSearchFacets, nameof(larsSearchFacets));
            Throw.IfNull(larsSearchRequestModel, nameof(larsSearchRequestModel));

            var filters = new List<LarsSearchFilterModel>();

            var notionalNVQLevelv2Filter = GetLarsSearchFilterModel(
                "Notional NVQ Level v2",
                "NotionalNVQLevelv2Filter",
                (value) => $"Level {value}",
                larsSearchFacets.NotionalNVQLevelv2,
                larsSearchRequestModel.NotionalNVQLevelv2Filter);

            var awardOrgCodeFilter = GetLarsSearchFilterModel(
                "Award Org Code",
                "AwardOrgCodeFilter",
                (value) => value,
                larsSearchFacets.AwardOrgCode,
                larsSearchRequestModel.AwardOrgCodeFilter);

            var sectorSubjectAreaTier1Filter = GetLarsSearchFilterModel(
                "Sector Subject Area Tier 1",
                "SectorSubjectAreaTier1Filter",
                (value) => value,
                larsSearchFacets.SectorSubjectAreaTier1,
                larsSearchRequestModel.SectorSubjectAreaTier1Filter);

            var sectorSubjectAreaTier2Filter = GetLarsSearchFilterModel(
                "Sector Subject Area Tier 2",
                "SectorSubjectAreaTier2Filter",
                (value) => value,
                larsSearchFacets.SectorSubjectAreaTier2,
                larsSearchRequestModel.SectorSubjectAreaTier2Filter);

            filters.Add(notionalNVQLevelv2Filter);
            filters.Add(awardOrgCodeFilter);
            filters.Add(sectorSubjectAreaTier1Filter);
            filters.Add(sectorSubjectAreaTier2Filter);

            return filters;
        }

        public IEnumerable<LarsSearchResultItemModel> GetLarsSearchResultItemModels(
            IEnumerable<LarsSearchResultItem> larsSearchResultItems)
        {
            Throw.IfNull(larsSearchResultItems, nameof(larsSearchResultItems));

            var items = new List<LarsSearchResultItemModel>();

            foreach (var item in larsSearchResultItems)
            {
                items.Add(new LarsSearchResultItemModel(
                    item.SearchScore,
                    item.LearnAimRef,
                    item.LearnAimRefTitle,
                    item.NotionalNVQLevelv2,
                    item.AwardOrgCode,
                    item.LearnDirectClassSystemCode1,
                    item.LearnDirectClassSystemCode2,
                    item.SectorSubjectAreaTier1,
                    item.SectorSubjectAreaTier2,
                    item.GuidedLearningHours,
                    item.TotalQualificationTime,
                    item.UnitType,
                    item.AwardOrgName));
            }

            return items;
        }

        internal static StringBuilder BuildUpFilterStringBuilder(
            StringBuilder stringBuilder,
            string fieldName,
            string[] filters)
        {
            Throw.IfNull(stringBuilder, nameof(stringBuilder));
            Throw.IfNullOrWhiteSpace(fieldName, nameof(fieldName));
            Throw.IfNullOrEmpty(filters, nameof(filters));

            if (stringBuilder.Length > 0 && filters.Length > 0)
            {
                new LarsSearchFilterBuilder(stringBuilder).And();
            }

            for (var i = 0; i < filters.Length; i++)
            {
                if (i == 0 && filters.Length > 1)
                {
                    new LarsSearchFilterBuilder(stringBuilder)
                        .Field(fieldName)
                        .EqualTo(filters[i])
                        .Or();
                }
                else
                {
                    new LarsSearchFilterBuilder(stringBuilder)
                        .Field(fieldName)
                        .EqualTo(filters[i]);
                }
            }

            return stringBuilder;
        }

        internal static string FormatSearchTerm(string searchTerm)
        {
            Throw.IfNullOrWhiteSpace(searchTerm, nameof(searchTerm));

            return string.Join(
                "+",
                searchTerm
                    .Split(' ')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        internal static LarsSearchFilterModel GetLarsSearchFilterModel(
            string title,
            string facetName,
            Func<string, string> textStrategy,
            IEnumerable<SearchFacet> searchFacets,
            IEnumerable<string> selectedValues)
        {
            Throw.IfNullOrWhiteSpace(title, nameof(title));
            Throw.IfNullOrWhiteSpace(facetName, nameof(facetName));
            Throw.IfNull(textStrategy, nameof(textStrategy));
            Throw.IfNull(selectedValues, nameof(selectedValues));

            var items = new List<LarsSearchFilterItemModel>();
            var count = 0;

            foreach (var item in searchFacets)
            {
                items.Add(new LarsSearchFilterItemModel(
                    $"{facetName}-{count++}",
                    facetName,
                    textStrategy?.Invoke(item.Value),
                    item.Value,
                    item.Count,
                    selectedValues.Contains(item.Value)));
            }

            var model = new LarsSearchFilterModel(title, items);

            return model;
        }
    }
}
