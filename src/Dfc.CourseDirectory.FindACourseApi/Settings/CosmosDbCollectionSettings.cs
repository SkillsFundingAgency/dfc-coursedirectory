using Dfc.CourseDirectory.FindACourseApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.FindACourseApi.Settings
{
    public class CosmosDbCollectionSettings : ICosmosDbCollectionSettings
    {
        public string CoursesCollectionId { get; set; }
    }
}
