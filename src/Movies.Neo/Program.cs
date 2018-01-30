using System;
using Movies.Entities;
using Movies.Neo.Services;

namespace Movies.Neo
{
    public static class Program
    {
        public static void Main()
        {
            // Generate database content if desired
            Console.Write("Do you want to flush the current Neo4j database and load it with movie data from file? (Y=Yes, N=No) ");
            var feedbackKey = Console.ReadKey();
            if (feedbackKey.Key == ConsoleKey.Y)
            {
                MovieLoader.LoadMovies();
            }
            else
            {
                // Examples of typical data manipulation
                using (var repository = new MovieDriverRepository())
                {
                    Console.WriteLine();
                    Console.WriteLine();

                    // Retrieve complete nodes and parse them as typed C# objects
                    var movies = repository.GetAllObjects<Movie>();
                    Console.WriteLine($"{movies.Count} all movies retrieved as typed Movie objects");
                    Console.WriteLine();


                    // Specify custom query and custom result
                    var jediStatement = "MATCH (p:Person) - [r:ACTED_IN] -> (m:Movie) WHERE m.title = 'Return of the Jedi' RETURN p.name AS Name, r.character AS Role, r.order AS Order ORDER BY r.order";
                    var jediActors = repository.GetNodes(jediStatement);
                    Console.WriteLine();
                    Console.WriteLine($"{jediActors.Count} custom person objects retrieved representing actors in the movie 'Return of the Jedi':");
                    foreach (var jediActor in jediActors)
                    {
                        Console.WriteLine($"{jediActor["Order"],3}  {jediActor["Name"],-20} {jediActor["Role"]} ");
                    }
                    Console.WriteLine();


                    // Delete dummy movie if present
                    const string deleteStatement = "MATCH (m:Movie) WHERE m.tmdbId = 0 DETACH DELETE m";
                    var summary = repository.Execute(deleteStatement);
                    Console.WriteLine($"{summary.Counters.NodesDeleted} Movie node deleted");
                    Console.WriteLine();


                    // Add a dummy movie
                    var movie = new Movie { TmdbId = 0, Title = "My movie", Plot = "Very dramatic story", Genres = new[] { "Action", "Comedy" } };
                    var nodeCount = repository.AddNode(movie);
                    Console.WriteLine($"{nodeCount} Movie node created");
                    Console.WriteLine();

                    // Retrieve one single movie
                    var movie1 = repository.GetMovie("tt0240772");
                    Console.WriteLine($"Movie '{movie1.Title}' retrieved");
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.Write("(Click any key to close window)");
            Console.ReadKey();
        }
    }
}
