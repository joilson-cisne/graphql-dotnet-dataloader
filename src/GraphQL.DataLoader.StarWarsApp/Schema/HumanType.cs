using System.Linq;
using GraphQL.DataLoader.StarWarsApp.Data;
using GraphQL.Types;

namespace GraphQL.DataLoader.StarWarsApp.Schema
{
    public class HumanType : ObjectGraphType<Human>
    {
        public HumanType(StarWarsContext db)
        {
            Name = "Human";
            Field(h => h.Name);
            Field(h => h.HumanId);
            Field(h => h.HomePlanet);
            Interface<CharacterInterface>();

            FetchDelegate<Droid> fetchFunc = ids =>
                db.Friendships
                    .Where(f => ids.Contains(f.HumanId))
                    .Select(f => new {Key = f.HumanId, f.Droid})
                    .ToLookup(f => f.Key, f => f.Droid);

            // Example 1 - field builder extension methods
            Field<ListGraphType<CharacterInterface>>()
                .Name("friends1")
                .Batch(h => h.HumanId)
                .Resolve(fetchFunc);

            // Example 2 - manually wire up a loader
            var friendsLoader = new DataLoader<Droid>(fetchFunc);
            Field<ListGraphType<CharacterInterface>>()
                .Name("friends2")
                .Resolve(ctx => friendsLoader.LoadAsync(ctx.Source.HumanId));

            // Example 3 - manually specify a resolver
            var friendsResolver = new BatchResolver<Human, Droid>(h => h.HumanId, fetchFunc);
            Field<ListGraphType<CharacterInterface>>()
                .Name("friends3")
                .Resolve(friendsResolver);

            // Example 4 - ResolveFieldContext extension method
            Field<ListGraphType<CharacterInterface>>()
                .Name("friends4")
                .Resolve(ctx => ctx.GetBatchLoader(fetchFunc).LoadAsync(ctx.Source.HumanId));
        }
    }
}
