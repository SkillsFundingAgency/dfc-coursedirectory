using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Models;
using System;

namespace Dfc.Providerportal.FindAnApprenticeship.Models
{
    public class ProgType : IProgType
    {
        public Guid Id { get; set; }
        public int ProgTypeId { get; set; }
        public string ProgTypeDesc { get; set; }
        public string ProgTypeDesc2 { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime? CreatedDateTimeUtc { get; set; }
        public DateTime? ModifiedDateTimeUtc { get; set; }
    }
}
