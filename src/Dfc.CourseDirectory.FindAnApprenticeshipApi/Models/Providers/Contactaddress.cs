namespace Dfc.Providerportal.FindAnApprenticeship.Models.Providers
{
    public class Contactaddress
    {
        public SAON SAON { get; set; }
        public PAON PAON { get; set; }
        public string StreetDescription { get; set; }
        public object UniqueStreetReferenceNumber { get; set; }
        public object Locality { get; set; }
        public string[] Items { get; set; }
        public int[] ItemsElementName { get; set; }
        public object PostTown { get; set; }
        public string PostCode { get; set; }
        public object UniquePropertyReferenceNumber { get; set; }
    }
}