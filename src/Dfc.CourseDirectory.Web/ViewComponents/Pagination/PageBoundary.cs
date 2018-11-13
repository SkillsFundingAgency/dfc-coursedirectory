using Dfc.CourseDirectory.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Pagination
{
    public class PageBoundary : ValueObject<PageBoundary>
    {
        public int StartAt { get; }
        public int PagesToDisplay { get; }

        public PageBoundary(
            int totalNoOfPages, 
            int pagesToDisplay,
            int currentPage,
            bool isSliding)
        {
            Throw.IfLessThan(0, totalNoOfPages, nameof(totalNoOfPages));
            Throw.IfLessThan(0, pagesToDisplay, nameof(pagesToDisplay));
            Throw.IfLessThan(0, currentPage, nameof(currentPage));

            StartAt = 1;
            PagesToDisplay = totalNoOfPages < pagesToDisplay
                ? totalNoOfPages
                : pagesToDisplay;

            if (isSliding)
            {
                var ceiling = (int)Math.Ceiling((decimal)PagesToDisplay / 2);

                if (ceiling <= currentPage)
                {
                    StartAt = currentPage + 1 - ceiling;
                    PagesToDisplay = (PagesToDisplay -1 + StartAt) < totalNoOfPages
                        ? PagesToDisplay - 1 + StartAt
                        : totalNoOfPages;
                }
            }
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StartAt;
            yield return PagesToDisplay;
        }
    }
}
