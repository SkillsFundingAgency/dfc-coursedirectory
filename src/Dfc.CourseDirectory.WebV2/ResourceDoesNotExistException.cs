using System;

namespace Dfc.CourseDirectory.WebV2
{
    public enum ResourceType
    {
        Provider
    }

    public class ResourceDoesNotExistException : Exception
    {
        public ResourceDoesNotExistException(ResourceType resourceType)
        {
            ResourceType = resourceType;
        }

        public ResourceType ResourceType { get; }

        public override string Message => $"{ResourceType} resource does not exist.";
    }
}
