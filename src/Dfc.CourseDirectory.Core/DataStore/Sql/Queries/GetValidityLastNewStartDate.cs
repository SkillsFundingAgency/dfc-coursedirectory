using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetValidityLastNewStartDate : ISqlQuery<List<DateTime?>>
    {
        public string LearnAimRef { get; set; }
    }
}
