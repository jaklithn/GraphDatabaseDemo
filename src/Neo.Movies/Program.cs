using System;
using System.Linq;
using Neo.Movies.Business.Entities;
using Neo.Movies.Business.Services;
using Neo.Movies.Services;

namespace Neo.Movies
{
    public static class Program
    {
        public static void Main()
        {
            // Generate database content if desired
            Console.Write("Do you want to flush the local Neo4j default database and load it with movie data? (Y=Yes, N=No) ");
            var feedbackKey = Console.ReadKey();
            if (feedbackKey.Key == ConsoleKey.Y)
            {
                MovieParser.LoadMovies();
            }

            // Try readouts
            var repository = new NeoDriverRepository();
            var persons = repository.GetAllObjects<Person>();
            var movies = repository.GetAllObjects<Movie>();
        }
    }
}
