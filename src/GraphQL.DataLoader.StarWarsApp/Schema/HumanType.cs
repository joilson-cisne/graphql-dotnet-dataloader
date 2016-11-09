using System.Linq;
using GraphQL.DataLoader.StarWarsApp.Data;
using GraphQL.Types;

namespace GraphQL.DataLoader.StarWarsApp.Schema
{
    public class HumanType : ObjectGraphType<Human>
    {
        public HumanType()
        {
            Name = "Human";
            Field(h => h.Name);
            Field(h => h.HumanId);
            Field(h => h.HomePlanet);
            Interface<CharacterInterface>();

            FetchDelegate<Droid> fetchFriends = ids =>
            {
                using (var db = new StarWarsContext())
                    return db.Friendships
                        .Where(f => ids.Contains(f.HumanId))
                        .Select(f => new {Key = f.HumanId, f.Droid})
                        .ToLookup(f => f.Key, f => f.Droid);
            };

            // Example 1 - FieldBuilder resolve overload extension method
            Field<ListGraphType<CharacterInterface>>()
                .Name("friends1")
                .Resolve(h => h.HumanId, fetchFriends);

            // Example 2 - ResolveFieldContext extension method
            Field<ListGraphType<CharacterInterface>>()
                .Name("friends4")
                .Resolve(ctx => ctx.GetBatchLoader(fetchFriends).LoadAsync(ctx.Source.HumanId));

            // Example 3 - manually wire up a loader
            var friendsLoader = new DataLoader<Droid>(fetchFriends);
            Field<ListGraphType<CharacterInterface>>()
                .Name("friends2")
                .Resolve(ctx => friendsLoader.LoadAsync(ctx.Source.HumanId));

            // Example 4 - manually specify a resolver
            var friendsResolver = new DataLoaderResolver<Human, Droid>(h => h.HumanId, fetchFriends);
            Field<ListGraphType<CharacterInterface>>()
                .Name("friends3")
                .Resolve(friendsResolver);
        }
    }
}
