using System;
using System.Text.RegularExpressions;
using FluentValidation;

namespace Dfc.CourseDirectory.WebV2.Validation
{
    public static class RuleBuilderExtensions
    {
        private static readonly Regex UkPhoneNumber = new Regex(
            @"^((\(?0\d{4}\)?\s?\d{3}\s?\d{3})|(\(?0\d{3}\)?\s?\d{3}\s?\d{4})|(\(?0\d{2}\)?\s?\d{4}\s?\d{4}))(\s?\#(\d{4}|\d{3}))?$",
            RegexOptions.Compiled);

        public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> field) =>
            field
                .Matches(UkPhoneNumber);

        public static IRuleBuilderOptions<T, string> Website<T>(this IRuleBuilder<T, string> field) =>
            field
                .Must(url =>
                {
                    if (string.IsNullOrEmpty(url))
                    {
                        return true;
                    }

                    var withPrefix = !url.Contains("://") ? $"http://{url}" : url;

                    return Uri.TryCreate(withPrefix, UriKind.Absolute, out var uri) &&
                        (uri.Scheme == "http" || uri.Scheme == "https");
                });
    }
}
