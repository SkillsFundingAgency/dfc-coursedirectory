using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class CreateUser : ISqlQuery<None>
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? ProviderId { get; set; }
    }
}
