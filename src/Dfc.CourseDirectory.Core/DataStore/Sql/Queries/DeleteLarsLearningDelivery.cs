using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteLarsLearningDelivery : ISqlQuery<OneOf<Success, NotFound>>
    {
        public string LearnAimRef { get; }

        public DeleteLarsLearningDelivery(string learnAimRef)
        {
            if (string.IsNullOrWhiteSpace(learnAimRef))
            {
                throw new ArgumentNullException(nameof(learnAimRef));
            }

            LearnAimRef = learnAimRef;
        }
    }
}