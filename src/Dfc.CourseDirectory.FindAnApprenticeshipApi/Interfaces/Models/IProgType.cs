using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Models
{
    public interface IProgType
    {
        Guid Id { get; }
        int ProgTypeId { get; }
        string ProgTypeDesc { get; }
        string ProgTypeDesc2 { get; }
        DateTime? EffectiveFrom { get; }
        DateTime? EffectiveTo { get; }
        DateTime? CreatedDateTimeUtc { get; }
        DateTime? ModifiedDateTimeUtc { get; }
    }
}
