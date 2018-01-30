using System.Collections.Generic;


namespace Movies.Entities
{
    public class MovieContainer
    {
        public List<Movie> Movies { get; }
        public List<Person> Persons { get; }
        public List<ActedIn> ActorRelations { get; set; }
        public List<Directed> DirectorRelations { get; set; }
        public List<Wrote> WriterRelations { get; set; }
        public List<Produced> ProducerRelations { get; set; }

        public MovieContainer()
        {
            Movies = new List<Movie>();
            Persons = new List<Person>();
            ActorRelations = new List<ActedIn>();
            DirectorRelations = new List<Directed>();
            WriterRelations = new List<Wrote>();
            ProducerRelations = new List<Produced>();
        }
    }
}