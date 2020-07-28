using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.Functions
{
    public class FixVenuesSchema
    {
        private readonly DocumentClient _documentClient;
        private readonly Configuration _configuration;
        private readonly ILogger _logger;

        public FixVenuesSchema(
            DocumentClient documentClient,
            Configuration configuration,
            ILoggerFactory loggerFactory)
        {
            _documentClient = documentClient;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<FixVenuesSchema>();
        }

        [FunctionName(nameof(FixVenuesSchema))]
        [NoAutomaticTrigger]
        public async Task Run(string input)
        {
            var updatedCount = 0;

            var query = _documentClient.CreateDocumentQuery<dynamic>(
                    UriFactory.CreateDocumentCollectionUri(_configuration.DatabaseId, _configuration.VenuesCollectionName),
                    "select * from venues c where is_defined(c.LATITUDE) or is_defined(c.LONGITUDE) or is_defined(c.ID)")
                .AsDocumentQuery();

            await query.ProcessAll(async chunk =>
            {
                foreach (Document doc in chunk)
                {
                    var jObj = JObject.Parse(doc.ToString());

                    if (jObj.ContainsKey("ID"))
                    {
                        jObj.Remove("ID");
                    }

                    if (jObj.ContainsKey("LATITUDE"))
                    {
                        jObj["Latitude"] = jObj["LATITUDE"];
                        jObj.Remove("LATITUDE");
                    }

                    if (jObj.ContainsKey("LONGITUDE"))
                    {
                        jObj["Longitude"] = jObj["LONGITUDE"];
                        jObj.Remove("LONGITUDE");
                    }

                    var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(jObj.ToString()));
                    var updatedDoc = JsonSerializable.LoadFrom<Document>(jsonStream);

                    await _documentClient.ReplaceDocumentAsync(updatedDoc);

                    updatedCount++;
                }
            });

            _logger.LogInformation($"Updated {updatedCount} venues.");
        }
    }
}
