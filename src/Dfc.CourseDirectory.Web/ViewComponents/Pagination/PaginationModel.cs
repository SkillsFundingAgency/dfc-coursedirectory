﻿using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.Pagination
{
    public class PaginationModel : ValueObject<PaginationModel>, IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
        public IEnumerable<PaginationItemModel> Items { get; }
        public int CurrentPageNo { get; }

        public PaginationModel()
        {
            Errors = new string[] { };
            Items = new PaginationItemModel[]
            {
            };
        }

        public PaginationModel(string error)
        {
            Errors = new string[] { error };
            Items = new PaginationItemModel[] { };
        }

        public PaginationModel(
            IEnumerable<PaginationItemModel> items)
        {
            Throw.IfNull(items, nameof(items));

            Errors = new string[] { };
            Items = items;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HasErrors;
            yield return Errors;
            yield return Items;
        }
    }
}