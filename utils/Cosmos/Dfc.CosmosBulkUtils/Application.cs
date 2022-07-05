using System;
using System.Threading.Tasks;
using CommandLine;
using Dfc.CosmosBulkUtils.Features.Delete;
using Dfc.CosmosBulkUtils.Features.Touch;
using Microsoft.Extensions.Logging;

namespace Dfc.CosmosBulkUtils
{
    public class Application
    {
        private readonly ILogger<Application> _logger;
        private readonly ITouchService _touchService;

        public Application(ILogger<Application> logger, ITouchService touchService)
        {
            _logger = logger;
            _touchService = touchService;
        }

        public async Task<int> Run(string[] args)
        {

                _logger.LogInformation("Running");

                await Parser.Default.ParseArguments<DeleteOptions, TouchOptions>(args)
                    .MapResult(
                        async (DeleteOptions opts) => await RunDelete(opts),
                        async (TouchOptions opts) => await _touchService.Execute(opts),
                        errors => Task.FromResult(1));



                _logger.LogInformation("Complete");
                return 0;

        }

        private Task<int> RunDelete(DeleteOptions opts)
        {
            return Task.FromResult(0);
        }
    }
}
