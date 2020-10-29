
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Dfc.CourseDirectory.FindACourseApi.Models;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface IVenueServiceWrapper
    {
        IEnumerable<AzureSearchVenueModel> GetVenues();
        T GetById<T>(Guid id);
    }
}
