using Dfc.CourseDirectory.WebV2.MultiPageTransaction;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification
{
    public class FlowModel : IMptxState
    {
        public string LarsCode { get; set; }
        public string WhoThisCourseIsFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYouWillLearn { get; set; }
        public string HowYouWillLearn { get; set; }
        public string WhatYouWillNeedToBring { get; set; }
        public string HowYouWillBeAssessed { get; set; }
        public string WhereNext { get; set; }

        public void SetCourse(string larsCode) => LarsCode = larsCode;

        public void SetCourseDescription(
          string whoThisCourseIsFor,
          string entryRequirements,
          string whatYouWillLearn,
          string howYouWillLearn,
          string whatYouWillNeedToBring,
          string howYouWillBeAssessed,
          string whereNext)
        {
            WhoThisCourseIsFor = whoThisCourseIsFor;
            EntryRequirements = entryRequirements;
            WhatYouWillLearn = whatYouWillLearn;
            HowYouWillLearn = howYouWillLearn;
            WhatYouWillNeedToBring = whatYouWillNeedToBring;
            HowYouWillBeAssessed = howYouWillBeAssessed;
            WhereNext = whereNext;
        }
    }
}
