using System;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public static class Errors
    {
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
                    return "Telephone";
                case "VENUE_WEBSITE_FORMAT":
                    return "Website";
            }

            throw new ArgumentException($"Unknown error code: '{errorCode}'.", nameof(errorCode));
        }
    }
}
