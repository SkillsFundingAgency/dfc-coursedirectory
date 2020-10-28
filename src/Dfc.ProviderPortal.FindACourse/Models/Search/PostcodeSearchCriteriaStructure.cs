
using System;
using Dfc.ProviderPortal.Packages;


namespace Dfc.ProviderPortal.FindACourse.Models
{
    public class PostcodeSearchCriteriaStructure
    {
        //public string APIKeyField { get; set; }
        public string Keyword { get; set; }
        public int? TopResults { get; set; }

        public PostcodeSearchCriteriaStructure()
        {
            //if (TopResults.HasValue)
            //    Throw.IfLessThan(1, TopResults.Value, "");
        }
    }
}
