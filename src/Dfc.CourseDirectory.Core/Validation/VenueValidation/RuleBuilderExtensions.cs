using System;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation.VenueValidation
{
    public static class RuleBuilderExtensions
    {
        public static void Email<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .EmailAddress()
                    .WithMessage("Enter an email address in the correct format, like name@example.com");
        }

        public static void PhoneNumber<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Apply(Rules.PhoneNumber)
                    .WithMessage("Enter a telephone number in the correct format");
        }

        public static void VenueName<T>(
            this IRuleBuilderInitial<T, string> field,
            int providerUkprn,
            Guid? venueId,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            field
                .NotEmpty()
                    .WithMessage("Enter location name")
                .MaximumLength(Constants.NameMaxLength)
                    .WithMessage($"Location name must be {Constants.NameMaxLength} characters or fewer")
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
                    .WithMessage("Location name must not already exist");
        }

        public static void Website<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Apply(Rules.Website)
                    .WithMessage("The format of URL is incorrect");
        }
    }
}
