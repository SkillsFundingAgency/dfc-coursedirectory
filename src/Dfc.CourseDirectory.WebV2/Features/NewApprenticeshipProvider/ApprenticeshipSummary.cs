using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.ApprenticeshipSummary
{
    public class Query : IRequest<OneOf<ModelWithErrors<ViewModel>, ViewModel>>
    {
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
        public string Name { get; set; }
        public IReadOnlyCollection<string> SubRegionNames { get; set; }
    }

    public class ViewModelClassroomBasedLocation
    {
        public Guid VenueId { get; set; }
        public string VenueName { get; set; }
        public int Radius { get; set; }
        public IEnumerable<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }
    }

    public class CompleteCommand : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
    }

    public class Handler :
        IRequestHandler<Query, OneOf<ModelWithErrors<ViewModel>, ViewModel>>,
        IRequestHandler<CompleteCommand, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly MptxInstanceContext<FlowModel> _flow;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IRegionCache _regionCache;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        public Handler(
            MptxInstanceContext<FlowModel> flow,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IRegionCache regionCache,
            IProviderContextProvider providerContextProvider,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _flow = flow;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _regionCache = regionCache;
            _providerContextProvider = providerContextProvider;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, ViewModel>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            ValidateFlowState();

            var providerId = _providerContextProvider.GetProviderId();

            ViewModel vm = await CreateViewModel(providerId);

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

            var providerId = _providerContextProvider.GetProviderId();

            var validator = new CompleteValidator();
            var validationResult = await validator.ValidateAsync(_flow.State);

            if (!validationResult.IsValid)
            {
                var vm = await CreateViewModel(providerId);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            var currentUser = _currentUserProvider.GetCurrentUser();
            var now = _clock.UtcNow;

            var apprenticeshipId = _flow.State.ApprenticeshipId.GetValueOrDefault(Guid.NewGuid());

            var providerVenues = await _sqlQueryDispatcher.ExecuteQuery(
                new GetVenuesByProvider()
                {
                    ProviderId = providerId
                });

            var locations = CreateLocations();

            if (_flow.State.ApprenticeshipId.HasValue)
            {
                // Update existing apprenticeship
                await _sqlQueryDispatcher.ExecuteQuery(
                    new UpdateApprenticeship()
                    {
                        ApprenticeshipId = apprenticeshipId,
                        MarketingInformation = _flow.State.ApprenticeshipMarketingInformation,
                        ApprenticeshipWebsite = _flow.State.ApprenticeshipWebsite,
                        ContactEmail = _flow.State.ApprenticeshipContactEmail,
                        ContactTelephone = _flow.State.ApprenticeshipContactTelephone,
                        ContactWebsite = _flow.State.ApprenticeshipContactWebsite,
                        ApprenticeshipLocations = locations,
                        UpdatedBy = currentUser,
                        UpdatedOn = now,
                        Standard = _flow.State.ApprenticeshipStandard
                    });
            }
            else
            {
                // Create apprenticeship
                await _sqlQueryDispatcher.ExecuteQuery(
                    new CreateApprenticeship()
                    {
                        ApprenticeshipId = apprenticeshipId,
                        ProviderId = providerId,
                        MarketingInformation = _flow.State.ApprenticeshipMarketingInformation,
                        ApprenticeshipWebsite = _flow.State.ApprenticeshipWebsite,
                        ContactEmail = _flow.State.ApprenticeshipContactEmail,
                        ContactTelephone = _flow.State.ApprenticeshipContactTelephone,
                        ContactWebsite = _flow.State.ApprenticeshipContactWebsite,
                        ApprenticeshipLocations = locations,
                        Status = ApprenticeshipStatus.Pending,
                        CreatedBy = currentUser,
                        CreatedOn = now,
                        Standard = _flow.State.ApprenticeshipStandard,
                    });
            }

            // Create QA submission
            await _sqlQueryDispatcher.ExecuteQuery(
                new CreateApprenticeshipQASubmission()
                {
                    ProviderId = providerId,
                    Apprenticeships = new List<CreateApprenticeshipQASubmissionApprenticeship>()
                    {
                        new CreateApprenticeshipQASubmissionApprenticeship()
                        {
                            ApprenticeshipId = apprenticeshipId,
                            ApprenticeshipMarketingInformation = _flow.State.ApprenticeshipMarketingInformation,
                            ApprenticeshipTitle = _flow.State.ApprenticeshipStandard.StandardName,
                            Locations = locations.Select(l => l.ApprenticeshipLocationType switch
                            {
                                ApprenticeshipLocationType.ClassroomBased => new CreateApprenticeshipQASubmissionApprenticeshipLocation(
                                    new CreateApprenticeshipQASubmissionApprenticeshipClassroomLocation()
                                    {
                                        DeliveryModes = l.DeliveryModes,
                                        Radius = l.Radius.Value,
                                        VenueName = providerVenues.Single(v => v.VenueId == l.VenueId).VenueName
                                    }),
                                ApprenticeshipLocationType.EmployerBased => new CreateApprenticeshipQASubmissionApprenticeshipLocation(
                                    l.National == true ?
                                        new CreateApprenticeshipQASubmissionApprenticeshipEmployerLocation(new National()) :
                                        new CreateApprenticeshipQASubmissionApprenticeshipEmployerLocation(
                                            new CreateApprenticeshipQASubmissionApprenticeshipEmployerLocationRegions()
                                            {
                                                SubRegionIds = l.SubRegionIds.ToArray()
                                            })),
                                _ => throw new NotSupportedException()
                            }).ToList()
                        }
                    },
                    ProviderMarketingInformation = _flow.State.ProviderMarketingInformation,
                    SubmittedByUserId = currentUser.UserId,
                    SubmittedOn = now
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId,
                    ApprenticeshipQAStatus = ApprenticeshipQAStatus.Submitted
                });

            // Ensure user cannot go 'back' to any part of this flow
            _flow.Complete();

            return new Success();

            IEnumerable<CreateApprenticeshipLocation> CreateLocations()
            {
                var locations = new List<CreateApprenticeshipLocation>();

                var locationType = _flow.State.ApprenticeshipLocationType.Value;

                if (locationType.HasFlag(ApprenticeshipLocationType.EmployerBased))
                {
                    locations.Add(_flow.State.ApprenticeshipIsNational.Value ?
                        CreateApprenticeshipLocation.CreateNationalEmployerBased() :
                        CreateApprenticeshipLocation.CreateRegionalEmployerBased(_flow.State.ApprenticeshipLocationSubRegionIds));
                }

                if (locationType.HasFlag(ApprenticeshipLocationType.ClassroomBased))
                {
                    locations.AddRange(_flow.State.ApprenticeshipClassroomLocations.Select(l =>
                        CreateApprenticeshipLocation.CreateClassroomBased(l.Value.DeliveryModes, l.Value.Radius, l.Value.VenueId)));
                }

                return locations;
            }
        }

        private async Task<ViewModel> CreateViewModel(Guid providerId)
        {
            var providerVenues = await _sqlQueryDispatcher.ExecuteQuery(
                new GetVenuesByProvider()
                {
                    ProviderId = providerId
                });

            var regions = await _regionCache.GetAllRegions();

            return new ViewModel()
            {
                ProviderId = providerId,
                StandardOrFrameworkTitle = _flow.State.ApprenticeshipStandard.StandardName,
                MarketingInformation = _flow.State.ApprenticeshipMarketingInformation,
                Website = _flow.State.ApprenticeshipWebsite,
                ContactTelephone = _flow.State.ApprenticeshipContactTelephone,
                ContactEmail = _flow.State.ApprenticeshipContactEmail,
                ContactWebsite = _flow.State.ApprenticeshipContactWebsite,
                ApprenticeshipLocationType = _flow.State.ApprenticeshipLocationType.Value,
                ApprenticeshipIsNational = _flow.State.ApprenticeshipIsNational,
                EmployerBasedLocationRegions = _flow.State.ApprenticeshipLocationSubRegionIds != null ?
                    GroupSubRegions() : null,
                ClassroomBasedLocations = _flow.State.ApprenticeshipClassroomLocations
                    ?.Values
                    ?.Select(l => new ViewModelClassroomBasedLocation()
                    {
                        DeliveryModes = l.DeliveryModes,
                        Radius = l.Radius,
                        VenueId = l.VenueId,
                        VenueName = providerVenues.Single(v => v.VenueId == l.VenueId).VenueName
                    })
                    ?.OrderBy(l => l.VenueName)
                    ?.ToList()
            };

            IReadOnlyCollection<ViewModelEmployerBasedLocationRegion> GroupSubRegions() => regions
                .SelectMany(r => r.SubRegions.Select(sr => new { SubRegion = sr, Region = r }))
                .Where(r => _flow.State.ApprenticeshipLocationSubRegionIds.Contains(r.SubRegion.Id))
                .GroupBy(x => x.Region)
                .Select(g => new ViewModelEmployerBasedLocationRegion()
                {
                    Name = g.Key.Name,
                    SubRegionNames = g.Select(sr => sr.SubRegion.Name).OrderBy(sr => sr).ToList()
                })
                .OrderBy(g => g.Name)
                .ToList();
        }

        private void ValidateFlowState()
        {
            // Require everything to be set here to create a valid apprenticeship
            // *except* classroom locations - we need to handle that being empty

            var state = _flow.State;

            var isValid = state.GotProviderDetails &&
                state.ApprenticeshipStandard != null &&
                state.ApprenticeshipLocationType != null &&
                (
                    (state.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.ClassroomBased)) ||
                    (state.ApprenticeshipLocationType.Value.HasFlag(ApprenticeshipLocationType.EmployerBased) &&
                        (state.ApprenticeshipIsNational.GetValueOrDefault() || (state.ApprenticeshipLocationSubRegionIds?.Count ?? 0) > 0))) &&
                state.GotApprenticeshipDetails;

            if (!isValid)
            {
                throw new InvalidStateException();
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
