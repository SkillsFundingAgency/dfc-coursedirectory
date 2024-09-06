using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using Dfc.CourseDirectory.Web.ViewModels.EditCourse;
using FluentValidation;

namespace Dfc.CourseDirectory.Web.Validation
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
