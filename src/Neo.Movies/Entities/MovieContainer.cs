using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Neo.Movies.Business.Entities;


namespace Neo.Movies.Entities
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

        [Obsolete("This method is now moved to the project generating the container. Remove from here as soon as container consistency is stable.")]
        public void RemoveInconsistencies()
        {
            var relationsA = ActorRelations.Select(x => (IRelation)x);
            var relationsD = DirectorRelations.Select(x => (IRelation)x);
            var relationsW = WriterRelations.Select(x => (IRelation)x);
            var relationsP = ProducerRelations.Select(x => (IRelation)x);
            var relations = relationsA.Union(relationsD).Union(relationsW).Union(relationsP);

            var movieIds = new HashSet<string>(Movies.Select(m => m.TmdbId.ToString()));
            var personIds = new HashSet<string>(Persons.Select(p => p.TmdbId.ToString()));
            var missingMovies = new HashSet<string>();
            var missingPersons = new HashSet<string>();

            foreach (var relation in relations)
            {
                if (!movieIds.Contains(relation.ToId))
                {
                    Debug.WriteLine($"Movie TmdbId={relation.ToId} was not found");
                    missingMovies.Add(relation.ToId);
                }
                if (!personIds.Contains(relation.FromId))
                {
                    Debug.WriteLine($"Person TmdbId={relation.FromId} was not found");
                    missingPersons.Add(relation.FromId);
                }
            }

            ActorRelations = ActorRelations.Where(r => !missingMovies.Contains(r.ToId) && !missingPersons.Contains(r.FromId)).ToList();
            DirectorRelations = DirectorRelations.Where(r => !missingMovies.Contains(r.ToId) && !missingPersons.Contains(r.FromId)).ToList();
            WriterRelations = WriterRelations.Where(r => !missingMovies.Contains(r.ToId) && !missingPersons.Contains(r.FromId)).ToList();
            ProducerRelations = ProducerRelations.Where(r => !missingMovies.Contains(r.ToId) && !missingPersons.Contains(r.FromId)).ToList();
        }
    }
}