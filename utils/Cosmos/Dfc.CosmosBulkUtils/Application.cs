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
        private readonly IDeleteService _deleteService;

        public Application(ILogger<Application> logger, ITouchService touchService, IDeleteService deleteService)
        {
            _logger = logger;
            _touchService = touchService;
            _deleteService = deleteService;
        }

        public async Task<int> Run(string[] args)
        {

                _logger.LogInformation("Running");

                await Parser.Default.ParseArguments<DeleteOptions, TouchOptions>(args)
                    .MapResult(
                        async (DeleteOptions opts) => await _deleteService.Execute(opts),
                        async (TouchOptions opts) => await _touchService.Execute(opts),
                        errors => Task.FromResult(1));



                _logger.LogInformation("Complete");
                return 0;

        }

    }
}
