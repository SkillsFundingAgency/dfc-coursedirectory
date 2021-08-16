using System;
using System.Collections.Generic;
using System.Linq;
using CourseConstants = Dfc.CourseDirectory.Core.Validation.CourseValidation.Constants;
using VenueConstants = Dfc.CourseDirectory.Core.Validation.VenueValidation.Constants;
using ApprenticeshipConstants = Dfc.CourseDirectory.Core.Validation.ApprenticeshipValidation.Constants;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class ErrorRegistry
    {
        public static IReadOnlyDictionary<string, Error> All { get; } = new[]
        {
            new Error("COURSE_ENTRY_REQUIREMENTS_MAXLENGTH", CourseConstants.EntryRequirementsMaxLength),
            new Error("COURSE_HOW_YOU_WILL_BE_ASSESSED_MAXLENGTH", CourseConstants.HowYouWillBeAssessedMaxLength),
            new Error("COURSE_HOW_YOU_WILL_LEARN_MAXLENGTH", CourseConstants.HowYoullLearnMaxLength),
            new Error("COURSE_LARS_QAN_INVALID"),
            new Error("COURSE_LARS_QAN_REQUIRED"),
            new Error("COURSE_WHAT_YOU_WILL_LEARN_MAXLENGTH", CourseConstants.WhatYouWillLearnMaxLength),
            new Error("COURSE_WHAT_CAN_DO_NEXT_MAXLENGTH", CourseConstants.WhatYouCanDoNextMaxLength),
            new Error("COURSE_WHAT_YOU_WILL_NEED_TO_BRING_MAXLENGTH", CourseConstants.WhatYouWillNeedToBringMaxLength),
            new Error("COURSE_WHERE_NEXT_MAXLENGTH", CourseConstants.WhereNextMaxLength),
            new Error("COURSE_WHO_THIS_COURSE_IS_FOR_MAXLENGTH", CourseConstants.WhoThisCourseIsForMaxLength),
            new Error("COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED"),
            new Error("COURSERUN_ATTENDANCE_PATTERN_NOT_ALLOWED"),
            new Error("COURSERUN_ATTENDANCE_PATTERN_REQUIRED"),
            new Error("COURSERUN_COST_DESCRIPTION_MAXLENGTH", CourseConstants.CostDescriptionMaxLength),
            new Error("COURSERUN_COST_INVALID"),
            new Error("COURSERUN_COST_REQUIRED"),
            new Error("COURSERUN_COURSE_NAME_FORMAT"),
            new Error("COURSERUN_COURSE_NAME_MAXLENGTH", CourseConstants.CourseNameMaxLength),
            new Error("COURSERUN_COURSE_NAME_REQUIRED"),
            new Error("COURSERUN_COURSE_WEB_PAGE_FORMAT"),
            new Error("COURSERUN_COURSE_WEB_PAGE_MAXLENGTH", CourseConstants.CourseWebPageMaxLength),
            new Error("COURSERUN_DELIVERY_MODE_REQUIRED"),
            new Error("COURSERUN_DURATION_RANGE"),
            new Error("COURSERUN_DURATION_REQUIRED"),
            new Error("COURSERUN_DURATION_UNIT_REQUIRED"),
            new Error("COURSERUN_FLEXIBLE_START_DATE_REQUIRED"),
            new Error("COURSERUN_NATIONAL_DELIVERY_NOT_ALLOWED"),
            new Error("COURSERUN_NATIONAL_DELIVERY_REQUIRED"),
            new Error("COURSERUN_PROVIDER_COURSE_REF_FORMAT"),
            new Error("COURSERUN_PROVIDER_COURSE_REF_MAXLENGTH", CourseConstants.ProviderCourseRefMaxLength),
            new Error("COURSERUN_PROVIDER_VENUE_REF_INVALID"),
            new Error("COURSERUN_PROVIDER_VENUE_REF_NOT_ALLOWED"),
            new Error("COURSERUN_START_DATE_INVALID"),
            new Error("COURSERUN_START_DATE_NOT_ALLOWED"),
            new Error("COURSERUN_START_DATE_REQUIRED"),
            new Error("COURSERUN_STUDY_MODE_NOT_ALLOWED"),
            new Error("COURSERUN_STUDY_MODE_REQUIRED"),
            new Error("COURSERUN_SUBREGIONS_INVALID"),
            new Error("COURSERUN_SUBREGIONS_NOT_ALLOWED"),
            new Error("COURSERUN_SUBREGIONS_REQUIRED"),
            new Error("COURSERUN_VENUE_REQUIRED"),
            new Error("COURSERUN_VENUE_NAME_INVALID"),
            new Error("COURSERUN_VENUE_NAME_NOT_ALLOWED"),
            new Error("COURSERUN_VENUE_NAME_NOT_ALLOWED_WITH_REF"),
            new Error("VENUE_ADDRESS_LINE1_FORMAT"),
            new Error("VENUE_ADDRESS_LINE1_MAXLENGTH", VenueConstants.AddressLine1MaxLength),
            new Error("VENUE_ADDRESS_LINE1_REQUIRED"),
            new Error("VENUE_ADDRESS_LINE2_FORMAT"),
            new Error("VENUE_ADDRESS_LINE2_MAXLENGTH", VenueConstants.AddressLine2MaxLength),
            new Error("VENUE_COUNTY_FORMAT"),
            new Error("VENUE_COUNTY_MAXLENGTH", VenueConstants.CountyMaxLength),
            new Error("VENUE_EMAIL_FORMAT"),
            new Error("VENUE_NAME_MAXLENGTH", VenueConstants.NameMaxLength),
            new Error("VENUE_NAME_REQUIRED"),
            new Error("VENUE_NAME_UNIQUE"),
            new Error("VENUE_POSTCODE_FORMAT"),
            new Error("VENUE_POSTCODE_REQUIRED"),
            new Error("VENUE_PROVIDER_VENUE_REF_MAXLENGTH", VenueConstants.ProviderVenueRefMaxLength),
            new Error("VENUE_PROVIDER_VENUE_REF_REQUIRED"),
            new Error("VENUE_PROVIDER_VENUE_REF_UNIQUE"),
            new Error("VENUE_TELEPHONE_FORMAT"),
            new Error("VENUE_TOWN_FORMAT"),
            new Error("VENUE_TOWN_MAXLENGTH", VenueConstants.TownMaxLength),
            new Error("VENUE_TOWN_REQUIRED"),
            new Error("VENUE_WEBSITE_FORMAT"),
            new Error("APPRENTICESHIP_DELIVERY_MODE_REQUIRED"),
            new Error("APPRENTICESHIP_DELIVERY_METHOD_REQUIRED"),
            new Error("APPRENTICESHIP_YOUR_VENUE_REFERENCE_MAXLENGTH", ApprenticeshipConstants.YourVenueReferenceMaxLength),
            new Error("APPRENTICESHIP_DELIVERYMODE_MUSTBE_DAY_OR_BLOCK"),
            new Error("APPRENTICESHIP_DELIVERYMODE_NOT_ALLOWED"),
            new Error("APPRENTICESHIP_RADIUS_REQUIRED"),
            new Error("APPRENTICESHIP_RADIUS_NOT_ALLOWED"),
            new Error("APPRENTICESHIP_VENUE_NOT_ALLOWED"),
            new Error("APPRENTICESHIP_SUBREGIONS_INVALID"),
            new Error("APPRENTICESHIP_SUBREGIONS_NOT_ALLOWED"),
            new Error("APPRENTICESHIP_NATIONALDELIVERY_NOT_ALLOWED")
        }.ToDictionary(e => e.ErrorCode, e => e);
    }

    public class Error
    {
        public Error(string errorCode, params object[] formatArgs)
        {
            ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
            FormatArgs = formatArgs;
        }

        public string ErrorCode { get; }
        public object[] FormatArgs { get; }

        public static implicit operator string(Error error) => error.GetMessage();

        public string GetMessage(ErrorMessageContext context = ErrorMessageContext.Default)
        {
            return TryGetFormattedMessage($"ERROR_{ErrorCode}", out var message) ?
                message :
                throw new InvalidOperationException($"Cannot find error message for error code: {ErrorCode}.");

            bool TryGetFormattedMessage(string name, out string message)
            {
                var result = Content.ResourceManager.GetString(name);

                if (result == null)
                {
                    message = default;
                    return false;
                }

                message = string.Format(result, FormatArgs);
                return true;
            }
        }

        public override string ToString() => GetMessage();
    }

    public enum ErrorMessageContext
    {
        Default = 0
    }
}
