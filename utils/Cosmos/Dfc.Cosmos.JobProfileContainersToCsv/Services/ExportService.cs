using System.Globalization;
using CsvHelper;
using Dfc.Cosmos.JobProfileContainersToCsv.Data;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace Dfc.Cosmos.JobProfileContainersToCsv.Services
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

        public async Task<IList<JObject>> GetCosmosData(ExportRequest request)
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
            }

            return results;
        }

        public void ExportToFile(List<IList<JObject>> containersData, string key, string outputFileName = "output.csv")
        {
            using var writer = new StreamWriter(outputFileName);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            var exportData = new List<ExportRow>();

            foreach (var item in containersData[0])
            {
                var row = item.ToObject<Dictionary<string, object>>();

                var exportRow = new ExportRow
                {
                    Id = (string)row[key],
                    CareerProgression = (string)row["CareerProgression"]
                };

                exportData.Add(exportRow);
            }

            foreach (var item in containersData[1])
            {
                var row = item.ToObject<Dictionary<string, object>>();
                var rowId = (string)row[key];
                var exportRow = exportData.FirstOrDefault(e => e.Id == rowId);

                if (exportRow != null)
                {
                    exportRow.CourseKeywords = (string)row["CourseKeywords"];
                    exportRow.ApprenticeshipStandards = ((JArray)row["ApprenticeshipStandards"]).ToString();
                }
            }

            foreach (var item in containersData[2])
            {
                var row = item.ToObject<Dictionary<string, object>>();
                var rowId = (string)row[key];
                var exportRow = exportData.FirstOrDefault(e => e.Id == rowId);

                if (exportRow != null)
                {
                    exportRow.UniversitySubject = (string)row["UniversitySubject"];
                    exportRow.UniversityFurtherInformation = (string)row["UniversityFurtherInformation"];
                }
            }

            foreach (var item in containersData[3])
            {
                var row = item.ToObject<Dictionary<string, object>>();
                var rowId = (string)row[key];
                var exportRow = exportData.FirstOrDefault(e => e.Id == rowId);

                if (exportRow != null)
                {
                    exportRow.CollegeSubject = (string)row["CollegeSubject"];
                    exportRow.CollegeFurtherInformation = (string)row["CollegeFurtherInformation"];
                }
            }

            foreach (var item in containersData[4])
            {
                var row = item.ToObject<Dictionary<string, object>>();
                var rowId = (string)row[key];
                var exportRow = exportData.FirstOrDefault(e => e.Id == rowId);

                if (exportRow != null)
                {
                    exportRow.ApprenticeshipSubject = (string)row["ApprenticeshipSubject"];
                    exportRow.ApprenticeshipFurtherInformation = (string)row["ApprenticeshipFurtherInformation"];
                }
            }

            foreach (var item in containersData[5])
            {
                var row = item.ToObject<Dictionary<string, object>>();
                var rowId = (string)row[key];
                var exportRow = exportData.FirstOrDefault(e => e.Id == rowId);

                if (exportRow != null)
                {
                    exportRow.EntryRouteSummary = (string)row["EntryRouteSummary"];
                    exportRow.Work = (string)row["Work"];
                    exportRow.Volunteering = (string)row["Volunteering"];
                    exportRow.DirectApplication = (string)row["DirectApplication"];
                    exportRow.OtherRoutes = (string)row["OtherRoutes"];
                    exportRow.CareerTips = (string)row["CareerTips"];
                    exportRow.ProfessionalAndIndustryBodies = (string)row["ProfessionalAndIndustryBodies"];
                    exportRow.FurtherInformation = (string)row["FurtherInformation"];
                }
            }

            foreach (var item in containersData[6])
            {
                var row = item.ToObject<Dictionary<string, object>>();
                var rowId = (string)row[key];
                var exportRow = exportData.FirstOrDefault(e => e.Id == rowId);

                if (exportRow != null)
                {
                    exportRow.Title = (string)row["Title"];
                    exportRow.CanonicalName = (string)row["CanonicalName"];
                }
            }

            foreach (var item in containersData[7])
            {
                var row = item.ToObject<Dictionary<string, object>>();
                var rowId = (string)row[key];
                var exportRow = exportData.FirstOrDefault(e => e.Id == rowId);

                if (exportRow != null)
                {
                    exportRow.SocCode = (string)row["SocCode"];
                    exportRow.SalaryStarter = (long)(row["SalaryStarter"] ?? default(long));
                    exportRow.SalaryExperienced = (long)(row["SalaryExperienced"] ?? default(long));
                    exportRow.MinimumHours = (row["MinimumHours"] ?? string.Empty).ToString();
                    exportRow.MaximumHours = (row["MaximumHours"] ?? string.Empty).ToString();
                }
            }

            foreach (var item in containersData[8])
            {
                var row = item.ToObject<Dictionary<string, object>>();
                var rowId = (string)row[key];
                var exportRow = exportData.FirstOrDefault(e => e.Id == rowId);

                if (exportRow != null)
                {
                    exportRow.RelatedCareers = ((JArray)row["RelatedCareers"]).ToString();
                }
            }

            foreach (var item in containersData[9])
            {
                var row = item.ToObject<Dictionary<string, object>>();
                var rowId = (string)row[key];
                var exportRow = exportData.FirstOrDefault(e => e.Id == rowId);

                if (exportRow != null)
                {
                    exportRow.OtherRequirements = (string)row["OtherRequirements"];
                    exportRow.DigitalSkill = (string)row["DigitalSkill"];
                }
            }

            foreach (var item in containersData[10])
            {
                var row = item.ToObject<Dictionary<string, object>>();
                var rowId = (string)row[key];
                var exportRow = exportData.FirstOrDefault(e => e.Id == rowId);

                if (exportRow != null)
                {
                    exportRow.Tasks = (string)row["Tasks"];
                }
            }

            csv.WriteHeader<ExportRow>();
            csv.NextRecord();
            foreach (var record in exportData)
            {
                csv.WriteRecord(record);
                csv.NextRecord();
            }
        }
    }
}
