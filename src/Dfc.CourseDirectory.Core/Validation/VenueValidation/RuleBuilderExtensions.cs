using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation.VenueValidation
{
    public static class RuleBuilderExtensions
    {
        private static readonly Regex _addressLinePattern = new Regex(
            @"^[a-zA-Z0-9\.\-']+(?: [a-zA-Z0-9\.\-']+)*$",
            RegexOptions.Compiled);

        private static readonly Regex _countyPattern = new Regex(
            @"^[a-zA-Z\.\-']+(?: [a-zA-Z\.\-']+)*$",
            RegexOptions.Compiled);

        private static readonly Regex _townPattern = new Regex(
            @"^[a-zA-Z\.\-']+(?: [a-zA-Z\.\-']+)*$",
            RegexOptions.Compiled);

        public static void AddressLine1<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NotEmpty()
                    .WithMessageFromErrorCode("VENUE_ADDRESS_LINE1_REQUIRED")
                .MaximumLength(Constants.AddressLine1MaxLength)
                    .WithMessageFromErrorCode("VENUE_ADDRESS_LINE1_MAXLENGTH")
                .Matches(_addressLinePattern)
                    .WithMessageFromErrorCode("VENUE_ADDRESS_LINE1_FORMAT");
        }

        public static void AddressLine2<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.AddressLine2MaxLength)
                    .WithMessageFromErrorCode("VENUE_ADDRESS_LINE2_MAXLENGTH")
                .Matches(_addressLinePattern)
                    .WithMessageFromErrorCode("VENUE_ADDRESS_LINE2_FORMAT");
        }

        public static void County<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.CountyMaxLength)
                    .WithMessageFromErrorCode("VENUE_COUNTY_MAXLENGTH")
                .Matches(_countyPattern)
                    .WithMessageFromErrorCode("VENUE_COUNTY_FORMAT");
        }

        public static void Email<T>(this IRuleBuilderInitial<T, string> field)
        {
            const string emailRegex = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-||_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+([a-z]+|\d|-|\.{0,1}|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])?([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$";

            field
                .Matches(emailRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)
                    .WithMessageFromErrorCode("VENUE_EMAIL_FORMAT");
        }

        public static void PhoneNumber<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Apply(Rules.PhoneNumber)
                    .WithMessageFromErrorCode("VENUE_TELEPHONE_FORMAT");
        }

        public static void Postcode<T>(this IRuleBuilderInitial<T, string> field, PostcodeInfo postcodeInfo)
        {
            field
                .NotEmpty()
                    .WithMessageFromErrorCode("VENUE_POSTCODE_REQUIRED")
                .Apply(Rules.Postcode)
                    .WithMessageFromErrorCode("VENUE_POSTCODE_FORMAT")
                .Must(postcode => postcodeInfo != null)  // i.e. a known postcode we can retrieve lat/lng for
                    .WithMessageFromErrorCode("VENUE_POSTCODE_FORMAT");
        }

        public static void Town<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NotEmpty()
                    .WithMessageFromErrorCode("VENUE_TOWN_REQUIRED")
                .MaximumLength(Constants.TownMaxLength)
                    .WithMessageFromErrorCode("VENUE_TOWN_MAXLENGTH")
                .Matches(_townPattern)
                    .WithMessageFromErrorCode("VENUE_TOWN_FORMAT");
        }

        public static void VenueName<T>(
            this IRuleBuilderInitial<T, string> field,
            Guid providerId,
            Guid? venueId,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            field
                .NotEmpty()
                    .WithMessageFromErrorCode("VENUE_NAME_REQUIRED")
                .MaximumLength(Constants.NameMaxLength)
                    .WithMessageFromErrorCode("VENUE_NAME_MAXLENGTH")
                .MustAsync(async (name, _) =>
                {
                    // Venue name must be distinct for this provider

                    var providerVenues = await sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider()
                    {
                        ProviderId = providerId
                    });

                    var otherVenuesWithSameName = providerVenues
                        .Where(v => v.VenueName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        .Where(v => v.VenueId != venueId);

                    return !otherVenuesWithSameName.Any();
                })
                    .WithMessageFromErrorCode("VENUE_NAME_UNIQUE");
        }

        public static void Website<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Apply(Rules.Website)
                    .WithMessageFromErrorCode("VENUE_WEBSITE_FORMAT");
        }
    }
}
