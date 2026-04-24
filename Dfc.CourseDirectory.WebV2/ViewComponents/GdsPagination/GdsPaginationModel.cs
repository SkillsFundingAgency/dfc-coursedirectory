using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.GdsPagination
{
    public class GdsPaginationModel
    {
        public static (IReadOnlyList<T> Items, GdsPaginationModel Pagination) Paginate<T>(
            IList<T> source,
            int page,
            int pageSize)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} cannot be less than 1.");
            }

            var totalItems = source.Count;
            var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling((decimal)totalItems / pageSize);
            var currentPage = Math.Min(page < 1 ? 1 : page, totalPages);
            var items = source.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            var pagination = new GdsPaginationModel
            {
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };
            return (items, pagination);
        }


        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }

        public int FirstItemOnPage => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        public int LastItemOnPage => Math.Min(CurrentPage * PageSize, TotalItems);
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public IEnumerable<int?> GetPageWindow()
        {
            var pages = new List<int?>();

            if (TotalPages <= 7)
            {
                for (var i = 1; i <= TotalPages; i++) pages.Add(i);
                return pages;
            }

            pages.Add(1);

            if (CurrentPage > 3)
            {
                pages.Add(null); // ellipsis
            }

            var windowStart = Math.Max(2, CurrentPage - 1);
            var windowEnd = Math.Min(TotalPages - 1, CurrentPage + 1);

            for (var i = windowStart; i <= windowEnd; i++)
                pages.Add(i);

            if (CurrentPage < TotalPages - 2)
            {
                pages.Add(null); // ellipsis
            }

            pages.Add(TotalPages);

            return pages;
        }
    }
}
