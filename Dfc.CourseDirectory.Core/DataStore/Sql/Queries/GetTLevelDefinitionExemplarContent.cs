using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetTLevelDefinitionExemplarContent : ISqlQuery<TLevelDefinitionExemplarContent>
    {
        public Guid TLevelDefinitionId { get; set; }
    }
}
