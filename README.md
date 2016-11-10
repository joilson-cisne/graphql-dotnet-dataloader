GraphQL + DataLoader in .NET
============================
Small server using [GraphQL](http://github.com/graphql-dotnet/graphql-dotnet) and an implementation of [DataLoader](http://github.com/facebook/dataloader) in .NET.

+ __GraphQL.DataLoader__ - Contains the DataLoader classes and a GraphQL resolver.
+ __GraphQL.DataLoader.StarWarsApp__ - Example usage.

If people find this useful I may publish it as a NuGet package - for now it's just for reference.

Setup
-----
```
cd src/GraphQL.DataLoader.StarWarsApp/
dotnet ef migrations add InitialSetup
dotnet ef database update
dotnet run
```