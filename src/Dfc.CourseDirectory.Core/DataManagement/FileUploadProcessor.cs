using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor : IFileUploadProcessor
    {
        private static readonly byte[] _bom = new byte[] { 0xEF, 0xBB, 0xBF };
        private static readonly TimeSpan _statusUpdatesPollInterval = TimeSpan.FromMilliseconds(500);

        private static bool _containerIsKnownToExist;

        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly IClock _clock;
        private readonly IRegionCache _regionCache;

        public FileUploadProcessor(
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            BlobServiceClient blobServiceClient,
            IClock clock,
            IRegionCache regionCache)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(Constants.ContainerName);
            _clock = clock;
            _regionCache = regionCache;
        }

        protected internal async Task<bool> FileIsEmpty(Stream stream)
        {
            CheckStreamIsProcessable(stream);

            if (stream.Length == 0)
            {
                return true;
            }

            // The file could be empty except for BOM. Check for that too.
            if (stream.Length == 3)
            {
                var buffer = new byte[3];

                await stream.ReadAsync(buffer, 0, 3);
                stream.Seek(-3, SeekOrigin.Current);

                if (buffer.SequenceEqual(_bom))
                {
                    return true;
                }
            }

            return false;
        }

        protected internal async Task<(FileMatchesSchemaResult Result, string[] MissingHeaders)> FileMatchesSchema<TRow>(Stream stream)
        {
            // Check file conforms to schema
            // There are two parts to this check; the first is to check we have all the required headers.
            // We don't care about the order and we don't care if there are additional columns.
            // If this fails we need to output the headers that are missing.
            // The second part is to check that each row has the correct column count
            // e.g. if there are 10 headers but a row has 9 columns then that's not valid.
            // If we encounter this we return SaveFileResult.InvalidFile, since that's not a valid CSV.
            // CsvHelper doesn't blow up if too many columns are provided so we don't bother checking for that.

            CheckStreamIsProcessable(stream);

            try
            {
                using (var streamReader = new StreamReader(stream, leaveOpen: true))
                using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    await csvReader.ReadAsync();
                    csvReader.ReadHeader();

                    var expectedHeaders = csvReader.Context.ReaderConfiguration.AutoMap<TRow>().MemberMaps
                        .Where(m => !m.Data.Ignore)
                        .Select(m => m.Data.Names.Single())
                        .ToArray();

                    var missingHeaders = expectedHeaders.Except(csvReader.Context.HeaderRecord).ToArray();
                    if (missingHeaders.Length != 0)
                    {
                        return (FileMatchesSchemaResult.InvalidHeader, missingHeaders);
                    }

                    try
                    {
                        await csvReader.GetRecordsAsync<TRow>().ToListAsync();
                    }
                    catch (CsvHelper.MissingFieldException)
                    {
                        return (FileMatchesSchemaResult.InvalidRows, Array.Empty<string>());
                    }
                }

                return (FileMatchesSchemaResult.Ok, Array.Empty<string>());
            }
            finally
            {
                stream.Seek(0L, SeekOrigin.Begin);
            }
        }

        protected internal async Task<bool> LooksLikeCsv(Stream stream)
        {
            CheckStreamIsProcessable(stream);

            // Check the file looks like a CSV. CsvReader will read whatever data it's given until it finds column and
            // row separators, even if the file is binary, so we can't use it and get meaningful errors out.
            // The check here tries to read a line of data, up to 512 bytes. (512 bytes is ample to fit a row of our
            // headers + some custom ones). If we can successfully read a line and its contents are valid ASCII
            // and there is at least one comma then that's a good enough signal.

            const int readBufferSize = 512;

            var buffer = ArrayPool<byte>.Shared.Rent(readBufferSize);

            try
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, readBufferSize);

                // Restore the Stream's position
                stream.Seek(-bytesRead, SeekOrigin.Current);

                var position = 0;

                // Check for BOM
                if (bytesRead >= _bom.Length)
                {
                    if (buffer.Take(_bom.Length).SequenceEqual(_bom))
                    {
                        position += _bom.Length;
                    }
                }

                var foundAComma = false;

                for (; position < bytesRead; position++)
                {
                    byte c = buffer[position];

                    if (c > 127)
                    {
                        // Outside of ASCII
                        return false;
                    }

                    if (c == '\n' && position > 1)
                    {
                        // We've hit the end of the line
                        return foundAComma;
                    }
                    else if (c == ',')
                    {
                        foundAComma = true;
                    }
                }

                // If we've read the entire buffer and not hit an LF and the there's more to read
                // then probably not a CSV
                if (stream.Length > readBufferSize)
                {
                    return false;
                }

                // We've read the entire file but haven't got an LF;
                // e.g. a single row of headers - could be valid
                return foundAComma;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private void CheckStreamIsProcessable(Stream stream)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream must be readable.", nameof(stream));
            }

            if (!stream.CanSeek)
            {
                throw new ArgumentException("Stream must be seekable.", nameof(stream));
            }
        }

        protected internal async Task<(FileMatchesSchemaResult Result, string[] Missing, string[] Invalid, string[] Expired)> CheckLearnAimRefs(Stream stream)
        {
            CheckStreamIsProcessable(stream);

            try
            {
                using (var streamReader = new StreamReader(stream, leaveOpen: true))
                using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
                {
                    await csvReader.ReadAsync();
                    csvReader.ReadHeader();

                    List<string> missing = new List<string>();
                    List<string> invalid = new List<string>();
                    List<string> expired = new List<string>();
                    List<CsvCourseRow> csvRecords = csvReader.GetRecords<CsvCourseRow>().ToList();

                    var validLearningAimRefs = await dispatcher.ExecuteQuery(new GetLearningDeliveries() { LearningAimRefs = csvRecords.Select(l=>l.LearnAimRef) });

                    int rowCount = 1;

                    foreach (var row in csvRecords)
                    {
                        rowCount++;
                        string larsRow = row.LearnAimRef.Trim();
                        var validLearningAimRef = validLearningAimRefs.Where(l => l.LearnAimRef == larsRow).FirstOrDefault();
                        //validLearningAimRef.
                        if (string.IsNullOrWhiteSpace(larsRow))
                        {
                            missing.Add(rowCount.ToString());
                        }
                        if (!string.IsNullOrWhiteSpace(larsRow) && validLearningAimRef == null)
                        {
                            invalid.Add(rowCount.ToString());
                        }
                        if (validLearningAimRef != null
                            && validLearningAimRef.EffectiveTo.HasValue
                            && validLearningAimRef.EffectiveTo < DateTime.Now)
                        {
                            expired.Add(string.Format("Row {0}, expired code {1}", rowCount.ToString(), larsRow));
                        }
                    }
                    if (missing.Count > 0 || invalid.Count > 0 || expired.Count > 0)
                    {
                        return (FileMatchesSchemaResult.InvalidLars, missing.ToArray(), invalid.ToArray(), expired.ToArray());
                    }
                }

                return (FileMatchesSchemaResult.Ok, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
            }
            finally
            {
                stream.Seek(0L, SeekOrigin.Begin);
            }
        }

        protected internal enum FileMatchesSchemaResult
        {
            Ok,
            InvalidHeader,
            InvalidRows,
            InvalidLars
        }
    }
}
