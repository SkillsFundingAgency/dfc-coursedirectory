using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    // Return all LearningAimRefs 
    public class GetLearningAimRefAndEffectiveTo : ISqlQuery<Lars>
    {
        public Lars Lars { get; set; }

        public string LearningAimRef { get; set; }
    }
}
