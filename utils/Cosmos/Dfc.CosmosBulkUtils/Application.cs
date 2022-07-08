using System;
using System.Threading.Tasks;
using CommandLine;
using Dfc.CosmosBulkUtils.Config;
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

        public async Task<int> Run(CmdOptions cmdOption)
        {

            _logger.LogInformation("Running");

            switch (cmdOption.Mode)
            {
                case ModeEnum.delete:
                    await _deleteService.Execute(cmdOption.Filename);
                    break;
                case ModeEnum.touch:
                    await _touchService.Execute(cmdOption.Filename);
                    break;
                default:
                    break;
            }

            _logger.LogInformation("Complete");
                return 0;

        }

    }
}
