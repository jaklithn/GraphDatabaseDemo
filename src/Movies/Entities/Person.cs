using System;


namespace Movies.Entities
{
    public class Person
    {
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime? DeathDate { get; set; }
        public string BirthPlace { get; set; }
        public string Biography { get; set; }
        public double Popularity { get; set; }
    }
}