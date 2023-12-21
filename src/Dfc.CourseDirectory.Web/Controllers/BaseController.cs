﻿using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
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

        public BaseController(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        protected bool IsCourseNonLars()
        {
            var nonLarsCourseString = Session.GetString(SessionNonLarsCourse);
            return !string.IsNullOrWhiteSpace(nonLarsCourseString) && nonLarsCourseString == "true";
        }

        protected async Task<Course> GetCourse(Guid? courseId, bool nonLarsCourse)
        {
            Course course = null;
            if (courseId.HasValue)
            {
                if (nonLarsCourse)
                {
                    course = await _sqlQueryDispatcher.ExecuteQuery(new GetNonLarsCourse() { CourseId = courseId.Value });
                    return course;
                }

                course = await _sqlQueryDispatcher.ExecuteQuery(new GetCourse() { CourseId = courseId.Value });
            }

            return course;
        }
    }
}
