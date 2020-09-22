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

        public static void Website<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .Apply(Rules.Website)
                    .WithMessage("The format of URL is incorrect");
        }
    }
}
