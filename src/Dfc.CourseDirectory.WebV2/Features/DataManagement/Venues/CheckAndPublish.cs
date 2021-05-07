using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.CheckAndPublish
{
    public class Query : IRequest<OneOf<UploadHasErrors, ViewModel>>
    {
    }

    public struct UploadHasErrors { }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, PublishResult>>
    {
        public bool Confirm { get; set; }
    }

    public class ViewModel : Command
    {
        public IReadOnlyCollection<ViewModelRow> UploadedRows { get; set; }
        public IReadOnlyCollection<ViewModelRow> SupplementaryRows { get; set; }
        public int UploadedRowCount { get; set; }
        public int SupplementaryRowCount { get; set; }
        public int TotalRowCount { get; set; }
    }

    public class ViewModelRow
    {
        public string ProviderVenueReference { get; set; }
        public string VenueName { get; set; }
        public IReadOnlyCollection<string> AddressParts { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, OneOf<UploadHasErrors, ViewModel>>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, PublishResult>>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;

        public Handler(
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor,
            ICurrentUserProvider currentUserProvider,
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory)
        {
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
            _currentUserProvider = currentUserProvider;
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
        }

        public async Task<OneOf<UploadHasErrors, ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var providerId = _providerContextProvider.GetProviderId();

                var venueUpload = await dispatcher.ExecuteQuery(new GetLatestVenueUploadForProviderWithStatus()
                {
                    ProviderId = providerId,
                    Statuses = new[] { UploadStatus.ProcessedSuccessfully, UploadStatus.ProcessedWithErrors }
                });

                if (venueUpload == null)
                {
                    throw new InvalidStateException();
                }

                if (venueUpload.UploadStatus == UploadStatus.ProcessedWithErrors)
                {
                    return new UploadHasErrors();
                }

                return await CreateViewModel(venueUpload.VenueUploadId, dispatcher);
            }
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, PublishResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            Guid venueUploadId;

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var providerId = _providerContextProvider.GetProviderId();

                var venueUpload = await dispatcher.ExecuteQuery(new GetLatestVenueUploadForProviderWithStatus()
                {
                    ProviderId = providerId,
                    Statuses = new[] { UploadStatus.ProcessedSuccessfully }
                });

                if (venueUpload == null)
                {
                    throw new InvalidStateException();
                }

                if (!request.Confirm)
                {
                    var vm = await CreateViewModel(venueUpload.VenueUploadId, dispatcher);
                    var validationResult = new ValidationResult(new[]
                    {
                        new ValidationFailure(nameof(request.Confirm), "Confirm you want to publish these venues")
                    });
                    return new ModelWithErrors<ViewModel>(vm, validationResult);
                }

                venueUploadId = venueUpload.VenueUploadId;
            }

            return await _fileUploadProcessor.PublishVenueUpload(venueUploadId, _currentUserProvider.GetCurrentUser());
        }

        private async Task<ViewModel> CreateViewModel(Guid venueUploadId, ISqlQueryDispatcher dispatcher)
        {
            var rows = await dispatcher.ExecuteQuery(new GetVenueUploadRows()
            {
                VenueUploadId = venueUploadId
            });

            var uploadedRows = rows.Where(r => !r.IsSupplementary).Select(MapRow).ToArray();
            var supplementaryRows = rows.Where(r => r.IsSupplementary).Select(MapRow).ToArray();

            return new ViewModel()
            {
                UploadedRows = uploadedRows,
                UploadedRowCount = uploadedRows.Length,
                SupplementaryRows = supplementaryRows,
                SupplementaryRowCount = supplementaryRows.Length,
                TotalRowCount = uploadedRows.Length + supplementaryRows.Length,
            };

            static ViewModelRow MapRow(VenueUploadRow row) => new ViewModelRow()
            {
                ProviderVenueReference = row.ProviderVenueRef,
                VenueName = row.VenueName,
                AddressParts = new[]
                {
                        row.AddressLine1,
                        row.AddressLine2,
                        row.Town,
                        row.County,
                        row.Postcode
                    }
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToArray()
            };
        }
    }
}
