using Dfc.CourseDirectory.Common;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class PageBoundary : ValueObject<PageBoundary>
    {
        public int StartAt { get; }
        public int EndAt { get; }

        public PageBoundary(
            int totalNoOfPages, 
            int noOfPagesToDisplay,
            int currentPageNo,
            bool isSliding)
        {
            Throw.IfLessThan(1, totalNoOfPages, nameof(totalNoOfPages));
            Throw.IfLessThan(1, noOfPagesToDisplay, nameof(noOfPagesToDisplay));
            Throw.IfLessThan(1, currentPageNo, nameof(currentPageNo));

            StartAt = 1;
            EndAt = totalNoOfPages < noOfPagesToDisplay
                ? totalNoOfPages
                : noOfPagesToDisplay;

            if (isSliding && totalNoOfPages >= noOfPagesToDisplay)
            {
                var ceiling = (int)Math.Ceiling((decimal)noOfPagesToDisplay / 2);

                if (ceiling <= currentPageNo)
                {
                    StartAt = (currentPageNo + 1 - ceiling) >= (totalNoOfPages - noOfPagesToDisplay) 
                        ? totalNoOfPages - noOfPagesToDisplay + 1
                        : currentPageNo + 1 - ceiling;
                    EndAt = (noOfPagesToDisplay - 1 + StartAt) < totalNoOfPages
                        ? noOfPagesToDisplay - 1 + StartAt
                        : totalNoOfPages;
                }
            }
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StartAt;
            yield return EndAt;
        }
    }
}
