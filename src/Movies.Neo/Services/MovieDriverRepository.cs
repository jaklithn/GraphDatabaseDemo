using Movies.Entities;
using Neo.Extensions;
using Neo.Services;


namespace Movies.Neo.Services
{
    public class MovieDriverRepository : NeoDriverRepository
    {
        public Movie GetMovie(string imdbId)
        {
            using (var session = Driver.Session())
            {
                var result = session.Run("MATCH (n:Movie) WHERE n.imdbId={id} RETURN n", new {id = imdbId});
                return result.GetObject<Movie>("n");
            }
        }
    }
}