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
using Dfc.CourseDirectory.Core.Services;
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
        public bool IsNonLars { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, UploadStatus>>
    {        
        public CourseType? CourseType { get; set; }
        public string Sector { get; set; }
        public string AwardingBody { get; set; }
        public EducationLevel? EducationLevel { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public int RowNumber { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseRef { get; set; }
        public DateInput StartDate { get; set; }
        public bool? FlexibleStartDate { get; set; }
        public bool? NationalDelivery { get; set; }
        public IEnumerable<string> SubRegionIds { get; set; }
        public string CourseWebPage { get; set; }
        public bool IsSecureWebsite { get; set; }
        public string Cost { get; set; }        
        public string CostDescription { get; set; }
        public int? Duration { get; set; }
        public CourseDurationUnit? DurationUnit { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public Guid? VenueId { get; set; }
        public bool IsNonLars { get; set; }
        public List<Sector> Sectors { get; set; }
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
        private readonly IWebRiskService _webRiskService;

        public Handler(
            IFileUploadProcessor fileUploadProcessor,
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock,
            IRegionCache regionCache,
            IWebRiskService webRiskService)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
            _regionCache = regionCache;
            _webRiskService = webRiskService;
        }

        public async Task<ModelWithErrors<ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var row = await GetRow(request.RowNumber,request.IsNonLars);
            var vm = await CreateViewModel(request.DeliveryMode, row, request.IsNonLars);
            NormalizeViewModel();

            var allRegions = await _regionCache.GetAllRegions();

            var validator = new CommandValidator(_clock, allRegions, request.IsNonLars, _webRiskService);
            var validationResult = await validator.ValidateAsync(vm);

            return new ModelWithErrors<ViewModel>(vm, validationResult);

            void NormalizeViewModel()
            {
                if (request.DeliveryMode != CourseDeliveryMode.ClassroomBased && request.DeliveryMode != CourseDeliveryMode.BlendedLearning)
                {
                    vm.VenueId = null;
                    vm.StudyMode = null;
                    vm.AttendancePattern = null;
                }

                if (request.DeliveryMode != CourseDeliveryMode.WorkBased)
                {
                    vm.NationalDelivery = null;
                    vm.SubRegionIds = null;
                }

                // If mutually exclusive fields are specified, force the user to choose

                if (vm.FlexibleStartDate == true && !string.IsNullOrEmpty(row.StartDate))
                {
                    vm.FlexibleStartDate = null;
                }

                if (vm.NationalDelivery == true && !string.IsNullOrEmpty(row.SubRegions))
                {
                    vm.NationalDelivery = null;
                }
            }
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, UploadStatus>> Handle(Command request, CancellationToken cancellationToken)
        {
            var row = await GetRow(request.RowNumber, request.IsNonLars);

            NormalizeCommand();

            var allRegions = await _regionCache.GetAllRegions();

            var validator = new CommandValidator(_clock, allRegions, request.IsNonLars, _webRiskService);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = await CreateViewModel(request.DeliveryMode, row, request.IsNonLars);
                request.Adapt(vm);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            var status = await _fileUploadProcessor.UpdateCourseUploadRowForProvider(
                _providerContextProvider.GetProviderId(),
                row.RowNumber,
                request.IsNonLars,
                new CourseUploadRowUpdate()
                {
                    AwardingBody = request.AwardingBody,
                    EducationLevel = request.EducationLevel,
                    CourseType = request.CourseType,
                    Sector = request.Sector,
                    DeliveryMode = request.DeliveryMode,
                    CourseName = request.CourseName,
                    ProviderCourseRef = request.ProviderCourseRef,
                    StartDate = request.StartDate.ToDateTime(),
                    FlexibleStartDate = request.FlexibleStartDate.Value,
                    NationalDelivery = request.NationalDelivery,
                    SubRegionIds = request.SubRegionIds?.ToArray(),
                    CourseWebPage = request.CourseWebPage,
                    Cost = decimal.TryParse(request.Cost, out var cost) ? cost : (decimal?)null,
                    CostDescription = request.CostDescription,
                    Duration = request.Duration.Value,
                    DurationUnit = request.DurationUnit.Value,
                    StudyMode = request.StudyMode,
                    AttendancePattern = request.AttendancePattern,
                    VenueId = request.VenueId
                });
            return status;

            void NormalizeCommand()
            {
                // Some fields only apply under certain conditions; ensure we don't save fields that are not applicable

                if (request.DeliveryMode != CourseDeliveryMode.ClassroomBased && request.DeliveryMode != CourseDeliveryMode.BlendedLearning)
                {
                    request.VenueId = null;
                    request.StudyMode = null;
                    request.AttendancePattern = null;
                }

                if (request.DeliveryMode != CourseDeliveryMode.WorkBased)
                {
                    request.NationalDelivery = null;
                    request.SubRegionIds = null;
                }

                if (request.FlexibleStartDate == true)
                {
                    request.StartDate = null;
                }

                if (request.NationalDelivery == true)
                {
                    request.SubRegionIds = null;
                }
            }
        }

        private async Task<ViewModel> CreateViewModel(CourseDeliveryMode deliveryMode, CourseUploadRowDetail row, bool isNonLars)
        {
            var providerVenues = (deliveryMode == CourseDeliveryMode.ClassroomBased || deliveryMode == CourseDeliveryMode.BlendedLearning )?
                (await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = _providerContextProvider.GetProviderId() }))
                    .Select(v => new ViewModelProviderVenuesItem()
                    {
                        VenueId = v.VenueId,
                        VenueName = v.VenueName
                    })
                    .OrderBy(v => v.VenueName)
                    .ToArray() :
                null;

            var sectors = (await _sqlQueryDispatcher.ExecuteQuery(new GetSectors())).ToList();

            var vm = new ViewModel()
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
            if (isNonLars)
            { 
                vm.CourseType = row.ResolvedCourseType; 
                vm.EducationLevel = row.ResolvedEducationLevel;
                vm.AwardingBody = row.AwardingBody;
                vm.Sector = sectors.FirstOrDefault(s => s.Code.Equals(row.Sector, StringComparison.InvariantCultureIgnoreCase))?.Code ?? null;
                vm.Sectors = sectors;
            }
            return vm;
        }

        private async Task<CourseUploadRowDetail> GetRow(int rowNumber, bool isNonLars)
        {
            var providerId = _providerContextProvider.GetProviderId();

            var row = await _fileUploadProcessor.GetCourseUploadRowDetailForProvider(providerId, rowNumber, isNonLars);
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

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(IClock clock, IReadOnlyCollection<Region> allRegions, bool isNonLars, IWebRiskService webRiskService)
            {
                if (isNonLars)
                {
                    RuleFor(c => c.CourseType).CourseType();
                    RuleFor(c => c.Sector).Sector();
                    RuleFor(c => c.AwardingBody).AwardingBody();
                    RuleFor(c => c.EducationLevel).EducationLevel();
                }
                
                RuleFor(c => c.CourseName).CourseName();
                RuleFor(c => c.ProviderCourseRef).ProviderCourseRef();
                RuleFor(c => c.StartDate).StartDate(now: clock.UtcNow, getFlexibleStartDate: c => c.FlexibleStartDate);
                RuleFor(c => c.FlexibleStartDate).FlexibleStartDate();
                RuleFor(c => c.NationalDelivery).NationalDelivery(getDeliveryMode: c => c.DeliveryMode);
                RuleFor(c => c.CourseWebPage).CourseWebPage(webRiskService);
                RuleFor(c => c.Cost)
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

                RuleFor(c => c.SubRegionIds.ToArray())
                .SubRegions(allRegions: allRegions, subRegionsWereSpecified: c => c.SubRegionIds?.Count() > 0, getDeliveryMode: c => c.DeliveryMode, getNationalDelivery: c => c.NationalDelivery);

            }
        }
    }
}
