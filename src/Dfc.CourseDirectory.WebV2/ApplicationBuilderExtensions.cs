using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCommitSqlTransaction(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CommitSqlTransactionMiddleware>();
        }

        public static IApplicationBuilder UseCourseDirectoryErrorHandling(this IApplicationBuilder app)
        {
            app.UseExceptionHandler("/error");

            app.UseStatusCodePagesWithReExecute("/error", "?code={0}");

            return app;
        }

        public static IApplicationBuilder UseV2StaticFiles(this IApplicationBuilder app)
        {
            return app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new ManifestEmbeddedFileProvider(
                    typeof(ApplicationBuilderExtensions).Assembly, "Content"),
                RequestPath = "/v2"
            });
        }
    }
}
