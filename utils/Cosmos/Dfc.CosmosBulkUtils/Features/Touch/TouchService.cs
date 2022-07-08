using System;
using System.Threading.Tasks;
using Dfc.CosmosBulkUtils.Services;
using Dfc.CosmosBulkUtils.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Dfc.CosmosBulkUtils.Features.Touch
{
    public class TouchService :  ITouchService
    {
        private readonly IFileService _fileService;
        private readonly IContainerService _containerService;
        private readonly ILogger<TouchService> _logger;

        public TouchService(ILogger<TouchService> logger, IFileService fileService, IContainerService containerService) 
        {
            _logger = logger;
            _fileService = fileService;
            _containerService = containerService;
        }

        public async Task<int> Execute(string filename)
        {
            
            
                var ids = _fileService.LoadRecordIds(filename);

                foreach (var id in ids)
                {
                    _logger.LogInformation("Attempting touch on {id}", id);
                    var item = await _containerService.Get(id);
                    await _containerService.Update(id, item);
                    _logger.LogInformation("Touch done for {id}", id);
                }

                return 0;
            
        }
    }
}
