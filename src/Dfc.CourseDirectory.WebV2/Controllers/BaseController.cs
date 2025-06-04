using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Controllers
{
    public class BaseController : Controller
    {
        private ISession Session => HttpContext.Session;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        protected const string SessionVenues = "Venues";
        protected const string SessionRegions = "Regions";
        protected const string SessionAddCourseSection1 = "AddCourseSection1";
        protected const string SessionAddCourseSection2 = "AddCourseSection2";
        protected const string SessionLastAddCoursePage = "LastAddCoursePage";
        protected const string SessionSummaryPageLoadedAtLeastOnce = "SummaryLoadedAtLeastOnce";
        protected const string SessionPublishedCourse = "PublishedCourse";
        protected const string SessionNonLarsCourse = "NonLarsCourse";
        protected const string SessionLearnAimRef = "LearnAimRef";
        protected const string SessionLearnAimRefTitle = "LearnAimRefTitle";
        protected const string SessionAwardOrgCode = "AwardOrgCode";
        protected const string SessionNotionalNvqLevelV2 = "NotionalNVQLevelv2";
        protected const string SessionLearnAimRefTypeDesc = "LearnAimRefTypeDesc";
        protected const string SessionSectors = "Sectors";
        protected const int DefaultSectorId = 1;

        public BaseController(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        protected bool IsCourseNonLars()
        {
            var nonLarsCourseString = Session.GetString(SessionNonLarsCourse);
            return !string.IsNullOrWhiteSpace(nonLarsCourseString) && nonLarsCourseString == "true";
        }

        protected async Task<Course> GetCourse(Guid? courseId) 
        { 
            return await GetCourse(courseId, IsCourseNonLars());
        }

        protected async Task<Course> GetCourse(Guid? courseId, bool nonLarsCourse)
        {            
            if (!courseId.HasValue)
            {
                return null;
            }

            Course course;
            if (nonLarsCourse)
            {
                course = await _sqlQueryDispatcher.ExecuteQuery(new GetNonLarsCourse() { CourseId = courseId.Value });
                return course;
            }

            course = await _sqlQueryDispatcher.ExecuteQuery(new GetCourse() { CourseId = courseId.Value });

            return course;
        }

        protected async Task<List<Sector>> GetSectors()
        {
            var sectors = Session.GetObject<List<Sector>>(SessionSectors);

            if (sectors != null)
            {
                return sectors;
            }

            sectors = (await _sqlQueryDispatcher.ExecuteQuery(new GetSectors())).ToList();

            Session.SetObject(SessionSectors, sectors);
            return sectors;
        }

        protected async Task<string> GetSectorDescription(int? sectorId)
        {
            if (!sectorId.HasValue)
                return string.Empty;

            var sectors = await GetSectors();
            var selectedSector = sectors.FirstOrDefault(s => s.Id == sectorId);

            return selectedSector?.Description ?? string.Empty;
        }
    }
}
