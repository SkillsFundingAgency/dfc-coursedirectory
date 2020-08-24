using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Behaviors;
using FluentValidation.Results;
using MediatR;
using OneOf;
using DeleteCourseRunQuery = Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries.DeleteCourseRun;

namespace Dfc.CourseDirectory.WebV2.Features.Courses.DeleteCourseRun
{
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

    public class Handler :
        IRequireUserCanAccessCourse<Request>,
        IRequestHandler<Request, ViewModel>,
        IRequireUserCanAccessCourse<Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, SuccessViewModel>>
    {
        private readonly IProviderInfoCache _providerInfoCache;
        private readonly IProviderOwnershipCache _providerOwnershipCache;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public Handler(
            IProviderInfoCache providerInfoCache,
            IProviderOwnershipCache providerOwnershipCache,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _providerInfoCache = providerInfoCache;
            _providerOwnershipCache = providerOwnershipCache;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Request request, CancellationToken cancellationToken)
        {
            var ukprn = await GetUkprnForCourse(request.CourseId);
            var (course, courseRun) = await GetCourseAndCourseRun(request.CourseId, request.CourseRunId, ukprn);
            return await CreateViewModel(course, courseRun);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, SuccessViewModel>> Handle(Command request, CancellationToken cancellationToken)
        {
            var ukprn = await GetUkprnForCourse(request.CourseId);
            var (course, courseRun) = await GetCourseAndCourseRun(request.CourseId, request.CourseRunId, ukprn);

            if (!request.Confirm)
            {
                var vm = await CreateViewModel(course, courseRun);
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to delete the course")
                });
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            await _cosmosDbQueryDispatcher.ExecuteQuery(new DeleteCourseRunQuery()
            {
                CourseId = request.CourseId,
                CourseRunId = request.CourseRunId,
                ProviderUkprn = ukprn
            });

            return new SuccessViewModel()
            {
                CourseId = request.CourseId,
                CourseRunId = request.CourseRunId,
                CourseName = courseRun.CourseName
            };
        }

        private async Task<ViewModel> CreateViewModel(Course course, CourseRun courseRun)
        {
            Venue venue = default;
            if (courseRun.VenueId.HasValue)
            {
                venue = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetVenueById() { VenueId = courseRun.VenueId.Value });
            }

            return new ViewModel()
            {
                CourseId = course.Id,
                CourseRunId = courseRun.Id,
                CourseName = courseRun.CourseName,
                DeliveryMode = courseRun.DeliveryMode,
                FlexibleStartDate = courseRun.FlexibleStartDate,
                StartDate = courseRun.StartDate,
                VenueName = venue?.VenueName,
                YourReference = courseRun.ProviderCourseID
            };
        }

        private async Task<(Course Course, CourseRun CourseRun)> GetCourseAndCourseRun(Guid courseId, Guid courseRunId, int ukprn)
        {
            (await _cosmosDbQueryDispatcher.ExecuteQuery(new GetCoursesByIds()
            {
                CourseIds = new[] { courseId },
                Ukprn = ukprn
            })).TryGetValue(courseId, out var course);

            if (course == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Course, courseId);
            }

            var courseRun = course.CourseRuns.SingleOrDefault(cr => cr.Id == courseRunId);

            if (courseRun == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.CourseRun, courseRunId);
            }

            return (course, courseRun);
        }

        private async Task<int> GetUkprnForCourse(Guid courseId)
        {
            var providerId = await _providerOwnershipCache.GetProviderForCourse(courseId);

            if (!providerId.HasValue)
            {
                throw new ResourceDoesNotExistException(ResourceType.Course, courseId);
            }

            var providerInfo = await _providerInfoCache.GetProviderInfo(providerId.Value);
            return providerInfo.Ukprn;
        }

        Guid IRequireUserCanAccessCourse<Request>.GetCourseId(Request request) => request.CourseId;

        Guid IRequireUserCanAccessCourse<Command>.GetCourseId(Command request) => request.CourseId;
    }
}
