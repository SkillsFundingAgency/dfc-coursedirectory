
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Dfc.CourseDirectory.FindACourseApi.Models;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface IFindACourseAzureSearchResult
    {
        Course Course { get; }
        dynamic Provider { get; }
        dynamic Venue { get; }
    }
}
