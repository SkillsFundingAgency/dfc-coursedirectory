using Dfc.CourseDirectory.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.PostcodeLookup
{
    public class PostcodeLookupItemModel : ValueObject<PostcodeLookupItemModel>
    {
        public string Id { get; }
        public string Text { get; }

        public PostcodeLookupItemModel(
            string id,
            string text)
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
