using System;

namespace Dfc.CourseDirectory.WebV2.LoqateAddressSearch
{
    public class LoqateErrorException : Exception
    {
        public LoqateErrorException(string error, string description, string cause, string resolution)
        {
            Error = error;
            Description = description;
            Cause = cause;
            Resolution = resolution;
        }

        public string Error { get; }
        public string Description { get; }
        public string Cause { get; }
        public string Resolution { get; }

        public override string ToString() => $"Error calling Loqate API:\n" +
            $"Error: {Error}\n" +
            $"Description: {Description}\n" +
            $"Cause: {Cause}\n" +
            $"Resolution: {Resolution}";
    }
}
