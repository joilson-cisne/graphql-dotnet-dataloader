using GraphQL.DataLoader.StarWarsApp.Data;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.DataLoader.StarWarsApp.Schema
{
    public class StarWarsQuery : ObjectGraphType
    {
        public StarWarsQuery(StarWarsContext db)
        {
            Name = "Query";

            Field<ListGraphType<HumanType>>()
                .Name("humans")
                .Resolve(ctx => db.Humans.ToListAsync());

            Field<ListGraphType<DroidType>>()
                .Name("droids")
                .Resolve(ctx => db.Droids.ToListAsync());
        }
    }
}
