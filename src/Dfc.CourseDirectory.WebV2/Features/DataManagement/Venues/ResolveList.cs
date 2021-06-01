using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.ResolveList
{
    public class Query : IRequest<OneOf<UploadHasNoErrors, ViewModel>>
    {
    }

    public struct UploadHasNoErrors { }

    public class ViewModel
    {
        public IReadOnlyCollection<ViewModelRow> ErrorRows { get; set; }
    }

    public class ViewModelRow
    {
        public int RowNumber { get; set; }
        public string ProviderVenueReference { get; set; }
        public string VenueName { get; set; }
        public IReadOnlyCollection<string> AddressParts { get; set; }
        public IReadOnlyCollection<string> ErrorFields { get; set; }
        public bool IsDeletable { get; set; }
    }

    public class Handler : IRequestHandler<Query, OneOf<UploadHasNoErrors, ViewModel>>
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;

        public Handler(IFileUploadProcessor fileUploadProcessor, IProviderContextProvider providerContextProvider)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
        }

        public async Task<OneOf<UploadHasNoErrors, ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var (uploadRows, uploadStatus) = await _fileUploadProcessor.GetVenueUploadRowsForProvider(
                _providerContextProvider.GetProviderId());

            if (uploadStatus == UploadStatus.ProcessedSuccessfully)
            {
                return new UploadHasNoErrors();
            }

            return new ViewModel()
            {
                ErrorRows = uploadRows
                    .Where(row => !row.IsValid)
                    .Select(row => new ViewModelRow()
                    {
                        RowNumber = row.RowNumber,
                        ProviderVenueReference = row.ProviderVenueRef,
                        VenueName = row.VenueName,
                        AddressParts =
                            new[]
                            {
                                row.AddressLine1,
                                row.AddressLine2,
                                row.Town,
                                row.County,
                                row.Postcode
                            }
                            .Where(part => !string.IsNullOrWhiteSpace(part))
                            .ToArray(),
                        ErrorFields = row.Errors.Select(e => Core.DataManagement.Errors.MapVenueErrorToFieldGroup(e)).Distinct().ToArray(),
                        IsDeletable = row.IsDeletable
                    })
                    .ToArray()
            };
        }
    }
}
