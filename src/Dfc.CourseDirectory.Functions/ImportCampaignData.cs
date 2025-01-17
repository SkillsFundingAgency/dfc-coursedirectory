using Dfc.CourseDirectory.Core.ReferenceData.Campaigns;
using Microsoft.Azure.Functions.Worker;

namespace Dfc.CourseDirectory.Functions
{
    public class ImportCampaignData
    {
        private readonly CampaignDataImporter _campaignDataImporter;

        public ImportCampaignData(CampaignDataImporter campaignDataImporter)
        {
            _campaignDataImporter = campaignDataImporter;
        }

        [Function(nameof(CampaignDataImporter))]
        public async Task Run(
           [BlobTrigger("%CampaignDataContainerName%/{campaignCode}.csv")] Stream blob,
           string campaignCode)
        {
            await _campaignDataImporter.ImportCampaignData(campaignCode, blob);
            blob.Dispose();
        }
    }
}
