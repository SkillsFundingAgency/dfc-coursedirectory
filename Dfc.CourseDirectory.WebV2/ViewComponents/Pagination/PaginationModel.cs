﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.WebV2.ViewComponents.Interfaces;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Pagination
{
    public class PaginationModel : IViewComponentModel
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
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Errors = new string[] { };
            Items = items;
        }
    }
}
