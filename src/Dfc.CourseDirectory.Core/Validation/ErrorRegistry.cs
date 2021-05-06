using System;
using System.Collections.Generic;
using System.Linq;
using VenueConstants = Dfc.CourseDirectory.Core.Validation.VenueValidation.Constants;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class ErrorRegistry
    {
        public static IReadOnlyDictionary<string, Error> All { get; } = new[]
        {
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
            new Error("VENUE_WEBSITE_FORMAT")
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
            if (context == ErrorMessageContext.DataManagement &&
                TryGetFormattedMessage($"ERROR_DM_{ErrorCode}", out var message))
            {
                return message;
            }

            return TryGetFormattedMessage($"ERROR_{ErrorCode}", out message) ?
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
        Default = 0,
        DataManagement = 1
    }
}
