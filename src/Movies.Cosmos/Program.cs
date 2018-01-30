using Movies.Cosmos.Services;


namespace Movies.Cosmos
{
    public static class Program
    {
        public static void Main()
        {
            MovieLoader.LoadMovies();
        }
    }
}