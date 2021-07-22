using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    // Return all LearningAimRefs 
    public class GetLearningDeliveries : ISqlQuery<IReadOnlyCollection<LearningDelivery>>
    {
        public IEnumerable<string> LearningAimRefs { get; set; }
    }
}
