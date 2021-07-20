using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Xunit;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<(ApprenticeshipUpload apprenticeshipUpload, ApprenticeshipUploadRow[] Rows)> CreateApprenticeshipUpload(
            Guid providerId,
            UserInfo createdBy,
            UploadStatus uploadStatus,
            Action<ApprenticeshipUploadRowBuilder> configureRows = null)
        {
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

            var (courseUpload, rows) = await CreateApprenticeshipUpload(
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

        public Task<(ApprenticeshipUpload apprenticeshipUpload, ApprenticeshipUploadRow[] Rows)> CreateApprenticeshipUpload(
            Guid providerId,
            UserInfo createdBy,
            DateTime? createdOn = null,
            DateTime? processingStartedOn = null,
            DateTime? processingCompletedOn = null,
            DateTime? publishedOn = null,
            DateTime? abandonedOn = null,
            bool? isValid = null,
            Action<ApprenticeshipUploadRowBuilder> configureRows = null)
        {
            var apprenticeshipUploadId = Guid.NewGuid();
            createdOn ??= _clock.UtcNow;

            return WithSqlQueryDispatcher(async dispatcher =>
            {
                ApprenticeshipUploadRow[] rows = null;

                await dispatcher.ExecuteQuery(new CreateApprenticeshipUpload()
                {
                    ApprenticeshipUploadId = apprenticeshipUploadId,
                    ProviderId = providerId,
                    CreatedBy = createdBy,
                    CreatedOn = createdOn.Value
                });

                if (processingStartedOn.HasValue)
                {
                    await dispatcher.ExecuteQuery(new SetApprenticeshipUploadProcessing()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        ProcessingStartedOn = processingStartedOn.Value
                    });
                }

                if (processingCompletedOn.HasValue)
                {
                    if (!processingStartedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingStartedOn));
                    }

                    if (!isValid.HasValue)
                    {
                        throw new ArgumentNullException(nameof(isValid));
                    }

                    await dispatcher.ExecuteQuery(new SetApprenticeshipUploadProcessed()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        ProcessingCompletedOn = processingCompletedOn.Value,
                        IsValid = isValid.Value
                    });

                    var rowBuilder = new ApprenticeshipUploadRowBuilder();

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

                    rows = (await dispatcher.ExecuteQuery(new UpsertApprenticeshipUploadRows()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
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

                    //set published handler
                }
                else if (abandonedOn.HasValue)
                {
                    if (!processingCompletedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingCompletedOn));
                    }
                    
                    //set abandoned handler
                }

                var apprenticeshipUpload = await dispatcher.ExecuteQuery(new GetApprenticeshipUpload()
                {
                    ApprenticeshipUploadId = apprenticeshipUploadId
                });

                return (apprenticeshipUpload, rows);
            });
        }

        public class ApprenticeshipUploadRowBuilder
        {
            private readonly List<UpsertApprenticeshipUploadRowsRecord> _records = new List<UpsertApprenticeshipUploadRowsRecord>();

            public ApprenticeshipUploadRowBuilder AddRow(string learnAimRef, Action<UpsertApprenticeshipUploadRowsRecord> configureRecord)
            {
                var record = CreateValidRecord(learnAimRef);
                configureRecord(record);
                _records.Add(record);
                return this;
            }

            public ApprenticeshipUploadRowBuilder AddRow(
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
                Guid? venueId,
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
                    venueId,
                    errors);

                _records.Add(record);

                return this;
            }

            public ApprenticeshipUploadRowBuilder AddValidRow(string learnAimRef)
            {
                var record = CreateValidRecord(learnAimRef);
                _records.Add(record);
                return this;
            }

            public ApprenticeshipUploadRowBuilder AddValidRows(string learnAimRef, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    AddValidRow(learnAimRef);
                }

                return this;
            }

            internal IReadOnlyCollection<UpsertApprenticeshipUploadRowsRecord> GetUpsertQueryRows() => _records;

            private UpsertApprenticeshipUploadRowsRecord CreateRecord(
                Guid courseId,
                Guid courseRunId,
                string learnAimRef,
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
                Guid? venueId,
                IEnumerable<string> errors = null)
            {
                var errorsArray = errors?.ToArray() ?? Array.Empty<string>();
                var isValid = !errorsArray.Any();

                return new UpsertApprenticeshipUploadRowsRecord()
                {
                    RowNumber = _records.Count + 2,
                    IsValid = isValid
                };
            }

            private UpsertApprenticeshipUploadRowsRecord CreateValidRecord(string learnAimRef)
            {
                return CreateRecord(
                    courseId: Guid.NewGuid(),
                    courseRunId: Guid.NewGuid(),
                    learnAimRef: learnAimRef,
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
                    attendancePattern: "",
                    venueId: null);
            }
        }
    }
}
