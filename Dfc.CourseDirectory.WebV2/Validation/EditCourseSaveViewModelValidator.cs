using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using Dfc.CourseDirectory.WebV2.ViewModels.EditCourse;
using FluentValidation;

namespace Dfc.CourseDirectory.WebV2.Validation
{
    public class EditCourseSaveViewModelValidator : AbstractValidator<EditCourseSaveViewModel>
    {
        public EditCourseSaveViewModelValidator()
        {
            RuleFor(c => c.CourseFor).WhoThisCourseIsFor();
            RuleFor(c => c.EntryRequirements).EntryRequirements();
            RuleFor(c => c.WhatWillLearn).WhatYouWillLearn();
            RuleFor(c => c.HowYouWillLearn).HowYouWillLearn();
            RuleFor(c => c.WhatYouNeed).WhatYouWillNeedToBring();
            RuleFor(c => c.HowAssessed).HowYouWillBeAssessed();
            RuleFor(c => c.WhereNext).WhereNext();
        }
    }
}
