using System;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.FindACourseApi
{
    public class ProblemDetailsException : Exception
    {
        public ProblemDetailsException(ProblemDetails details)
        {
            ProblemDetails = details;
        }

        public ProblemDetails ProblemDetails { get; }
    }
}
