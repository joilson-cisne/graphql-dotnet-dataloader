using GraphQL.DataLoader.StarWarsApp.Data;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.DataLoader.StarWarsApp.Schema
{
    public class StarWarsQuery : ObjectGraphType
    {
        public StarWarsQuery()
        {
            Name = "Query";

            Field<ListGraphType<HumanType>>()
                .Name("humans")
                .Resolve(ctx =>
                {
                    using (var db = new StarWarsContext())
                        return db.Humans.ToListAsync();
                });

            Field<ListGraphType<DroidType>>()
                .Name("droids")
                .Resolve(ctx =>
                {
                    using (var db = new StarWarsContext())
                        return db.Droids.ToListAsync();
                });
        }
    }
}
