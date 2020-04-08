using System;
using FluentValidation;

namespace Dfc.CourseDirectory.WebV2.Validation
{
    public static class HtmlValidation
    {
        public static IRuleBuilderOptions<T, string> ValidHtml<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int? maxLength = null)
        {
            var builder = ruleBuilder.Must(value =>
            {
                try
                {
                    Html.SanitizeHtml(value);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });

            if (maxLength.HasValue)
            {
                builder = builder.Must(value =>
                    value == null ||
                    Html.StripTags(Html.SanitizeHtml(value)).Length <= maxLength);
            }

            return builder;
        }
    }
}
