using System;
using System.Linq;
using FluentValidation;
using GovUk.Frontend.AspNetCore;

namespace Dfc.CourseDirectory.WebV2.Validation
{
    public static class ValidatorBaseExtensions
    {
        public static IRuleBuilderInitial<T, Date?> Date<T>(
            this IRuleBuilderInitial<T, Date?> builder,
            string displayName,
            string missingErrorMessage)
        {
            if (!(builder is FluentValidation.Internal.RuleBuilder<T, Date?> b) ||
                !(b.ParentValidator is ValidatorBase<T> validatorBase))
            {
                throw new InvalidOperationException(
                    "Rule can only be applied on validators inheriting from ValidatorBase<T>.");
            }

            return builder.Custom((property, context) =>
            {
                if (validatorBase.ActionContext.ModelState.TryGetValue(context.PropertyName, out var entry)
                    && entry.Errors.SingleOrDefault()?.Exception is DateParseException dateParseException)
                {
                    // Reference https://design-system.service.gov.uk/components/date-input/#error-messages

                    var dayIsMissing = string.IsNullOrEmpty(entry.GetModelStateForProperty("Day").AttemptedValue);
                    var monthIsMissing = string.IsNullOrEmpty(entry.GetModelStateForProperty("Month").AttemptedValue);
                    var yearIsMissing = string.IsNullOrEmpty(entry.GetModelStateForProperty("Year").AttemptedValue);

                    if (dayIsMissing && monthIsMissing && yearIsMissing)
                    {
                        // If nothing is entered
                        // Say 'Enter [whatever it is]'. For example, 'Enter your date of birth'.

                        context.AddFailure(missingErrorMessage);
                    }
                    else if (dayIsMissing || monthIsMissing || yearIsMissing)
                    {
                        // If the date is incomplete
                        // Say '[whatever it is] must include a [whatever is missing]'.
                        // For example, 'Date of birth must include a month' or 'Date of birth must include a day and month'.

                        var missingParts = (dayIsMissing ? new[] { "day" } : Array.Empty<string>())
                            .Concat(monthIsMissing ? new[] { "month" } : Array.Empty<string>())
                            .Concat(yearIsMissing ? new[] { "year" } : Array.Empty<string>());

                        context.AddFailure($"{displayName} must include a {string.Join(" and ", missingParts)}");
                    }
                    else
                    {
                        // If the date entered can’t be correct
                        // For example, '13' in the month field can’t be correct.
                        // Highlight the day, month or year field with the incorrect information. Or highlight the date as a whole if there’s incorrect information in more than one field, or it’s not clear which field is incorrect.
                        // Say '[Whatever it is] must be a real date'. For example, 'Date of birth must be a real date'.

                        context.AddFailure($"{displayName} must be a real date");
                    }
                }
            });
        }
    }
}
