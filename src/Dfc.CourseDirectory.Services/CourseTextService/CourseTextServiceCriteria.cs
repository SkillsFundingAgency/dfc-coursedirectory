using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextServiceCriteria :ValueObject<CourseTextServiceCriteria>, ICourseTextSearchCriteria
    {
        public string LARSRef { get; set; }
        public CourseTextServiceCriteria(string larsRef)
        {
            Throw.IfNullOrWhiteSpace(LARSRef, nameof(LARSRef));
            LARSRef = larsRef;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return LARSRef;
        }
    }
}
