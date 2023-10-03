using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class Validity
    {
        public string LearnAimRef { get; set; }
        public string ValidityCategory { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string LastNewStartDate { get; set; }
    }
}
