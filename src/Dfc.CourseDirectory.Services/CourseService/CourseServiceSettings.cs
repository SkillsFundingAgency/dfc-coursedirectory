using System;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public int BulkUploadSecondsPerRecord { get; set; }

        public Uri ToAddCourseUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/AddCourse");
        }

        public Uri ToGetYourCoursesUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetCoursesByLevelForUKPRN");
        }

        public Uri ToUpdateCourseUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateCourse");
        }

        public Uri ToGetCourseByIdUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetCourseById");
        }

        public Uri ToUpdateStatusUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateStatus");
        }

        public Uri ToGetCourseCountsByStatusForUKPRNUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetCourseCountsByStatusForUKPRN");
        }

        public Uri ToChangeCourseRunStatusesForUKPRNSelectionUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/ChangeCourseRunStatusesForUKPRNSelection");
        }

        public Uri ToArchiveCourseRunsByUKPRNUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/ArchiveCourseRunsByUKPRN");
        }

        public Uri ToDeleteBulkUploadCoursesUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/DeleteBulkUploadCourses");
        }

        public Uri ToGetCourseMigrationReportByUKPRN()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetCourseMigrationReportByUKPRN");
        }

        public Uri ToArchiveCoursesExceptBulkUploadReadytoGoLiveUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/ArchiveCoursesExceptBulkUploadReadytoGoLive");
        }
    }
}
