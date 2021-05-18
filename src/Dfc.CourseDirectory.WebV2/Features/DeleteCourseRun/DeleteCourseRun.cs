using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation.Results;
using FormFlow;
using MediatR;
using OneOf;
using DeleteCourseRunQuery = Dfc.CourseDirectory.Core.DataStore.Sql.Queries.DeleteCourseRun;
using Venue = Dfc.CourseDirectory.Core.DataStore.Sql.Models.Venue;

namespace Dfc.CourseDirectory.WebV2.Features.DeleteCourseRun
{
    [JourneyState]
    public class JourneyModel
    {
        public string CourseName { get; set; }
        public Guid ProviderId { get; set; }
    }

    public class Request : IRequest<ViewModel>
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
    }

    public class ViewModel : Command
    {
        public string CourseName { get; set; }
        public string YourReference { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public bool FlexibleStartDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string VenueName { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, SuccessViewModel>>
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public bool Confirm { get; set; }
    }

    public class SuccessViewModel
    {
        public Guid CourseId { get; set; }
        public Guid CourseRunId { get; set; }
        public string CourseName { get; set; }
    }

    public class ConfirmedQuery : IRequest<ConfirmedViewModel>
    {
    }

    public class ConfirmedViewModel
    {
        public Guid ProviderId { get; set; }
        public string CourseName { get; set; }
        public bool HasOtherCourseRuns { get; set; }
    }

    public class Handler :
        IRequestHandler<Request, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, SuccessViewModel>>,
        IRequestHandler<ConfirmedQuery, ConfirmedViewModel>
    {
        private readonly IProviderOwnershipCache _providerOwnershipCache;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly JourneyInstance<JourneyModel> _journeyInstance;
        private readonly IClock _clock;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(
            IProviderOwnershipCache providerOwnershipCache,
            ISqlQueryDispatcher sqlQueryDispatcher,
            JourneyInstance<JourneyModel> journeyInstance,
            IClock clock,
            ICurrentUserProvider currentUserProvider)
        {
            _providerOwnershipCache = providerOwnershipCache;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _journeyInstance = journeyInstance;
            _clock = clock;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ViewModel> Handle(Request request, CancellationToken cancellationToken)
        {
            var (course, courseRun) = await GetCourseAndCourseRun(request.CourseId, request.CourseRunId);
            return await CreateViewModel(course, courseRun);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, SuccessViewModel>> Handle(Command request, CancellationToken cancellationToken)
        {
            var (course, courseRun) = await GetCourseAndCourseRun(request.CourseId, request.CourseRunId);

            if (!request.Confirm)
            {
                var vm = await CreateViewModel(course, courseRun);
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to delete the course")
                });
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            var providerId = (await _providerOwnershipCache.GetProviderForCourse(request.CourseId)).Value;

            await _sqlQueryDispatcher.ExecuteQuery(new DeleteCourseRunQuery()
            {
                CourseId = request.CourseId,
                CourseRunId = request.CourseRunId,
                DeletedBy = _currentUserProvider.GetCurrentUser(),
                DeletedOn = _clock.UtcNow,
            });

            // The next page needs this info - stash it in the JourneyModel
            // since it will no longer able to query for it.
            _journeyInstance.UpdateState(new JourneyModel()
            {
                CourseName = courseRun.CourseName,
                ProviderId = providerId
            });

            _journeyInstance.Complete();

            return new SuccessViewModel()
            {
                CourseId = request.CourseId,
                CourseRunId = request.CourseRunId,
                CourseName = courseRun.CourseName
            };
        }

        public async Task<ConfirmedViewModel> Handle(ConfirmedQuery request, CancellationToken cancellationToken)
        {
            var providerId = _journeyInstance.State.ProviderId;

            var liveCourses = await _sqlQueryDispatcher.ExecuteQuery(new GetCoursesForProvider()
            {
                ProviderId = providerId
            });

            return new ConfirmedViewModel()
            {
                ProviderId = _journeyInstance.State.ProviderId,
                CourseName = _journeyInstance.State.CourseName,
                HasOtherCourseRuns = liveCourses.Any()
            };
        }

        private async Task<ViewModel> CreateViewModel(Course course, CourseRun courseRun)
        {
            Venue venue = default;
            if (courseRun.VenueId.HasValue)
            {
                venue = await _sqlQueryDispatcher.ExecuteQuery(new GetVenue() { VenueId = courseRun.VenueId.Value });
            }

            return new ViewModel()
            {
                CourseId = course.CourseId,
                CourseRunId = courseRun.CourseRunId,
                CourseName = courseRun.CourseName,
                DeliveryMode = courseRun.DeliveryMode,
                FlexibleStartDate = courseRun.FlexibleStartDate,
                StartDate = courseRun.StartDate,
                VenueName = venue?.VenueName,
                YourReference = courseRun.ProviderCourseId
            };
        }

        private async Task<(Course Course, CourseRun CourseRun)> GetCourseAndCourseRun(Guid courseId, Guid courseRunId)
        {
            var course = await _sqlQueryDispatcher.ExecuteQuery(new GetCourse() { CourseId = courseId });

            if (course == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Course, courseId);
            }

            var courseRun = course.CourseRuns.SingleOrDefault(cr => cr.CourseRunId == courseRunId);

            if (courseRun == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.CourseRun, courseRunId);
            }

            return (course, courseRun);
        }
    }
}
