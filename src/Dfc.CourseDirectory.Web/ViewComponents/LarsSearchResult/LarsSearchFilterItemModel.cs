using Dfc.CourseDirectory.Common;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult
{
    public class LarsSearchFilterItemModel : ValueObject<LarsSearchFilterItemModel>
    {
        public string Id { get; }
        public string Name { get; }
        public string Text { get; set; }
        public string Value { get; }
        public int Count { get; }
        public bool IsSelected { get; }

        public LarsSearchFilterItemModel(
            string id,
            string name,
            string text,
            string value,
            int count,
            bool isSelected)
        {
            Throw.IfNullOrWhiteSpace(id, nameof(id));
            Throw.IfNullOrWhiteSpace(name, nameof(name));
            Throw.IfNullOrWhiteSpace(text, nameof(text));
            Throw.IfNullOrWhiteSpace(value, nameof(value));
            Throw.IfLessThan(0, count, nameof(count));

            Id = id;
            Name = name;
            Text = text;
            Value = value;
            Count = count;
            IsSelected = isSelected;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
            yield return Name;
            yield return Text;
            yield return Value;
            yield return Count;
            yield return IsSelected;
        }
    }
}