namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Models
{
    public interface IAddress
    {
        string Address1 { get; set; }

        string Address2 { get; set; }

        string County { get; set; }

        double? Latitude { get; set; }

        double? Longitude { get; set; }

        string Postcode { get; set; }

        string Town { get; set; }

        string Email { get; set; }

        string Website { get; set; }

        string Phone { get; set; }
    }
}