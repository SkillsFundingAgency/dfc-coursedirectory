﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.ReferenceData.Campaigns
{
    public class CampaignDataImporter
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly BlobContainerClient _blobContainerClient;

        public CampaignDataImporter(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory, BlobServiceClient blobServiceClient)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(Constants.CampaignContainerName);
        }

        public async Task ImportCampaignData(string campaignCode, Stream csvStream)
        {
            
            using var streamReader = new StreamReader(csvStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

            const int batchSize = 100;

            var importJobId = Guid.NewGuid();

            var records = csvReader.GetRecordsAsync<CsvRow>();

            await foreach (var batch in records.Buffer(batchSize))
            {
                using var sqlDispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

                await sqlDispatcher.ExecuteQuery(new UpsertCampaignProviderCourses()
                {
                    CampaignCode = campaignCode,
                    ImportJobId = importJobId,
                    Records = batch.Select(r => new UpsertCampaignProviderCoursesRecord()
                    {
                        LearnAimRef = r.LearnAimRef,
                        ProviderUkprn = r.ProviderUkprn
                    })
                });

                await sqlDispatcher.Commit();
            }

            {
                using var sqlDispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

                await sqlDispatcher.ExecuteQuery(new RemoveStaleCampaignProviderCourses()
                {
                    CampaignCode = campaignCode,
                    ImportJobId = importJobId
                });

                await sqlDispatcher.Commit();
            }
            var blobName = $"{campaignCode}.csv";
            await _blobContainerClient.DeleteBlobIfExistsAsync(blobName);
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
