using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.Upload
{
    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public IFormFile File { get; set; }
    }

    public class Handler : IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly IVenueUploadProcessor _venueUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(
            IVenueUploadProcessor venueUploadProcessor,
            IProviderContextProvider providerContextProvider,
            ICurrentUserProvider currentUserProvider)
        {
            _venueUploadProcessor = venueUploadProcessor;
            _providerContextProvider = providerContextProvider;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                return new ModelWithErrors<Command>(request, result);
            }

            using var stream = request.File.OpenReadStream();

            var saveFileResult = await _venueUploadProcessor.SaveFile(
                _providerContextProvider.GetProviderId(),
                stream,
                _currentUserProvider.GetCurrentUser());

            if (saveFileResult.Status == SaveFileResultStatus.InvalidFile)
            {
                return new ModelWithErrors<Command>(
                    request,
                    CreateValidationResultFromError("The selected file must be a CSV"));
            }
            else if (saveFileResult.Status == SaveFileResultStatus.InvalidHeader)
            {
                // TODO PTCD-920
                throw new NotImplementedException();
            }
            else if (saveFileResult.Status == SaveFileResultStatus.EmptyFile)
            {
                return new ModelWithErrors<Command>(
                    request,
                    CreateValidationResultFromError("The selected file is empty"));
            }

            Debug.Assert(saveFileResult.Status == SaveFileResultStatus.Success);

            return new Success();

            static ValidationResult CreateValidationResultFromError(string message) =>
                new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.File), message)
                });
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.File).NotNull().WithMessage("Select a CSV");
        }
    }
}
