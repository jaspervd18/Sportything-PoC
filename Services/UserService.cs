using Internship.BlazorServerPOC.Models;
using Microsoft.AspNetCore.Http;
using Neo4j.Driver;
using System.Globalization;

namespace Internship.BlazorServerPOC.Services
{
    public class UserService : IUserService
    {
        private readonly IDriver _driver;

        /// Initializes a new instance of Workoutservice that handles movie database calls.
        public UserService()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var neo4j = config.GetSection("Neo4j");
            var username = neo4j["username"];
            var password = neo4j["password"];
            var uri = neo4j["uri"];
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
        }

        /// Get a paginated list of Workouts.
        public async Task<IEnumerable<User>> AllAsync()
        {
            // Open a new session.
            await using var session = _driver.AsyncSession();

            // Execute a query in a new Read Transaction.
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(@"MATCH (u:User) RETURN u");

                var users = new List<User>();

                await cursor.ForEachAsync(record =>
                {
                    var userProperties = record["u"].As<INode>().Properties;

                    var user = new User
                    {
                        Name = userProperties["name"].As<string>(),
                        Gender = userProperties["gender"].As<string>(),
                        DateOfBirth = DateTime.ParseExact(userProperties["dateOfBirth"].As<string>(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        Location = userProperties["location"].As<string>(),
                    };

                    users.Add(user);
                });

                return users;
            });
        }

        public async Task<bool> SetWorkoutUserLikeRelationAsync(string workoutTitle, string userName)
        {
            var query = @"
                MATCH (w:Workout {title: $workoutTitle})
                MATCH (u:User {name: $userName})
                MERGE (u)-[:LIKED]->(w)
            ";

            var parameters = new { workoutTitle, userName };

            await using var session = _driver.AsyncSession();

            var result = await session.ExecuteWriteAsync(async tx =>
            {
                var cursor = await tx.RunAsync(query, parameters);
                return await cursor.ConsumeAsync();
            });

            return result.Counters.RelationshipsCreated > 0;
        }

        public async Task<bool> CheckWorkoutUserLikeRelationAsync(string workoutTitle, string userName)
        {
            await using var session = _driver.AsyncSession();

            return await session.ExecuteReadAsync(async tx =>
            {
                var parameter = new Dictionary<string, object>
            {
                { "userName", userName },
                { "workoutTitle", workoutTitle }
            };
                var cursor = await tx.RunAsync("match (u:User {name: $userName})-[:LIKED]->(w:Workout {title: $workoutTitle}) return w", parameter);
                return await cursor.FetchAsync();
            });
        }

    }
}
