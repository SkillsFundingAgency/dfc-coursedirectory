using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Dfc.CourseDirectory.Core.Validation.CourseValidation
{
    public static class RuleBuilderExtensions
    {
        public static void AttendancePattern<T>(
            this IRuleBuilderInitial<T, CourseAttendancePattern?> field,
            Func<T, bool> attendancePatternWasSpecified,
            Func<T, CourseDeliveryMode?> getDeliveryMode)
        {
            field
                // Required for classroom based delivery modes
                .Must((t, v) => attendancePatternWasSpecified(t) && v.HasValue)
                    .When(t => getDeliveryMode(t) == CourseDeliveryMode.ClassroomBased, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_ATTENDANCE_PATTERN_REQUIRED")
                // Not allowed for delivery modes other than classroom based
                .Must((t, v) => !attendancePatternWasSpecified(t))
                    .When(
                        t =>
                        {
                            var deliveryMode = getDeliveryMode(t);
                            return deliveryMode.HasValue && deliveryMode != CourseDeliveryMode.ClassroomBased;
                        },
                        ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_ATTENDANCE_PATTERN_NOT_ALLOWED");
        }

        public static void Cost<T>(
            this IRuleBuilderInitial<T, decimal?> field,
            Func<T, bool> costWasSpecified,
            Func<T, string> getCostDescription)
        {
            field
                .Cascade(CascadeMode.Stop)
                // Must be a valid decimal if specified
                .Must(v => v.HasValue)
                    .When(t => costWasSpecified(t), ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_COST_INVALID")
                // Required if cost description is not specified
                .NotNull()
                    .When(t => string.IsNullOrWhiteSpace(getCostDescription(t)), ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_COST_REQUIRED");
        }

        public static void CostDescription<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.CostDescriptionMaxLength)
                    .WithMessageFromErrorCode("COURSERUN_COST_DESCRIPTION_MAXLENGTH");
        }

        public static void CourseName<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NormalizeWhitespace()
                .NotEmpty()
                    .WithMessageFromErrorCode("COURSERUN_COURSE_NAME_REQUIRED")
                .MaximumLength(Constants.CourseNameMaxLength)
                    .WithMessageFromErrorCode("COURSERUN_COURSE_NAME_MAXLENGTH")
                .Matches(@"^[a-zA-Z0-9/\n/\r/\\u/\¬\!\£\$\%\^\&\*\\é\\è\\ﬁ\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\•\·\●\\’\‘\“\”\—\-\–\‐\‐\…\:/\°\®\\â\\ç\\ñ\\ü\\ø\♦\™\\t/\s\¼\¾\½\" + "\"" + "\\\\]+$")
                    .WithMessageFromErrorCode("COURSERUN_COURSE_NAME_FORMAT");
        }

        public static void CourseWebPage<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Cascade(CascadeMode.Stop)
                .MaximumLength(Constants.CourseWebPageMaxLength)
                    .WithMessageFromErrorCode("COURSERUN_COURSE_WEB_PAGE_MAXLENGTH")
                .Apply(Rules.Website)
                    .WithMessageFromErrorCode("COURSERUN_COURSE_WEB_PAGE_FORMAT");
        }

        public static void DeliveryMode<T>(this IRuleBuilderInitial<T, CourseDeliveryMode?> field)
        {
            field
                .NotNull()
                    .WithMessageFromErrorCode("COURSERUN_DELIVERY_MODE_REQUIRED");
        }

        public static void Duration<T>(this IRuleBuilderInitial<T, int?> field)
        {
            field
                .NotNull()
                    .WithMessageFromErrorCode("COURSERUN_DURATION_REQUIRED")
                .Must(c => !c.HasValue || (c.Value < 1000 && c.Value > 0))
                    .WithMessageFromErrorCode("COURSERUN_DURATION_RANGE");
        }

        public static void DurationUnit<T>(this IRuleBuilderInitial<T, CourseDurationUnit?> field)
        {
            field
                .NotNull()
                    .WithMessageFromErrorCode("COURSERUN_DURATION_UNIT_REQUIRED");
        }

        public static void EntryRequirements<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.EntryRequirementsMaxLength)
                    .WithMessageFromErrorCode("COURSE_ENTRY_REQUIREMENTS_MAXLENGTH");
        }

        public static void FlexibleStartDate<T>(this IRuleBuilderInitial<T, bool?> field)
        {
            field
                .NotNull()
                    .WithMessageFromErrorCode("COURSERUN_FLEXIBLE_START_DATE_REQUIRED");
        }

        public static void HowYouWillBeAssessed<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.HowYouWillBeAssessedMaxLength)
                    .WithMessageFromErrorCode("COURSE_HOW_YOU_WILL_BE_ASSESSED_MAXLENGTH");
        }

        public static void HowYouWillLearn<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.HowYoullLearnMaxLength)
                    .WithMessageFromErrorCode("COURSE_HOW_YOU_WILL_LEARN_MAXLENGTH");
        }

        public static void LearnAimRef<T>(
            this IRuleBuilderInitial<T, string> field,
            IReadOnlyCollection<string> validLearnAimRefs)
        {
            field
                .NotEmpty()
                    .WithMessageFromErrorCode("COURSE_LARS_QAN_REQUIRED")
                .Must(v => string.IsNullOrWhiteSpace(v) || validLearnAimRefs.Contains(v, StringComparer.OrdinalIgnoreCase))
                    .WithMessageFromErrorCode("COURSE_LARS_QAN_INVALID");
        }

        public static void NationalDelivery<T>(this IRuleBuilderInitial<T, bool?> field, Func<T, CourseDeliveryMode?> getDeliveryMode)
        {
            field
                // Required for work based delivery mode
                .NotNull()
                    .When(t => getDeliveryMode(t) == CourseDeliveryMode.WorkBased, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_NATIONAL_DELIVERY_REQUIRED")
                // Not allowed for delivery modes other than work based
                .Null()
                    .When(
                        t =>
                        {
                            var deliveryMode = getDeliveryMode(t);
                            return deliveryMode.HasValue && deliveryMode != CourseDeliveryMode.WorkBased;
                        },
                        ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_NATIONAL_DELIVERY_NOT_ALLOWED");
        }

        public static void ProviderCourseRef<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NormalizeWhitespace()
                .MaximumLength(Constants.ProviderCourseRefMaxLength)
                    .WithMessageFromErrorCode("COURSERUN_PROVIDER_COURSE_REF_MAXLENGTH")
                .Matches(@"^[a-zA-Z0-9/\n/\r/\\u/\¬\!\£\$\%\^\&\*\\é\\è\\ﬁ\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\•\·\●\\’\‘\“\”\—\-\–\‐\‐\…\:/\°\®\\â\\ç\\ñ\\ü\\ø\♦\™\\t/\s\¼\¾\½\" + "\"" + "\\\\]+$")
                    .WithMessageFromErrorCode("COURSERUN_PROVIDER_COURSE_REF_FORMAT");
        }

        public static void ProviderVenueRef<T>(
            this IRuleBuilderInitial<T, string> field,
            Func<T, CourseDeliveryMode?> getDeliveryMode,
            Func<T, string> getVenueName,
            Guid? matchedVenueId)
        {
            field
                .Cascade(CascadeMode.Stop)
                .NormalizeWhitespace()
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMode = getDeliveryMode(obj);
                    var isSpecified = !string.IsNullOrEmpty(v);

                    // Not allowed for delivery modes other than classroom based
                    if (isSpecified && deliveryMode != CourseDeliveryMode.ClassroomBased)
                    {
                        ctx.AddFailure(CreateFailure("COURSERUN_PROVIDER_VENUE_REF_NOT_ALLOWED"));
                        return;
                    }

                    if (deliveryMode != CourseDeliveryMode.ClassroomBased)
                    {
                        return;
                    }

                    // If not specified and Venue Name isn't specified then it's required
                    if (!isSpecified && string.IsNullOrEmpty(getVenueName(obj)))
                    {
                        ctx.AddFailure(CreateFailure("COURSERUN_VENUE_REQUIRED"));
                        return;
                    }

                    // If specified then it must match a venue
                    if (isSpecified && !matchedVenueId.HasValue)
                    {
                        ctx.AddFailure(CreateFailure("COURSERUN_PROVIDER_VENUE_REF_INVALID"));
                        return;
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }

        public static void StartDate<T>(this IRuleBuilderInitial<T, DateInput> field, DateTime now, Func<T, bool?> getFlexibleStartDate)
        {
            field
                .Cascade(CascadeMode.Stop)
                // Must be valid
                .Apply(builder => Rules.Date(builder, displayName: "Start date"))
                // Required if flexible start date is false
                .NotEmpty()
                    .When(t => getFlexibleStartDate(t) == false, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_START_DATE_REQUIRED")
                // Not allowed if flexible start date is not false
                .Empty()
                    .When(t => getFlexibleStartDate(t) == true, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_START_DATE_NOT_ALLOWED")
                // Must be in the future
                .Must(v => v.Value >= now.Date)
                    .When(t => getFlexibleStartDate(t) == false, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_START_DATE_INVALID");
        }

        public static void StudyMode<T>(
            this IRuleBuilderInitial<T, CourseStudyMode?> field,
            Func<T, bool> studyModeWasSpecified,
            Func<T, CourseDeliveryMode?> getDeliveryMode)
        {
            field
                // Required for classroom based delivery modes
                .Must((t, v) => studyModeWasSpecified(t) && v.HasValue)
                    .When(t => getDeliveryMode(t) == CourseDeliveryMode.ClassroomBased, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_STUDY_MODE_REQUIRED")
                // Not allowed for delivery modes other than classroom based
                .Must((t, v) => !studyModeWasSpecified(t))
                    .When(
                        t =>
                        {
                            var deliveryMode = getDeliveryMode(t);
                            return deliveryMode.HasValue && deliveryMode != CourseDeliveryMode.ClassroomBased;
                        },
                        ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_STUDY_MODE_NOT_ALLOWED");
        }

        public static void SubRegions<T>(
            this IRuleBuilderInitial<T, IReadOnlyCollection<Region>> field,
            Func<T, bool> subRegionsWereSpecified,
            Func<T, CourseDeliveryMode?> getDeliveryMode,
            Func<T, bool?> getNationalDelivery)
        {
            field
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMode = getDeliveryMode(obj);
                    var isSpecified = subRegionsWereSpecified(obj);
                    var nationalDelivery = getNationalDelivery(obj);

                    // Not allowed when delivery mode is not work based or national is true
                    if (isSpecified && (deliveryMode != CourseDeliveryMode.WorkBased || nationalDelivery == true))
                    {
                        ctx.AddFailure(CreateFailure("COURSERUN_SUBREGIONS_NOT_ALLOWED"));
                        return;
                    }

                    if (deliveryMode != CourseDeliveryMode.WorkBased)
                    {
                        return;
                    }

                    // Required when national delivery is false and delivery mode is work based
                    if (!isSpecified && nationalDelivery == false)
                    {
                        ctx.AddFailure(CreateFailure("COURSERUN_SUBREGIONS_REQUIRED"));
                        return;
                    }

                    // All sub regions specified must be valid and there should be at least one
                    if (isSpecified && (v == null || v.Count == 0))
                    {
                        ctx.AddFailure(CreateFailure("COURSERUN_SUBREGIONS_INVALID"));
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }

        public static void VenueId<T>(
            this IRuleBuilderInitial<T, Guid?> field,
            Func<T, CourseDeliveryMode?> getDeliveryMode)
        {
            field
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMode = getDeliveryMode(obj);
                    var isSpecified = v.HasValue;

                    // Not allowed for delivery modes other than classroom based
                    if (isSpecified && deliveryMode != CourseDeliveryMode.ClassroomBased)
                    {
                        ctx.AddFailure(CreateFailure("COURSERUN_VENUE_NAME_NOT_ALLOWED"));
                        return;
                    }

                    if (deliveryMode == CourseDeliveryMode.ClassroomBased && !isSpecified)
                    {
                        ctx.AddFailure(CreateFailure("COURSERUN_VENUE_REQUIRED"));
                        return;
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }

        public static void VenueName<T>(
            this IRuleBuilderInitial<T, string> field,
            Func<T, CourseDeliveryMode?> getDeliveryMode,
            Func<T, string> getProviderVenueRef,
            Guid? matchedVenueId)
        {
            field
                .NormalizeWhitespace()
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMode = getDeliveryMode(obj);
                    var isSpecified = !string.IsNullOrEmpty(v);

                    // Not allowed for delivery modes other than classroom based
                    if (isSpecified && deliveryMode != CourseDeliveryMode.ClassroomBased)
                    {
                        ctx.AddFailure(CreateFailure("COURSERUN_VENUE_NAME_NOT_ALLOWED"));
                        return;
                    }

                    if (deliveryMode != CourseDeliveryMode.ClassroomBased)
                    {
                        return;
                    }

                    if (isSpecified && !matchedVenueId.HasValue)
                    {
                        // We don't want both a ref and a name but if the ref resolves a venue and that venue's name
                        // matches this name then we let it go. If it doesn't match then yield an error.
                        if (!string.IsNullOrEmpty(getProviderVenueRef(obj)))
                        {
                            ctx.AddFailure(CreateFailure("COURSERUN_VENUE_NAME_NOT_ALLOWED_WITH_REF"));
                            return;
                        }

                        // Couldn't find a match from name
                        ctx.AddFailure(CreateFailure("COURSERUN_VENUE_NAME_INVALID"));
                        return;
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }

        public static void WhatYouWillLearn<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.WhatYouWillLearnMaxLength)
                    .WithMessageFromErrorCode("COURSE_WHAT_YOU_WILL_LEARN_MAXLENGTH");
        }

        public static void WhatYouCanDoNextLearn<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.WhatYouCanDoNextMaxLength)
                    .WithMessageFromErrorCode("COURSE_WHAT_YOU_CAN_DO_NEXT_MAXLENGTH");
        }

        public static void WhatYouWillNeedToBring<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.WhatYouWillNeedToBringMaxLength)
                    .WithMessageFromErrorCode("COURSE_WHAT_YOU_WILL_NEED_TO_BRING_MAXLENGTH");
        }

        public static void WhereNext<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.WhereNextMaxLength)
                    .WithMessageFromErrorCode("COURSE_WHERE_NEXT_MAXLENGTH");
        }

        public static void WhoThisCourseIsFor<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NotEmpty()
                    .WithMessageFromErrorCode("COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED")
                .MaximumLength(Constants.WhoThisCourseIsForMaxLength)
                    .WithMessageFromErrorCode("COURSE_WHO_THIS_COURSE_IS_FOR_MAXLENGTH");
        }
    }
}
