using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation.Results;
using MediatR;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.DeleteRowGroup
{
    public class Query : IRequest<ViewModel>
    {
        public int RowNumber { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, UploadStatus>>
    {
        public int RowNumber { get; set; }
        public bool Confirm { get; set; }
    }

    public class ViewModel : Command
    {
        public string LearnAimRef { get; set; }
        public string LearnAimRefTitle { get; set; }
        public IReadOnlyCollection<CourseDeliveryMode> DeliveryModes { get; set; }
        public IReadOnlyCollection<string> GroupErrorFields { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, UploadStatus>>
    {
        private static readonly CourseDeliveryMode[] _allDeliveryModes = new[]
        {
            CourseDeliveryMode.ClassroomBased,
            CourseDeliveryMode.Online,
            CourseDeliveryMode.WorkBased
        };

        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            IFileUploadProcessor fileUploadProcessor,
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) => CreateViewModel(request.RowNumber);

        public async Task<OneOf<ModelWithErrors<ViewModel>, UploadStatus>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!request.Confirm)
            {
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to delete the course")
                });
                return new ModelWithErrors<ViewModel>(await CreateViewModel(request.RowNumber), validationResult);
            }

            var providerId = _providerContextProvider.GetProviderId();
            var row = await _fileUploadProcessor.GetCourseUploadRowDetailForProvider(providerId, request.RowNumber);

            if (row == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.CourseUploadRow, request.RowNumber);
            }

            return await _fileUploadProcessor.DeleteCourseUploadRowGroupForProvider(providerId, row.CourseId);
        }

        private async Task<ViewModel> CreateViewModel(int rowNumber)
        {
            var providerId = _providerContextProvider.GetProviderId();

            var rootRow = await _fileUploadProcessor.GetCourseUploadRowDetailForProvider(providerId, rowNumber);

            if (rootRow == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.CourseUploadRow, rowNumber);
            }

            var rowGroup = await _fileUploadProcessor.GetCourseUploadRowGroupForProvider(providerId, rootRow.CourseId);
            var deliveryModes = DeduceDeliveryModes();

            var groupErrors = rowGroup.First().Errors
                .Where(e => Core.DataManagement.Errors.GetCourseErrorComponent(e) == CourseErrorComponent.Course)
                .ToArray();

            var learnAimRef = rowGroup.Select(r => r.LearnAimRef).Distinct().Single();
            var learningDelivery = (await _sqlQueryDispatcher.ExecuteQuery(new GetLearningDeliveries() { LearnAimRefs = new[] { learnAimRef } }))[learnAimRef];

            return new ViewModel()
            {
                DeliveryModes = deliveryModes,
                LearnAimRef = learnAimRef,
                LearnAimRefTitle = learningDelivery.LearnAimRefTitle,
                RowNumber = rowNumber,
                GroupErrorFields = groupErrors.Select(e => Core.DataManagement.Errors.MapCourseErrorToFieldGroup(e)).Distinct().ToArray()
            };

            IReadOnlyCollection<CourseDeliveryMode> DeduceDeliveryModes()
            {
                var deliveryModes = new HashSet<CourseDeliveryMode>();

                var allRowsHaveValidDeliveryMode = true;

                foreach (var row in rowGroup)
                {
                    var rowDeliveryMode = ParsedCsvCourseRow.ResolveDeliveryMode(row.DeliveryMode);

                    if (rowDeliveryMode != null)
                    {
                        deliveryModes.Add(rowDeliveryMode.Value);
                    }
                    else
                    {
                        allRowsHaveValidDeliveryMode = false;
                        break;
                    }
                }

                return (allRowsHaveValidDeliveryMode ? deliveryModes : (IEnumerable<CourseDeliveryMode>)_allDeliveryModes)
                    .OrderBy(dm => (int)dm)
                    .ToArray();
            }
        }
    }
}
