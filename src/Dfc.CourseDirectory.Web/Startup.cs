﻿using System;
using System.IO;
using System.Net.Http;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core.BackgroundWorkers;
using Dfc.CourseDirectory.Core.BinaryStorageProvider;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Web.Configuration;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Middleware;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Security.AuthorizationPolicies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;

        public Startup(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            Configuration = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddSingleton(Configuration);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddTransient<IUkrlpWcfClientFactory, UkrlpWcfClientFactory>();
            services.Configure<CourseForComponentSettings>(Configuration.GetSection("AppUISettings:CourseForComponentSettings"));
            services.Configure<EntryRequirementsComponentSettings>(Configuration.GetSection("AppUISettings:EntryRequirementsComponentSettings"));
            services.Configure<WhatWillLearnComponentSettings>(Configuration.GetSection("AppUISettings:WhatWillLearnComponentSettings"));
            services.Configure<HowYouWillLearnComponentSettings>(Configuration.GetSection("AppUISettings:HowYouWillLearnComponentSettings"));
            services.Configure<WhatYouNeedComponentSettings>(Configuration.GetSection("AppUISettings:WhatYouNeedComponentSettings"));
            services.Configure<HowAssessedComponentSettings>(Configuration.GetSection("AppUISettings:HowAssessedComponentSettings"));
            services.Configure<WhereNextComponentSettings>(Configuration.GetSection("AppUISettings:WhereNextComponentSettings"));

            services.AddOptions();

            services.AddTransient((provider) => new HttpClient());

            services.Configure<LarsSearchSettings>(Configuration.GetSection(nameof(LarsSearchSettings)));

            services.AddScoped<IPaginationHelper, PaginationHelper>();

            services.AddScoped<ICourseService, CourseService>();

            services.Configure<EnvironmentSettings>(Configuration.GetSection(nameof(EnvironmentSettings)));
            services.AddScoped<IEnvironmentHelper, EnvironmentHelper>();

            services.Configure<BlobStorageBinaryStorageProviderSettings>(Configuration.GetSection(nameof(BlobStorageBinaryStorageProviderSettings)));

            services.AddSingleton<QueueBackgroundWorkScheduler>();
            services.AddHostedService(sp => sp.GetRequiredService<QueueBackgroundWorkScheduler>());
            services.AddSingleton<IBackgroundWorkScheduler>(sp => sp.GetRequiredService<QueueBackgroundWorkScheduler>());

            services.AddSingleton(new BlobServiceClient(Configuration["BlobStorageSettings:ConnectionString"]));

            services.AddCourseDirectory(_env, Configuration);
            services.AddSignalR();

            var mvcBuilder = services
                .AddMvc(options =>
                {
                    options.Filters.Add(new RedirectOnMissingUKPRNActionFilter());
                })
                .AddSessionStateTempDataProvider();

#if DEBUG
            mvcBuilder.AddRazorRuntimeCompilation(options =>
            {
                // Fix auto reload on IIS when views in V2 project are changed
                // (see https://github.com/aspnet/Razor/issues/2426#issuecomment-420750249)
                var v2ProjectPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "Dfc.CourseDirectory.WebV2");
                options.FileProviders.Add(new PhysicalFileProvider(v2ProjectPath));
            });
#endif

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Developer"));
                options.AddPolicy("ElevatedUserRole", policy => policy.RequireRole("Developer", "Helpdesk"));
                options.AddPolicy("SuperUser", policy => policy.RequireRole("Developer", "Helpdesk", "Provider Superuser"));
                options.AddPolicy("Helpdesk", policy => policy.RequireRole("Helpdesk"));
                options.AddPolicy("ProviderSuperUser", policy => policy.RequireRole("Provider Superuser"));
                options.AddPolicy("Provider", policy => policy.RequireRole("Provider User", "Provider Superuser"));

                options.AddPolicy(
                    "Fe",
                    policy => policy.AddRequirements(new ProviderTypeRequirement(Core.Models.ProviderType.FE)));
            });

            if (_env.IsProduction())
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = Configuration.GetConnectionString("Redis");
                });
            }
            else
            {
#if DEBUG
                // Adds FileDistributedCache for persistent cached state, including session state, during development ONLY.
                services.AddSingleton<IDistributedCache>(_ => new FileDistributedCache(() => DateTimeOffset.UtcNow));
#else
                services.AddDistributedMemoryCache();
#endif
            }

            services.Configure<FormOptions>(x => x.ValueCountLimit = 10000);

            services.AddResponseCaching();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(40);
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            var dataProtectionBuilder = services.AddDataProtection();
            if (_env.IsProduction())
            {
                dataProtectionBuilder.PersistKeysToAzureBlobStorage(GetDataProtectionBlobToken());
            }

            //TODO
            //services.Configure<GoogleAnalyticsOptions>(options => Configuration.GetSection("GoogleAnalytics").Bind(options));

            var dfeSettings = new DfeSignInSettings();
            Configuration.GetSection("DFESignInSettings").Bind(dfeSettings);
            services.AddDfeSignIn(dfeSettings);

            Uri GetDataProtectionBlobToken()
            {
                var cloudStorageAccount = new CloudStorageAccount(
                    new Microsoft.Azure.Storage.Auth.StorageCredentials(
                        Configuration["BlobStorageSettings:AccountName"],
                        Configuration["BlobStorageSettings:AccountKey"]),
                    useHttps: true);

                var blobClient = cloudStorageAccount.CreateCloudBlobClient();

                var container = blobClient.GetContainerReference(Configuration["DataProtection:ContainerName"]);

                var blob = container.GetBlockBlobReference(Configuration["DataProtection:BlobName"]);

                var sharedAccessPolicy = new SharedAccessBlobPolicy()
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create
                };

                var sasToken = blob.GetSharedAccessSignature(sharedAccessPolicy);

                return new Uri(blob.Uri + sasToken);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseCourseDirectoryErrorHandling();
                app.UseHsts(options => options.MaxAge(days: 365).IncludeSubdomains());
            }

            app.UseCommitSqlTransaction();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseCsp(options => options
                .DefaultSources(s => s.Self())
                .ScriptSources(s => s
                    .Self()
                    .CustomSources(
                        //Home page
                        "sha256-wd6zPqofWjb5TSs4XzK3yLqmM6aUHeduDqEKaDQSWoU=",
                        "sha256-xY2DAB/H7eBQZT2luzwwjJh9xLZKg/fW/ETrbm3/4NM=",
                        "sha256-1V3JOTXaBEUCkDaNHDHobJB7YGiySFvHg+nmsbHLVfA=",
                        "sha256-l1eTVSK8DTnK8+yloud7wZUqFrI0atVo6VlC6PJvYaQ=",
                        "sha256-D6hPuqCvWlkPnuH57KXYatEnpXvAE85f2XHQPy0d3fg=",
                        //provider-search
                        "sha256-0jU/CEHAhHwt+/mNkmey1Qza6wbRwHlUITi5Yb033II=",
                        "sha256-RXW8jc88WJcCqt1yHMMnDtB80ex0nt3h/Tg3iaZESM8=",
                        //Courses
                        "sha256-RgdfcsyCABvzOyqKOFYzQZlxPad4TgV+Ll1GbmDf0T8=",
                        "sha256-8ejSnu9XPMeRxkxdLe2apSbYS0kwdMFWQ4Je7f8OZ18=",
                        //data-upload
                        "sha256-8mM6pQH4tNl8njGuUbm8zDyM4gItu1D+byUinrcbKCk=",
                        //ProviderCourses
                        "sha256-fUgH5XnPX9RfHSGD/tc9BW56h7qGMQmbkF8ephN+O84=",
                        "sha256-f265cqgVJGKF5FzP79WDAY7j3k5HwpnQyKvZq2YERz8=",
                        "sha256-RXW8jc88WJcCqt1yHMMnDtB80ex0nt3h/Tg3iaZESM8=",
                        "sha256-vcmTH07Am5+kH5JEfaANVMFlwDp67o1clpuk26pVsoo=",
                        //Courses - CopyCourseRun
                        "sha256-2clrXW+/bEbufPlAwRVqvk/W2LGlkC/jej3qSUX+h4g=",
                        "sha256-UREXRjOigNA7d4vVChFbfHyzl1+DWBUHCF4antGyqNU=",
                        "sha256-DNnLQcF+8AYGGsdspJInJ7ZZrRJ6dV4o5FGq66/oLPE=",
                        "sha256-bwiUuMVp/Stp46vAXZMeKQmhMI6tdZ1Yx4RSj7gvieM=",
                        "sha256-vwoeBAEG9+ZzZi+NvCog0gCipVdqPSwIas2vbD+CNFU=",
                        "sha256-PNdTPqwsL8wM1mVq8q9r67TxclTEEaFHiXshczqmu28=",
                        "sha256-yiXTcuJK6mwrqILfFetO15Pcpq4DzPFdMBTc3gVtbpo=",
                        "sha256-0HAFMtm1VzJDFQVbBhovhV/tae4xRJ8cfEu1koNDf3M=",
                        //Courses delete
                        "sha256-OYkOIzgZMarSeILfpnILAN3UBc497xQPKoluN3dLq/4=",
                        //Courses - EditCourseRun
                        "sha256-oIYTvhTO4ThyjBA5aSgReXOgPoQWR6lYxYDsbYgrlDQ=",
                        //t-levels
                        "sha256-RVouMPytFTnZo+SPrlAgypQidwPQt25rN2siNrPGPLg=",
                        //Venues - delete
                        "sha256-pBkIZd5tfDFCLPmEDcf2/Rr0dUVf3Wn2+PE+E7hFCu0=",
                        //Dashboard
                        "sha256-RXW8jc88WJcCqt1yHMMnDtB80ex0nt3h/Tg3iaZESM8=",
                        //CourseSummary
                        "sha256-vcmTH07Am5+kH5JEfaANVMFlwDp67o1clpuk26pVsoo=",
                        //RegulatedQualification
                        "sha256-wlhq0O7MI86vRqF5r0g82JnVyJtT0Sta+p+ThlaYJgA=",
                        //UnregulatedCourses
                        "sha256-bvwZCPRDhFUpa5Z9qFxBLFkCUvvRc6eFVEGukYQpgx4=",
                        "sha256-LFsotuEqS5Hq/sNF1lWC3h0CvmFCCFKDw3szys+mzUo=",
                        "https://cloud.tinymce.com",
                        "https://cdn.tiny.cloud",
                        "www.googletagmanager.com",
                        "https://cdnjs.cloudflare.com/",
                        "https://www.google-analytics.com"))
                .StyleSources(s => s
                    .Self()
                    .UnsafeInline()
                    .CustomSources(
                        "https://cdn.tiny.cloud/",
                        "https://www.googletagmanager.com/",
                        "https://tagmanager.google.com/",
                        "https://fonts.googleapis.com/",
                        "https://cloud.tinymce.com/",
                        "https://cdnjs.cloudflare.com/"))
                .FormActions(s => s
                    .Self()
                    )
                .FontSources(s => s
                    .Self()
                    .CustomSources(
                        "data:",
                        "https://fonts.googleapis.com/",
                        "https://fonts.gstatic.com/",
                        "https://cdn.tiny.cloud/"))
                .ImageSources(s => s
                    .Self()
                    .CustomSources(
                        "*",
                        "data:",
                        "https://cdn.tiny.cloud/"))
                .FrameAncestors(s => s.Self())
                .FrameSources(s => s
                    .Self()
                    .CustomSources(
                        "https://optimize.google.com"))
                .ConnectSources(s => s
                    .Self()
                    .CustomSources(
                        "https://www.google-analytics.com",
                        "https://region1.google-analytics.com"))
                );

            //Preventing ClickJacking Attacks
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Xss-Protection"] = "1; mode=block";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                context.Response.Headers["Feature-Policy"] = "accelerometer 'none'; camera 'none'; geolocation 'none'; gyroscope 'none'; magnetometer 'none'; microphone 'none'; payment 'none'; usb 'none'";

                context.Response.GetTypedHeaders().CacheControl =
                  new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                  {
                      NoCache = true,
                      NoStore = true,
                      MustRevalidate = true,
                  };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new string[] { "Pragma: no-cache" };

                await next();
            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseMiddleware<ProviderContextMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "onboardprovider",
                    pattern: "{controller=ProviderSearch}/{action=OnBoardProvider}/{id?}");

                endpoints.MapControllers();

                endpoints.MapV2Hubs();
            });
        }
    }
}
