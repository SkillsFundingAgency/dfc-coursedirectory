using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Dfc.CourseDirectory.Core.Validation.ApprenticeshipValidation
{
    public static class RuleBuilderExtensions
    {
        public static void ContactEmail<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .NotEmpty()
                    .WithMessage("Enter email")
                .EmailAddress()
                    .WithMessage("Email must be a valid email address");

        public static void ContactTelephone<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .NotEmpty()
                    .WithMessage("Enter telephone")
                .Apply(Rules.PhoneNumber)
                    .WithMessage("Telephone must be a valid UK phone number");

        public static void ContactWebsite<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .Apply(Rules.Website)
                    .WithMessage("Contact us page must be a real webpage, like http://www.provider.com/apprenticeship");

        public static void MarketingInformation<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .NotEmpty()
                    .WithMessage("Enter apprenticeship information for employers")
                .ValidHtml(maxLength: Constants.MarketingInformationStrippedMaxLength)
                    .WithMessage($"Apprenticeship information for employers must be {Constants.MarketingInformationStrippedMaxLength} characters or fewer");

        public static void Website<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .Apply(Rules.Website)
                    .WithMessage("Website must be a real webpage, like http://www.provider.com/apprenticeship");

        public static void StandardCode<T>(this IRuleBuilderInitial<T, int?> field) =>
            field
                .NotNull()
                    .WithMessage("Enter standard code");

        public static void StandardVersion<T>(this IRuleBuilderInitial<T, int?> field) =>
            field
                .NotNull()
                    .WithMessage("Enter standard version");

        public static void ApprenticeshipWebpage<T>(this IRuleBuilderInitial<T, string> field) =>
            field
            .Apply(Rules.Website)
                .WithMessage("Website must be a real webpage, like http://www.provider.com/apprenticeship");

        public static void DeliveryMode<T>(this IRuleBuilderInitial<T, ApprenticeshipDeliveryMode?> field, Func<T, ApprenticeshipLocationType?> getDeliveryMethod)
        {
            field
                 .Custom((v, ctx) =>
                 {
                     var obj = (T)ctx.InstanceToValidate;
                     var deliveryMethod = getDeliveryMethod(obj);
                     if (deliveryMethod == ApprenticeshipLocationType.ClassroomBased)
                     {
                         if(v == ApprenticeshipDeliveryMode.BlockRelease || v == ApprenticeshipDeliveryMode.DayRelease)
                             return;

                         ctx.AddFailure(CreateFailure("APPRENTICESHIP_DELIVERYMODE_MUSTBE_DAY_OR_BLOCK"));
                         return;
                     }
                     
                     if(deliveryMethod == ApprenticeshipLocationType.EmployerBased)
                     {
                         if (v.HasValue)
                         {
                             ctx.AddFailure(CreateFailure("APPRENTICESHIP_DELIVERYMODE_NOT_ALLOWED"));
                             return;
                         }
                     }

                     ValidationFailure CreateFailure(string errorCode) =>
                         ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                 });

        }

        public static void NationalDelivery<T>(this IRuleBuilderInitial<T, bool?> field, Func<T, ApprenticeshipLocationType?> getDeliveryMethod)
        {
            field
                 .Custom((v, ctx) =>
                 {
                     var obj = (T)ctx.InstanceToValidate;
                     var deliveryMethod = getDeliveryMethod(obj);
                     if (deliveryMethod != ApprenticeshipLocationType.EmployerBased)
                     {
                         if (v.HasValue)
                         {
                             ctx.AddFailure(CreateFailure("APPRENTICESHIP_NATIONALDELIVERY_NOT_ALLOWED"));
                             return;
                         }
                     }

                     ValidationFailure CreateFailure(string errorCode) =>
                         ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                 });

        }


        public static void DeliveryMethod<T>(this IRuleBuilderInitial<T, ApprenticeshipLocationType?> field)
        {
            field
                .NotNull()
                    .WithMessageFromErrorCode("APPRENTICESHIP_DELIVERY_MODE_REQUIRED");
        }

        public static void SubRegions<T>(
            this IRuleBuilderInitial<T, IReadOnlyCollection<Region>> field,
            Func<T, bool> subRegionsWereSpecified,
            Func<T, ApprenticeshipLocationType?> getDeliveryMethod)
        {
            field
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMethod = getDeliveryMethod(obj);
                    var isSpecified = subRegionsWereSpecified(obj);

                    if (isSpecified && (deliveryMethod != ApprenticeshipLocationType.ClassroomBased))
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_SUBREGIONS_NOT_ALLOWED"));
                        return;
                    }
                    if (deliveryMethod != ApprenticeshipLocationType.EmployerBased)
                    {
                        return;
                    }
                    if (isSpecified && (v == null || v.Count == 0))
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_SUBREGIONS_INVALID"));
                    }
                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }

    }
}
