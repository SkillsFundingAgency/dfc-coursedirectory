using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation.Results;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.DeleteUpload
{
    public class Query : IRequest<Command>
    {
        public bool IsNonLars { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public bool IsNonLars { get; set; }
        public bool Confirm { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;

        public Handler(IFileUploadProcessor fileUploadProcessor, IProviderContextProvider providerContextProvider)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken) =>
            Task.FromResult(new Command() { IsNonLars = request.IsNonLars });

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!request.Confirm)
            {
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to delete course data upload")
                });
                return new ModelWithErrors<Command>(new Command(), validationResult);
            }

            await _fileUploadProcessor.DeleteCourseUploadForProvider(_providerContextProvider.GetProviderId(), request.IsNonLars);

            return new Success();
        }
    }
}
