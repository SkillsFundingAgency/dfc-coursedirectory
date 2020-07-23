using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Services.Tests.BulkUploadService.Apprenticeship
{
    public class LocationVariationFact
    {
        public string deliveryMethod { get; set; }
        public string deliveryMode { get; set; }
        public string acrossEngland { get; set; }
        public string nationalDelivery { get; set; }
        public string venue { get; set; }
        public string region { get; set; }
        public string subRegion { get; set; }
        public string expectedError { get; set; }
        public ApprenticeshipDeliveryMode? expectedOutputDeliveryMode { get; set; }

        public override string ToString()
        {
            // ToString() is used by the test runner, so this makes it possible to tell which test is which
            return $"method:'{deliveryMethod}',mode:'{deliveryMode}',england:'{acrossEngland}'," +
                   $"national:'{nationalDelivery}',venue:'{venue}',region:'{region}',subRegion:'{subRegion}'";
        }
    }
}
