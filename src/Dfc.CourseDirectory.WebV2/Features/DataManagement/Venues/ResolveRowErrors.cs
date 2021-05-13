using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.VenueValidation;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.ResolveRowErrors
{
    public class Query : IRequest<OneOf<NotFound, ModelWithErrors<Command>>>
    {
        public int RowNumber { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, UploadStatus>>
    {
        public int RowNumber { get; set; }
        public string ProviderVenueRef { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Website { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, OneOf<NotFound, ModelWithErrors<Command>>>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, UploadStatus>>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<OneOf<NotFound, ModelWithErrors<Command>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var (row, otherRows) = await GetRow(request.RowNumber);

            var command = new Command()
            {
                RowNumber = request.RowNumber,
                ProviderVenueRef = row.ProviderVenueRef,
                Name = row.VenueName,
                AddressLine1 = row.AddressLine1,
                AddressLine2 = row.AddressLine2,
                Town = row.Town,
                County = row.County,
                Postcode = row.Postcode,
                Email = row.Email,
                Telephone = row.Telephone,
                Website = row.Website
            };

            var postcodeInfo = Postcode.TryParse(command.Postcode, out var postcode) ?
                await _sqlQueryDispatcher.ExecuteQuery(new GetPostcodeInfo() { Postcode = postcode }) :
                null;

            var validator = new CommandValidator(otherRows, postcodeInfo);
            var validationResult = await validator.ValidateAsync(command);

            return new ModelWithErrors<Command>(command, validationResult);
        }

        public async Task<OneOf<ModelWithErrors<Command>, UploadStatus>> Handle(Command request, CancellationToken cancellationToken)
        {
            var (_, otherRows) = await GetRow(request.RowNumber);

            var postcodeInfo = Postcode.TryParse(request.Postcode, out var postcode) ?
                await _sqlQueryDispatcher.ExecuteQuery(new GetPostcodeInfo() { Postcode = postcode }) :
                null;

            var validator = new CommandValidator(otherRows, postcodeInfo);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            return await _fileUploadProcessor.UpdateVenueUploadRowForProvider(
                _providerContextProvider.GetProviderId(),
                request.RowNumber,
                new Core.DataManagement.Schemas.CsvVenueRow()
                {
                    ProviderVenueRef = request.ProviderVenueRef,
                    VenueName = request.Name,
                    AddressLine1 = request.AddressLine1,
                    AddressLine2 = request.AddressLine2,
                    Town = request.Town,
                    County = request.County,
                    Postcode = request.Postcode,
                    Email = request.Email,
                    Telephone = request.Telephone,
                    Website = request.Website
                });
        }

        private async Task<(VenueUploadRow row, IReadOnlyCollection<VenueUploadRow> otherRows)> GetRow(int rowNumber)
        {
            var (uploadRows, uploadStatus) = await _fileUploadProcessor.GetVenueUploadRowsForProvider(_providerContextProvider.GetProviderId());

            if (uploadStatus != UploadStatus.ProcessedWithErrors)
            {
                throw new InvalidUploadStatusException(uploadStatus, UploadStatus.ProcessedWithErrors);
            }

            var row = uploadRows.SingleOrDefault(r => r.RowNumber == rowNumber);

            if (row == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.VenueUploadRow, rowNumber);
            }

            if (row.IsValid)
            {
                throw new InvalidStateException();
            }

            var otherRows = uploadRows.Except(new[] { row }).ToArray();

            return (row, otherRows);
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(
                IEnumerable<VenueUploadRow> otherRows,
                PostcodeInfo postcodeInfo)
            {
                RuleFor(c => c.ProviderVenueRef)
                    .ProviderVenueRef(_ => Task.FromResult(otherRows.Select(r => r.ProviderVenueRef)));

                RuleFor(c => c.Name)
                    .VenueName(_ => Task.FromResult(otherRows.Select(r => r.VenueName)));

                RuleFor(c => c.AddressLine1).AddressLine1();
                RuleFor(c => c.AddressLine2).AddressLine2();
                RuleFor(c => c.Town).Town();
                RuleFor(c => c.County).County();
                RuleFor(c => c.Postcode).Postcode(_ => postcodeInfo);
                RuleFor(c => c.Email).Email();
                RuleFor(c => c.Telephone).PhoneNumber();
                RuleFor(c => c.Website).Website();
            }
        }
    }
}
