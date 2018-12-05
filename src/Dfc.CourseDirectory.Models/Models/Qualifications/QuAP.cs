using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Qualifications;
using Dfc.CourseDirectory.Models.Models.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Qualifications
{
    public class QuAP : ValueObject<QuAP>, IQuAP
    {
        public Guid ID { get; }
        public Qualification Qualification { get; }
        public Provider Provider { get; }

        public QuAP(
            Guid id,
            Qualification qualification,
            Provider provider)
        {
            Throw.IfNull(id, nameof(id));
            Throw.IfNull(qualification, nameof(qualification));
            Throw.IfNull(provider, nameof(provider));

            ID = id;
            Qualification = qualification;
            Provider = provider;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ID;
            yield return Qualification;
            yield return Provider;
        }
    }
}
