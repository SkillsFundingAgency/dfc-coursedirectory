using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation.ApprenticeshipValidation
{
    public static class RuleBuilderExtensions
    {
        public static void ContactEmail<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .NotEmpty()
                    .WithMessage("Enter email")
                .EmailAddress()
                    .WithMessage("Email must be a valid email address");

        public static void ContactTelephone<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .NotEmpty()
                    .WithMessage("Enter telephone")
                .Apply(Rules.PhoneNumber)
                    .WithMessage("Telephone must be a valid UK phone number");

        public static void ContactWebsite<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .Apply(Rules.Website)
                    .WithMessage("Contact us page must be a real webpage, like http://www.provider.com/apprenticeship");

        public static void MarketingInformation<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .NotEmpty()
                    .WithMessage("Enter apprenticeship information for employers")
                .ValidHtml(maxLength: Constants.MarketingInformationStrippedMaxLength)
                    .WithMessage($"Apprenticeship information for employers must be {Constants.MarketingInformationStrippedMaxLength} characters or fewer");

        public static void Website<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .Apply(Rules.Website)
                    .WithMessage("Website must be a real webpage, like http://www.provider.com/apprenticeship");
    }
}
