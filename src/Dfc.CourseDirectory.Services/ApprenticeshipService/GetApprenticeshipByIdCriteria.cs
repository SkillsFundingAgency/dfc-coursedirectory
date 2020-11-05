using System;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public class GetApprenticeshipByIdCriteria
    {
        public Guid Id { get; }

        public GetApprenticeshipByIdCriteria(Guid id)
        {
            Id = id;
        }
    }
}
