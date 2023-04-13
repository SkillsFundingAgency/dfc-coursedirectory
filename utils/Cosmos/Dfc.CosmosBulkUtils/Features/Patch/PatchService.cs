using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dfc.CosmosBulkUtils.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Dfc.CosmosBulkUtils.Features.Patch
{
    public class PatchService : IPatchService
    {
        private readonly ILogger<PatchService> _logger;
        private readonly IFileService _fileService;
        private readonly IContainerService _containerService;

        public PatchService(ILogger<PatchService> logger, IFileService fileService, IContainerService containerService)
        {
            _logger = logger;
            _fileService = fileService;
            _containerService = containerService;
        }
        public async Task<int> Execute(PatchOptions options)
        {
            _logger.LogInformation("Loading patch config {0}", options.Filename);
            var config = _fileService.LoadPatchConfig(options.Filename);

            await _containerService.Patch(config);





            return 0;
        }
    }
}
