using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;
using Dfc.CourseDirectory.Core;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.ApprenticeshipSummary
{
    public class Query : IRequest<OneOf<ModelWithErrors<ViewModel>, ViewModel>>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public Guid ProviderId { get; set; }
        public string StandardOrFrameworkTitle { get; set; }
        public string MarketingInformation { get; set; }
        public string Website { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        public bool? ApprenticeshipIsNational { get; set; }
        public IReadOnlyCollection<ViewModelEmployerBasedLocationRegion> EmployerBasedLocationRegions { get; set; }
        public IReadOnlyCollection<ViewModelClassroomBasedLocation> ClassroomBasedLocations { get; set; }
    }

    public class ViewModelEmployerBasedLocationRegion
    {
        public string RegionId { get; set; }
        public string Name { get; set; }
    }

    public class ViewModelClassroomBasedLocation
    {
        public Guid VenueId { get; set; }
        public string VenueName { get; set; }
        public int Radius { get; set; }
        public ApprenticeshipDeliveryModes DeliveryModes { get; set; }
    }

    public class CompleteCommand : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid ProviderId { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, OneOf<ModelWithErrors<ViewModel>, ViewModel>>,
        IRequireUserCanSubmitQASubmission<Query>,
        IRequestHandler<CompleteCommand, OneOf<ModelWithErrors<ViewModel>, Success>>,
        IRequireUserCanSubmitQASubmission<CompleteCommand>
    {
        private readonly MptxInstanceContext<FlowModel> _flow;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;
        private readonly IProviderInfoCache _providerInfoCache;

        public Handler(
            MptxInstanceContext<FlowModel> flow,
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock,
            IProviderInfoCache providerInfoCache)
        {
            _flow = flow;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
            _providerInfoCache = providerInfoCache;
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, ViewModel>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            ValidateFlowState();

            var providerId = request.ProviderId;
            var provider = await _providerInfoCache.GetProviderInfo(providerId);

            ViewModel vm = await CreateViewModel(provider);

            var validator = new CompleteValidator();
            var validationResult = await validator.ValidateAsync(_flow.State);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }
            else
            {
                return vm;
            }
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(
            CompleteCommand request,
            CancellationToken cancellationToken)
        {
            ValidateFlowState();

            var providerId = request.ProviderId;
            var provider = await _providerInfoCache.GetProviderInfo(providerId);

            var validator = new CompleteValidator();
            var validationResult = await validator.ValidateAsync(_flow.State);

            if (!validationResult.IsValid)
            {
                var vm = await CreateViewModel(provider);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            var currentUser = _currentUserProvider.GetCurrentUser();
            var now = _clock.UtcNow;

            var apprenticeshipId = Guid.NewGuid();
            var providerUkprn = provider.Ukprn;

            // Create apprenticeship
            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new CreateApprenticeship()
                {
                    ApprenticeshipLocations = await CreateLocations(providerUkprn),
                    ApprenticeshipTitle = _flow.State.ApprenticeshipStandardOrFramework.StandardOrFrameworkTitle,
                    ApprenticeshipType = _flow.State.ApprenticeshipStandardOrFramework.IsStandard ?
                        ApprenticeshipType.StandardCode :
                        ApprenticeshipType.FrameworkCode,
                    ContactEmail = _flow.State.ApprenticeshipContactEmail,
                    ContactTelephone = _flow.State.ApprenticeshipContactTelephone,
                    ContactWebsite = _flow.State.ApprenticeshipContactWebsite,
                    CreatedByUser = currentUser,
                    CreatedDate = _clock.UtcNow,
                    Id = apprenticeshipId,
                    MarketingInformation = _flow.State.ApprenticeshipMarketingInformation,
                    ProviderId = request.ProviderId,
                    ProviderUkprn = providerUkprn,
                    StandardOrFramework = _flow.State.ApprenticeshipStandardOrFramework,
                    Url = _flow.State.ApprenticeshipWebsite
                });

            // Create QA submission
            await _sqlQueryDispatcher.ExecuteQuery(
                new CreateApprenticeshipQASubmission()
                {
                    ProviderId = request.ProviderId,
                    Apprenticeships = new List<CreateApprenticeshipQASubmissionApprenticeship>()
                    {
                        new CreateApprenticeshipQASubmissionApprenticeship()
                        {
                            ApprenticeshipId = apprenticeshipId,
                            ApprenticeshipMarketingInformation = _flow.State.ApprenticeshipMarketingInformation,
                            ApprenticeshipTitle = _flow.State.ApprenticeshipStandardOrFramework.StandardOrFrameworkTitle
                        }
                    },
                    ProviderMarketingInformation = _flow.State.ProviderMarketingInformation,
                    SubmittedByUserId = currentUser.UserId,
                    SubmittedOn = now
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = request.ProviderId,
                    ApprenticeshipQAStatus = ApprenticeshipQAStatus.Submitted
                });

            // Ensure user cannot go 'back' to any part of this flow
            _flow.Complete();

            return new Success();

            async Task<IEnumerable<CreateApprenticeshipLocation>> CreateLocations(int providerUkprn)
            {
                var locations = new List<CreateApprenticeshipLocation>();

                var locationType = _flow.State.ApprenticeshipLocationType.Value;

                if (locationType.HasFlag(ApprenticeshipLocationType.EmployerBased))
                {
                    locations.Add(_flow.State.ApprenticeshipIsNational.Value ?
                        CreateApprenticeshipLocation.CreateNational() :
                        CreateApprenticeshipLocation.CreateRegions(_flow.State.ApprenticeshipLocationSubRegionIds));
                }

                if (locationType.HasFlag(ApprenticeshipLocationType.ClassroomBased))
                {
                    var providerVenues = await _cosmosDbQueryDispatcher.ExecuteQuery(
                        new GetAllVenuesForProvider()
                        {
                            ProviderUkprn = providerUkprn
                        });

                    locations.AddRange(_flow.State.ApprenticeshipClassroomLocations.Select(l =>
                    {
                        var venue = providerVenues.Single(v => v.Id == l.Value.VenueId);

                        // REVIEW: This is likely a data modeling error;
                        // duplicating here to ensure legacy UI works
                        var deliveryModes = l.Value.DeliveryModes;
                        if (locationType.HasFlag(ApprenticeshipLocationType.EmployerBased))
                        {
                            deliveryModes |= ApprenticeshipDeliveryModes.EmployerAddress;
                        }

                        return CreateApprenticeshipLocation.CreateFromVenue(
                            venue,
                            l.Value.Radius,
                            deliveryModes,
                            locationType);
                    }));
                }

                return locations;
            }
        }

        private async Task<ViewModel> CreateViewModel(ProviderInfo provider)
        {
            var providerVenues = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetAllVenuesForProvider()
                {
                    ProviderUkprn = provider.Ukprn
                });

            return new ViewModel()
            {
                ProviderId = provider.ProviderId,
                StandardOrFrameworkTitle = _flow.State.ApprenticeshipStandardOrFramework.StandardOrFrameworkTitle,
                MarketingInformation = _flow.State.ApprenticeshipMarketingInformation,
                Website = _flow.State.ApprenticeshipWebsite,
                ContactTelephone = _flow.State.ApprenticeshipContactTelephone,
                ContactEmail = _flow.State.ApprenticeshipContactEmail,
                ContactWebsite = _flow.State.ApprenticeshipContactWebsite,
                ApprenticeshipLocationType = _flow.State.ApprenticeshipLocationType.Value,
                ApprenticeshipIsNational = _flow.State.ApprenticeshipIsNational,
                EmployerBasedLocationRegions = _flow.State?.ApprenticeshipLocationSubRegionIds
                    ?.Select(id => new ViewModelEmployerBasedLocationRegion()
                    {
                        RegionId = id,
                        Name = Region.All.SelectMany(r => r.SubRegions).Single(sr => sr.Id == id).Name
                    })
                    ?.OrderBy(l => l.Name)
                    ?.ToList(),
                ClassroomBasedLocations = _flow.State?.ApprenticeshipClassroomLocations
                    ?.Values
                    ?.Select(l => new ViewModelClassroomBasedLocation()
                    {
                        DeliveryModes = l.DeliveryModes,
                        Radius = l.Radius,
                        VenueId = l.VenueId,
                        VenueName = providerVenues.Single(v => v.Id == l.VenueId).VenueName
                    })
                    ?.OrderBy(l => l.VenueName)
                    ?.ToList()
            };
        }

        Guid IRequireUserCanSubmitQASubmission<Query>.GetProviderId(Query request) => request.ProviderId;

        Guid IRequireUserCanSubmitQASubmission<CompleteCommand>.GetProviderId(CompleteCommand request) => request.ProviderId;

        private void ValidateFlowState()
        {
            // Require everything to be set here to create a valid apprenticeship
            // *except* classroom locations - we need to handle that being empty

            var state = _flow.State;

            var isValid = state.GotProviderDetails &&
                state.ApprenticeshipStandardOrFramework != null &&
                state.ApprenticeshipLocationType != null &&
                (
                    (state.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.ClassroomBased)) ||
                    (state.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.EmployerBased) &&
                        (state.ApprenticeshipIsNational.GetValueOrDefault() || (state.ApprenticeshipLocationSubRegionIds?.Count ?? 0) > 0))) &&
                state.GotApprenticeshipDetails;

            if (!isValid)
            {
                throw new ErrorException<InvalidFlowState>(new InvalidFlowState());
            }
        }

        private class CompleteValidator : AbstractValidator<FlowModel>
        {
            public CompleteValidator()
            {
                // Require at least one classroom location when ApprenticeshipLocationType includes ClassroomBased
                RuleFor(m => m)
                    .Must(m => (m.ApprenticeshipClassroomLocations?.Count ?? 0) > 0)
                    .When(m => m.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.ClassroomBased))
                    .WithMessageForAllRules("Add at least one classroom based location");
            }
        }
    }
}
