using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Services;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Functions
{
    public class GetApprenticeshipsAsProviderByUkprn
    {
        private readonly IApprenticeshipService _apprenticeshipService;

        public GetApprenticeshipsAsProviderByUkprn(IApprenticeshipService apprenticeshipService)
        {
            _apprenticeshipService = apprenticeshipService ?? throw new ArgumentNullException(nameof(apprenticeshipService));
        }

        [FunctionName("GetApprenticeshipsAsProviderByUkprn")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequest req, ILogger log)
        {
            string fromQuery = req.Query["ukprn"];

            if (string.IsNullOrWhiteSpace(fromQuery))
            {
                return new BadRequestObjectResult(ErrorResult("Empty or missing UKPRN value."));
            }

            if (!int.TryParse(fromQuery, out int ukprn))
            {
                return new BadRequestObjectResult(ErrorResult("Invalid UKPRN value, expected a valid integer."));
            }

            try
            {
                log.LogInformation($"[{DateTime.UtcNow:G}] Retrieving Apprenticeships for {nameof(ukprn)} {{{nameof(ukprn)}}}...", ukprn);

                var apprenticeships = await _apprenticeshipService.GetApprenticeshipsByUkprn(ukprn);
                
                if (!apprenticeships.Any())
                {
                    return new NotFoundObjectResult(ErrorResult($"No apprentiships found for UKPRN {ukprn}."));
                }
                    
                var result = (await _apprenticeshipService.ApprenticeshipsToDasProviders(apprenticeships.ToList())).Single();

                return new OkObjectResult(DasProviderResultViewModel.FromDasProviderResult(result));
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        private static DasProviderResultViewModel ErrorResult(params string[] errors)
        {
            return new DasProviderResultViewModel
            {
                Success = false,
                Messages = errors
            };
        }
    }
}
