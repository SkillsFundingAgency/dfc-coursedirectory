using System;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation.TLevelValidation
{
    public static class RuleBuilderExtensions
    {
        public static void EntryRequirements<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.EntryRequirementsMaxLength)
                    .WithMessage($"Entry requirements must be {Constants.EntryRequirementsMaxLength} characters or fewer");
        }

        public static void HowYoullBeAssessed<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.HowYoullBeAssessedMaxLength)
                    .WithMessage($"How you'll be assessed must be {Constants.HowYoullBeAssessedMaxLength} characters or fewer");
        }

        public static void HowYoullLearn<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.HowYoullLearnMaxLength)
                    .WithMessage($"How you'll learn must be {Constants.HowYoullLearnMaxLength} characters or fewer");
        }

        public static void StartDate<T>(
            this IRuleBuilderInitial<T, DateInput> field,
            Guid? tLevelId,
            Guid providerId,
            Guid tLevelDefinitionId,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            field
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                    .WithMessage("Enter a start date")
                .Apply(builder => Rules.Date(builder, displayName: "Start date"))
                .CustomAsync(async (v, ctx, _) =>
                {
                    var existingTLevels = await sqlQueryDispatcher.ExecuteQuery(
                        new GetTLevelsForProvider() { ProviderId = providerId });

                    if (existingTLevels.Any(tl =>
                        tl.TLevelDefinition.TLevelDefinitionId == tLevelDefinitionId &&
                        tl.StartDate == v.Value &&
                        tl.TLevelId != tLevelId))
                    {
                        ctx.AddFailure("Start date already exists");
                    }
                });
        }

        public static void Website<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NotEmpty()
                    .WithMessage("Enter a webpage")
                .Apply(Rules.Website)
                    .WithMessage("Website must be a real webpage")
                .MaximumLength(Constants.WebsiteMaxLength)
                    .WithMessage($"T Level webpage must be {Constants.WebsiteMaxLength} characters or fewer");
        }

        public static void WhatYouCanDoNext<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.WhatYouCanDoNextMaxLength)
                    .WithMessage($"What you can do next must be {Constants.WhatYouCanDoNextMaxLength} characters or fewer");
        }

        public static void WhatYoullLearn<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.WhatYoullLearnMaxLength)
                    .WithMessage($"What you'll learn must be {Constants.WhatYoullLearnMaxLength} characters or fewer");
        }

        public static void WhoFor<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NotEmpty()
                    .WithMessage("Enter who this T Level is for")
                .MaximumLength(Constants.WhoForMaxLength)
                    .WithMessage($"Who this T Level is for must be {Constants.WhoForMaxLength} characters or fewer");
        }

        public static void YourReference<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.YourReferenceMaxLength)
                    .WithMessage($"Your reference must be {Constants.YourReferenceMaxLength} characters or fewer");
        }
    }
}
