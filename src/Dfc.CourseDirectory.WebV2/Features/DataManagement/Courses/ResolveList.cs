using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.Models;
using MediatR;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.ResolveList
{
    public class Query : IRequest<OneOf<UploadHasNoErrors, ViewModel>>
    {
    }

    public struct UploadHasNoErrors { }

    public class ViewModel
    {
        public IReadOnlyCollection<ViewModelCourse> ErrorRows { get; set; }
    }

    public class ViewModelCourse
    {
        public Guid CourseId { get; set; }
        public string LarsQan { get; set; }
        public string CourseName { get; set; }
        public IReadOnlyCollection<ViewModelRow> CourseRows { get; set; }
    }

    public class ViewModelRow
    {
        public int RowNumber { get; set; }
        public string ProviderCourseRef { get; set; }

        public string StartDate { get; set; }
        public string VenueName { get; set; }
        public string DeliveryMode { get; set; }
        public IReadOnlyCollection<string> ErrorFields { get; set; }
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
            var (uploadRows, uploadStatus) = await _fileUploadProcessor.GetCourseUploadRowsForProvider(
                _providerContextProvider.GetProviderId());

            if (uploadStatus == UploadStatus.ProcessedSuccessfully)
            {
                return new UploadHasNoErrors();
            }

            return new ViewModel()
            {
                ErrorRows = uploadRows
                    .Where(row => !row.IsValid)
                    .GroupBy(row => row.CourseId)
                    .Select(g => new ViewModelCourse()
                    {
                        CourseId = g.Key,
                        LarsQan = g.Select(p => p.LarsQan).FirstOrDefault(),
                        CourseName = g.Select(p => p.CourseName).FirstOrDefault(),
                        CourseRows = g.Select(r => new ViewModelRow()
                        {
                            //CourseId = row.Key,
                            ProviderCourseRef = r.ProviderCourseRef,
                            StartDate = r.StartDate,
                            VenueName = r.VenueName,
                            DeliveryMode = r.DeliveryMode,
                            ErrorFields = r.Errors.Select(e => Core.DataManagement.Errors.MapCourseErrorToFieldGroup(e)).Distinct().ToArray()
                        }).OrderBy(
                                    g => g.StartDate
                                ).ThenByDescending(
                                    g => g.DeliveryMode
                                ).ToArray()
                    }).OrderBy(
                        g => g.LarsQan
                    ).ThenByDescending(
                        g => g.CourseId
                    )

                    .ToArray()
            };
        }
    }
}
