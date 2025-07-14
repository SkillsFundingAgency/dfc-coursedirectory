using System;

namespace Dfc.CourseDirectory.Core.MultiPageTransaction
{
    public class InvalidParentException : Exception
    {
        public InvalidParentException(string parentInstanceId)
        {
            ParentInstanceId = parentInstanceId ?? throw new ArgumentNullException(nameof(parentInstanceId));
        }

        public string ParentInstanceId { get; }

        public override string ToString() => $"Specified parent is not valid: '{ParentInstanceId}'.";
    }
}
