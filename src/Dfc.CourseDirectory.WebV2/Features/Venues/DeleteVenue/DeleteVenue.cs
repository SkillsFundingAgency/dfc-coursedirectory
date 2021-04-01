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
using Venue = Dfc.CourseDirectory.Core.DataStore.Sql.Models.Venue;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.DeleteVenue
{
    [JourneyState]
    public class JourneyModel
    {
        public string VenueName { get; set; }
    }

    public class Query : IRequest<OneOf<NotFound, ViewModel>>
    {
        public Guid VenueId { get; set; }
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<OneOf<NotFound, ModelWithErrors<ViewModel>, Success>>
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
        IRequestHandler<Query, OneOf<NotFound, ViewModel>>,
        IRequestHandler<Command, OneOf<NotFound, ModelWithErrors<ViewModel>, Success>>,
        IRequestHandler<DeletedQuery, DeletedViewModel>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderInfoCache _providerInfoCache;
        private readonly IProviderOwnershipCache _providerOwnershipCache;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private readonly IClock _clock;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderInfoCache providerInfoCache,
            IProviderOwnershipCache providerOwnershipCache,
            ICurrentUserProvider currentUserProvider,
            JourneyInstanceProvider journeyInstanceProvider,
            IClock clock)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerInfoCache = providerInfoCache;
            _providerOwnershipCache = providerOwnershipCache;
            _currentUserProvider = currentUserProvider;
            _journeyInstanceProvider = journeyInstanceProvider;
            _clock = clock;
        }

        public async Task<OneOf<NotFound, ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var venue = await _sqlQueryDispatcher.ExecuteQuery(new GetVenue { VenueId = request.VenueId });

            if (venue == null)
            {
                return new NotFound();
            }

            var providerUkprn = (await _providerInfoCache.GetProviderInfo(venue.ProviderId)).Ukprn;

            var getCourses = _cosmosDbQueryDispatcher.ExecuteQuery(new GetAllCoursesForProvider
            {
                ProviderUkprn = providerUkprn,
                CourseStatuses = CourseStatus.Live | CourseStatus.Pending | CourseStatus.BulkUploadPending | CourseStatus.BulkUploadReadyToGoLive | CourseStatus.APIPending | CourseStatus.APIReadyToGoLive | CourseStatus.MigrationPending | CourseStatus.MigrationReadyToGoLive
            });
            
            var getApprenticeships = _cosmosDbQueryDispatcher.ExecuteQuery(new GetApprenticeships
            {
                Predicate = a => a.ProviderUKPRN == providerUkprn && a.ApprenticeshipLocations.Any(al =>
                    al.RecordStatus != (int)ApprenticeshipStatus.Archived
                    && al.RecordStatus != (int)ApprenticeshipStatus.Deleted
                    && al.VenueId == request.VenueId)
            });
            
            var getTLevels = _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.GetTLevelsForProvider
            {
                ProviderId = request.ProviderId
            });

            await Task.WhenAll(getCourses, getApprenticeships, getTLevels);

            return CreateViewModel(venue, request.ProviderId, await getCourses, (await getApprenticeships).Values, await getTLevels);
        }

        public async Task<OneOf<NotFound, ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var queryResult = await Handle(new Query { VenueId = request.VenueId, ProviderId = request.ProviderId }, cancellationToken);

            return await queryResult.Match(
                notFound => Task.FromResult<OneOf<NotFound, ModelWithErrors<ViewModel>, Success>>(notFound),
                async vm =>
                {
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

                    if (!(result.Value is Success))
                    {
                        return new NotFound();
                    }

                    _providerOwnershipCache.OnVenueDeleted(request.VenueId);

                    _journeyInstanceProvider.GetOrCreateInstance(() => new JourneyModel { VenueName = vm.VenueName })
                        .Complete();

                    return new Success();
                });
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

        private static ViewModel CreateViewModel(
            Venue venue,
            Guid providerId,
            IEnumerable<Course> courseRuns,
            IEnumerable<Apprenticeship> apprenticeships,
            IEnumerable<SqlModels.TLevel> tLevels) => new ViewModel
        {
            VenueId = venue.VenueId,
            ProviderId = providerId,
            AffectedCourses = GetAffectedCourses(courseRuns, venue.VenueId),
            AffectedApprenticeships = GetAffectedApprenticeships(apprenticeships, venue.VenueId),
            AffectedTLevels = GetAffectedTLevels(tLevels, venue.VenueId),
            VenueName = venue.VenueName,
            AddressParts = new[]
            {
                venue.AddressLine1,
                venue.AddressLine2,
                venue.Town,
                venue.County
            }.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray(),
            PostCode = venue.Postcode
        };

        private static IReadOnlyCollection<AffectedCourseViewModel> GetAffectedCourses(IEnumerable<Course> affectedCourses, Guid venueId) => affectedCourses
            .SelectMany(c =>
                c.CourseRuns.Where(cr => cr.VenueId == venueId && cr.RecordStatus != CourseStatus.Deleted && cr.RecordStatus != CourseStatus.Archived),
                (c, cr) => new AffectedCourseViewModel
                {
                    CourseId = c.Id,
                    CourseRunId = cr.Id,
                    CourseName = cr.CourseName
                })
            .ToArray();

        private static IReadOnlyCollection<AffectedApprenticeshipViewModel> GetAffectedApprenticeships(IEnumerable<Apprenticeship> affectedApprenticeships, Guid venueId) => affectedApprenticeships
            .Where(a => a.ApprenticeshipLocations.Any(al => al.VenueId == venueId && al.RecordStatus != (int)ApprenticeshipStatus.Deleted && al.RecordStatus != (int)ApprenticeshipStatus.Archived))
            .Select(
                a => new AffectedApprenticeshipViewModel
                {
                    ApprenticeshipId = a.Id,
                    ApprenticeshipName = a.ApprenticeshipTitle
                })
            .ToArray();

        private static IReadOnlyCollection<AffectedTLevelViewModel> GetAffectedTLevels(IEnumerable<SqlModels.TLevel> tLevels, Guid venueId) => tLevels
            .Where(t => t.Locations.Any(l => l.VenueId == venueId && l.TLevelLocationStatus != TLevelLocationStatus.Deleted))
            .Select(t =>
                new AffectedTLevelViewModel
                {
                    TLevelId = t.TLevelId,
                    TLevelName = t.TLevelDefinition.Name
                })
            .ToArray();

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
