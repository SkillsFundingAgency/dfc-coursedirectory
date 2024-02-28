using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Guid> CreateNonLarsSubType(Guid? nonLarsSubTypeId = null, string name = "skills bootcamp", int isActive = 1, DateTime? addedOn = null)
        {
            var id = nonLarsSubTypeId ?? Guid.NewGuid();

            var result = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new CreateNonLarsSubType
            {
                NonLarsSubTypeId = id,
                Name = name,
                IsActive = isActive,
                AddedOn = addedOn ?? DateTime.Now
            }));

            return id;
        }

        public Task<IReadOnlyCollection<NonLarsSubType>> CreateInitialNonLarsSubTypes() =>
            WithSqlQueryDispatcher(async dispatcher =>
            {
                var createCommands = new[]
                {
                    new CreateNonLarsSubType()
                    {
                        NonLarsSubTypeId = new Guid("236d0a46-adea-44c6-83d7-b5b2196e1bd3"),
                        Name = "Skills Bootcamp",
                        AddedOn = DateTime.Now,
                        IsActive = 1
                    },
                    new CreateNonLarsSubType()
                    {
                        NonLarsSubTypeId = new Guid("024e3b2c-aa65-4808-8d17-74697c03591a"),
                        Name = "Skills Toolkit",
                        AddedOn = DateTime.Now,
                        IsActive = 1
                    }
                    
                };

                foreach (var createCommand in createCommands)
                {
                    await dispatcher.ExecuteQuery(createCommand);
                }

                return await dispatcher.ExecuteQuery(new GetAllNonLarsSubTypes());
            });
    }
}
