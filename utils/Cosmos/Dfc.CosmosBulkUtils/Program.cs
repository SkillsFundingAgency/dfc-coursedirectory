using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Dfc.CosmosBulkUtils.Config;
using Dfc.CosmosBulkUtils.Features.Delete;
using Dfc.CosmosBulkUtils.Features.Patch;
using Dfc.CosmosBulkUtils.Features.Touch;
using Dfc.CosmosBulkUtils.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;

namespace Dfc.CosmosBulkUtils
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);
            Log.Logger = new LoggerConfiguration().ReadFrom
                .Configuration(builder.Build())
                .CreateLogger();

            try
            {
                Log.Logger.Information("Initialise...");

                var result = await CommandLine.Parser.Default.ParseArguments<TouchOptions, DeleteOptions, PatchOptions>(args)
                    .MapResult(
                    (
                        TouchOptions opts) =>  Execute(opts), (DeleteOptions opts) => Execute(opts), (PatchOptions opts) => Execute(opts), error => Task.FromResult(1)
                    );


                return result;

            }
            catch (Exception e)
            {
                Log.Logger.Error("Failed", e);
                throw;
            }


        }

        private async static Task<int> Execute(CmdOptions options)
        {
            var host = CreateHost(options);
            var app = ActivatorUtilities.CreateInstance<Application>(host.Services);
            return await app.Run(options);

        }

        private static IHost CreateHost(CmdOptions options)
        {
            var host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                            services.Configure<CosmosDbSettings>(p =>
                            {
                                p.EndpointUrl = options.EndpointUrl;
                                p.AccessKey = options.AccessKey;
                                p.ContainerId = options.ContainerId;
                                p.DatabaseId = options.DatabaseId;

                            });

                        services.AddTransient<IContainerService, ContainerService>();
                        services.AddTransient<ITouchService, TouchService>();
                        services.AddTransient<IDeleteService, DeleteService>();
                        services.AddTransient<IFileService, FileService>();
                        services.AddTransient<Application>();
                    })
                    .UseSerilog()
                    .Build();

            return host;
        }

        private static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false);
        }
    }
}
