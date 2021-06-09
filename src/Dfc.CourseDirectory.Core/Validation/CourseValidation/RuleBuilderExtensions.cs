using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation.CourseValidation
{
    public static class RuleBuilderExtensions
    {
        private static readonly string[] _dateFormats = new[] { "dd/MM/yyyy" };

        public static void AttendancePattern<T>(this IRuleBuilderInitial<T, string> field, Func<T, string> getDeliveryMode)
        {
            field
                .Transform(CsvCourseRow.ResolveAttendancePattern)
                .AttendancePattern(t => CsvCourseRow.ResolveDeliveryMode(getDeliveryMode(t)));
        }

        public static void AttendancePattern<T>(this IRuleBuilderInitial<T, CourseAttendancePattern?> field, Func<T, CourseDeliveryMode?> getDeliveryMode)
        {
            field
                // Required for classroom based delivery modes
                .NotNull()
                    .When(t => getDeliveryMode(t) == CourseDeliveryMode.ClassroomBased, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_ATTENDANCE_PATTERN_REQUIRED")
                // Not allowed for delivery modes other than classroom based
                .Null()
                    .When(
                        t =>
                        {
                            var deliveryMode = getDeliveryMode(t);
                            return deliveryMode.HasValue && deliveryMode != CourseDeliveryMode.ClassroomBased;
                        },
                        ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_ATTENDANCE_PATTERN_NOT_ALLOWED");
        }

        public static void Cost<T>(this IRuleBuilderInitial<T, string> field, Func<T, string> getCostDescription)
        {
            field
                .Transform(ResolveCost)
                .Cost(getCostDescription);
        }

        public static void Cost<T>(this IRuleBuilderInitial<T, decimal?> field, Func<T, string> getCostDescription)
        {
            field
                // Required if cost description is not specified
                .NotNull()
                    .When(t => string.IsNullOrWhiteSpace(getCostDescription(t)), ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_COST_REQUIRED")
                // Not allowed if cost description is specified
                .Null()
                    .When(t => !string.IsNullOrWhiteSpace(getCostDescription(t)), ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_COST_NOT_ALLOWED");
        }

        public static void CostDescription<T>(this IRuleBuilderInitial<T, string> field, Func<T, string> getCost)
        {
            field
                .CostDescription(t => ResolveCost(getCost(t)));
        }

        public static void CostDescription<T>(this IRuleBuilderInitial<T, string> field, Func<T, decimal?> getCost)
        {
            field
                .MaximumLength(Constants.CostDescriptionMaxLength)
                    .When(t => !getCost(t).HasValue, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_COST_DESCRIPTION_MAXLENGTH")
                // Not allowed if cost is specified
                .Must(v => string.IsNullOrWhiteSpace(v))
                    .When(t => getCost(t).HasValue, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_COST_DESCRIPTION_NOT_ALLOWED");
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
                .Apply(Rules.Website)
                    .WithMessageFromErrorCode("COURSERUN_COURSE_WEB_PAGE_FORMAT");
        }

        public static void DeliveryMode<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Transform(CsvCourseRow.ResolveDeliveryMode)
                .DeliveryMode();
        }

        public static void DeliveryMode<T>(this IRuleBuilderInitial<T, CourseDeliveryMode?> field)
        {
            field
                .NotNull()
                    .WithMessageFromErrorCode("COURSERUN_DELIVERY_MODE_REQUIRED");
        }

        public static void Duration<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Transform(c => int.TryParse(c ?? string.Empty, out var value) ? value : (int?)null)
                .Duration();
        }

        public static void Duration<T>(this IRuleBuilderInitial<T, int?> field)
        {
            field
                .NotNull()
                    .WithMessageFromErrorCode("COURSERUN_DURATION_REQUIRED")
                .Must(c => !c.HasValue || (c.Value < 1000 && c.Value > 0))
                    .WithMessageFromErrorCode("COURSERUN_DURATION_RANGE");
        }

        public static void DurationUnit<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Transform(CsvCourseRow.ResolveDurationUnit)
                .DurationUnit();
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

        public static void FlexibleStartDate<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Transform(CsvCourseRow.ResolveFlexibleStartDate)
                .FlexibleStartDate();
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

        public static void LarsQan<T>(
            this IRuleBuilderInitial<T, string> field,
            IReadOnlyCollection<string> validLearningAimRefs)
        {
            field
                .NotEmpty()
                    .WithMessageFromErrorCode("COURSE_LARS_QAN_REQUIRED")
                .Must(v => string.IsNullOrWhiteSpace(v) || validLearningAimRefs.Contains(v, StringComparer.OrdinalIgnoreCase))
                    .WithMessageFromErrorCode("COURSE_LARS_QAN_INVALID");
        }

        public static void NationalDelivery<T>(this IRuleBuilderInitial<T, string> field, Func<T, string> getDeliveryMode)
        {
            field
                .Transform(CsvCourseRow.ResolveNationalDelivery)
                .NationalDelivery(t => CsvCourseRow.ResolveDeliveryMode(getDeliveryMode(t)));
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
            Func<T, string> getDeliveryMode,
            Func<T, string> getVenueName,
            IReadOnlyCollection<Venue> providerVenues)
        {
            field
                .ProviderVenueRef(getDeliveryMode: t => CsvCourseRow.ResolveDeliveryMode(getDeliveryMode(t)), getVenueName, providerVenues);
        }

        public static void ProviderVenueRef<T>(
            this IRuleBuilderInitial<T, string> field,
            Func<T, CourseDeliveryMode?> getDeliveryMode,
            Func<T, string> getVenueName,
            IReadOnlyCollection<Venue> providerVenues)
        {
            field
                .NormalizeWhitespace()
                // Must match a venue for the provider
                // N.B. Using Count() == 1 here instead of Any() or SingleOrDefault() because we have some duplicates in bad data;
                // better to fail early here than later on during the publish process
                .Must(
                    venueRef => string.IsNullOrWhiteSpace(venueRef) ||
                        providerVenues.Count(v => v.ProviderVenueRef?.Equals(venueRef, StringComparison.OrdinalIgnoreCase) == true) == 1)
                    .When(t => getDeliveryMode(t) == CourseDeliveryMode.ClassroomBased, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_PROVIDER_VENUE_REF_INVALID")
                // Required for classroom based delivery modes if venue name is not specified
                .NotNull()
                    .When(
                        t =>
                        {
                            var deliveryMode = getDeliveryMode(t);
                            return deliveryMode.HasValue && deliveryMode == CourseDeliveryMode.ClassroomBased &&
                                string.IsNullOrWhiteSpace(getVenueName(t));
                        },
                        ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_VENUE_REQUIRED")
                // Not allowed for delivery modes other than classroom based
                .Null()
                    .When(
                        t =>
                        {
                            var deliveryMode = getDeliveryMode(t);
                            return deliveryMode.HasValue && deliveryMode != CourseDeliveryMode.ClassroomBased;
                        },
                        ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_PROVIDER_VENUE_REF_NOT_ALLOWED");
        }

        public static void StartDate<T>(this IRuleBuilderInitial<T, string> field, Func<T, string> getFlexibleStartDate)
        {
            field
                .Transform(c => DateTime.TryParseExact(c, _dateFormats, provider: null, DateTimeStyles.None, out var dt) ? dt : (DateTime?)null)
                .StartDate(t => CsvCourseRow.ResolveFlexibleStartDate(getFlexibleStartDate(t)));
        }

        public static void StartDate<T>(this IRuleBuilderInitial<T, DateTime?> field, Func<T, bool?> getFlexibleStartDate)
        {
            field
                // Required if flexible start date is false
                .NotEmpty()
                    .When(t => getFlexibleStartDate(t) == false, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_START_DATE_REQUIRED")
                // Not allowed if flexible start date is not false
                .Empty()
                    .When(t => getFlexibleStartDate(t) == true, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_START_DATE_NOT_ALLOWED");
        }

        public static void StudyMode<T>(this IRuleBuilderInitial<T, string> field, Func<T, string> getDeliveryMode)
        {
            field
                .Transform(CsvCourseRow.ResolveStudyMode)
                .StudyMode(t => CsvCourseRow.ResolveDeliveryMode(getDeliveryMode(t)));
        }

        public static void StudyMode<T>(this IRuleBuilderInitial<T, CourseStudyMode?> field, Func<T, CourseDeliveryMode?> getDeliveryMode)
        {
            field
                // Required for classroom based delivery modes
                .NotNull()
                    .When(t => getDeliveryMode(t) == CourseDeliveryMode.ClassroomBased, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_STUDY_MODE_REQUIRED")
                // Not allowed for delivery modes other than classroom based
                .Null()
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
            this IRuleBuilderInitial<T, string> field,
            Func<T, string> getNationalDelivery,
            IReadOnlyCollection<Region> allRegions)
        {
            field
                .NormalizeWhitespace()
                // Required when national delivery is false
                .NotEmpty()
                    .When(t => CsvCourseRow.ResolveNationalDelivery(getNationalDelivery(t)) == false, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_SUBREGIONS_REQUIRED")
                // All specified regions must be valid and there should be at least 1
                .Must(
                    sr =>
                    {
                        if (string.IsNullOrWhiteSpace(sr))
                        {
                            return true;
                        }

                        var allSubRegions = allRegions.SelectMany(r => r.SubRegions).ToLookup(sr => sr.Name, StringComparer.OrdinalIgnoreCase);

                        var values = sr
                            .Split(CsvCourseRow.SubRegionDelimiter, StringSplitOptions.RemoveEmptyEntries)
                            .Select(v => v.Trim())
                            .ToArray();

                        return values.All(v => allSubRegions.Contains(v)) && values.Length > 0;
                    })
                    .WithMessageFromErrorCode("COURSERUN_SUBREGIONS_INVALID")
                .Empty()
                    .When(t => CsvCourseRow.ResolveNationalDelivery(getNationalDelivery(t)) == true, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_SUBREGIONS_NOT_ALLOWED");
        }

        public static void VenueName<T>(
            this IRuleBuilderInitial<T, string> field,
            Func<T, string> getDeliveryMode,
            Func<T, string> getProviderVenueRef,
            IReadOnlyCollection<Venue> providerVenues)
        {
            field
                .VenueName(t => CsvCourseRow.ResolveDeliveryMode(getDeliveryMode(t)), getProviderVenueRef, providerVenues);
        }

        public static void VenueName<T>(
            this IRuleBuilderInitial<T, string> field,
            Func<T, CourseDeliveryMode?> getDeliveryMode,
            Func<T, string> getProviderVenueRef,
            IReadOnlyCollection<Venue> providerVenues)
        {
            field
                .NormalizeWhitespace()
                // Must match a venue for the provider
                // N.B. Using Count() == 1 here instead of Any() or SingleOrDefault() because we have some duplicates in bad data;
                // better to fail early here than later on during the publish process
                .Must(
                    venueName => string.IsNullOrWhiteSpace(venueName) ||
                        providerVenues.Count(v => v.VenueName.Equals(venueName, StringComparison.OrdinalIgnoreCase)) == 1)
                    .When(t => getDeliveryMode(t) == CourseDeliveryMode.ClassroomBased, ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_VENUE_NAME_INVALID")
                // Not allowed for delivery modes other than classroom based
                .Null()
                    .When(
                        t =>
                        {
                            var deliveryMode = getDeliveryMode(t);
                            return deliveryMode.HasValue && deliveryMode != CourseDeliveryMode.ClassroomBased;
                        },
                        ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_VENUE_NAME_NOT_ALLOWED")
                // Not allowed if provider venue ref is specified
                .Null()
                    .When(t => !string.IsNullOrWhiteSpace(getProviderVenueRef(t)), ApplyConditionTo.CurrentValidator)
                    .WithMessageFromErrorCode("COURSERUN_VENUE_NAME_NOT_ALLOWED_WITH_REF");
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

        private static int GetDecimalPlaces(decimal n)
        {
            n = Math.Abs(n);
            n -= (int)n;

            var decimalPlaces = 0;
            while (n > 0)
            {
                decimalPlaces++;
                n *= 10;
                n -= (int)n;
            }

            return decimalPlaces;
        }

        private static decimal? ResolveCost(string value) =>
            decimal.TryParse(value, out var result) && GetDecimalPlaces(result) <= 2 ? result : (decimal?)null;
    }
}
