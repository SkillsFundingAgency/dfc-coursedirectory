using System;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public static class Errors
    {
        public static CourseErrorComponent GetCourseErrorComponent(string errorCode) =>
            errorCode.StartsWith("COURSERUN_") ? CourseErrorComponent.CourseRun :
            errorCode.StartsWith("COURSE_") ? CourseErrorComponent.Course :
            throw new ArgumentException($"Unknown error code: '{errorCode}'.", nameof(errorCode));

        public static ApprenticeshipErrorComponent GetApprenticeshipErrorComponent(string errorCode) =>
            errorCode.StartsWith("APPRENTICESHIP_") ? ApprenticeshipErrorComponent.Apprenticeship :
            throw new ArgumentException($"Unknown error code: '{errorCode}'.", nameof(errorCode));

        public static string MapVenueErrorToFieldGroup(string errorCode)
        {
            switch (errorCode)
            {
                case "VENUE_ADDRESS_LINE1_FORMAT":
                case "VENUE_ADDRESS_LINE1_MAXLENGTH":
                case "VENUE_ADDRESS_LINE1_REQUIRED":
                case "VENUE_ADDRESS_LINE2_FORMAT":
                case "VENUE_ADDRESS_LINE2_MAXLENGTH":
                case "VENUE_COUNTY_FORMAT":
                case "VENUE_COUNTY_MAXLENGTH":
                case "VENUE_POSTCODE_FORMAT":
                case "VENUE_POSTCODE_REQUIRED":
                case "VENUE_TOWN_FORMAT":
                case "VENUE_TOWN_MAXLENGTH":
                case "VENUE_TOWN_REQUIRED":
                    return "Address";
                case "VENUE_EMAIL_FORMAT":
                    return "Email";
                case "VENUE_NAME_MAXLENGTH":
                case "VENUE_NAME_REQUIRED":
                case "VENUE_NAME_UNIQUE":
                    return "Venue name";
                case "VENUE_PROVIDER_VENUE_REF_MAXLENGTH":
                case "VENUE_PROVIDER_VENUE_REF_REQUIRED":
                case "VENUE_PROVIDER_VENUE_REF_UNIQUE":
                    return "Your venue reference";
                case "VENUE_TELEPHONE_FORMAT":
                    return "Phone";
                case "VENUE_WEBSITE_FORMAT":
                    return "Website";
            }

            throw new ArgumentException($"Unknown error code: '{errorCode}'.", nameof(errorCode));
        }

        public static string MapApprenticeshipErrorToFieldGroup(string errorCode)
        {
            switch (errorCode)
            {
                case "APPRENTICESHIP_CONTACTUS_FORMAT":
                    return "Contact Us";
                case "APPRENTICESHIP_DELIVERYMODE_MUSTBE_DAY_OR_BLOCK":
                case "APPRENTICESHIP_DELIVERYMODE_NOT_ALLOWED":
                    return "Delivery Mode";
                case "APPRENTICESHIP_EMAIL_FORMAT":
                case "APPRENTICESHIP_EMAIL_REQUIRED":
                    return "Apprenticeship Email";
                case "APPRENTICESHIP_INFORMATION_MAXLENGTH":
                case "APPRENTICESHIP_INFORMATION_REQUIRED":
                    return "Apprenticeship Information";
                case "APPRENTICESHIP_NATIONALDELIVERY_NOT_ALLOWED":
                    return "National Delivery";
                case "APPRENTICESHIP_RADIUS_NOT_ALLOWED":
                case "APPRENTICESHIP_RADIUS_REQUIRED":
                    return "Radius";
                case "APPRENTICESHIP_STANDARD_CODE_REQUIRED":
                    return "Standard Code";
                case "APPRENTICESHIP_STANDARD_VERSION_REQUIRED":
                    return "Standard Version";
                case "APPRENTICESHIP_TELEPHONE_FORMAT":
                case "APPRENTICESHIP_TELEPHONE_REQUIRED":
                    return "Telephone";
                case "APPRENTICESHIP_VENUE_NAME_INVALID":
                case "APPRENTICESHIP_VENUE_NAME_NOT_ALLOWED":
                case "APPRENTICESHIP_VENUE_NAME_NOT_ALLOWED_WITH_REF":
                case "APPRENTICESHIP_VENUE_NOT_ALLOWED":
                case "APPRENTICESHIP_VENUE_REQUIRED":
                case "APPRENTICESHIP_PROVIDER_VENUE_REF_INVALID":
                case "APPRENTICESHIP_PROVIDER_VENUE_REF_NOT_ALLOWED":
                    return "Venue";
                case "APPRENTICESHIP_WEBSITE_FORMAT":
                    return "Course description";
                case "APPRENTICESHIP_SUBREGIONS_INVALID":
                case "APPRENTICESHIP_SUBREGIONS_NOT_ALLOWED":
                case "APPRENTICESHIP_SUBREGIONS_REQUIRED":
                    return "Location";
            }

            throw new ArgumentException($"Unknown error code: '{errorCode}'.", nameof(errorCode));
        }


        public static string MapCourseErrorToFieldGroup(string errorCode)
        {
            switch (errorCode)
            {
                case "COURSE_ENTRY_REQUIREMENTS_MAXLENGTH":
                case "COURSE_HOW_YOU_WILL_BE_ASSESSED_MAXLENGTH":
                case "COURSE_HOW_YOU_WILL_LEARN_MAXLENGTH":
                    return "Course description";
                case "COURSE_LARS_QAN_INVALID":
                case "COURSE_LARS_QAN_REQUIRED":
                    return "LARS";
                case "COURSE_WHAT_YOU_WILL_LEARN_MAXLENGTH":
                case "COURSE_WHAT_CAN_DO_NEXT_MAXLENGTH":
                case "COURSE_WHAT_YOU_WILL_NEED_TO_BRING_MAXLENGTH":
                case "COURSE_WHERE_NEXT_MAXLENGTH":
                case "COURSE_WHO_THIS_COURSE_IS_FOR_MAXLENGTH":
                case "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED":
                    return "Course description";
                case "COURSERUN_ATTENDANCE_PATTERN_NOT_ALLOWED":
                case "COURSERUN_ATTENDANCE_PATTERN_REQUIRED":
                    return "Attendance pattern";
                case "COURSERUN_COST_DESCRIPTION_MAXLENGTH":
                case "COURSERUN_COST_INVALID":
                case "COURSERUN_COST_REQUIRED":
                    return "Cost";
                case "COURSERUN_COURSE_NAME_FORMAT":
                case "COURSERUN_COURSE_NAME_MAXLENGTH":
                case "COURSERUN_COURSE_NAME_REQUIRED":
                    return "Course name";
                case "COURSERUN_COURSE_WEB_PAGE_FORMAT":
                case "COURSERUN_COURSE_WEB_PAGE_MAXLENGTH":
                    return "Course webpage";
                case "COURSERUN_DELIVERY_MODE_REQUIRED":
                    return "Delivery mode";
                case "COURSERUN_DURATION_RANGE":
                case "COURSERUN_DURATION_REQUIRED":
                    return "Duration";
                case "COURSERUN_DURATION_UNIT_REQUIRED":
                    return "Duration unit";
                case "COURSERUN_FLEXIBLE_START_DATE_REQUIRED":
                    return "Start date";
                case "COURSERUN_NATIONAL_DELIVERY_NOT_ALLOWED":
                case "COURSERUN_NATIONAL_DELIVERY_REQUIRED":
                    return "Location";
                case "COURSERUN_PROVIDER_COURSE_REF_FORMAT":
                case "COURSERUN_PROVIDER_COURSE_REF_MAXLENGTH":
                    return "Your reference";
                case "COURSERUN_PROVIDER_VENUE_REF_INVALID":
                case "COURSERUN_PROVIDER_VENUE_REF_NOT_ALLOWED":
                    return "Your venue reference";
                case "COURSERUN_START_DATE_INVALID":
                case "COURSERUN_START_DATE_NOT_ALLOWED":
                case "COURSERUN_START_DATE_REQUIRED":
                    return "Start date";
                case "COURSERUN_STUDY_MODE_NOT_ALLOWED":
                case "COURSERUN_STUDY_MODE_REQUIRED":
                    return "Course hours";
                case "COURSERUN_SUBREGIONS_INVALID":
                case "COURSERUN_SUBREGIONS_NOT_ALLOWED":
                case "COURSERUN_SUBREGIONS_REQUIRED":
                    return "Location";
                case "COURSERUN_VENUE_REQUIRED":
                case "COURSERUN_VENUE_NAME_INVALID":
                case "COURSERUN_VENUE_NAME_NOT_ALLOWED":
                case "COURSERUN_VENUE_NAME_NOT_ALLOWED_WITH_REF":
                    return "Venue name";
            }

            throw new ArgumentException($"Unknown error code: '{errorCode}'.", nameof(errorCode));
        }
    }

    public enum CourseErrorComponent { Course, CourseRun }

    public enum ApprenticeshipErrorComponent { Apprenticeship }
}
