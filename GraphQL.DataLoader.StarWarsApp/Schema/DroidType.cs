using System;
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

            FetchDelegate<Human> fetchFriends = ids =>
            {
                Console.WriteLine("Fetching friends of droids " + string.Join(", ", ids));
                using (var db = new StarWarsContext())
                    return db.Friendships
                        .Where(f => ids.Contains(f.DroidId))
                        .Select(f => new {Key = f.DroidId, f.Human})
                        .ToLookup(x => x.Key, x => x.Human);
            };

            // See HumanType.cs for more examples
            Field<ListGraphType<CharacterInterface>>()
                .Name("friends")
                .Resolve(d => d.DroidId, fetchFriends);
        }
    }
}
