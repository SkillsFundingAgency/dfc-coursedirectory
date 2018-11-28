﻿using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult
{
    public class VenueSearchResultItemModel : ValueObject<VenueSearchResultItemModel>, IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
        public string VenueName { get; }
        public string AddressLine1 { get; }
        public string AddressLine2 { get; }
        public string AddressLine3 { get; }
        public string PostCode { get; }

        public VenueSearchResultItemModel(
            string venueName,
            string addressLine1,
            string addressLine2,
            string addressLine3,
            string postCode)
        {
            Errors = new string[] { };
            VenueName = venueName;
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            AddressLine3 = addressLine3;
            PostCode = postCode;
        }

        public VenueSearchResultItemModel()
        {
            Errors = new string[] { };
        }

        public VenueSearchResultItemModel(string error)
        {
            Errors = new string[] { error };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HasErrors;
            yield return Errors;
            yield return VenueName;
            yield return AddressLine1;
            yield return AddressLine2;
            yield return AddressLine3;
            yield return PostCode;


        }
    }
}