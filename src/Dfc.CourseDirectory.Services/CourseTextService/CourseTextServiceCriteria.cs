using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextServiceCriteria :ICourseTextSearchCriteria
    {
        public string LARSRef { get; set; }
        public CourseTextServiceCriteria(string larsRef)
        {
            Throw.IfNullOrWhiteSpace(larsRef, nameof(larsRef));
            LARSRef = larsRef;
        }
    }
}
