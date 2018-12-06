using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Models.Models.Qualifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Qualifications
{
    public interface IQuAP
    {
        Guid ID { get; }
        Qualification Qualification { get; }
        Provider Provider { get; }
    }
}
