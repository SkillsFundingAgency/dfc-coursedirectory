using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.ResolveRowDetails
{
    public class Query : IRequest<ModelWithErrors<ViewModel>>
    {
        public int RowNumber { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, UploadStatus>>
    {
        public CourseDeliveryMode DeliveryMode { get; set; }
        public int RowNumber { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseRef { get; set; }
        public DateInput StartDate { get; set; }
        public bool? FlexibleStartDate { get; set; }
        public bool? NationalDelivery { get; set; }
        public IEnumerable<string> SubRegionIds { get; set; }
        public string CourseWebPage { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }
        public int? Duration { get; set; }
        public CourseDurationUnit? DurationUnit { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public Guid? VenueId { get; set; }
    }

    public class ViewModel : Command
    {
        public IReadOnlyCollection<ViewModelProviderVenuesItem> ProviderVenues { get; set; }
    }

    public class ViewModelProviderVenuesItem
    {
        public Guid VenueId { get; set; }
        public string VenueName { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ModelWithErrors<ViewModel>>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, UploadStatus>>
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;
        private readonly IRegionCache _regionCache;

        public Handler(
            IFileUploadProcessor fileUploadProcessor,
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock,
            IRegionCache regionCache)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
            _regionCache = regionCache;
        }

        public async Task<ModelWithErrors<ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var row = await GetRow(request.RowNumber);
            var vm = await CreateViewModel(request.DeliveryMode, row);

            var allRegions = await _regionCache.GetAllRegions();

            NormalizeCommand(vm);

            var validator = new CommandValidator(_clock, allRegions);
            var validationResult = await validator.ValidateAsync(vm);

            return new ModelWithErrors<ViewModel>(vm, validationResult);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, UploadStatus>> Handle(Command request, CancellationToken cancellationToken)
        {
            var row = await GetRow(request.RowNumber);

            var allRegions = await _regionCache.GetAllRegions();

            NormalizeCommand(request);

            var validator = new CommandValidator(_clock, allRegions);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = await CreateViewModel(request.DeliveryMode, row);
                request.Adapt(vm);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            return await _fileUploadProcessor.UpdateCourseUploadRowForProvider(
                _providerContextProvider.GetProviderId(),
                row.RowNumber,
                new CourseUploadRowUpdate()
                {
                    DeliveryMode = request.DeliveryMode,
                    CourseName = request.CourseName,
                    ProviderCourseRef = request.ProviderCourseRef,
                    StartDate = request.StartDate.ToDateTime(),
                    FlexibleStartDate = request.FlexibleStartDate.Value,
                    NationalDelivery = request.NationalDelivery,
                    SubRegionIds = request.SubRegionIds?.ToArray(),
                    CourseWebPage = request.CourseWebPage,
                    Cost = decimal.Parse(request.Cost),
                    CostDescription = request.CostDescription,
                    Duration = request.Duration.Value,
                    DurationUnit = request.DurationUnit.Value,
                    StudyMode = request.StudyMode,
                    AttendancePattern = request.AttendancePattern,
                    VenueId = request.VenueId
                });
        }

        private async Task<ViewModel> CreateViewModel(CourseDeliveryMode deliveryMode, CourseUploadRowDetail row)
        {
            var providerVenues = deliveryMode == CourseDeliveryMode.ClassroomBased ?
                (await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = _providerContextProvider.GetProviderId() }))
                    .Select(v => new ViewModelProviderVenuesItem()
                    {
                        VenueId = v.VenueId,
                        VenueName = v.VenueName
                    })
                    .OrderBy(v => v.VenueName)
                    .ToArray() :
                null;

            return new ViewModel()
            {
                DeliveryMode = deliveryMode,
                RowNumber = row.RowNumber,
                CourseName = row.CourseName,
                ProviderCourseRef = row.ProviderCourseRef,
                StartDate = row.ResolvedStartDate,
                FlexibleStartDate = row.ResolvedFlexibleStartDate,
                NationalDelivery = row.ResolvedNationalDelivery,
                SubRegionIds = row.ResolvedSubRegionIds,
                CourseWebPage = row.CourseWebPage,
                Cost = ParsedCsvCourseRow.MapCost(row.ResolvedCost),
                CostDescription = row.CostDescription,
                Duration = row.ResolvedDuration,
                DurationUnit = row.ResolvedDurationUnit,
                StudyMode = row.ResolvedStudyMode,
                AttendancePattern = row.ResolvedAttendancePattern,
                VenueId = row.VenueId,
                ProviderVenues = providerVenues
            };
        }

        private async Task<CourseUploadRowDetail> GetRow(int rowNumber)
        {
            var providerId = _providerContextProvider.GetProviderId();

            var row = await _fileUploadProcessor.GetCourseUploadRowDetailForProvider(providerId, rowNumber);
            if (row == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.CourseUploadRow, rowNumber);
            }

            if (row.IsValid)
            {
                throw new InvalidStateException();
            }

            return row;
        }

        private void NormalizeCommand(Command command)
        {
            // Some fields only apply under certain conditions; ensure we ignore fields that are not applicable

            if (command.DeliveryMode != CourseDeliveryMode.ClassroomBased)
            {
                command.VenueId = null;
                command.StudyMode = null;
                command.AttendancePattern = null;
            }
            
            if (command.DeliveryMode != CourseDeliveryMode.WorkBased)
            {
                command.NationalDelivery = null;
                command.SubRegionIds = null;
            }

            if (command.FlexibleStartDate == true)
            {
                command.StartDate = null;
            }

            if (command.NationalDelivery == true)
            {
                command.SubRegionIds = null;
            }
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(IClock clock, IReadOnlyCollection<Region> allRegions)
            {
                RuleFor(c => c.CourseName).CourseName();
                RuleFor(c => c.ProviderCourseRef).ProviderCourseRef();
                RuleFor(c => c.StartDate).StartDate(now: clock.UtcNow, getFlexibleStartDate: c => c.FlexibleStartDate);
                RuleFor(c => c.FlexibleStartDate).FlexibleStartDate();
                RuleFor(c => c.NationalDelivery).NationalDelivery(getDeliveryMode: c => c.DeliveryMode);
                RuleFor(c => c.CourseWebPage).CourseWebPage();
                RuleFor(c => c.Cost)
                    .Transform(v => decimal.TryParse(v, out var parsed) ? parsed : (decimal?)null)
                    .Cost(costWasSpecified: c => !string.IsNullOrWhiteSpace(c.Cost), getCostDescription: c => c.CostDescription);
                RuleFor(c => c.CostDescription).CostDescription();
                RuleFor(c => c.Duration).Duration();
                RuleFor(c => c.DurationUnit).DurationUnit();
                RuleFor(c => c.VenueId).VenueId(getDeliveryMode: c => c.DeliveryMode);

                RuleFor(c => c.StudyMode).StudyMode(
                    studyModeWasSpecified: c => c.StudyMode.HasValue,
                    getDeliveryMode: c => c.DeliveryMode);

                RuleFor(c => c.AttendancePattern).AttendancePattern(
                    attendancePatternWasSpecified: c => c.AttendancePattern.HasValue,
                    getDeliveryMode: c => c.DeliveryMode);

                RuleFor(c => c.SubRegionIds)
                    .Transform(ids =>
                    {
                        return allRegions
                            .SelectMany(r => r.SubRegions)
                            .Join(ids ?? Array.Empty<string>(), sr => sr.Id, id => id, (sr, id) => sr)
                            .ToArray();
                    })
                    .SubRegions(
                        subRegionsWereSpecified: c => c.SubRegionIds?.Count() > 0,
                        getDeliveryMode: c => c.DeliveryMode,
                        getNationalDelivery: c => c.NationalDelivery);
            }
        }
    }
}
