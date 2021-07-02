using System;
using System.Diagnostics;
using Dfc.CourseDirectory.Core.Models;
using GovUk.Frontend.AspNetCore;

namespace Dfc.CourseDirectory.WebV2.ModelBinding
{
    public class DateInputModelConverter : GovUk.Frontend.AspNetCore.DateInputModelConverter
    {
        public override bool CanConvertModelType(Type modelType) => modelType == typeof(DateInput);

        public override object CreateModelFromElements(Type modelType, (int Day, int Month, int Year) elements)
        {
            var dateTime = new DateTime(elements.Year, elements.Month, elements.Day);
            return new DateInput(dateTime);
        }

        public override (int Day, int Month, int Year)? GetElementsFromModel(Type modelType, object model)
        {
            var dateInput = (DateInput)model;

            if (dateInput != null && dateInput.IsValid)
            {
                return (dateInput.Value.Day, dateInput.Value.Month, dateInput.Value.Year);
            }
            else
            {
                return null;
            }
        }

        public override bool TryCreateModelFromErrors(Type modelType, DateInputParseErrors errors, out object model)
        {
            var invalidReasons = InvalidDateInputReasons.None |
                ((errors & DateInputParseErrors.InvalidDay) != 0 ? InvalidDateInputReasons.InvalidDate : 0) |
                ((errors & DateInputParseErrors.InvalidMonth) != 0 ? InvalidDateInputReasons.InvalidDate : 0) |
                ((errors & DateInputParseErrors.InvalidYear) != 0 ? InvalidDateInputReasons.InvalidDate : 0) |
                ((errors & DateInputParseErrors.MissingDay) != 0 ? InvalidDateInputReasons.MissingDay : 0) |
                ((errors & DateInputParseErrors.MissingMonth) != 0 ? InvalidDateInputReasons.MissingMonth : 0) |
                ((errors & DateInputParseErrors.MissingYear) != 0 ? InvalidDateInputReasons.MissingYear : 0);

            Debug.Assert(invalidReasons != InvalidDateInputReasons.None);

            model = new DateInput(invalidReasons);
            return true;
        }
    }
}
