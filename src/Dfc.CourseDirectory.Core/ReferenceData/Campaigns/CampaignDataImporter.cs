using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.ReferenceData.Campaigns
{
    public class CampaignDataImporter
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;

        public CampaignDataImporter(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
        }

        public async Task ImportCampaignData(string campaignCode, Stream csvStream)
        {
            using var sqlDispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

            using var streamReader = new StreamReader(csvStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

            var rows = csvReader.GetRecords<CsvRow>();

            await sqlDispatcher.ExecuteQuery(new SetCampaignProviderCourses()
            {
                CampaignCode = campaignCode,
                Records = rows.Select(r => new SetCampaignProviderCoursesRecord()
                {
                    LearnAimRef = r.LearnAimRef,
                    ProviderUkprn = r.ProviderUkprn
                })
            });

            await sqlDispatcher.Commit();
        }

        private class CsvRow
        {
            [Name("UKPRN")]
            public int ProviderUkprn { get; set; }
            [Name("LARS code")]
            public string LearnAimRef { get; set; }
        }
    }
}
