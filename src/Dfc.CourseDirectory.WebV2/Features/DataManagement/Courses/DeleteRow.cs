using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation.Results;
using MediatR;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.DeleteRow
{
    public class Query : IRequest<ViewModel>
    {
        public int RowNumber { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, UploadStatus>>
    {
        public bool Confirm { get; set; }
        public int RowNumber { get; set; }
    }

    public class ViewModel : Command
    {
        public string CourseName { get; set; }
        public string StartDate { get; set; }
        public string Errors { get; set; }
        public string DeliveryMode { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, UploadStatus>>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public Handler(
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor)
        {
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) => CreateViewModel(request.RowNumber);

        public async Task<OneOf<ModelWithErrors<ViewModel>, UploadStatus>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!request.Confirm)
            {
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to delete this course")
                });
                return new ModelWithErrors<ViewModel>(await CreateViewModel(request.RowNumber), validationResult);
            }

            return await _fileUploadProcessor.DeleteCourseUploadRowForProvider(_providerContextProvider.GetProviderId(), request.RowNumber);
        }

        private async Task<ViewModel> CreateViewModel(int rowNumber)
        {
            var row = await _fileUploadProcessor.GetCourseUploadRowDetailForProvider(_providerContextProvider.GetProviderId(), rowNumber);

            if (row == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.CourseUploadRow, rowNumber);
            }

            return new ViewModel()
            {
                RowNumber = rowNumber,
                CourseName = row.CourseName,
                DeliveryMode = row.DeliveryMode,
                Errors = GetUniqueErrorMessages(row),
                StartDate = row.StartDate
            };
        }

        private string GetUniqueErrorMessages(CourseUploadRow row)
        {
            var errors = row.Errors.Select(errorCode => Core.DataManagement.Errors.MapCourseErrorToFieldGroup(errorCode));
            return string.Join(",", errors.Distinct().ToList());
        }
    }
}
