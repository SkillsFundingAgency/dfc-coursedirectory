using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using FluentValidation;
using Mapster;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        public async Task ProcessCourseFile(Guid courseUploadId, Stream stream)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var setProcessingResult = await dispatcher.ExecuteQuery(new SetCourseUploadProcessing()
                {
                    CourseUploadId = courseUploadId,
                    ProcessingStartedOn = _clock.UtcNow
                });

                if (setProcessingResult != SetCourseUploadProcessingResult.Success)
                {
                    await DeleteBlob();

                    return;
                }

                await dispatcher.Commit();
            }

            // At this point `stream` should be a CSV that's already known to conform to `CsvCourseRow`'s schema.
            // We read all the rows upfront because validation needs to group rows into courses.
            // We also don't expect massive files here so reading everything into memory is ok.
            List<CsvCourseRow> rows;
            using (var streamReader = new StreamReader(stream))
            using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                rows = await csvReader.GetRecordsAsync<CsvCourseRow>().ToListAsync();
            }

            var rowsCollection = new CourseDataUploadRowInfoCollection(
                rows: rows.Select((r, i) => new CourseDataUploadRowInfo(r, rowNumber: i + 2)));

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var venueUpload = await dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUploadId });
                var providerId = venueUpload.ProviderId;

                await ValidateCourseUploadRows(dispatcher, courseUploadId, providerId, rowsCollection);

                await dispatcher.Commit();
            }

            await DeleteBlob();

            Task DeleteBlob()
            {
                var blobName = $"{Constants.CoursesFolder}/{courseUploadId}.csv";
                return _blobContainerClient.DeleteBlobIfExistsAsync(blobName);
            }
        }

        public async Task<SaveFileResult> SaveCourseFile(Guid providerId, Stream stream, UserInfo uploadedBy)
        {
            CheckStreamIsProcessable(stream);

            if (await FileIsEmpty(stream))
            {
                return SaveFileResult.EmptyFile();
            }

            if (!await LooksLikeCsv(stream))
            {
                return SaveFileResult.InvalidFile();
            }

            var (fileMatchesSchemaResult, missingHeaders) = await FileMatchesSchema<CsvCourseRow>(stream);
            if (fileMatchesSchemaResult == FileMatchesSchemaResult.InvalidHeader)
            {
                return SaveFileResult.InvalidHeader(missingHeaders);
            }


            var courseUploadId = Guid.NewGuid();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                // Check there isn't an existing unprocessed upload for this provider

                var existingUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId
                });

                if (existingUpload != null && existingUpload.UploadStatus.IsUnprocessed())
                {
                    return SaveFileResult.ExistingFileInFlight();
                }

                // Abandon any existing un-published upload (there will be one at most)
                if (existingUpload != null)
                {
                    await dispatcher.ExecuteQuery(new SetCourseUploadAbandoned()
                    {
                        CourseUploadId = existingUpload.CourseUploadId,
                        AbandonedOn = _clock.UtcNow
                    });
                }

                await dispatcher.ExecuteQuery(new CreateCourseUpload()
                {
                    CourseUploadId = courseUploadId,
                    CreatedBy = uploadedBy,
                    CreatedOn = _clock.UtcNow,
                    ProviderId = providerId
                });

                await dispatcher.Transaction.CommitAsync();
            }

            await UploadToBlobStorage();

            return SaveFileResult.Success(courseUploadId, UploadStatus.Created);

            async Task UploadToBlobStorage()
            {
                if (!_containerIsKnownToExist)
                {
                    await _blobContainerClient.CreateIfNotExistsAsync();
                    _containerIsKnownToExist = true;
                }

                var blobName = $"{Constants.CoursesFolder}/{courseUploadId}.csv";
                await _blobContainerClient.UploadBlobAsync(blobName, stream);
            }
        }

        // internal for testing
        internal Guid? FindVenue(ParsedCsvCourseRow row, IReadOnlyCollection<Venue> providerVenues)
        {
            if (row.ResolvedDeliveryMode != CourseDeliveryMode.ClassroomBased)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(row.ProviderVenueRef))
            {
                // N.B. Using `Count()` here instead of `Single()` to protect against bad data where we have duplicates

                var matchedVenues = providerVenues
                    .Where(v => RefMatches(row.ProviderVenueRef, v))
                    .ToArray();

                if (matchedVenues.Length != 1)
                {
                    return null;
                }

                var venue = matchedVenues[0];

                // If VenueName was provided too then it must match
                if (!string.IsNullOrEmpty(row.VenueName) && !NameMatches(row.VenueName, venue))
                {
                    return null;
                }

                return venue.VenueId;
            }

            if (!string.IsNullOrEmpty(row.VenueName))
            {
                // N.B. Using `Count()` here instead of `Single()` to protect against bad data where we have duplicates

                var matchedVenues = providerVenues
                    .Where(v => NameMatches(row.VenueName, v))
                    .ToArray();

                if (matchedVenues.Length != 1)
                {
                    return null;
                }

                return matchedVenues[0].VenueId;
            }

            return null;

            static bool NameMatches(string name, Venue venue) => name.Equals(venue.VenueName, StringComparison.OrdinalIgnoreCase);

            static bool RefMatches(string providerVenueRef, Venue venue) => providerVenueRef.Equals(venue.ProviderVenueRef, StringComparison.OrdinalIgnoreCase);
        }

        // internal for testing
        internal async Task<(UploadStatus uploadStatus, IReadOnlyCollection<CourseUploadRow> Rows)> ValidateCourseUploadRows(
            ISqlQueryDispatcher sqlQueryDispatcher,
            Guid courseUploadId,
            Guid providerId,
            CourseDataUploadRowInfoCollection rows)
        {
            var regions = await _regionCache.GetAllRegions();

            var validLearningAimRefs = await sqlQueryDispatcher.ExecuteQuery(new GetLearningAimRefs()
            {
                LearningAimRefs = rows.Select(r => r.Data.LarsQan).Where(v => !string.IsNullOrWhiteSpace(v))
            });

            var providerVenues = await sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });

            var uploadIsValid = true;

            // Group rows into courses
            var courseGroups = rows.GroupBy(r => r.Data, new CsvCourseRowCourseComparer());

            var upsertRecords = new List<SetCourseUploadRowsRecord>();

            foreach (var group in courseGroups)
            {
                var courseId = Guid.NewGuid();

                foreach (var row in group)
                {
                    var rowNumber = row.RowNumber;
                    var courseRunId = Guid.NewGuid();

                    var parsedRow = ParsedCsvCourseRow.FromCsvCourseRow(row.Data);

                    var matchedVenueId = FindVenue(parsedRow, providerVenues);

                    var validator = new CourseUploadRowValidator(regions, validLearningAimRefs, matchedVenueId);

                    var rowValidationResult = validator.Validate(parsedRow);
                    var errors = rowValidationResult.Errors.Select(e => e.ErrorCode).ToArray();
                    var rowIsValid = rowValidationResult.IsValid;
                    uploadIsValid &= rowIsValid;

                    upsertRecords.Add(new SetCourseUploadRowsRecord()
                    {
                        RowNumber = rowNumber,
                        IsValid = rowIsValid,
                        Errors = errors,
                        CourseId = courseId,
                        CourseRunId = courseRunId,
                        LarsQan = parsedRow.LarsQan,
                        WhoThisCourseIsFor = parsedRow.WhoThisCourseIsFor,
                        EntryRequirements = parsedRow.EntryRequirements,
                        WhatYouWillLearn = parsedRow.WhatYouWillLearn,
                        HowYouWillLearn = parsedRow.HowYouWillLearn,
                        WhatYouWillNeedToBring = parsedRow.WhatYouWillNeedToBring,
                        HowYouWillBeAssessed = parsedRow.HowYouWillBeAssessed,
                        WhereNext = parsedRow.WhereNext,
                        CourseName = parsedRow.CourseName,
                        ProviderCourseRef = parsedRow.ProviderCourseRef,
                        DeliveryMode = parsedRow.DeliveryMode,
                        StartDate = parsedRow.StartDate,
                        FlexibleStartDate = parsedRow.FlexibleStartDate,
                        VenueName = parsedRow.VenueName,
                        ProviderVenueRef = parsedRow.ProviderVenueRef,
                        NationalDelivery = parsedRow.NationalDelivery,
                        SubRegions = parsedRow.SubRegions,
                        CourseWebpage = parsedRow.CourseWebPage,
                        Cost = parsedRow.Cost,
                        CostDescription = parsedRow.CostDescription,
                        Duration = parsedRow.Duration,
                        DurationUnit = parsedRow.DurationUnit,
                        StudyMode = parsedRow.StudyMode,
                        AttendancePattern = parsedRow.AttendancePattern,
                        ResolvedDeliveryMode = parsedRow.ResolvedDeliveryMode,
                        ResolvedStartDate = parsedRow.ResolvedStartDate,
                        ResolvedFlexibleStartDate = parsedRow.ResolvedFlexibleStartDate,
                        ResolvedNationalDelivery = parsedRow.ResolvedNationalDelivery,
                        ResolvedCost = parsedRow.ResolvedCost,
                        ResolvedDuration = parsedRow.ResolvedDuration,
                        ResolvedDurationUnit = parsedRow.ResolvedDurationUnit,
                        ResolvedStudyMode = parsedRow.ResolvedStudyMode,
                        ResolvedAttendancePattern = parsedRow.ResolvedAttendancePattern
                    });
                }
            }

            var updatedRows = await sqlQueryDispatcher.ExecuteQuery(new SetCourseUploadRows()
            {
                CourseUploadId = courseUploadId,
                ValidatedOn = _clock.UtcNow,
                Records = upsertRecords
            });

            await sqlQueryDispatcher.ExecuteQuery(new SetCourseUploadProcessed()
            {
                CourseUploadId = courseUploadId,
                ProcessingCompletedOn = _clock.UtcNow,
                IsValid = uploadIsValid
            });

            var uploadStatus = uploadIsValid ? UploadStatus.ProcessedSuccessfully : UploadStatus.ProcessedWithErrors;

            return (uploadStatus, updatedRows);
        }

        // internal for testing
        internal class CourseUploadRowValidator : AbstractValidator<ParsedCsvCourseRow>
        {
            public CourseUploadRowValidator(
                IReadOnlyCollection<Region> allRegions,
                IReadOnlyCollection<string> validLearningAimRefs,
                Guid? matchedVenueId)
            {
                RuleFor(c => c.LarsQan).LarsQan(validLearningAimRefs);
                RuleFor(c => c.WhoThisCourseIsFor).WhoThisCourseIsFor();
                RuleFor(c => c.EntryRequirements).EntryRequirements();
                RuleFor(c => c.WhatYouWillLearn).WhatYouWillLearn();
                RuleFor(c => c.HowYouWillLearn).HowYouWillLearn();
                RuleFor(c => c.WhatYouWillNeedToBring).WhatYouWillNeedToBring();
                RuleFor(c => c.HowYouWillBeAssessed).HowYouWillBeAssessed();
                RuleFor(c => c.WhereNext).WhereNext();
                RuleFor(c => c.CourseName).CourseName();
                RuleFor(c => c.ProviderCourseRef).ProviderCourseRef();
                RuleFor(c => c.ResolvedDeliveryMode).DeliveryMode();
                RuleFor(c => c.ResolvedStartDate).StartDate(c => c.ResolvedFlexibleStartDate);
                RuleFor(c => c.ResolvedFlexibleStartDate).FlexibleStartDate();
                RuleFor(c => c.VenueName).VenueName(c => c.ResolvedDeliveryMode, c => c.ProviderVenueRef, matchedVenueId);
                RuleFor(c => c.ProviderVenueRef).ProviderVenueRef(c => c.ResolvedDeliveryMode, c => c.VenueName, matchedVenueId);
                RuleFor(c => c.ResolvedNationalDelivery).NationalDelivery(c => c.ResolvedDeliveryMode);
                RuleFor(c => c.SubRegions).SubRegions(c => c.ResolvedDeliveryMode, c => c.ResolvedNationalDelivery, allRegions);
                RuleFor(c => c.CourseWebPage).CourseWebPage();
                RuleFor(c => c.ResolvedCost).Cost(c => c.CostDescription);
                RuleFor(c => c.CostDescription).CostDescription(c => c.ResolvedCost);
                RuleFor(c => c.ResolvedDuration).Duration();
                RuleFor(c => c.ResolvedDurationUnit).DurationUnit();
                RuleFor(c => c.ResolvedStudyMode).StudyMode(
                    studyModeWasSpecified: t => !string.IsNullOrEmpty(t.StudyMode),
                    c => c.ResolvedDeliveryMode);
                RuleFor(c => c.ResolvedAttendancePattern).AttendancePattern(
                    attendancePatternWasSpecified: t => !string.IsNullOrEmpty(t.AttendancePattern),
                    c => c.ResolvedDeliveryMode);
            }
        }

        internal class ParsedCsvCourseRow : CsvCourseRow
        {
            private const string DateFormat = "dd/MM/yyyy";

            private ParsedCsvCourseRow()
            {
            }

            public CourseDeliveryMode? ResolvedDeliveryMode => ResolveDeliveryMode(DeliveryMode);
            public DateTime? ResolvedStartDate => ResolveStartDate(StartDate);
            public bool? ResolvedFlexibleStartDate => ResolveFlexibleStartDate(FlexibleStartDate);
            public bool? ResolvedNationalDelivery => ResolveNationalDelivery(NationalDelivery);
            public decimal? ResolvedCost => ResolveCost(Cost);
            public int? ResolvedDuration => ResolveDuration(Duration);
            public CourseDurationUnit? ResolvedDurationUnit => ResolveDurationUnit(DurationUnit);
            public CourseStudyMode? ResolvedStudyMode => ResolveStudyMode(StudyMode);
            public CourseAttendancePattern? ResolvedAttendancePattern => ResolveAttendancePattern(AttendancePattern);

            public static ParsedCsvCourseRow FromCsvCourseRow(CsvCourseRow row)
            {
                var parsedRow = new ParsedCsvCourseRow();
                return row.Adapt(parsedRow);
            }

            public static CourseAttendancePattern? ResolveAttendancePattern(string value) => value?.ToLower() switch
            {
                "daytime" => CourseAttendancePattern.Daytime,
                "evening" => CourseAttendancePattern.Evening,
                "weekend" => CourseAttendancePattern.Weekend,
                "day/block release" => CourseAttendancePattern.DayOrBlockRelease,
                _ => (CourseAttendancePattern?)null
            };

            public static decimal? ResolveCost(string value) =>
                decimal.TryParse(value, out var result) && GetDecimalPlaces(result) <= 2 ? result : (decimal?)null;

            public static CourseDeliveryMode? ResolveDeliveryMode(string value) => value?.ToLower() switch
            {
                "classroom based" => CourseDeliveryMode.ClassroomBased,
                "classroom" => CourseDeliveryMode.ClassroomBased,
                "online" => CourseDeliveryMode.Online,
                "work based" => CourseDeliveryMode.WorkBased,
                "work" => CourseDeliveryMode.WorkBased,
                _ => (CourseDeliveryMode?)null
            };

            public static int? ResolveDuration(string value) =>
                int.TryParse(value, out var duration) ? duration : (int?)null;

            public static CourseDurationUnit? ResolveDurationUnit(string value) => value?.ToLower() switch
            {
                "hours" => CourseDurationUnit.Hours,
                "days" => CourseDurationUnit.Days,
                "weeks" => CourseDurationUnit.Weeks,
                "months" => CourseDurationUnit.Months,
                "years" => CourseDurationUnit.Years,
                _ => (CourseDurationUnit?)null
            };

            public static bool? ResolveFlexibleStartDate(string value) => value?.ToLower() switch
            {
                "yes" => true,
                "no" => false,
                "" => false,
                _ => null
            };

            public static bool? ResolveNationalDelivery(string value) => value?.ToLower() switch
            {
                "yes" => true,
                "no" => false,
                _ => null
            };

            public static DateTime? ResolveStartDate(string value) =>
                DateTime.TryParseExact(value, DateFormat, null, DateTimeStyles.None, out var dt) ? dt : (DateTime?)null;

            public static CourseStudyMode? ResolveStudyMode(string value) => value?.ToLower() switch
            {
                "full time" => CourseStudyMode.FullTime,
                "part time" => CourseStudyMode.PartTime,
                "flexible" => CourseStudyMode.Flexible,
                _ => (CourseStudyMode?)null
            };

            private static int GetDecimalPlaces(decimal n)
            {
                n = Math.Abs(n);
                n -= (int)n;

                var decimalPlaces = 0;
                while (n > 0)
                {
                    decimalPlaces++;
                    n *= 10;
                    n -= (int)n;
                }

                return decimalPlaces;
            }
        }

        private class CsvCourseRowCourseComparer : IEqualityComparer<CsvCourseRow>
        {
            public bool Equals([AllowNull] CsvCourseRow x, [AllowNull] CsvCourseRow y)
            {
                if (x is null && y is null)
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                // Don't group together records that have no LARS code
                if (string.IsNullOrEmpty(x.LarsQan) || string.IsNullOrEmpty(y.LarsQan))
                {
                    return false;
                }

                return
                    x.LarsQan == y.LarsQan &&
                    x.WhoThisCourseIsFor == y.WhoThisCourseIsFor &&
                    x.EntryRequirements == y.EntryRequirements &&
                    x.WhatYouWillLearn == y.WhatYouWillLearn &&
                    x.HowYouWillLearn == y.HowYouWillLearn &&
                    x.WhatYouWillNeedToBring == y.WhatYouWillNeedToBring &&
                    x.HowYouWillBeAssessed == y.HowYouWillBeAssessed &&
                    x.WhereNext == y.WhereNext;
            }

            public int GetHashCode([DisallowNull] CsvCourseRow obj) =>
                HashCode.Combine(
                    obj.LarsQan,
                    obj.WhoThisCourseIsFor,
                    obj.EntryRequirements,
                    obj.WhatYouWillLearn,
                    obj.HowYouWillLearn,
                    obj.WhatYouWillNeedToBring,
                    obj.HowYouWillBeAssessed,
                    obj.WhereNext);
        }
    }
}
