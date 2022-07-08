using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Dfc.CosmosBulkUtils.Config;
using Dfc.CosmosBulkUtils.Features.Delete;
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


                var parsedCmdLines = Parser.Default.ParseArguments<CmdOptions>(args);



                var host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        parsedCmdLines.WithParsed<CmdOptions>(o =>
                        {
                            services.Configure<CosmosDbSettings>(p => 
                            {
                                p.EndpointUrl = o.EndpointUrl;
                                p.AccessKey = o.AccessKey;
                                p.ContainerId = o.ContainerId;
                                p.DatabaseId = o.DatabaseId;
                                
                            });
                        }).WithNotParsed(errors => {
                            throw new ApplicationException("Error required cmd line args not provided");

                        });
                        //services.Configure<CosmosDbSettings>(
                        //    context.Configuration.GetRequiredSection(CosmosDbSettings.SectionName));
                        services.AddTransient<IContainerService, ContainerService>();
                        services.AddTransient<ITouchService, TouchService>();
                        services.AddTransient<IDeleteService, DeleteService>();
                        services.AddTransient<IFileService, FileService>();
                        services.AddTransient<Application>();
                    })
                    .UseSerilog()
                    .Build();


                var svc = ActivatorUtilities.CreateInstance<Application>(host.Services);


                return await svc.Run(parsedCmdLines.Value);

            }
            catch (Exception e)
            {
                Log.Logger.Error("Failed", e);
                throw;
            }


        }

        private static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false);
        }
    }
}
