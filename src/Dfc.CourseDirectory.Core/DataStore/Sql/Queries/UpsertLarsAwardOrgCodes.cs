using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertLarsAwardOrgCodes : ISqlQuery<None>
    {
        public IEnumerable<UpsertLarsAwardOrgCodesRecord> Records { get; set; }
    }

    public class UpsertLarsAwardOrgCodesRecord
    {
        public string AwardOrgCode { get; set; }
        public string AwardOrgUKPRN { get; set; }
        public string AwardOrgName { get; set; }
        public string AwardOrgShortName { get; set; }
        public string AwardOrgAcronym { get; set; }
        public string AwardOrgNonExtant { get; set; }
        public string AwardOrgNotes { get; set; }
        public string AwardOrgHigherEducationInstitution { get; set; }
        public string EffectiveFrom { get; set; }
        public string EffectiveTo { get; set; }
    }
}
