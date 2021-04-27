using FluentValidation;
using FluentValidation.Internal;

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
                item.Options.SetErrorMessage(errorMessage);
            }

            return rule;
        }

        public static IRuleBuilderOptions<T, TProperty> WithMessageFromErrorCode<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            string errorCode,
            params object[] args)
        {
            var message = ContentExtensions.GetMessageForErrorCode(errorCode, args);

            return rule
                .WithErrorCode(errorCode)
                .WithMessage(message);
        }
    }
}
