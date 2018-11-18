using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class LarsSearchHelper : ILarsSearchHelper
    {
        public ILarsSearchCriteria GetLarsSearchCriteria(
            LarsSearchRequestModel larsSearchRequestModel,
            int currentPageNo,
            int itemsPerPage,
            IEnumerable<LarsSearchFacet> facets)
        {
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

        internal static StringBuilder BuildUpFilterStringBuilder(
            StringBuilder sb,
            string fieldName,
            string[] filters)
        {
            if (sb.Length > 0 && filters.Length > 0)
            {
                new LarsSearchFilterBuilder(sb).And();
            }

            for (var i = 0; i < filters.Length; i++)
            {
                if (i == 0 && filters.Length > 1)
                {
                    new LarsSearchFilterBuilder(sb)
                        .Field(fieldName)
                        .EqualTo(filters[i])
                        .Or();
                }
                else
                {
                    new LarsSearchFilterBuilder(sb)
                        .Field(fieldName)
                        .EqualTo(filters[i]);
                }
            }

            return sb;
        }

        internal static string FormatSearchTerm(string searchTerm)
        {
            return string.Join(
                "+",
                searchTerm
                    .Split(' ')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
