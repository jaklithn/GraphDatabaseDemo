using System.Linq;
using Movies.Entities;
using Neo.Services;


namespace Movies.Neo.Services
{
    public class MovieClientRepository : NeoClientRepository
    {
        public Movie GetMovie(string imdbId)
        {
            var query = Client.Cypher
                .Match("(m:Movie)")
                .Where((Movie m) => m.ImdbId == imdbId)
                .Return<Movie>("m");
            return query.Results.FirstOrDefault();
        }
    }
}