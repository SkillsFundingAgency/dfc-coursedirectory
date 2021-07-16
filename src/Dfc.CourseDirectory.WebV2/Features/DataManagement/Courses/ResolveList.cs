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
        public IReadOnlyCollection<ViewModelRow> ErrorRows { get; set; }
    }

    public class ViewModelRow
    {
        public int RowNumber { get; set; }
        public Guid CourseId { get; set; }
        public string LarsQan { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseRef { get; set; }
        public string StartDate { get; set; }
        public string VenueName { get; set; }
        public string DeliveryMode { get; set; }
        public bool DeliveryError { get; set; }
        public bool DescriptionError { get; set; }
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
                    .Select(row => new ViewModelRow()
                    {
                        RowNumber = row.RowNumber,
                        CourseId = row.CourseId,
                        LarsQan = row.LarsQan,
                        CourseName = row.CourseName,
                        ProviderCourseRef = row.ProviderCourseRef,
                        StartDate = row.StartDate,
                        VenueName = row.VenueName,
                        DeliveryMode = row.DeliveryMode,
                        ErrorFields = row.Errors.Select(e => Core.DataManagement.Errors.MapCourseErrorToFieldGroup(e)).Distinct().ToArray(),
                        DeliveryError = row.Errors.Select(e => Core.DataManagement.Errors.MapCourseErrorToFieldGroup(e)).Distinct().Contains("Delivery mode"),
                        DescriptionError = row.Errors.Select(e => Core.DataManagement.Errors.MapCourseErrorToFieldGroup(e)).Distinct().Contains("Course description")
                    }).OrderBy(
                        g => g.DeliveryMode
                    ).ThenByDescending(
                        g => g.DescriptionError
                    ).ToArray()
            };
        }
    }
}
