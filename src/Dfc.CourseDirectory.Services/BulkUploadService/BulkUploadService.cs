using CsvHelper;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dfc.CourseDirectory.Services.BulkUploadService
{
    public class BulkUploadService : IBulkUploadService
    {
        private readonly ILogger<BulkUploadService> _logger;

        public BulkUploadService(ILogger<BulkUploadService> logger)
        {
            Throw.IfNull(logger, nameof(logger));

            _logger = logger;
        }

        public List<string> ProcessBulkUpload(string bulkUploadFilePath, int providerUKPRN) 
        {
            var errors = new List<string>();
            var courses = new List<BulkUploadCourse>();
            int bulkUploadLineNumber = 1;
            string previousLearnAimRef = string.Empty;
            List<int> lineNumbersInCourse = new List<int>();

            using (var reader = new StreamReader(bulkUploadFilePath))  
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        bool isCourseHeader = false;
                        string currentLearnAimRef = csv.GetField("LARS_QAN");
                        if(bulkUploadLineNumber.Equals(1) || currentLearnAimRef != previousLearnAimRef)
                        {
                            isCourseHeader = true;
                            lineNumbersInCourse.Add(bulkUploadLineNumber);
                        }
                        //else
                        //{
                        //    previousLearnAimRef = currentLearnAimRef;
                        //}

                        var record = new BulkUploadCourse
                        {
                            IsCourseHeader = isCourseHeader,
                            BulkUploadLineNumber = bulkUploadLineNumber,
                            LearnAimRef = currentLearnAimRef,
                            ProviderUKPRN = providerUKPRN,
                            CourseDescription = csv.GetField("WHO_IS_THIS_COURSE_FOR")
                            //Id = csv.GetField<int>("Id"),
                            //Name = csv.GetField("Name")
                        };
                        courses.Add(record);

                        previousLearnAimRef = currentLearnAimRef;
                        bulkUploadLineNumber++;
                    }
                }
            }

            return errors;
        }
    }
}
