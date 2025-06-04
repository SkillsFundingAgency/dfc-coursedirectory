using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Services;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using Dfc.CourseDirectory.WebV2.ViewModels.CopyCourse;
using FluentValidation;

namespace Dfc.CourseDirectory.WebV2.Validation
{
    public class CopyCourseRunSaveViewModelValidator : AbstractValidator<CopyCourseRunSaveViewModel>
    {
        public CopyCourseRunSaveViewModelValidator(IReadOnlyCollection<Region> allRegions, IClock clock, IWebRiskService webRiskService)
        {
            RuleFor(c => c.AttendanceMode)
                .AttendancePattern(attendancePatternWasSpecified: c => c.AttendanceMode.HasValue, getDeliveryMode: c => c.DeliveryMode);

            RuleFor(c => c.CostDecimal)
                .Cost(costWasSpecified: c => !string.IsNullOrEmpty(c.Cost), getCostDescription: c => c.CostDescription);

            RuleFor(c => c.CostDescription)
                .CostDescription();

            RuleFor(c => c.CourseName).CourseName();

            RuleFor(c => c.CourseProviderReference).ProviderCourseRef();

            RuleFor(c => c.DeliveryMode).IsInEnum();

            RuleFor(c => c.DurationLengthInt)
                .Duration();

            RuleFor(c => c.DurationUnit).IsInEnum();

            RuleFor(c => c.National)
                .NationalDelivery(getDeliveryMode: c => c.DeliveryMode);

            RuleFor(c => c.SelectedRegions)
                .SubRegions(allRegions: allRegions, subRegionsWereSpecified: c => c.SelectedRegions?.Count() > 0, getDeliveryMode: c => c.DeliveryMode, getNationalDelivery: c => c.National);

            RuleFor(c => c.StudyMode)
                .StudyMode(studyModeWasSpecified: c => c.StudyMode.HasValue, getDeliveryMode: c => c.DeliveryMode);

            RuleFor(c => c.Url).CourseWebPage(webRiskService);

            RuleFor(c => c.VenueId)
                .VenueId(getDeliveryMode: c => c.DeliveryMode);
        }
    }
}
