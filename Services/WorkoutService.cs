using Internship.BlazorServerPOC.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using System.Globalization;
using System.Text;

namespace Internship.BlazorServerPOC.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IDriver _driver;

        /// Initializes a new instance of Workoutservice that handles movie database calls.
        public WorkoutService()
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

        /// Get a list of Workouts.
        public async Task<IEnumerable<Workout>> AllAsync(ActivityType? activityType = null)
        {
            // Open a new session.
            await using var session = _driver.AsyncSession();

            // Execute a query in a new Read Transaction.
            return await session.ExecuteReadAsync(async tx =>
            {
                var cypherQuery = new StringBuilder(@"MATCH (u:User)-[:LOGGED]->(w:Workout)");

                if (activityType != null)
                {
                    cypherQuery.Append(" WHERE w.activityType = $activityType");
                }

                cypherQuery.Append(" OPTIONAL MATCH (u2:User)-[:LIKED]->(w:Workout) RETURN w, u, COUNT(DISTINCT u2) AS likesCount ORDER BY w.date DESC");

                var parameters = new Dictionary<string, object>();

                if (activityType != null)
                {
                    parameters.Add("activityType", activityType.Value.ToString());
                }

                var cursor = await tx.RunAsync(cypherQuery.ToString(), parameters);

                var workouts = new List<Workout>();

                await cursor.ForEachAsync(record =>
                {
                    var workoutProperties = record["w"].As<INode>().Properties;
                    var userProperties = record["u"].As<INode>().Properties;

                    var UsersLiked = fetchUserLikesFromWorkout(workoutProperties["title"].As<string>());

                    var workout = new Workout
                    {
                        Title = workoutProperties["title"].As<string>(),
                        Description = workoutProperties["description"].As<string>(),
                        Date = DateTime.ParseExact(workoutProperties["date"].As<string>(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        ActivityType = Enum.Parse<ActivityType>(workoutProperties["activityType"].As<string>(), true),
                        Distance = double.Parse(workoutProperties["distance"].As<string>()),
                        LikesCount = record["likesCount"].As<int>(),
                        UserName = userProperties["name"].As<string>(),
                        UsersLiked = UsersLiked.Result,
                        IsPrivate = workoutProperties["private"].As<bool>()
                    };

                    workouts.Add(workout);
                });

                return workouts;
            });
        }

        private async Task<List<string>> fetchUserLikesFromWorkout(string workoutTitle)
        {
            // Open a new session.
            await using var session = _driver.AsyncSession();


            return await session.ExecuteReadAsync(async tx =>
            {

                // Execute a query in a new Read Transaction.
                var cursor = await tx.RunAsync(@"MATCH (u:User)-[:LIKED]->(w:Workout {title: $workoutTitle})
                                 RETURN u", new { workoutTitle });

                var users = new List<string>();

                await cursor.ForEachAsync(record =>
                {
                    var userProperties = record["u"].As<INode>().Properties;
                    users.Add(userProperties["name"].As<string>());
                });

                return users;
            });
        }

        /// Get a list of Workouts from a specific user. <summary>
        public async Task<IEnumerable<Workout>> AllFromUserAsync(string userName)
        {
            // Open a new session.
            await using var session = _driver.AsyncSession();

            // Execute a query in a new Read Transaction.
            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(@"MATCH (u:User {name: $userName})-[:LOGGED]->(w:Workout)
                                 OPTIONAL MATCH (u:User)-[:LIKED]->(w:Workout)
                                 RETURN w, COUNT(DISTINCT u) AS likesCount
                                 ORDER BY w.date DESC", new { userName });

                var workouts = new List<Workout>();

                await cursor.ForEachAsync(record =>
                {
                    var workoutProperties = record["w"].As<INode>().Properties;

                    var workout = new Workout
                    {
                        Title = workoutProperties["title"].As<string>(),
                        Description = workoutProperties["description"].As<string>(),
                        Date = DateTime.ParseExact(workoutProperties["date"].As<string>(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        ActivityType = Enum.Parse<ActivityType>(workoutProperties["activityType"].As<string>(), true),
                        Distance = double.Parse(workoutProperties["distance"].As<string>()),
                        LikesCount = record["likesCount"].As<int>(),
                        IsPrivate = workoutProperties["private"].As<bool>()
                    };

                    workouts.Add(workout);
                });

                return workouts;
            });
        }

        // Create a new Workout.
        public async Task CreateAsync(Workout workout, string userName)
        {
            // Open a new session.
            await using var session = _driver.AsyncSession();

            // Execute a query in a new Write Transaction.
            await session.ExecuteWriteAsync(async tx =>
            {
                var parameters = new Dictionary<string, object>
                {
                    {"userName", userName},
                    {"title", workout.Title},
                    {"description", workout.Description},
                    {"date", workout.Date.ToString("dd/MM/yyyy")},
                    {"activityType", workout.ActivityType.ToString()},
                    {"distance", workout.Distance.ToString()}
                };

                await tx.RunAsync("MATCH (u:User {name: $userName}) MERGE (u)-[:LOGGED]->(w:Workout {title: $title, description: $description, date: $date, activityType: $activityType, distance: $distance, private: false})", parameters);
            });
        }

        // Delete a Workout.
        public async Task DeleteAsync(string workoutTitle)
        {
            // Open a new session.
            await using var session = _driver.AsyncSession();

            // Execute a query in a new Write Transaction.
            await session.ExecuteWriteAsync(async tx =>
            {
                var parameters = new Dictionary<string, object>
                {
                    {"title", workoutTitle}
                };

                await tx.RunAsync("MATCH (w:Workout {title: $title}) DETACH DELETE w", parameters);
            });
        }

        // Set a Workout to private.
        public async Task SetWorkoutPrivate(string workoutTitle)
        {
            // Open a new session.
            await using var session = _driver.AsyncSession();

            // Execute a query in a new Write Transaction.
            await session.ExecuteWriteAsync(async tx =>
            {
                var parameters = new Dictionary<string, object>
                {
                    {"title", workoutTitle}
                };

                await tx.RunAsync("MATCH (w:Workout {title: $title}) SET w.private = true", parameters);
            });
        }

        // Set a Workout to public.
        public async Task SetWorkoutPublic(string workoutTitle)
        {
            // Open a new session.
            await using var session = _driver.AsyncSession();

            // Execute a query in a new Write Transaction.
            await session.ExecuteWriteAsync(async tx =>
            {
                var parameters = new Dictionary<string, object>
                {
                    {"title", workoutTitle}
                };

                await tx.RunAsync("MATCH (w:Workout {title: $title}) SET w.private = false", parameters);
            });
        }
    }
}
