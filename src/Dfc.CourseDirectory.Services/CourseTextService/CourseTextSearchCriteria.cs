using System;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextSearchCriteria
    {
        public string LARSRef { get; set; }
        public CourseTextSearchCriteria(string larsRef)
        {
            if (string.IsNullOrWhiteSpace(larsRef))
            {
                throw new ArgumentNullException($"{nameof(larsRef)} cannot be null or empty or whitespace.", nameof(larsRef));
            }

            LARSRef = larsRef;
        }
    }
}
