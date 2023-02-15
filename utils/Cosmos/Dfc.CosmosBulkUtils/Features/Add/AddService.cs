using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dfc.CosmosBulkUtils.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dfc.CosmosBulkUtils.Features.Add
{
    public class AddService : IAddService
    {
        private readonly ILogger<AddService> _logger;
        private readonly IFileService _fileService;
        private readonly IContainerService _containerService;

        public AddService(ILogger<AddService> logger, IFileService fileService, IContainerService containerService)
        {
            _logger = logger;
            _fileService = fileService;
            _containerService = containerService;
        }
        public async Task<int> Execute(string path, string partitionKey)
        {
            var success = new List<string>();
            var failed = new List<string>();

            foreach (string file in Directory.EnumerateFiles(path, "*.json"))
            {
                var document = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(file));
                _logger.LogInformation("Attempting to add {0}", file);
                var result = await _containerService.Add(file, document, partitionKey);

                if (result)
                {
                    success.Add(file);
                } else
                {
                    failed.Add(file);
                }

            }

            if (failed.Count > 0)
            {
                _logger.LogError("The following failed to upsert");
                failed.ForEach(x => _logger.LogError(x));
                return 1;
            } else
            {
                _logger.LogInformation("Complete");
                return 0;
            }
        }
    }
}
