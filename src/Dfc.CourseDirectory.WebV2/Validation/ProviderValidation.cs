using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;

namespace Dfc.CourseDirectory.WebV2.Validation
{
    public static class ProviderValidation
    {
        public const int AliasMaxLength = 100;
        public const int CourseDirectoryNameMaxLength = 100;

        public static bool BriefOverviewIsEditable(ApprenticeshipQAStatus qaStatus, UserInfo user) =>
            user.IsDeveloper ||
            (user.IsProvider && qaStatus != ApprenticeshipQAStatus.Passed && qaStatus != ApprenticeshipQAStatus.Submitted);

        public static bool CourseDirectoryNameIsEditable(UserInfo user) => user.IsDeveloper;

        public static void RuleForAlias<T>(this IRuleBuilder<T, string> ruleBuilder) =>
            ruleBuilder
                .MaximumLength(AliasMaxLength)
                .WithMessage(ValidationMessageTemplates.MessageForMaxStringLength("Your alias", AliasMaxLength));

        public static void RuleForCourseDirectoryName<T>(this IRuleBuilder<T, string> ruleBuilder) =>
            ruleBuilder
                .MaximumLength(CourseDirectoryNameMaxLength)
                .WithMessage(ValidationMessageTemplates.MessageForMaxStringLength("Course Directory name", CourseDirectoryNameMaxLength));
    }
}
