using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.DataLoader.StarWarsApp.Data;
using GraphQL.Types;

namespace GraphQL.DataLoader.StarWarsApp.Schema
{
    public class DroidType : ObjectGraphType<Droid>
    {
        public DroidType()
        {
            Name = "Droid";
            Field(d => d.Name);
            Field(d => d.DroidId);
            Field(d => d.PrimaryFunction);
            Interface<CharacterInterface>();

            Func<IEnumerable<int>, IDictionary<int, IEnumerable<Human>>> fetchFriends = ids =>
            {
                Console.WriteLine("Fetching friends of droids " + string.Join(", ", ids));
                using (var db = new StarWarsContext())
                    return db.Friendships
                        .Where(f => ids.Contains(f.DroidId))
                        .Select(f => new {Key = f.DroidId, f.Human})
                        .GroupBy(x => x.Key, x => x.Human)
                        .ToDictionary(x => x.Key, x => x.AsEnumerable());
            };

            var friendsLoader = new DataLoader<int, IEnumerable<Human>>(fetchFriends);
            Field<ListGraphType<CharacterInterface>>()
                .Name("friends")
                .Resolve(ctx => friendsLoader.LoadAsync(ctx.Source.DroidId));
        }
    }
}
