using System;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.ProviderPortal.FindACourse
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
