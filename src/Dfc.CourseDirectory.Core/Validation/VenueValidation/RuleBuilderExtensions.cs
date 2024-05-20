using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation.VenueValidation
{
    public static class RuleBuilderExtensions
    {
        private static readonly Regex _countyPattern = new Regex(
            @"^[a-zA-Z\.\-',]+(?: [a-zA-Z\.\-',]+)*$",
            RegexOptions.Compiled);

        private static readonly Regex _townPattern = new Regex(
            @"^[a-zA-Z\.\-',]+(?: [a-zA-Z\.\-',]+)*$",
            RegexOptions.Compiled);

        public static Regex AddressLinePattern { get; } = new Regex(
            @"^[a-zA-Z0-9\.\-',]+(?: [a-zA-Z0-9\.\-',]+)*$",
            RegexOptions.Compiled);

        public static void AddressLine1<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NormalizeWhitespace()
                .NotEmpty()
                    .WithMessageFromErrorCode("VENUE_ADDRESS_LINE1_REQUIRED")
                .MaximumLength(Constants.AddressLine1MaxLength)
                    .WithMessageFromErrorCode("VENUE_ADDRESS_LINE1_MAXLENGTH")
                .Matches(AddressLinePattern)
                    .WithMessageFromErrorCode("VENUE_ADDRESS_LINE1_FORMAT");
        }

        public static void AddressLine2<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NormalizeWhitespace()
                .MaximumLength(Constants.AddressLine2MaxLength)
                    .WithMessageFromErrorCode("VENUE_ADDRESS_LINE2_MAXLENGTH")
                .Matches(AddressLinePattern)
                    .WithMessageFromErrorCode("VENUE_ADDRESS_LINE2_FORMAT");
        }

        public static void County<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NormalizeWhitespace()
                .MaximumLength(Constants.CountyMaxLength)
                    .WithMessageFromErrorCode("VENUE_COUNTY_MAXLENGTH")
                .Matches(_countyPattern)
                    .WithMessageFromErrorCode("VENUE_COUNTY_FORMAT");
        }

        public static void Email<T>(this IRuleBuilderInitial<T, string> field)
        {
            const string emailRegex = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-||_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+([a-z]+|\d|-|\.{0,1}|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])?([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$";

            field
                .NormalizeWhitespace()
                .Matches(emailRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)
                    .WithMessageFromErrorCode("VENUE_EMAIL_FORMAT");
        }

        public static void PhoneNumber<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NormalizeWhitespace()
                .Apply(Rules.PhoneNumber)
                    .WithMessageFromErrorCode("VENUE_TELEPHONE_FORMAT");
        }

        public static void Postcode<T>(this IRuleBuilderInitial<T, string> field, Func<Postcode, PostcodeInfo> getPostcodeInfo)
        {
            field
                .NormalizeWhitespace()
                .NotEmpty()
                    .WithMessageFromErrorCode("VENUE_POSTCODE_REQUIRED")
                // i.e. Postcode must exist in our Postcode info table (so we can resolve a lat/lng)
                .Must(postcode => postcode == null || (Models.Postcode.TryParse(postcode, out var pc) && getPostcodeInfo(pc) != null))
                    .WithMessageFromErrorCode("VENUE_POSTCODE_FORMAT");
        }

        public static void ProviderVenueRef<T>(
            this IRuleBuilderInitial<T, string> field,
            Guid providerId,
            Guid? venueId,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            field
                .ProviderVenueRef(
                    getOtherVenueProviderVenueRefs: async _ =>
                    {
                        var venues = await sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider()
                        {
                            ProviderId = providerId
                        });

                        return venues
                            .Where(v => v.VenueId != venueId && !string.IsNullOrEmpty(v.ProviderVenueRef))
                            .Select(v => v.ProviderVenueRef)
                            .ToArray();
                    });
        }

        public static void ProviderVenueRef<T>(
            this IRuleBuilderInitial<T, string> field,
            Func<T, Task<IEnumerable<string>>> getOtherVenueProviderVenueRefs)
        {
            field
                .NormalizeWhitespace()
                .NotEmpty()
                    .WithMessageFromErrorCode("VENUE_PROVIDER_VENUE_REF_REQUIRED")
                .MaximumLength(Constants.ProviderVenueRefMaxLength)
                    .WithMessageFromErrorCode("VENUE_PROVIDER_VENUE_REF_MAXLENGTH")
                .MustAsync(async (obj, providerVenueRef, _) =>
                {
                    // Field is optional
                    if (string.IsNullOrEmpty(providerVenueRef))
                    {
                        return true;
                    }

                    // Venue name must be distinct for this provider
                    var otherVenueRefs = await getOtherVenueProviderVenueRefs(obj);

                    var otherVenuesWithSameRef = otherVenueRefs
                        .Where(v => (v ?? string.Empty).Equals(providerVenueRef, StringComparison.OrdinalIgnoreCase));

                    return !otherVenuesWithSameRef.Any();
                })
                .WithMessageFromErrorCode("VENUE_PROVIDER_VENUE_REF_UNIQUE");
        }

        public static void Town<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NormalizeWhitespace()
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
                .VenueName(
                    getOtherVenueNames: async _ =>
                    {
                        var venues = await sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider()
                        {
                            ProviderId = providerId
                        });

                        return venues
                            .Where(v => v.VenueId != venueId)
                            .Select(v => v.VenueName)
                            .ToArray();
                    });
        }

        public static IRuleBuilderOptions<T, string> VenueName<T>(
            this IRuleBuilderInitial<T, string> field,
            Func<T, Task<IEnumerable<string>>> getOtherVenueNames)
        {
            return field
                .NormalizeWhitespace()
                .NotEmpty()
                    .WithMessageFromErrorCode("VENUE_NAME_REQUIRED")
                .MaximumLength(Constants.NameMaxLength)
                    .WithMessageFromErrorCode("VENUE_NAME_MAXLENGTH")
                .MustAsync(async (obj, name, _) =>
                {
                    // Venue name must be distinct for this provider
                    var otherVenueNames = await getOtherVenueNames(obj);

                    var otherVenuesWithSameName = otherVenueNames
                        .Where(v => (v ?? string.Empty).Equals(name, StringComparison.OrdinalIgnoreCase));

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
