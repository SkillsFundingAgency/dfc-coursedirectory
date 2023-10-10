using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.DeleteUpload
{
    public class Query : IRequest<Command>
    {
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public bool Confirm { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly ILogger<Handler> _log;
        private readonly IProviderContextProvider _providerContextProvider;

        public Handler(IFileUploadProcessor fileUploadProcessor, ILogger<Handler> log, IProviderContextProvider providerContextProvider)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _log = log;
            _providerContextProvider = providerContextProvider;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken) =>
            Task.FromResult(new Command());

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var providerId = _providerContextProvider.GetProviderId();
            _log.LogInformation($"Deleting venue upload for the provider: [{providerId}]");

            if (!request.Confirm)
            {
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to delete these venues")
                });
                _log.LogWarning($"Venue Upload not deleted. Confirmation required to delete venues for provider: [{providerId}].");

                return new ModelWithErrors<Command>(new Command(), validationResult);
            }

            await _fileUploadProcessor.DeleteVenueUploadForProvider(providerId);

            return new Success();
        }
    }
}
