using System;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }

        public Uri GetCourseTextUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetCourseTextByLARS");
        }
    }
}
