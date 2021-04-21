using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
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
                    .WithMessage("Enter address line 1")
                .MaximumLength(Constants.AddressLine1MaxLength)
                    .WithMessage($"Address line 1 must be {Constants.AddressLine1MaxLength} characters or less")
                .Matches(_addressLinePattern)
                    .WithMessage("Address line 1 must only include letters a to z, numbers, hyphens and spaces");
        }

        public static void AddressLine2<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.AddressLine2MaxLength)
                    .WithMessage($"Address line 2 must be {Constants.AddressLine2MaxLength} characters or less")
                .Matches(_addressLinePattern)
                    .WithMessage("Address line 2 must only include letters a to z, numbers, hyphens and spaces");
        }

        public static void County<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.CountyMaxLength)
                    .WithMessage($"County must be {Constants.CountyMaxLength} characters or less")
                .Matches(_countyPattern)
                    .WithMessage("County must only include letters a to z, numbers, hyphens and spaces");
        }

        public static void Email<T>(this IRuleBuilderInitial<T, string> field)
        {
            const string emailRegex = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-||_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+([a-z]+|\d|-|\.{0,1}|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])?([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$";

            field
                .Matches(emailRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)
                    .WithMessage("Enter an email address in the correct format");
        }

        public static void PhoneNumber<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Apply(Rules.PhoneNumber)
                    .WithMessage("Enter a telephone number in the correct format");
        }

        public static void Postcode<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NotEmpty()
                    .WithMessage("Enter a postcode")
                .Apply(Rules.Postcode)
                    .WithMessage("Enter a real postcode");
        }

        public static void Town<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NotEmpty()
                    .WithMessage("Enter a town or city")
                .MaximumLength(Constants.TownMaxLength)
                    .WithMessage($"Town or city must be {Constants.TownMaxLength} characters or less")
                .Matches(_townPattern)
                    .WithMessage("Town or city must only include letters a to z, numbers, hyphens and spaces");
        }

        public static void VenueName<T>(
            this IRuleBuilderInitial<T, string> field,
            int providerUkprn,
            Guid? venueId,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            field
                .NotEmpty()
                    .WithMessage("Enter a venue name")
                .MaximumLength(Constants.NameMaxLength)
                    .WithMessage($"Venue name must be {Constants.NameMaxLength} characters or fewer")
                .MustAsync(async (name, _) =>
                {
                    // Venue name must be distinct for this provider

                    var providerVenues = await cosmosDbQueryDispatcher.ExecuteQuery(new GetVenuesByProvider()
                    {
                        ProviderUkprn = providerUkprn
                    });

                    var otherVenuesWithSameName = providerVenues
                        .Where(v => v.VenueName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        .Where(v => v.Id != venueId);

                    return !otherVenuesWithSameName.Any();
                })
                    .WithMessage("Venue name must not already exist");
        }

        public static void Website<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Apply(Rules.Website)
                    .WithMessage("Enter a website in the correct format");
        }
    }
}
