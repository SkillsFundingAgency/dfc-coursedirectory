using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class GetVenueByIdCriteria : IGetVenueByIdCriteria
    {
        public string Id { get; }

        public GetVenueByIdCriteria(
            string id)
        {
            Throw.IfNullOrWhiteSpace(id, nameof(id));

            Id = id;
        }
    }
}
