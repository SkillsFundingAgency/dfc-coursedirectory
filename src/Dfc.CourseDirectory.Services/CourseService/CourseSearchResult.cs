using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseSearchResult
    {
        public IEnumerable<CourseSearchOuterGrouping> Value { get; set; }

        public CourseSearchResult(
            IEnumerable<IEnumerable<IEnumerable<Course>>> courses)
        {
            if (courses == null)
            {
                throw new ArgumentNullException(nameof(courses));
            }

            Value = courses.Select(c => new CourseSearchOuterGrouping(c));
        }

        public CourseSearchResult(
            IEnumerable<CourseSearchOuterGrouping> courses)
        {
            if (courses == null)
            {
                throw new ArgumentNullException(nameof(courses));
            }

            Value = courses.Select(c => new CourseSearchOuterGrouping(c.Value, c.Level));
        }
    }

    public class CourseSearchOuterGrouping
    {
        public string QualType { get; set; }
        public string Level { get; set; }
        public IEnumerable<CourseSearchInnerGrouping> Value { get; set; }

        public CourseSearchOuterGrouping(
            IEnumerable<CourseSearchInnerGrouping> courses, 
            string level)
        {
            if (string.IsNullOrEmpty(level))
            {
                throw new ArgumentException($"{nameof(level)} cannot be null or empty.");
            }

            if (courses == null)
            {
                throw new ArgumentNullException(nameof(courses));
            }

            Level = level;
            Value = courses.Select(c => new CourseSearchInnerGrouping(c.LARSRef));
        }

        public CourseSearchOuterGrouping(
            IEnumerable<IEnumerable<Course>> courses)
        {
            if (courses == null)
            {
                throw new ArgumentNullException(nameof(courses));
            }

            Level = courses?.FirstOrDefault()?.FirstOrDefault()?.NotionalNVQLevelv2;
            Value = courses.Select(c => new CourseSearchInnerGrouping(c));
        }
    }

    public class CourseSearchInnerGrouping
    {
        public string LARSRef { get; set; }
        public IEnumerable<Course> Value { get; set; }

        public CourseSearchInnerGrouping(string larsRef)
        {
            if (string.IsNullOrEmpty(larsRef))
            {
                throw new ArgumentException($"{nameof(larsRef)} cannot be null or empty.");
            }

            LARSRef = larsRef;
            Value = new List<Course>();
        }

        public CourseSearchInnerGrouping(
            IEnumerable<Course> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = value;
            LARSRef = value?.FirstOrDefault()?.LearnAimRef;
        }
    }
}
