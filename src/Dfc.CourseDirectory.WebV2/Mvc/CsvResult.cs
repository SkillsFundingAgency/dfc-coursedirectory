using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Mvc
{
    public class CsvResult<T> : IActionResult
        where T : class
    {
        private readonly string _fileName;
        private readonly OneOf<IEnumerable<T>, IAsyncEnumerable<T>> _records;

        public CsvResult(string fileName, IEnumerable<T> records)
            : this(fileName)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            _records = OneOf<IEnumerable<T>, IAsyncEnumerable<T>>.FromT0(records);
        }

        public CsvResult(string fileName, IAsyncEnumerable<T> records)
            : this(fileName)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            _records = OneOf<IEnumerable<T>, IAsyncEnumerable<T>>.FromT1(records);
        }

        private CsvResult(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException($"'{nameof(fileName)}' cannot be null or whitespace.", nameof(fileName));
            }

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                throw new ArgumentException($"'{nameof(fileName)}' contains invalid characters.", nameof(fileName));
            }

            _fileName = fileName;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            context.HttpContext.Response.Headers.Add(HeaderNames.ContentType, "text/csv");
            context.HttpContext.Response.Headers.Add(
                HeaderNames.ContentDisposition,
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = _fileName
                }.ToString());

            await using (var stream = context.HttpContext.Response.Body)
            await using (var writer = new StreamWriter(stream))
            await using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteHeader<T>();
                await csvWriter.NextRecordAsync();
                await writer.FlushAsync();

                await _records.Match(
                    async records =>
                    {
                        foreach (var record in records)
                        {
                            await WriteRecord(record);
                        }
                    },
                    async records =>
                    {
                        await foreach (var record in records)
                        {
                            await WriteRecord(record);
                        }
                    });

                async Task WriteRecord(T record)
                {
                    csvWriter.WriteRecord(record);
                    await csvWriter.NextRecordAsync();
                    await writer.FlushAsync();
                }
            }
        }
    }
}
