namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextSearchCriteria
    {
        public string LARSRef { get; set; }
        public CourseTextSearchCriteria(string larsRef)
        {
            Throw.IfNullOrWhiteSpace(larsRef, nameof(larsRef));
            LARSRef = larsRef;
        }
    }
}
