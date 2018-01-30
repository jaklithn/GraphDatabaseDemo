using Cosmos.Movies.Services;


namespace Cosmos.Movies
{
    public static class Program
    {
        public static void Main()
        {
            MovieLoader.LoadMovies();
        }
    }
}