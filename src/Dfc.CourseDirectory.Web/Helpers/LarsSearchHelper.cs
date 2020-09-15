
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.UnregulatedProvision;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;


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
            sb = BuildUpFilterStringBuilder(sb, "AwardOrgAimRef", larsSearchRequestModel.AwardOrgAimRefFilter);


            if (sb.Length != 0) {
                StringBuilder sbTemp = new StringBuilder();
                new LarsSearchFilterBuilder(sbTemp).AppendOpeningBracket().Field("CertificationEndDate")
                    .GreaterThanOrEqualTo(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd")).Or()
                    .Field("CertificationEndDate eq null").AppendClosingBracket()
                    .And().AppendOpeningBracket();
                sbTemp.Append(sb);
                new LarsSearchFilterBuilder(sbTemp).AppendClosingBracket();
                sb = sbTemp;
            }
            else {
                new LarsSearchFilterBuilder(sb).Field("CertificationEndDate")
                    .GreaterThanOrEqualTo(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd")).Or()
                    .Field("CertificationEndDate eq null");
            }

            var skip = currentPageNo == 1 ? 0 : itemsPerPage * (currentPageNo - 1);

            var criteria = new LarsSearchCriteria(
                FormatSearchTerm(larsSearchRequestModel.SearchTerm),
                itemsPerPage,
                skip,
                new LarsSearchFilterBuilder(sb).Build(),
                facets);

            return criteria;
        }


        public IZCodeSearchCriteria GetZCodeSearchCriteria(
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
            sb = BuildUpFilterStringBuilder(sb, "AwardOrgAimRef", larsSearchRequestModel.AwardOrgAimRefFilter);
            sb = BuildUpFilterStringBuilder(sb, "SectorSubjectAreaTier1", larsSearchRequestModel.SectorSubjectAreaTier1Filter);
            sb = BuildUpFilterStringBuilder(sb, "SectorSubjectAreaTier2", larsSearchRequestModel.SectorSubjectAreaTier2Filter);


            if (sb.Length != 0) {
                new LarsSearchFilterBuilder(sb).And().AppendOpeningBracket().Field("CertificationEndDate")
                    .GreaterThanOrEqualTo(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd")).Or()
                    .Field("CertificationEndDate eq null").AppendClosingBracket();
            } else {
                new LarsSearchFilterBuilder(sb).Field("CertificationEndDate")
                    .GreaterThanOrEqualTo(DateTimeOffset.UtcNow.ToString("yyyy-MM-dd")).Or()
                    .Field("CertificationEndDate eq null");
            }

            var skip = currentPageNo == 1 ? 0 : itemsPerPage * (currentPageNo - 1);

            var criteria = new ZCodeSearchCriteria(
                "Z*",
                "LearnAimRef",
               itemsPerPage,
               skip,
               new LarsSearchFilterBuilder(sb).Build(),
               facets);

            return criteria;
        }


        public IEnumerable<LarsSearchFilterModel> GetUnRegulatedSearchFilterModels(
            LarsSearchFacets larsSearchFacets,
            LarsSearchRequestModel larsSearchRequestModel)
        {
            Throw.IfNull(larsSearchFacets, nameof(larsSearchFacets));
            Throw.IfNull(larsSearchRequestModel, nameof(larsSearchRequestModel));

            var filters = new List<LarsSearchFilterModel>();

            var notionalNVQLevelv2Filter = GetLarsSearchFilterModel(
                "Level",
                "NotionalNVQLevelv2Filter",
                (value) => $"Level {value}",
                larsSearchFacets.NotionalNVQLevelv2,
                larsSearchRequestModel.NotionalNVQLevelv2Filter);

            var awardOrgAimRefFilter = GetLarsSearchFilterModel(
                "Category",
                "AwardOrgAimRefFilter",
                (value) => value,
                larsSearchFacets.AwardOrgAimRef,
                larsSearchRequestModel.AwardOrgAimRefFilter);

            filters.Add(notionalNVQLevelv2Filter);

            var r = awardOrgAimRefFilter.Items.ToList();

            foreach (var award in awardOrgAimRefFilter.Items)
            {
                if (award.Value == "APP H CAT C" || award.Value == "APP H CAT D" || award.Value == "APP H CAT H" || award.Value == "APP H CAT N") {
                    r.Remove(award);
                } else {
                    award.Text = Categories.AllCategories.Where(x => x.Id == award.Text).Select(x => x.Category).SingleOrDefault();
                }
            }

            awardOrgAimRefFilter.Items = r;
            filters.Add(awardOrgAimRefFilter);
            return filters;
        }


        public IEnumerable<LarsSearchFilterModel> GetLarsSearchFilterModels(
            LarsSearchFacets larsSearchFacets,
            LarsSearchRequestModel larsSearchRequestModel)
        {
            Throw.IfNull(larsSearchFacets, nameof(larsSearchFacets));
            Throw.IfNull(larsSearchRequestModel, nameof(larsSearchRequestModel));

            var filters = new List<LarsSearchFilterModel>();

            var notionalNVQLevelv2Filter = GetLarsSearchFilterModel(
                "Qualification level",
                "NotionalNVQLevelv2Filter",
                 //(value) => $"Level {value}",
                 (value) => value,
                larsSearchFacets.NotionalNVQLevelv2,
                larsSearchRequestModel.NotionalNVQLevelv2Filter);

            var awardOrgCodeFilter = GetLarsSearchFilterModel(
                "Awarding organisation",
                "AwardOrgCodeFilter",
                (value) => value,
                larsSearchFacets.AwardOrgCode,
                larsSearchRequestModel.AwardOrgCodeFilter);

            filters.Add(notionalNVQLevelv2Filter);
            filters.Add(awardOrgCodeFilter);
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
                    item.GuidedLearningHours,
                    item.TotalQualificationTime,
                    item.UnitType,
                    item.AwardOrgName,
                    item.LearnAimRefTypeDesc,
                    item.CertificationEndDate));
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
            Throw.IfNull(filters, nameof(filters));
            bool openingBracketAppended = false;
            if (stringBuilder.Length > 0 && filters.Length > 0)
            {
                new LarsSearchFilterBuilder(stringBuilder).And()
                                                          .AppendOpeningBracket();
                openingBracketAppended = true;
            }

            for (var i = 0; i < filters.Length; i++)
            {
                if (i == 0)
                {
                    if (openingBracketAppended)
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
                            .EqualTo(filters[i])
                            .Or().PrependOpeningBracket();
                    }
                }
                if (filters.Length - 1 > i)
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
                        .EqualTo(filters[i]).AppendClosingBracket();
                }
            }

            return stringBuilder;
        }

        internal static string FormatSearchTerm(string searchTerm)
        {
            Throw.IfNullOrWhiteSpace(searchTerm, nameof(searchTerm));

            var segments = new string(searchTerm.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray())
                .Split(" ")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => $"{s.Trim()}*");

            return string.Join("+", segments);
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

            var textValue = string.Empty;

            foreach (var item in searchFacets.OrderBy(x=>x.Value))
            {
                textValue = string.Empty;

                if (title.ToLower().Contains("level"))
                {
                    switch (item.Value.ToLower())
                    {
                        case "e":
                            textValue = "Entry level";
                            break;
                        case "x":
                            textValue = "Unknown or not applicable";
                            break;
                        case "h":
                            textValue = "Higher";
                            break;
                        case "m":
                            textValue = "Mixed";
                            break;
                        default:
                            textValue = "Level " + item.Value;
                            break;

                    }
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(item.Value))
                    {

                        //If this occurs, filter is blank, and should be skipped
                        continue;
                    }
                    else
                    {
                        textValue = item.Value;
                    }
                    

                }

                string value = string.IsNullOrWhiteSpace(item.Value) ? "(blank)" : item.Value;
                items.Add(new LarsSearchFilterItemModel(
                    $"{facetName}-{count++}",
                    facetName,
                   textValue,
                    value,
                    item.Count,
                    selectedValues.Contains(value)));
            }

            var entryItem = items.Where(x => x.Text.ToLower().Contains("entry")).SingleOrDefault();

            if (entryItem != null)
            {
                items.Remove(entryItem);
                items.Insert(0, entryItem);
            }

            var model = new LarsSearchFilterModel(title, items);

            return model;
        }
    }
}
