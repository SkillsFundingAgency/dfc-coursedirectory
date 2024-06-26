﻿using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.ReferenceData.Campaigns;
using Microsoft.Azure.WebJobs;

namespace Dfc.CourseDirectory.Functions
{
    public class ImportCampaignData
    {
        private readonly CampaignDataImporter _campaignDataImporter;

        public ImportCampaignData(CampaignDataImporter campaignDataImporter)
        {
            _campaignDataImporter = campaignDataImporter;
        }

        [FunctionName(nameof(CampaignDataImporter))]
        public async Task Run(
           [BlobTrigger("%CampaignDataContainerName%/{campaignCode}.csv")] Stream blob,
           string campaignCode)
        {
            //using var stream = new MemoryStream();
            //await blob.DownloadToStreamAsync(stream);
           // stream.Seek(0L, SeekOrigin.Begin);

            await _campaignDataImporter.ImportCampaignData(campaignCode, blob);
            blob.Dispose();
        }
    }
}
