using Movies.Services;


namespace Movies.Cosmos.Services
{
    public static class MovieLoader
    {
        public static void LoadMovies()
        {
            var cosmosRepository = new CosmosRepository();
            cosmosRepository.RunDemo().Wait();

            var movieContainer = MovieParser.ParseFromFile();
            cosmosRepository.LoadMovies(movieContainer).Wait();
        }
    }
}