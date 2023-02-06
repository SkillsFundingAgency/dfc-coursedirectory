using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CosmosToCsv.Data;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace Dfc.CosmosToCsv.Services
{
    public class ExportService
    {
        private readonly CosmosClient _client;

        public ExportService(string endpointUrl, string accessKey)
        {
            ArgumentNullException.ThrowIfNull(endpointUrl);
            ArgumentNullException.ThrowIfNull(accessKey);

            _client = new CosmosClient(endpointUrl, accessKey,
                new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway });
        }

        public async Task<IList<JObject>> ExportData(ExportRequest request)
        {
            ArgumentNullException.ThrowIfNull(request.DatabaseId);
            ArgumentNullException.ThrowIfNull(request.ContainerId);
            ArgumentNullException.ThrowIfNull(request.Query);

            var results = new List<JObject>();

            var container = _client.GetContainer(request.DatabaseId, request.ContainerId);
            var queryResults = container.GetItemQueryIterator<JObject>(request.Query);

            while (queryResults.HasMoreResults)
            {
                var currentResults = await queryResults.ReadNextAsync();
                results.AddRange(currentResults);
                Console.WriteLine("Found {0}", results.Count);
            }

            return results;
        }

        public void ExportToFile(IList<JObject> data, string key, string outputFileName = "output.csv")
        {
            using var writer = new StreamWriter(outputFileName);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            var headerRow = data.First()?.ToObject<Dictionary<string, object>>()?.Keys;
            if (headerRow == null) return;
            foreach (var header in headerRow) csv.WriteField(header);

            csv.WriteField("Order");

            csv.NextRecord();

            var order = 0;
            var previousKey = "";

            foreach (var item in data)
            {
                var row = item.ToObject<Dictionary<string, object>>();

                foreach (var header in headerRow)
                    csv.WriteField(row.ContainsKey(header) ? row[header] : string.Empty);


                if (row.ContainsKey(key) && row[key].ToString() == previousKey && previousKey != null)
                    order++;
                else
                    order = 0;

                csv.WriteField(order);

                previousKey = row.ContainsKey(key) ? row[key].ToString() : null;


                csv.NextRecord();
            }
        }
    }
}
