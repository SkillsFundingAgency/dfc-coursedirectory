using System.Collections.Generic;
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
using Microsoft.Extensions.Logging;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.DeleteRow
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
        public string YourRef { get; set; }
        public string VenueName { get; set; }
        public string Address { get; set; }
        public string Errors { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, UploadStatus>>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ILogger<Handler> _log;
        private readonly IFileUploadProcessor _fileUploadProcessor;

        public Handler(
            ILogger<Handler> log,
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor)
        {
            _log = log;
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) => CreateViewModel(request.RowNumber);

        public async Task<OneOf<ModelWithErrors<ViewModel>, UploadStatus>> Handle(Command request, CancellationToken cancellationToken)
        {
            var providerId = _providerContextProvider.GetProviderId();

            _log.LogInformation($"Deleting Venue upload row for the provider: [{providerId}]");

            if (!request.Confirm)
            {
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to delete this venue")
                });
                _log.LogWarning($"Row not deleted. Confirmation required to delete venue for provider: [{providerId}].");

                return new ModelWithErrors<ViewModel>(await CreateViewModel(request.RowNumber), validationResult);
            }

            return await _fileUploadProcessor.DeleteVenueUploadRowForProvider(providerId, request.RowNumber);
        }

        private async Task<ViewModel> CreateViewModel(int rowNumber)
        {
            var providerId = _providerContextProvider.GetProviderId();
            _log.LogInformation($"Creating view model for venue upload row delete page.");

            var (rows, _) = await _fileUploadProcessor.GetVenueUploadRowsForProvider(providerId);

            var row = rows.SingleOrDefault(r => r.RowNumber == rowNumber);
            if (row == null)
            {
                _log.LogError($"Venue Upload Row [{rowNumber}] is null. Resource does not exist for provider: [{providerId}].");

                throw new ResourceDoesNotExistException(ResourceType.VenueUploadRow, rowNumber);
            }

            if (!row.IsDeletable)
            {
                _log.LogError($"Venue Upload Row [{rowNumber}] is not deletable for provider: [{providerId}].");

                throw new InvalidStateException(InvalidStateReason.VenueUploadRowCannotBeDeleted);
            }

            return new ViewModel()
            {
                RowNumber = rowNumber,
                Address = FormatAddress(row),
                Errors = GetUniqueErrorMessages(row),
                VenueName = row.VenueName,
                YourRef = row.ProviderVenueRef
            };
        }

        private string FormatAddress(VenueUploadRow row)
        {
            var addressParts = new List<string> { row.AddressLine1, row.AddressLine2, row.County, row.Postcode };
            var address = addressParts.Where(p => !string.IsNullOrEmpty(p)).ToList();
            return string.Join(",", address);
        }

        private string GetUniqueErrorMessages(VenueUploadRow row)
        {
            var errors = row.Errors.Select(errorCode => Core.DataManagement.Errors.MapVenueErrorToFieldGroup(errorCode));
            return string.Join(",", errors.Distinct().ToList());
        }
    }
}
