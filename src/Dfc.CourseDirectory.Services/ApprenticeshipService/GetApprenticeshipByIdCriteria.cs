using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public class GetApprenticeshipByIdCriteria : ValueObject<GetApprenticeshipByIdCriteria>, IGetApprenticeshipByIdCriteria
    {
        public Guid Id { get; }

        public GetApprenticeshipByIdCriteria(
            Guid id)
        {
            Throw.IfNullGuid(id, nameof(id));

            Id = id;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}