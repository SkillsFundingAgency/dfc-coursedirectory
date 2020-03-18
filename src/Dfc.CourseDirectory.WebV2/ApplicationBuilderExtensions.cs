using System.IO;
using System.Reflection;
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
#if DEBUG
            // Use a PhysicalFileProvider for local dev so we don't have to rebuild on every change
            var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var contentPath = Path.GetFullPath(Path.Combine(binPath, "../../../../../src/Dfc.CourseDirectory.WebV2/Content"));
#endif

            return app.UseStaticFiles(new StaticFileOptions()
            {
#if DEBUG
                FileProvider = new PhysicalFileProvider(contentPath),
#else
                FileProvider = new ManifestEmbeddedFileProvider(
                    typeof(ApplicationBuilderExtensions).Assembly, "Content"),
#endif
                RequestPath = "/v2"
            });
        }
    }
}
