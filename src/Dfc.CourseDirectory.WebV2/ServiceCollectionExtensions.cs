using System.Data;
using System.Data.SqlClient;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.Filters;
using Dfc.CourseDirectory.WebV2.Security;
using GovUk.Frontend.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCourseDirectory(
            this IServiceCollection services,
            IHostingEnvironment environment,
            IConfiguration configuration)
        {
            var thisAssembly = typeof(ServiceCollectionExtensions).Assembly;

            if (!environment.IsTesting())
            {
                services.AddTransient<ICosmosDbQueryDispatcher, CosmosDbQueryDispatcher>();
                services.AddSingleton<Configuration>();

                services.Scan(scan => scan
                    .FromAssembliesOf(typeof(ICosmosDbQuery<>))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICosmosDbQueryHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime());
            }
            
            services
                .AddMvc(options =>
                {
                    options.Conventions.Add(new AddFeaturePropertyModelConvention());
                    options.Conventions.Add(new AuthorizeActionModelConvention());

                    options.Filters.Add(new RedirectToProviderSelectionActionFilter());
                    options.Filters.Add(new VerifyApprenticeshipIdActionFilter());
                    options.Filters.Add(new ResourceDoesNotExistExceptionFilter());
                    options.Filters.Add(new CommitSqlTransactionActionFilter());

                    options.ModelBinderProviders.Insert(0, new CurrentProviderModelBinderProvider());
                })
                .AddApplicationPart(thisAssembly)
                .AddRazorOptions(options =>
                {
                    // TODO When the legacy views are all moved this check can go away
                    if (environment.IsTesting())
                    {
                        options.ViewLocationFormats.Clear();
                    }

                    options.ViewLocationFormats.Add("/SharedViews/{0}.cshtml");

                    options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());
                });

            services.Scan(scan => scan
                .FromAssembliesOf(typeof(ISqlQuery<>))
                .AddClasses(classes => classes.AssignableTo(typeof(ISqlQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            services.AddSingleton<HostingOptions>();
            services.AddSingleton<IProviderOwnershipCache, ProviderOwnershipCache>();
            services.AddSingleton<IProviderInfoCache, ProviderInfoCache>();
            services.AddGovUkFrontend(new GovUkFrontendAspNetCoreOptions()
            {
                // Avoid import being added to old pages
                AddImportsToHtml = false
            });
            services.AddMediatR(typeof(ServiceCollectionExtensions));
            services.AddScoped<ISqlQueryDispatcher, SqlQueryDispatcher>();
            services.AddScoped<SqlConnection>(_ => new SqlConnection(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<SqlTransaction>(sp =>
            {
                var connection = sp.GetRequiredService<SqlConnection>();
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                var transaction = connection.BeginTransaction(IsolationLevel.Snapshot);

                var marker = sp.GetRequiredService<SqlTransactionMarker>();
                marker.OnTransactionCreated(transaction);

                return transaction;
            });
            services.AddScoped<SqlTransactionMarker>();
            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<ICurrentUserProvider, ClaimsPrincipalCurrentUserProvider>();
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IFeatureFlagProvider, ConfigurationFeatureFlagProvider>();

            return services;
        }
    }
}
