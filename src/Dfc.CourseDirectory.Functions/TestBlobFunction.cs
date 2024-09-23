using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class TestBlobFunction
    {
        private readonly ILogger<TestBlobFunction> _logger;

        public TestBlobFunction(ILogger<TestBlobFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(TestBlobFunction))]
        public async Task Run([BlobTrigger("data-uploads/courses/{courseUploadId}.csv", Connection = "")] Stream file,
            Guid courseUploadId)
        {
            using var blobStreamReader = new StreamReader(file);
            var content = await blobStreamReader.ReadToEndAsync();
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {courseUploadId} \n Data: {content}");
        }
    }
}
