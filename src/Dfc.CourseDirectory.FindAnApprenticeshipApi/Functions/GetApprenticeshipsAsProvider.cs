using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Azure;
using Dfc.Providerportal.FindAnApprenticeship.Models;
using Dfc.Providerportal.FindAnApprenticeship.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Dfc.Providerportal.FindAnApprenticeship.Functions
{
    public class GetApprenticeshipsAsProvider
    {
        private readonly IBlobStorageClient _blobStorageClient;
        private readonly Func<DateTimeOffset> _nowUtc;

        public GetApprenticeshipsAsProvider(IBlobStorageClient blobStorageClient, Func<DateTimeOffset> nowUtc)
        {
            _blobStorageClient = blobStorageClient ?? throw new ArgumentNullException(nameof(blobStorageClient));
            _nowUtc = nowUtc ?? throw new ArgumentNullException(nameof(nowUtc));
        }

        [FunctionName("GetApprenticeshipsAsProvider")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bulk/providers")]HttpRequest req, ILogger log, CancellationToken ct)
        {
            try
            {
                var requestDateTime = _nowUtc();

                log.LogInformation($"{nameof(GetApprenticeshipsAsProvider)} requested at {{{nameof(requestDateTime)}}}.", requestDateTime);

                for (int i = 0; i <= 1; i++)
                {
                    var exportKey = new ExportKey(requestDateTime.AddDays(-i));

                    var blobClient = _blobStorageClient.GetBlobClient(exportKey);

                    try
                    {
                        var result = await blobClient.DownloadAsync(ct);

                        log.LogInformation($"{nameof(GetApprenticeshipsAsProvider)} returned {{{nameof(exportKey)}}}.", exportKey);

                        return new FileStreamResult(result.Value.Content, "application/json");
                    }
                    catch (RequestFailedException ex)
                        when (ex.Status == StatusCodes.Status404NotFound)
                    {
                        log.LogInformation($"{nameof(GetApprenticeshipsAsProvider)} failed to find {{{nameof(exportKey)}}}.", exportKey);
                        continue;
                    }
                }

                log.LogInformation($"{nameof(GetApprenticeshipsAsProvider)} failed to find an export for {{{nameof(requestDateTime)}}}.", requestDateTime);

                return new NotFoundResult();
            } 
            catch (Exception ex)
            {
                log.LogError(ex, $"{nameof(GetApprenticeshipsAsProvider)} failed with exception.");
                return new InternalServerErrorResult();
            }
        }
    }
}