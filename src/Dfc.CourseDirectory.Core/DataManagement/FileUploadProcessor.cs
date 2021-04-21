using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Dfc.CourseDirectory.Core.DataStore.Sql;

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

        public FileUploadProcessor(
            ISqlQueryDispatcherFactory sqlQueryDispatcherFactory,
            BlobServiceClient blobServiceClient,
            IClock clock)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(Constants.ContainerName);
            _clock = clock;
        }

        private static async Task<bool> FileIsEmpty(Stream stream)
        {
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

        private static async Task<bool> LooksLikeCsv(Stream stream)
        {
            // Check the file looks like a CSV. CsvReader will read whatever data it's given until it finds column and
            // row separators, even if the file is binary, so we can't use it and get meaningful errors out.
            // The check here tries to read a line of data, up to 512 bytes. (512 bytes is ample to fit a row of our
            // headers + some custom ones). If we can successfully read a line and its contents are valid ASCII
            // then that's a good enough signal.

            const int readBufferSize = 512;

            var buffer = ArrayPool<byte>.Shared.Rent(readBufferSize);

            try
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, readBufferSize);

                // Restore the Stream's position
                stream.Seek(-bytesRead, SeekOrigin.Current);

                for (int i = 0; i < bytesRead; i++)
                {
                    byte c = buffer[i];

                    if (c > 127)
                    {
                        // Outside of ASCII
                        return false;
                    }

                    if (c == '\n' && i > 1)
                    {
                        // We've hit the end of the line without errors
                        return true;
                    }
                }

                // We've finished reading the whole buffer and not found an EOL - probably not a CSV
                return false;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
