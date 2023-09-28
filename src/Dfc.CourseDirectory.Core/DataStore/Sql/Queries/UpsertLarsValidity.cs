using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertLarsValidity : ISqlQuery<None>
    {
        public IEnumerable<UpsertLarsValidityRecord> Records { get; set; }
    }

    public class UpsertLarsValidityRecord
    {
        public string LearnAimRef { get; set; }
        public string ValidityCategory { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string LastNewStartDate { get; set; }
        public string Created_On { get; set; }
        public string Created_By { get; set; }
        public string Modified_On { get; set; }
        public string Modified_By { get; set; }
    }
}
