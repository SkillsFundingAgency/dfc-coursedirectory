using System;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class GetVenueByIdCriteria
    {
        public string Id { get; }

        public GetVenueByIdCriteria(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException($"{nameof(id)} cannot be null or empty or whitespace.", nameof(id));
            }

            Id = id;
        }
    }
}
