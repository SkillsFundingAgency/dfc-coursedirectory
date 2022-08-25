using System;
using System.Threading.Tasks;
using CommandLine;
using Dfc.CosmosBulkUtils.Config;
using Dfc.CosmosBulkUtils.Features.Delete;
using Dfc.CosmosBulkUtils.Features.Patch;
using Dfc.CosmosBulkUtils.Features.Touch;
using Microsoft.Extensions.Logging;

namespace Dfc.CosmosBulkUtils
{
    public class Application
    {
        private readonly ILogger<Application> _logger;
        private readonly ITouchService _touchService;
        private readonly IDeleteService _deleteService;
        private readonly IPatchService _patchService;

        public Application(ILogger<Application> logger, ITouchService touchService, IDeleteService deleteService, IPatchService patchService)
        {
            _logger = logger;
            _touchService = touchService;
            _deleteService = deleteService;
            _patchService = patchService;
        }

        public async Task<int> Run(CmdOptions cmdOption)
        {
            _logger.LogInformation("Running");

            switch (cmdOption)
            {
                case TouchOptions touchOptions:
                    return await _touchService.Execute(touchOptions.Filename);
                case DeleteOptions deleteOptions:
                    return await _deleteService.Execute(deleteOptions.Filename);
                case PatchOptions patchOptions:
                    return await _patchService.Execute(patchOptions);
                    default:
                    return 1;

            }



            //_logger.LogInformation("Complete");
            //    return 0;

        }

    }
}
