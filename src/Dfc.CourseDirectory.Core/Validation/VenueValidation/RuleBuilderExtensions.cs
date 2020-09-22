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
    }
}
