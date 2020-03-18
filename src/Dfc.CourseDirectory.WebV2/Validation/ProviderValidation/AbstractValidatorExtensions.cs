using FluentValidation;

namespace Dfc.CourseDirectory.WebV2.Validation.ProviderValidation
{
    public static class AbstractValidatorExtensions
    {
        public static void MarketingInformation<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NotEmpty()
                .ValidHtml(maxLength: Constants.MarketingInformationStrippedMaxLength)
                .WithMessageForAllRules("PLACEHOLDER");
        }
    }
}
