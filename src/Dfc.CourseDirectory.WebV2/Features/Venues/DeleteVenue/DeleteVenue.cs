using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using FormFlow;
using MediatR;
using OneOf;
using OneOf.Types;
using SqlModels = Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using SqlQueries = Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.DeleteVenue
{
    [JourneyState]
    public class JourneyModel
    {
        public string VenueName { get; set; }
    }

    public class Query : IRequest<ViewModel>
    {
        public Guid VenueId { get; set; }
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid VenueId { get; set; }
        public Guid ProviderId { get; set; }
        public bool Confirm { get; set; }
        public IReadOnlyCollection<AffectedCourseViewModel> AffectedCourses { get; set; }
        public IReadOnlyCollection<AffectedApprenticeshipViewModel> AffectedApprenticeships { get; set; }
        public IReadOnlyCollection<AffectedTLevelViewModel> AffectedTLevels { get; set; }
    }

    public class ViewModel : Command
    {
        public string VenueName { get; set; }
        public IReadOnlyCollection<string> AddressParts { get; set; }
        public string PostCode { get; set; }
    }

    public class AffectedCourseViewModel
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public string CourseName { get; set; }
    }

    public class AffectedApprenticeshipViewModel
    {
        public Guid ApprenticeshipId { get; set; }
        public string ApprenticeshipName { get; set; }
    }

    public class AffectedTLevelViewModel
    {
        public Guid TLevelId { get; set; }
        public string TLevelName { get; set; }
    }

    public class DeletedQuery : IRequest<DeletedViewModel>
    {
    }

    public class DeletedViewModel
    {
        public string VenueName { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>,
        IRequestHandler<DeletedQuery, DeletedViewModel>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderOwnershipCache _providerOwnershipCache;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private readonly IClock _clock;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderOwnershipCache providerOwnershipCache,
            ICurrentUserProvider currentUserProvider,
            JourneyInstanceProvider journeyInstanceProvider,
            IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerOwnershipCache = providerOwnershipCache;
            _currentUserProvider = currentUserProvider;
            _journeyInstanceProvider = journeyInstanceProvider;
            _clock = clock;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) => CreateViewModel(request.VenueId);

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var vm = await CreateViewModel(request.VenueId);
            var validationResult = await new CommandValidator(vm).ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            var result = await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.DeleteVenue()
            {
                VenueId = request.VenueId,
                DeletedOn = _clock.UtcNow,
                DeletedBy = _currentUserProvider.GetCurrentUser()
            });

            if (result.Value is NotFound)
            {
                throw new ResourceDoesNotExistException(ResourceType.Venue, request.VenueId);
            }

            _providerOwnershipCache.OnVenueDeleted(request.VenueId);

            _journeyInstanceProvider.GetOrCreateInstance(() => new JourneyModel { VenueName = vm.VenueName })
                .Complete();

            return new Success();
        }

        public Task<DeletedViewModel> Handle(DeletedQuery request, CancellationToken cancellationToken)
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<JourneyModel>();
            journeyInstance.ThrowIfNotCompleted();

            return Task.FromResult(new DeletedViewModel
            {
                VenueName = journeyInstance.State.VenueName
            });
        }

        private async Task<ViewModel> CreateViewModel(Guid venueId)
        {
            var offeringInfo = await _sqlQueryDispatcher.ExecuteQuery(new GetVenueOfferingInfo() { VenueId = venueId });

            if (offeringInfo == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Venue, venueId);
            }

            var providerId = offeringInfo.Venue.ProviderId;

            var linkedCourses = offeringInfo.LinkedCourses.Count > 0 ?
                (await _sqlQueryDispatcher.ExecuteQuery(new GetCoursesForProvider() { ProviderId = providerId })) :
                Array.Empty<SqlModels.Course>();

            var linkedApprenticeships = offeringInfo.LinkedApprenticeships.Count > 0 ?
                (await _cosmosDbQueryDispatcher.ExecuteQuery(
                    new GetApprenticeships()
                    {
                        Predicate = a => a.ProviderUKPRN == offeringInfo.Venue.ProviderUkprn &&
                            a.ApprenticeshipLocations.Any(al => al.VenueId == venueId && al.RecordStatus != (int)ApprenticeshipStatus.Deleted)
                    })) :
                new Dictionary<Guid, Apprenticeship>();

            var linkedTLevels = offeringInfo.LinkedTLevels.Count > 0 ?
                (await _sqlQueryDispatcher.ExecuteQuery(new GetTLevelsForProvider() { ProviderId = providerId })) :
                Array.Empty<TLevel>();

            return new ViewModel()
            {
                VenueId = offeringInfo.Venue.VenueId,
                ProviderId = offeringInfo.Venue.ProviderId,
                AffectedCourses = offeringInfo.LinkedCourses
                    .Select(c => new AffectedCourseViewModel()
                    {
                        CourseId = c.CourseId,
                        CourseRunId = c.CourseRunId,
                        CourseName = linkedCourses
                            .Single(x => c.CourseId == x.CourseId)
                            .CourseRuns.Single(cr => cr.CourseRunId == c.CourseRunId)
                            .CourseName
                    })
                    .ToArray(),
                AffectedApprenticeships = offeringInfo.LinkedApprenticeships
                    .Select(a => new AffectedApprenticeshipViewModel()
                    {
                        ApprenticeshipId = a.ApprenticeshipId,
                        ApprenticeshipName = linkedApprenticeships[a.ApprenticeshipId].ApprenticeshipTitle
                    })
                    .ToArray(),
                AffectedTLevels = offeringInfo.LinkedTLevels
                    .Select(t => new AffectedTLevelViewModel()
                    {
                        TLevelId = t.TLevelId,
                        TLevelName = linkedTLevels
                            .Single(tl => tl.TLevelId == t.TLevelId)
                            .TLevelDefinition.Name
                    })
                    .ToArray(),
                VenueName = offeringInfo.Venue.VenueName,
                AddressParts = new[]
                {
                    offeringInfo.Venue.AddressLine1,
                    offeringInfo.Venue.AddressLine2,
                    offeringInfo.Venue.Town,
                    offeringInfo.Venue.County
                }.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray(),
                PostCode = offeringInfo.Venue.Postcode
            };
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(ViewModel viewModel)
            {
                RuleFor(c => c.AffectedCourses)
                    .Must(_ => viewModel.AffectedCourses.Count <= 0)
                    .WithMessage("The affected courses have changed");

                RuleFor(c => c.AffectedApprenticeships)
                    .Must(_ => viewModel.AffectedApprenticeships.Count <= 0)
                    .WithMessage("The affected apprenticeships have changed");

                RuleFor(c => c.AffectedTLevels)
                    .Must(_ => viewModel.AffectedTLevels.Count <= 0)
                    .WithMessage("The affected T Levels have changed");

                RuleFor(c => c.Confirm)
                    .Equal(true)
                    .WithMessage("Confirm you want to delete the location");
            }
        }
    }
}
