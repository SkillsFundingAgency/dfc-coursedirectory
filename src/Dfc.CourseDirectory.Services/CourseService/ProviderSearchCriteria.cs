
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using System.Collections.Generic;


namespace Dfc.CourseDirectory.Services.CourseService
{
    // TODO - Provider search is in the course service for now, needs moving!
    public class ProviderSearchCriteria : ValueObject<ProviderSearchCriteria>, IProviderSearchCriteria
    {
        //public string APIKeyField { get; set; }
        public string Keyword { get; set; }
        public string[] Town { get; set; }
        public string[] Region { get; set; }

        public int? TopResults { get; set; }

        public ProviderSearchCriteria()
        {
            Keyword = "";
            Town = new string[] { };
            Region = new string[] { };
            TopResults = null;
        }

        public ProviderSearchCriteria(string keyword, string[] town, string[] region, int? topResults)
        {
            Throw.IfNull(keyword, nameof(keyword));
            Throw.IfNull(town, nameof(town));
            Throw.IfNull(region, nameof(region));
            Keyword = keyword;
            Town = town;
            Region = region;
            TopResults = topResults;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Keyword;
            yield return Town;
            yield return Region;
            yield return TopResults;
        }
    }
}
