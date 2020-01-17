using Microsoft.AspNetCore.Builder;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCourseDirectoryErrorHandling(this IApplicationBuilder app)
        {
            app.UseExceptionHandler("/error");

            app.UseStatusCodePagesWithReExecute("/error", "?code={0}");

            return app;
        }
    }
}
