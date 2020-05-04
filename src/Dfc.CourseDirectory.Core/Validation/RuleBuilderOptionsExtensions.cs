using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Resources;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class RuleBuilderOptionsExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> WithMessageForAllRules<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            string errorMessage)
        {
            foreach (var item in (rule as RuleBuilder<T, TProperty>).Rule.Validators)
            {
                item.Options.ErrorMessageSource = new StaticStringSource(errorMessage);
            }

            return rule;
        }
    }
}
