using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetAllRegionsHandler : ISqlQueryHandler<GetAllRegions, IReadOnlyCollection<Region>>
    {
        public async Task<IReadOnlyCollection<Region>> Execute(SqlTransaction transaction, GetAllRegions query)
        {
            var sql = @"SELECT RegionId, Name, ParentRegionId, Position.Lat Latitude, Position.Long Longitude FROM Pttcd.Regions";

            var rows = (await transaction.Connection.QueryAsync<Row>(sql, transaction: transaction)).AsList();

            var rootRegions = rows.Where(r => r.ParentRegionId == null);
            var subRegions = rows.Where(r => r.ParentRegionId != null).GroupBy(r => r.ParentRegionId).ToDictionary(g => g.Key, g => g.ToList());

            return rootRegions
                .Select(r => new Region()
                {
                    Id = r.RegionId,
                    Name = r.Name,
                    SubRegions = subRegions
                        .GetValueOrDefault(r.RegionId, new List<Row>())
                        .Select(sr => new Region()
                        {
                            Id = sr.RegionId,
                            Name = sr.Name,
                            SubRegions = Array.Empty<Region>(),
                            Latitude = sr.Latitude,
                            Longitude = sr.Longitude
                        })
                        .ToList(),
                    Latitude = r.Latitude,
                    Longitude = r.Longitude
                })
                .ToList();
        }

        private class Row
        {
            public string RegionId { get; set; }
            public string Name { get; set; }
            public string ParentRegionId { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}
