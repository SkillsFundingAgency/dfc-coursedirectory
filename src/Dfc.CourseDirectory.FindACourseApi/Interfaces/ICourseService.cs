using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindACourseApi.Models;

namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface ICourseService
    {
        Task<AzureSearchCourseDetail> CourseDetail(Guid courseId, Guid courseRunId);
    }
}
