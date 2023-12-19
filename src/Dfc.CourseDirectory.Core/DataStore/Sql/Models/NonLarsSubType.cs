using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class NonLarsSubType
    {
        public Guid NonLarsSubTypeId { get; set; }

        public string Name { get; set; }

        public DateTime AddedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public int IsActive { get; set; }
    }
}
