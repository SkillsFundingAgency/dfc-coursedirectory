﻿using System;
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
using FluentValidation.Results;
using FormFlow;
using MediatR;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.CheckAndPublish
{
    public class Query : IRequest<OneOf<UploadHasErrors, ViewModel>>
    {
        public bool IsNonLars { get; set; }
    }

    public struct UploadHasErrors { }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, PublishResult>>
    {
        public bool Confirm { get; set; }
        public bool IsNonLars { get; set; }
    }

    public class ViewModel : Command
    {
        public IReadOnlyCollection<ViewModelRowGroup> RowGroups { get; set; }
        public int RowCount { get; set; }
    }

    public class ViewModelRowGroup
    {
        public Guid CourseId { get; set; }
        public string LearnAimRef { get; set; }
        public IReadOnlyCollection<ViewModelRow> CourseRows { get; set; }
        public string LearnAimRefTitle { get; set; }
    }

    public class ViewModelRow
    {
        public string AwardingBody { get; set; }
        public string EducationLevel { get; set; }
        public string CourseType { get; set; }
        public string Sector { get; set; }
        public string DeliveryMode { get; set; }
        public string CourseName { get; set; }
        public string StartDate { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, OneOf<UploadHasErrors, ViewModel>>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, PublishResult>>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            IProviderContextProvider providerContextProvider,
            IFileUploadProcessor fileUploadProcessor,
            ICurrentUserProvider currentUserProvider,
            JourneyInstanceProvider journeyInstanceProvider,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _providerContextProvider = providerContextProvider;
            _fileUploadProcessor = fileUploadProcessor;
            _currentUserProvider = currentUserProvider;
            _journeyInstanceProvider = journeyInstanceProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<OneOf<UploadHasErrors, ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var (uploadRows, uploadStatus) = await _fileUploadProcessor.GetCourseUploadRowsForProvider(_providerContextProvider.GetProviderId(), request.IsNonLars);

            if (uploadStatus == UploadStatus.ProcessedWithErrors)
            {
                return new UploadHasErrors();
            }

            return await CreateViewModel(uploadRows, request.IsNonLars);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, PublishResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            var providerId = _providerContextProvider.GetProviderId();

            if (!request.Confirm)
            {
                var (uploadRows, uploadStatus) = await _fileUploadProcessor.GetCourseUploadRowsForProvider(providerId, request.IsNonLars);

                var vm = await CreateViewModel(uploadRows, request.IsNonLars);
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to publish these courses")
                });
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            var publishResult = await _fileUploadProcessor.PublishCourseUploadForProvider(providerId, _currentUserProvider.GetCurrentUser(), request.IsNonLars);

            if (publishResult.Status == PublishResultStatus.Success)
            {
                var journeyInstance = _journeyInstanceProvider.GetOrCreateInstance(() => new PublishJourneyModel());
                journeyInstance.UpdateState(state => state.CoursesPublished = publishResult.PublishedCount);
            }

            return publishResult;
        }

        private async Task<ViewModel> CreateViewModel(IReadOnlyCollection<CourseUploadRow> rows, bool isNonLars)
        {
            if (isNonLars)
            {
                return new ViewModel()
                {
                    RowGroups = rows
                        .GroupBy(t => t.CourseId)
                        .Select(g => new ViewModelRowGroup()
                        {
                            CourseId = g.Key,
                            CourseRows = g
                                .Select(r => new ViewModelRow()
                                {
                                    CourseType = r.CourseType,
                                    Sector = r.Sector,
                                    AwardingBody = r.AwardingBody,
                                    EducationLevel = r.EducationLevel,
                                    CourseName = r.CourseName,
                                    StartDate = r.StartDate,
                                    DeliveryMode = r.DeliveryMode
                                })
                                .OrderBy(r => r.StartDate)
                                .ThenBy(r => r.DeliveryMode)
                                .ToArray()
                        })
                        .OrderBy(g => g.CourseId)
                        .ToArray(),
                    RowCount = rows.Count
                };
            }
            else
            {
                var learningDelivery = (await _sqlQueryDispatcher.ExecuteQuery(new GetLearningDeliveries() { LearnAimRefs = rows.Select(x => x.LearnAimRef).Distinct().ToArray() }));

                return new ViewModel()
                {
                    RowGroups = rows
                        .GroupBy(t => t.CourseId)
                        .Select(g => new ViewModelRowGroup()
                        {
                            CourseId = g.Key,
                            LearnAimRef = g.Select(r => r.LearnAimRef).Distinct().Single(),
                            LearnAimRefTitle = learningDelivery[g.Select(r => r.LearnAimRef).Distinct().Single()].LearnAimRefTitle,
                            CourseRows = g
                                .Select(r => new ViewModelRow()
                                {
                                    CourseName = r.CourseName,
                                    StartDate = r.StartDate,
                                    DeliveryMode = r.DeliveryMode
                                })
                                .OrderBy(r => r.StartDate)
                                .ThenBy(r => r.DeliveryMode)
                                .ToArray()
                        })
                        .OrderBy(g => g.LearnAimRef)
                        .ThenBy(g => g.CourseId)
                        .ToArray(),
                    RowCount = rows.Count
                };
            }
        }
    }
}
