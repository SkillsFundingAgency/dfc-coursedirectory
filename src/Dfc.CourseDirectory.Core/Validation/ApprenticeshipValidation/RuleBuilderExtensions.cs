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


        public static void YourVenueReference<T>(
          this IRuleBuilderInitial<T, string> field,
          Func<T, ApprenticeshipLocationType?> getDeliveryMode,
          Func<T, string> getVenueName,
          Guid? matchedVenueId)
        {
            field
                .Cascade(CascadeMode.Stop)
                .NormalizeWhitespace()
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMode = getDeliveryMode(obj);
                    var isSpecified = !string.IsNullOrEmpty(v);

                    // Not allowed for delivery modes other than classroom based
                    if (isSpecified && deliveryMode != ApprenticeshipLocationType.ClassroomBased)
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_PROVIDER_VENUE_REF_NOT_ALLOWED"));
                        return;
                    }

                    if (deliveryMode != ApprenticeshipLocationType.ClassroomBased)
                    {
                        return;
                    }

                    // If not specified and Venue Name isn't specified then it's required
                    if (!isSpecified && string.IsNullOrEmpty(getVenueName(obj)))
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_VENUE_REQUIRED"));
                        return;
                    }

                    // If specified then it must match a venue
                    if (isSpecified && !matchedVenueId.HasValue)
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_PROVIDER_VENUE_REF_INVALID"));
                        return;
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }


        public static void Venue<T>(
            this IRuleBuilderInitial<T, string> field,
            Func<T, ApprenticeshipLocationType?> getDeliveryMode)
        {
            field
                .NormalizeWhitespace()
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMode = getDeliveryMode(obj);
                    var isSpecified = !string.IsNullOrEmpty(v);

                    // Not allowed for delivery modes other than classroom based
                    if (isSpecified && deliveryMode != ApprenticeshipLocationType.ClassroomBased)
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_VENUE_NOT_ALLOWED"));
                        return;
                    }

                    if (deliveryMode != ApprenticeshipLocationType.ClassroomBased)
                    {
                        return;
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }

        public static void Radius<T>(this IRuleBuilderInitial<T, int?> field,
            Func<T, ApprenticeshipLocationType?> getDeliveryMethod)
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
                             ctx.AddFailure(CreateFailure("APPRENTICESHIP_RADIUS_NOT_ALLOWED"));
                         }
                     }
                     else
                     {
                         if (!v.HasValue)
                         {
                             ctx.AddFailure(CreateFailure("APPRENTICESHIP_RADIUS_REQUIRED"));
                         }
                     }

                     ValidationFailure CreateFailure(string errorCode) =>
                         ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                 });
        }

        public static void DeliveryMode<T>(this IRuleBuilderInitial<T, ApprenticeshipDeliveryMode?> field, Func<T, ApprenticeshipLocationType?> getDeliveryMethod)
        {
            field
                 .Custom((v, ctx) =>
                 {
                     var obj = (T)ctx.InstanceToValidate;
                     var deliveryMethod = getDeliveryMethod(obj);
                     if (deliveryMethod == ApprenticeshipLocationType.ClassroomBased)
                     {
                         if (v == ApprenticeshipDeliveryMode.BlockRelease || v == ApprenticeshipDeliveryMode.DayRelease)
                             return;

                         ctx.AddFailure(CreateFailure("APPRENTICESHIP_DELIVERYMODE_MUSTBE_DAY_OR_BLOCK"));
                     }

                     if (deliveryMethod == ApprenticeshipLocationType.EmployerBased)
                     {
                         if (v.HasValue)
                         {
                             ctx.AddFailure(CreateFailure("APPRENTICESHIP_DELIVERYMODE_NOT_ALLOWED"));
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
                         }
                         else
                             return;
                     }

                     ValidationFailure CreateFailure(string errorCode) =>
                         ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                 });
        }


        public static void DeliveryMethod<T>(this IRuleBuilderInitial<T, ApprenticeshipLocationType?> field)
        {
            field
                .NotNull()
                    .WithErrorCode("APPRENTICESHIP_DELIVERY_METHOD_REQUIRED");
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

                    if (isSpecified && (deliveryMethod != ApprenticeshipLocationType.EmployerBased))
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_SUBREGIONS_NOT_ALLOWED"));
                    }
                    if (deliveryMethod != ApprenticeshipLocationType.EmployerBased)
                    {
                        return;
                    }
                    if (isSpecified && (v == null || v.Count == 0))
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_SUBREGIONS_INVALID"));
                    }

                    if(!isSpecified && (v == null || v.Count == 0) && deliveryMethod == ApprenticeshipLocationType.EmployerBased)
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_SUBREGIONS_REQUIRED"));
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }

        public static void VenueName<T>(
            this IRuleBuilderInitial<T, string> field,
            Func<T, ApprenticeshipLocationType?> getDeliveryMethod,
            Func<T, string> getProviderVenueRef,
            Guid? matchedVenueId)
        {
            field
                .NormalizeWhitespace()
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMode = getDeliveryMethod(obj);
                    var isSpecified = !string.IsNullOrEmpty(v);

                    // Not allowed for delivery modes other than classroom based
                    if (isSpecified && deliveryMode != ApprenticeshipLocationType.ClassroomBased)
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_VENUE_NAME_NOT_ALLOWED"));
                        return;
                    }

                    if (deliveryMode != ApprenticeshipLocationType.ClassroomBased)
                    {
                        return;
                    }

                    if (isSpecified && !matchedVenueId.HasValue)
                    {
                        // We don't want both a ref and a name but if the ref resolves a venue and that venue's name
                        // matches this name then we let it go. If it doesn't match then yield an error.
                        if (!string.IsNullOrEmpty(getProviderVenueRef(obj)))
                        {
                            ctx.AddFailure(CreateFailure("APPRENTICESHIP_VENUE_NAME_NOT_ALLOWED_WITH_REF"));
                            return;
                        }

                        // Couldn't find a match from name
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_VENUE_NAME_INVALID"));
                        return;
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }

    }
}
