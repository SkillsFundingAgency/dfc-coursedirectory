using FluentValidation;

namespace Dfc.CourseDirectory.WebV2.Validation.ProviderValidation
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
                    .WithMessage("Enter provider marketing information")
                .ValidHtml(maxLength: Constants.MarketingInformationStrippedMaxLength)
                    .WithMessage($"Marketing information must be {Constants.MarketingInformationStrippedMaxLength} characters or fewer");
        }
    }
}
