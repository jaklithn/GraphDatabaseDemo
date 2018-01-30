using System;
using Cosmos.Movies.Business.Services;
using Movies.Services;
using Utility.Services;


namespace Cosmos.Movies.Services
{
    public static class MovieLoader
    {
        public static void LoadMovies()
        {
            Console.WriteLine();
            var dt = new DebugTimer(true);

            var movieContainer = MovieParser.ParseFromFile();

            var cosmosRepository = new CosmosRepository();
            cosmosRepository.LoadMovies(movieContainer).Wait();
        }
    }
}