using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;

namespace Dfc.CourseDirectory.Services
{
    public class PostCodeSearchResultItem : ValueObject<PostCodeSearchResultItem>, IPostCodeSearchResultItem
    {
        public string Id { get; }
        public string Text { get; }


        public PostCodeSearchResultItem(
            string id,
            string text
           )
        {
            Throw.IfNullOrWhiteSpace(id, nameof(id));
            Throw.IfNullOrWhiteSpace(text, nameof(text));

            Id = id;
            Text = text;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
            yield return Text;
        }
    }
}