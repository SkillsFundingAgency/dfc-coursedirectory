using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation.ProviderValidation
{
    public static class RuleBuilderExtensions
    {
        public static void Alias<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.AliasMaxLength)
                    .WithMessage($"Alias must be {Constants.AliasMaxLength} characters or fewer");
        }

        public static void CourseDirectoryName<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.CourseDirectoryNameMaxLength)
                .WithMessage($"Course Directory name must be {Constants.CourseDirectoryNameMaxLength} characters or fewer");
        }

        public static void MarketingInformation<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NotEmpty()
                    .WithMessage("Enter brief overview of your organisation for employers")
                .ValidHtml(maxLength: Constants.MarketingInformationStrippedMaxLength)
                    .WithMessage($"Brief overview of your organisation for employers must be {Constants.MarketingInformationStrippedMaxLength} characters or fewer");
        }
    }
}
