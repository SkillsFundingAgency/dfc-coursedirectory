using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common.Settings;
using Dfc.CourseDirectory.Models.Models.Environment;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.ApprenticeshipService;
using Dfc.CourseDirectory.Services.AuthService;
using Dfc.CourseDirectory.Services.BaseDataAccess;
using Dfc.CourseDirectory.Services.BlobStorageService;
using Dfc.CourseDirectory.Services.BulkUploadService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.CourseTextService;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.AuthService;
using Dfc.CourseDirectory.Services.Interfaces.BaseDataAccess;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using Dfc.CourseDirectory.Services.Interfaces.OnspdService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.OnspdService;
using Dfc.CourseDirectory.Services.ProviderService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.BackgroundWorkers;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.HostedServices;
using Dfc.CourseDirectory.Web.ViewComponents;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.Security;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
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
        private readonly ILogger<Startup> _logger;
        private readonly IWebHostEnvironment _env;
        //Undefined is only part of these policy until the batch import to update ProviderType is run
        private readonly List<string> _feClaims = new List<string> {"Fe", "Both", "Undefined" };
        private readonly List<string> _apprenticeshipClaims = new List<string> { "Apprenticeship", "Both", "Undefined" };
        public Startup(IWebHostEnvironment env, ILogger<Startup> logger, IConfiguration config)
        {
            _env = env;
            _logger = logger;
            Configuration = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddSingleton(Configuration);

            _logger.LogCritical("Logging from ConfigureServices.");
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton<IConfiguration>(Configuration);
            services.Configure<VenueNameComponentSettings>(Configuration.GetSection("AppUISettings:VenueNameComponentSettings"));
            services.Configure<CourseForComponentSettings>(Configuration.GetSection("AppUISettings:CourseForComponentSettings"));
            services.Configure<EntryRequirementsComponentSettings>(Configuration.GetSection("AppUISettings:EntryRequirementsComponentSettings"));
            services.Configure<WhatWillLearnComponentSettings>(Configuration.GetSection("AppUISettings:WhatWillLearnComponentSettings"));
            services.Configure<HowYouWillLearnComponentSettings>(Configuration.GetSection("AppUISettings:HowYouWillLearnComponentSettings"));
            services.Configure<WhatYouNeedComponentSettings>(Configuration.GetSection("AppUISettings:WhatYouNeedComponentSettings"));
            services.Configure<HowAssessedComponentSettings>(Configuration.GetSection("AppUISettings:HowAssessedComponentSettings"));
            services.Configure<WhereNextComponentSettings>(Configuration.GetSection("AppUISettings:WhereNextComponentSettings"));

            services.AddOptions();

            services.Configure<BaseDataAccessSettings>(options =>
            {
                options.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
            });
            services.AddScoped<IBaseDataAccess, BaseDataAccess>();

            services.Configure<ProviderServiceSettings>(Configuration.GetSection(nameof(ProviderServiceSettings)));
            services.AddScoped<IProviderService, ProviderService>();
            services.AddScoped<IProviderSearchHelper, ProviderSearchHelper>();

            services.AddTransient((provider) => new HttpClient());

            services.AddScoped<IAuthService, AuthService>();
            services.Configure<GovukPhaseBannerSettings>(Configuration.GetSection(nameof(GovukPhaseBannerSettings)));
            services.Configure<ApprenticeshipSettings>(Configuration.GetSection(nameof(ApprenticeshipSettings)));
            services.AddScoped<IGovukPhaseBannerService, GovukPhaseBannerService>();


            services.Configure<LarsSearchSettings>(Configuration.GetSection(nameof(LarsSearchSettings)));
            services.AddScoped<ILarsSearchService, LarsSearchService>();

            services.AddScoped<ILarsSearchHelper, LarsSearchHelper>();
            services.AddScoped<IPaginationHelper, PaginationHelper>();


            services.AddScoped<IVenueSearchHelper, VenueSearchHelper>();
            services.Configure<VenueServiceSettings>(Configuration.GetSection(nameof(VenueServiceSettings)));
            services.AddScoped<IVenueService, VenueService>();

            services.Configure<CourseServiceSettings>(Configuration.GetSection(nameof(CourseServiceSettings)));
            services.Configure<FindACourseServiceSettings>(Configuration.GetSection(nameof(FindACourseServiceSettings)));
            services.AddScoped<ICourseService, CourseService>();

            services.Configure<CourseTextServiceSettings>(Configuration.GetSection(nameof(CourseTextServiceSettings)));
            services.AddScoped<ICourseTextService, CourseTextService>();

            services.Configure<OnspdSearchSettings>(Configuration.GetSection(nameof(OnspdSearchSettings)));
            services.AddScoped<IOnspdService, OnspdService>();
            services.AddScoped<IOnspdSearchHelper, OnspdSearchHelper>();
            services.AddScoped<IUserHelper, UserHelper>();
            services.AddScoped<ICSVHelper, CSVHelper>();
            services.AddScoped<ICourseProvisionHelper, CourseProvisionHelper>();
            services.Configure<ApprenticeshipServiceSettings>(Configuration.GetSection(nameof(ApprenticeshipServiceSettings)));
            services.AddScoped<IApprenticeshipService, ApprenticeshipService>();

            services.AddScoped<IBulkUploadService, BulkUploadService>();
            services.AddScoped<IApprenticeshipBulkUploadService, ApprenticeshipBulkUploadService>();
            services.Configure<BlobStorageSettings>(Configuration.GetSection(nameof(BlobStorageSettings)));
            services.AddScoped<IBlobStorageService, BlobStorageService>();
            services.Configure<EnvironmentSettings>(Configuration.GetSection(nameof(EnvironmentSettings)));
            services.AddScoped<IEnvironmentHelper, EnvironmentHelper>();
            services.AddScoped<IApprenticeshipProvisionHelper, ApprenticeshipProvisionHelper>();

            {
                var endpoint = new Uri(Configuration["CosmosDbSettings:EndpointUri"]);
                var key = Configuration["CosmosDbSettings:PrimaryKey"];
                var documentClient = new DocumentClient(endpoint, key);
                services.AddSingleton(documentClient);
            }

            services.AddCourseDirectory(_env, Configuration);

            var mvcBuilder = services
                .AddMvc(options =>
                {
                    options.Filters.Add(new RedirectOnMissingUKPRNActionFilter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
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
                options.AddPolicy("Apprenticeship", policy =>
                    policy.RequireAssertion(x => (!x.User.IsInRole("Provider Superuser") && !x.User.IsInRole("Provider User")) ||
                                                 x.User.Claims.Any(c => c.Type == "ProviderType" &&
                                                                        _apprenticeshipClaims.Contains(c.Value))));
                options.AddPolicy("Fe", policy =>
                    policy.RequireAssertion(x => (!x.User.IsInRole("Provider Superuser") && !x.User.IsInRole("Provider User")) ||
                                                                             x.User.Claims.Any(c => c.Type == "ProviderType" && 
                                                                                                    _feClaims.Contains(c.Value, StringComparer.OrdinalIgnoreCase))));
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
                services.AddDistributedMemoryCache();
            }

            services.Configure<FormOptions>(x => x.ValueCountLimit = 2048);

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


            services.AddTransient<ITagHelperComponent, GoogleAnalyticsTagHelperComponent>();


            // Register the background worker helper
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.Configure<FormOptions>(x => x.ValueCountLimit = 10000);

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
            RunStartupTasks().GetAwaiter().GetResult();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseCourseDirectoryErrorHandling();
                app.UseHsts();
            }

            app.UseCommitSqlTransaction();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseGdsFrontEnd();
            app.UseSession();

            //Preventing ClickJacking Attacks
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                context.Response.Headers["X-Content-Type-Options"] ="nosniff";
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
            });

            async Task RunStartupTasks()
            {
                var startupTasks = serviceProvider.GetServices<IStartupTask>();
                foreach (var t in startupTasks)
                {
                    await t.Execute();
                }
            }
        }
    }
}
