using System.IO;
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
        public Task Execute(
           [BlobTrigger("%CampaignDataContainerName%/{campaignCode}.csv")] Stream file,
           string campaignCode)
        {
            return _campaignDataImporter.ImportCampaignData(campaignCode, file);
        }
    }
}
