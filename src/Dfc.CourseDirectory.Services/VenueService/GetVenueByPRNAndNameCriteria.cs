using System;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class GetVenuesByPRNAndNameCriteria
    {
        public string PRN { get; }
        public string Name { get; }

        public GetVenuesByPRNAndNameCriteria(
            string prn,
            string name)
        {
            if (string.IsNullOrWhiteSpace(prn))
            {
                throw new ArgumentNullException($"{nameof(prn)} cannot be null or empty or whitespace.", nameof(prn));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException($"{nameof(name)} cannot be null or empty or whitespace.", nameof(name));
            }

            PRN = prn;
            Name = name;
        }
    }
}
