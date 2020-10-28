
using System;
using Dfc.ProviderPortal.FindACourse.Models;


namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface ICourseDetailResult
    {
        Course Course { get; set; }
        dynamic /*Provider*/ Provider { get; set; }
        dynamic /*Venue*/ Venue { get; set; }
        dynamic /*Qualification*/ Qualification { get; set; }
    }
}
