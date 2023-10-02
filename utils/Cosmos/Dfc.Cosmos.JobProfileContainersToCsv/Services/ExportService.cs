using System.Data.SqlClient;
using System.Globalization;
using CsvHelper;
using Dfc.Cosmos.JobProfileContainersToCsv.Config;
using Dfc.Cosmos.JobProfileContainersToCsv.Data;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace Dfc.Cosmos.JobProfileContainersToCsv.Services
{
    public class ExportService
    {
        public async Task ExportToCsvFile(CosmosDbSettings cosmosDbConfig, SqlDbSettings sqlDbConfig)
        {
            var exportData = new List<ExportRow>();

            await ExtractCosmosData(cosmosDbConfig, exportData);
            await ExtractSqlData(sqlDbConfig, exportData);            
            await WriteCsvFile(cosmosDbConfig, exportData);
        }

        private async Task WriteCsvFile(CosmosDbSettings cosmosDbConfig, List<ExportRow> exportData)
        {
            string outputFileName = cosmosDbConfig.Filename;
            var writer = new StreamWriter(outputFileName);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteHeader<ExportRow>();
            csv.NextRecord();
            foreach (var record in exportData)
            {
                csv.WriteRecord(record);
                csv.NextRecord();
            }
        }

        private async Task ExtractCosmosData(CosmosDbSettings cosmosDbConfig, List<ExportRow> exportData)
        {
            var containersData = await GetCosmosContainersData(cosmosDbConfig);
            var key = cosmosDbConfig.Key;

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
                    var jArray = (JArray)row["ApprenticeshipStandards"];
                    exportRow.ApprenticeshipStandards = string.Join(", ", jArray.Values<string>());
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

                    var jArray = (JArray)row["HiddenAlternativeTitles"];
                    exportRow.HiddenAlternativeTitles = string.Join(", ", jArray.Values<string>());
                }
            }

            foreach (var item in containersData[8])
            {
                var row = item.ToObject<Dictionary<string, object>>();
                var rowId = (string)row[key];
                var exportRow = exportData.FirstOrDefault(e => e.Id == rowId);

                if (exportRow != null)
                {
                    var jArray = (JArray)row["RelatedCareers"];
                    exportRow.RelatedCareers = string.Join(", ", jArray.Values<string>());
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
        }

        private async Task<IList<JObject>> GetCosmosData(ExportRequest request, string endpointUrl, string accessKey)
        {
            ArgumentNullException.ThrowIfNull(request.DatabaseId);
            ArgumentNullException.ThrowIfNull(request.ContainerId);
            ArgumentNullException.ThrowIfNull(request.Query);

            var results = new List<JObject>();

            var _client = new CosmosClient(endpointUrl, accessKey, new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway });
            var container = _client.GetContainer(request.DatabaseId, request.ContainerId);
            var queryResults = container.GetItemQueryIterator<JObject>(request.Query);

            while (queryResults.HasMoreResults)
            {
                var currentResults = await queryResults.ReadNextAsync();
                results.AddRange(currentResults);
            }

            return results;
        }

        private async Task<List<IList<JObject>>> GetCosmosContainersData(CosmosDbSettings config)
        {
            List<IList<JObject>> cosmosContainersData = new List<IList<JObject>>();

            foreach (var containerSettings in config.Containers)
            {
                var request = new ExportRequest
                {
                    DatabaseId = config.DatabaseId,
                    ContainerId = containerSettings.ContainerId,
                    Query = new QueryDefinition(containerSettings.Query)
                };

                var cosmosData = await GetCosmosData(request, config.EndpointUrl, config.AccessKey);
                cosmosContainersData.Add(cosmosData);

                Console.WriteLine("Found {0} records for container {1}", cosmosData.Count, containerSettings.ContainerId);
            }

            return cosmosContainersData;
        }

        private async Task<List<JobProfile>> GetJobProfilesFromSqlDb(SqlDbSettings config)
        {            
            var categoriesQuery = "SELECT ContentItemId, DisplayText from [dbo].[ContentItemIndex] WHERE ContentType = 'JobProfileCategory'";
            var jobProfilesQuery = "SELECT cii.DocumentId, GraphSyncPartId AS JobProfileId, jpi.JobProfileTitle, JobProfileCategory FROM [dbo].[JobProfileIndex] jpi INNER JOIN dbo.ContentItemIndex cii on cii.DocumentId = jpi.DocumentId";

            var categories = await GetCategories(categoriesQuery, config.ConnectionString);

            var jobProfiles = await GetJobProfiles(jobProfilesQuery, config.ConnectionString);
            
            foreach (var jobProfile in jobProfiles)
            {
                var jobProfileCategoryTitles = new List<string>();

                foreach (var jobProfileCategoryId in jobProfile.Categories.Split(","))
                {
                    if (categories.ContainsKey(jobProfileCategoryId))
                    {
                        jobProfileCategoryTitles.Add(categories[jobProfileCategoryId]);
                    }
                }

                jobProfile.Categories = string.Join(", ", jobProfileCategoryTitles);
            }

            return jobProfiles;
        }        

        private async Task ExtractSqlData(SqlDbSettings sqlDbConfig, List<ExportRow> exportData)
        {
            var jobProfiles = await GetJobProfilesFromSqlDb(sqlDbConfig);
            foreach (var row in exportData)
            {
                var jobProfile = jobProfiles.FirstOrDefault(jp => jp.Id == row.Id);

                if (jobProfile == null)
                    continue;

                row.JobProfileCategory = jobProfile.Categories;
            }
        }        

        private async Task<List<JobProfile>> GetJobProfiles(string sql, string connectionString)
        {
            var jobProfiles = new List<JobProfile>();            
            using (SqlConnection _sqlConnection = new SqlConnection(connectionString))
            using (SqlDataAdapter sda = new SqlDataAdapter(sql, _sqlConnection))
            {
                await _sqlConnection.OpenAsync();
                var command = new SqlCommand(sql, _sqlConnection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var jobProfile = new JobProfile
                    {
                        DocumentId = reader["DocumentId"].ToString(),
                        Id = reader["JobProfileId"].ToString(),
                        Title = reader["JobProfileTitle"].ToString(),
                        Categories = reader["JobProfileCategory"].ToString()
                    };                   
                    jobProfiles.Add(jobProfile);
                }

                await _sqlConnection.CloseAsync();
            }

            return jobProfiles;
        }        

        private async Task<Dictionary<string, string>> GetCategories(string sql, string connectionString)
        {
            var categories = new Dictionary<string, string>();            
            using (SqlConnection _sqlConnection = new SqlConnection(connectionString))
            using (SqlDataAdapter sda = new SqlDataAdapter(sql, _sqlConnection))
            {
                await _sqlConnection.OpenAsync();
                var command = new SqlCommand(sql, _sqlConnection);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var key = reader["ContentItemId"].ToString();
                    if (!categories.ContainsKey(key))
                    {
                        categories.Add(key, reader["DisplayText"].ToString());
                    }
                }

                await _sqlConnection.CloseAsync();
            }

            return categories;
        }                
    }
}
