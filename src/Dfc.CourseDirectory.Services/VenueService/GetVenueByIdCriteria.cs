namespace Dfc.CourseDirectory.Services.VenueService
{
    public class GetVenueByIdCriteria
    {
        public string Id { get; }

        public GetVenueByIdCriteria(string id)
        {
            Throw.IfNullOrWhiteSpace(id, nameof(id));

            Id = id;
        }
    }
}
