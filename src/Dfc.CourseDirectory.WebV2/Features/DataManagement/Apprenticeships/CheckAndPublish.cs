using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation.Results;
using FormFlow;
using MediatR;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Apprenticeships.CheckAndPublish
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
        public IReadOnlyCollection<ViewModelRowGroup> RowGroups { get; set; }
        public int RowCount { get; set; }
    }

    public class ViewModelRowGroup
    {
        public int StandardCode { get; set; }
        public int StandardVersion { get; set; }
        public string StandardName { get; set; }
        public IReadOnlyCollection<ApprenticeshipLocationType> LocationDeliveryMethods { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, OneOf<UploadHasErrors, ViewModel>>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, PublishResult>>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private readonly IStandardsCache _standardsCache;

        public Handler(
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor,
            ICurrentUserProvider currentUserProvider,
            JourneyInstanceProvider journeyInstanceProvider,
            IStandardsCache standardsCache)
        {
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
            _currentUserProvider = currentUserProvider;
            _journeyInstanceProvider = journeyInstanceProvider;
            _standardsCache = standardsCache;
        }

        public async Task<OneOf<UploadHasErrors, ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var (uploadRows, uploadStatus) = await _fileUploadProcessor.GetApprenticeshipUploadRowsForProvider(_providerContextProvider.GetProviderId());

            if (uploadStatus == UploadStatus.ProcessedWithErrors)
            {
                return new UploadHasErrors();
            }

            return await CreateViewModel(uploadRows);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, PublishResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            var providerId = _providerContextProvider.GetProviderId();

            if (!request.Confirm)
            {
                var (uploadRows, uploadStatus) = await _fileUploadProcessor.GetApprenticeshipUploadRowsForProvider(providerId);

                var vm = await CreateViewModel(uploadRows);
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to publish these apprenticeships")
                });
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            var publishResult = await _fileUploadProcessor.PublishApprenticeshipUploadForProvider(providerId, _currentUserProvider.GetCurrentUser());

            if (publishResult.Status == PublishResultStatus.Success)
            {
                var journeyInstance = _journeyInstanceProvider.GetOrCreateInstance(() => new PublishJourneyModel());
                journeyInstance.UpdateState(state => state.ApprenticeshipsPublished = publishResult.PublishedCount);
            }

            return publishResult;
        }

        private async Task<ViewModel> CreateViewModel(IReadOnlyCollection<ApprenticeshipUploadRow> rows)
        {
            var standards = (await _standardsCache.GetAllStandards())
                .ToDictionary(s => (s.StandardCode, s.Version), s => s);

            return new ViewModel()
            {
                RowGroups = rows
                    .GroupBy(t =>
                    {
                        return (ApprenticeshipId: t.ApprenticeshipId, Standard: standards[(t.StandardCode, t.StandardVersion)]);
                    })
                    .Select(g => new ViewModelRowGroup()
                    {
                        StandardCode = g.Key.Standard.StandardCode,
                        StandardVersion = g.Key.Standard.Version,
                        StandardName = g.Key.Standard.StandardName,
                        LocationDeliveryMethods = g
                            .Select(r => r.ResolvedDeliveryMethod.Value)
                            .Distinct()
                            .OrderBy(dm => (int)dm)
                            .ToArray()
                    })
                    .ToArray(),
                RowCount = rows.Count
            };
        }
    }
}
