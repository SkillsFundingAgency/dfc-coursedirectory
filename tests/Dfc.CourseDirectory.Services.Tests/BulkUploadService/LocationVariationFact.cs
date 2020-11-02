using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Services.Tests.BulkUploadService
{
    public class LocationVariationFact
    {
        public string DeliveryMethod { get; set; }
        public string DeliveryMode { get; set; }
        public string AcrossEngland { get; set; }
        public string NationalDelivery { get; set; }
        public string Venue { get; set; }
        public string Region { get; set; }
        public string SubRegion { get; set; }
        public string ExpectedError { get; set; }
        public ApprenticeshipDeliveryMode? ExpectedOutputDeliveryMode { get; set; }

        public override string ToString()
        {
            // ToString() is used by the test runner, so this makes it possible to tell which test is which
            return $"method:'{DeliveryMethod}',mode:'{DeliveryMode}',england:'{AcrossEngland}'," +
                   $"national:'{NationalDelivery}',venue:'{Venue}',region:'{Region}',subRegion:'{SubRegion}'";
        }
    }
}
