namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IAddressSelectionResult
    {
        string Id { get; }
        string Line1 { get; }

        string Line2 { get; }

        string County { get; }

        string City { get; }

        string PostCode { get; }

    }
}


