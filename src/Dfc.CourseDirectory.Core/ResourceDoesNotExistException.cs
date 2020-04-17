using System;

namespace Dfc.CourseDirectory.Core
{
    public enum ResourceType
    {
        Provider
    }

    public class ResourceDoesNotExistException : Exception
    {
        public ResourceDoesNotExistException(ResourceType resourceType, object resourceId)
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }

        public object ResourceId { get; }
        public ResourceType ResourceType { get; }

        public override string Message => $"{ResourceType} resource with ID {ResourceId} does not exist.";
    }
}
