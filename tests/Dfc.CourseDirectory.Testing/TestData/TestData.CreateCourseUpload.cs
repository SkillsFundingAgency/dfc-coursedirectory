using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Xunit;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<(CourseUpload CourseUpload, CourseUploadRow[] Rows)> CreateCourseUpload(
            Guid providerId,
            UserInfo createdBy,
            UploadStatus uploadStatus,
            Action<CourseUploadRowBuilder> configureRows = null)
        {
            if (uploadStatus != UploadStatus.Created &&
                uploadStatus != UploadStatus.Processing &&
                uploadStatus != UploadStatus.ProcessedSuccessfully &&
                uploadStatus != UploadStatus.ProcessedWithErrors)
            {
                throw new NotImplementedException();
            }

            var createdOn = _clock.UtcNow;

            DateTime? processingStartedOn = uploadStatus >= UploadStatus.Processing ? createdOn.AddSeconds(3) : (DateTime?)null;
            DateTime? processingCompletedOn = uploadStatus >= UploadStatus.ProcessedWithErrors ? processingStartedOn.Value.AddSeconds(30) : (DateTime?)null;
            DateTime? publishedOn = uploadStatus == UploadStatus.Published ? processingCompletedOn.Value.AddHours(2) : (DateTime?)null;
            DateTime? abandonedOn = uploadStatus == UploadStatus.Abandoned ? processingCompletedOn.Value.AddHours(2) : (DateTime?)null;

            var isValid = uploadStatus switch
            {
                UploadStatus.ProcessedWithErrors => false,
                UploadStatus.Created | UploadStatus.Processing => (bool?)null,
                _ => true
            };

            var (courseUpload, rows) = await CreateCourseUpload(
                providerId,
                createdBy,
                createdOn,
                processingStartedOn,
                processingCompletedOn,
                publishedOn,
                abandonedOn,
                isValid,
                configureRows);

            Assert.Equal(uploadStatus, courseUpload.UploadStatus);

            return (courseUpload, rows);
        }

        public Task<(CourseUpload CourseUpload, CourseUploadRow[] Rows)> CreateCourseUpload(
            Guid providerId,
            UserInfo createdBy,
            DateTime? createdOn = null,
            DateTime? processingStartedOn = null,
            DateTime? processingCompletedOn = null,
            DateTime? publishedOn = null,
            DateTime? abandonedOn = null,
            bool? isValid = null,
            Action<CourseUploadRowBuilder> configureRows = null)
        {
            var courseUploadId = Guid.NewGuid();
            createdOn ??= _clock.UtcNow;

            return WithSqlQueryDispatcher(async dispatcher =>
            {
                CourseUploadRow[] rows = null;

                await dispatcher.ExecuteQuery(new CreateCourseUpload()
                {
                    CourseUploadId = courseUploadId,
                    ProviderId = providerId,
                    CreatedBy = createdBy,
                    CreatedOn = createdOn.Value
                });

                if (processingStartedOn.HasValue)
                {
                    await dispatcher.ExecuteQuery(new SetCourseUploadProcessing()
                    {
                        CourseUploadId = courseUploadId,
                        ProcessingStartedOn = processingStartedOn.Value
                    });

                    if (!isValid.HasValue)
                    {
                        throw new ArgumentNullException(nameof(isValid));
                    }
                }

                if (processingCompletedOn.HasValue)
                {
                    if (!processingStartedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingStartedOn));
                    }

                    await dispatcher.ExecuteQuery(new SetCourseUploadProcessed()
                    {
                        CourseUploadId = courseUploadId,
                        ProcessingCompletedOn = processingCompletedOn.Value,
                        IsValid = isValid.Value
                    });

                    var rowBuilder = new CourseUploadRowBuilder();

                    if (configureRows != null)
                    {
                        configureRows(rowBuilder);
                    }
                    else
                    {
                        if (isValid.Value)
                        {
                            var learnAimRef = await CreateLearningAimRef();
                            rowBuilder.AddValidRows(learnAimRef, 3);
                        }
                        else
                        {
                            rowBuilder.AddRow(learnAimRef: string.Empty, record =>
                            {
                                record.IsValid = false;
                                record.Errors = new[] { ErrorRegistry.All["COURSE_LARS_QAN_REQUIRED"].ErrorCode };
                            });
                        }
                    }

                    rows = (await dispatcher.ExecuteQuery(new SetCourseUploadRows()
                    {
                        CourseUploadId = courseUploadId,
                        Records = rowBuilder.GetUpsertQueryRows(),
                        UpdatedOn = processingCompletedOn.Value,
                        ValidatedOn = processingCompletedOn.Value
                    })).ToArray();
                }

                if (publishedOn.HasValue)
                {
                    if (!processingCompletedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingCompletedOn));
                    }

                    await dispatcher.ExecuteQuery(new PublishCourseUpload()
                    {
                        CourseUploadId = courseUploadId,
                        PublishedBy = createdBy,
                        PublishedOn = publishedOn.Value
                    });
                }
                else if (abandonedOn.HasValue)
                {
                    if (!processingCompletedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingCompletedOn));
                    }

                    await dispatcher.ExecuteQuery(new SetCourseUploadAbandoned()
                    {
                        CourseUploadId = courseUploadId,
                        AbandonedOn = abandonedOn.Value
                    });
                }

                var courseUpload = await dispatcher.ExecuteQuery(new GetCourseUpload()
                {
                    CourseUploadId = courseUploadId
                });

                return (courseUpload, rows);
            });
        }

        public class CourseUploadRowBuilder
        {
            private readonly List<SetCourseUploadRowsRecord> _records = new List<SetCourseUploadRowsRecord>();

            public CourseUploadRowBuilder AddRow(string learnAimRef, Action<SetCourseUploadRowsRecord> configureRecord)
            {
                var record = CreateValidRecord(learnAimRef);
                configureRecord(record);
                _records.Add(record);
                return this;
            }

            public CourseUploadRowBuilder AddRow(
                Guid courseId,
                Guid courseRunId,
                string larsQan,
                string whoThisCourseIsFor,
                string entryRequirements,
                string whatYouWillLearn,
                string howYouWillLearn,
                string whatYouWillNeedToBring,
                string howYouWillBeAssessed,
                string whereNext,
                string courseName,
                string providerCourseRef,
                string deliveryMode,
                string startDate,
                string flexibleStartDate,
                string venueName,
                string providerVenueRef,
                string nationalDelivery,
                string subRegions,
                string courseWebpage,
                string cost,
                string costDescription,
                string duration,
                string durationUnit,
                string studyMode,
                string attendancePattern,
                IEnumerable<string> errors = null)
            {
                var record = CreateRecord(
                    courseId,
                    courseRunId,
                    larsQan,
                    whoThisCourseIsFor,
                    entryRequirements,
                    whatYouWillLearn,
                    howYouWillLearn,
                    whatYouWillNeedToBring,
                    howYouWillBeAssessed,
                    whereNext,
                    courseName,
                    providerCourseRef,
                    deliveryMode,
                    startDate,
                    flexibleStartDate,
                    venueName,
                    providerVenueRef,
                    nationalDelivery,
                    subRegions,
                    courseWebpage,
                    cost,
                    costDescription,
                    duration,
                    durationUnit,
                    studyMode,
                    attendancePattern,
                    errors);

                _records.Add(record);

                return this;
            }

            public CourseUploadRowBuilder AddValidRow(string learnAimRef)
            {
                var record = CreateValidRecord(learnAimRef);
                _records.Add(record);
                return this;
            }

            public CourseUploadRowBuilder AddValidRows(string learnAimRef, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    AddValidRow(learnAimRef);
                }

                return this;
            }

            internal IReadOnlyCollection<SetCourseUploadRowsRecord> GetUpsertQueryRows() => _records;

            private SetCourseUploadRowsRecord CreateRecord(
                Guid courseId,
                Guid courseRunId,
                string larsQan,
                string whoThisCourseIsFor,
                string entryRequirements,
                string whatYouWillLearn,
                string howYouWillLearn,
                string whatYouWillNeedToBring,
                string howYouWillBeAssessed,
                string whereNext,
                string courseName,
                string providerCourseRef,
                string deliveryMode,
                string startDate,
                string flexibleStartDate,
                string venueName,
                string providerVenueRef,
                string nationalDelivery,
                string subRegions,
                string courseWebpage,
                string cost,
                string costDescription,
                string duration,
                string durationUnit,
                string studyMode,
                string attendancePattern,
                IEnumerable<string> errors = null)
            {
                var errorsArray = errors?.ToArray() ?? Array.Empty<string>();
                var isValid = !errorsArray.Any();

                return new SetCourseUploadRowsRecord()
                {
                    RowNumber = _records.Count + 2,
                    IsValid = isValid,
                    Errors = errorsArray,
                    CourseId = courseId,
                    CourseRunId = courseRunId,
                    LarsQan = larsQan,
                    WhoThisCourseIsFor = whoThisCourseIsFor,
                    EntryRequirements = entryRequirements,
                    WhatYouWillLearn = whatYouWillLearn,
                    HowYouWillLearn = howYouWillLearn,
                    WhatYouWillNeedToBring = whatYouWillNeedToBring,
                    HowYouWillBeAssessed = howYouWillBeAssessed,
                    WhereNext = whereNext,
                    CourseName = courseName,
                    ProviderCourseRef = providerCourseRef,
                    DeliveryMode = deliveryMode,
                    StartDate = startDate,
                    FlexibleStartDate = flexibleStartDate,
                    VenueName = venueName,
                    ProviderVenueRef = providerVenueRef,
                    NationalDelivery = nationalDelivery,
                    SubRegions = subRegions,
                    CourseWebpage = courseWebpage,
                    Cost = cost,
                    CostDescription = costDescription,
                    Duration = duration,
                    DurationUnit = durationUnit,
                    StudyMode = studyMode,
                    AttendancePattern = attendancePattern
                };
            }

            private SetCourseUploadRowsRecord CreateValidRecord(string learnAimRef)
            {
                return CreateRecord(
                    courseId: Guid.NewGuid(),
                    courseRunId: Guid.NewGuid(),
                    larsQan: learnAimRef,
                    whoThisCourseIsFor: "Who this course is for",
                    entryRequirements: "",
                    whatYouWillLearn: "",
                    howYouWillLearn: "",
                    whatYouWillNeedToBring: "",
                    howYouWillBeAssessed: "",
                    whereNext: "",
                    courseName: "Course name",
                    providerCourseRef: "",
                    deliveryMode: "Online",
                    startDate: "",
                    flexibleStartDate: "yes",
                    venueName: "",
                    providerVenueRef: "",
                    nationalDelivery: "",
                    subRegions: "",
                    courseWebpage: "",
                    cost: "",
                    costDescription: "Free",
                    duration: "2",
                    durationUnit: "years",
                    studyMode: "",
                    attendancePattern: "");
            }
        }
    }
}
