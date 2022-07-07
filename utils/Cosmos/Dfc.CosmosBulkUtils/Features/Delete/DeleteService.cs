using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dfc.CosmosBulkUtils.Services;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Dfc.CosmosBulkUtils.Features.Delete
{
    public class DeleteService : IDeleteService
    {
        private readonly ILogger<DeleteService> _logger;
        private readonly IFileService _fileService;
        private readonly IContainerService _containerService;

        public DeleteService(ILogger<DeleteService> logger, IFileService fileService, IContainerService containerService)
        {
            _logger = logger;
            _fileService = fileService;
            _containerService = containerService;
        }

        public async Task Execute(DeleteOptions options)
        {
            var ids = _fileService.LoadRecordIds(options.Filename);

            if (!this.Confirm(ids.Count))
            {
                Log.Information("Aborting");
                return;
            }

            int successful = 0, failed = 0;

            foreach (var id in ids)
            {
                if (await _containerService.Delete(id))
                {
                    successful++;
                }
                else
                {
                    failed++;
                }
            }

            _logger.LogInformation("Summary Successful={successful} Failed={failed}", successful, failed);
        }

        private bool Confirm(int totalRecords)
        {
            var settings = _containerService.GetSettings();
            Console.WriteLine("A total of {0} will be deleted from {1} {2} {3}", totalRecords, settings.EndpointUrl, settings.DatabaseId, settings.ContainerId );
            Console.WriteLine("Press Y to proceed - or any other key to quit");

            return Console.ReadKey().KeyChar.ToString().Equals("y", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
