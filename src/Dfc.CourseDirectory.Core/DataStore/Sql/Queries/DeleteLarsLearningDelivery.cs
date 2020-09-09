namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteLarsLearningDelivery : ISqlQuery<string>
    {
        public string LearnAimRef { get; set; }
    }
}