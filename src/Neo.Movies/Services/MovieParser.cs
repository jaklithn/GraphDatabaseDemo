using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;
using Neo.Movies.Business.Entities;
using Neo.Movies.Business.Services;
using Neo.Movies.Entities;
using Newtonsoft.Json;
using Utility.Services;

namespace Neo.Movies.Services
{
    public static class MovieParser
    {
        private const string ZipFileName = "MovieContainer.zip";
        private const string JsonFileName = "MovieContainer.json";


        public static void LoadMovies()
        {
            Console.WriteLine();
            var dt = new DebugTimer(true);

            var movieContainer = ParseFromFile();

            var repository = new NeoDriverRepository();
            //var repository = new NeoClientRepository();

            repository.ClearDatabase();
            dt.Write("Cleared Database");

            repository.AddNodes(movieContainer.Persons);
            dt.Write($"Added {movieContainer.Persons.Count} Person nodes");

            repository.AddNodes(movieContainer.Movies);
            dt.Write($"Added {movieContainer.Movies.Count} Movie nodes");

            repository.AddIndex(nameof(Movie), nameof(Movie.TmdbId));
            repository.AddIndex(nameof(Person), nameof(Person.TmdbId));
            dt.Write("Added 2 indexes");

            var actorMapping = new MappingConfig { RelationName = "ACTED_IN", FromNode = nameof(Person), FromProperty = nameof(Person.TmdbId), ToNode = nameof(Movie), ToProperty = nameof(Movie.TmdbId) };
            var directorMapping = new MappingConfig { RelationName = "DIRECTED", FromNode = nameof(Person), FromProperty = nameof(Person.TmdbId), ToNode = nameof(Movie), ToProperty = nameof(Movie.TmdbId) };
            var writerMapping = new MappingConfig { RelationName = "WROTE", FromNode = nameof(Person), FromProperty = nameof(Person.TmdbId), ToNode = nameof(Movie), ToProperty = nameof(Movie.TmdbId) };
            var producerMapping = new MappingConfig { RelationName = "PRODUCED", FromNode = nameof(Person), FromProperty = nameof(Person.TmdbId), ToNode = nameof(Movie), ToProperty = nameof(Movie.TmdbId) };

            repository.AddRelations(movieContainer.ActorRelations, actorMapping);
            dt.Write($"Added {movieContainer.ActorRelations.Count} Actor relations");
            repository.AddRelations(movieContainer.DirectorRelations, directorMapping);
            dt.Write($"Added {movieContainer.DirectorRelations.Count} Director relations");
            repository.AddRelations(movieContainer.WriterRelations, writerMapping);
            dt.Write($"Added {movieContainer.WriterRelations.Count} Writer relations");
            repository.AddRelations(movieContainer.ProducerRelations, producerMapping);
            dt.Write($"Added {movieContainer.ProducerRelations.Count} Producer relations");
            double itemCount = movieContainer.Movies.Count + movieContainer.Persons.Count + 
                               movieContainer.ActorRelations.Count + movieContainer.DirectorRelations.Count +
                               movieContainer.ProducerRelations.Count + movieContainer.WriterRelations.Count;
            dt.WriteTotal();
            
            Console.WriteLine();
            Console.WriteLine("Successfully finished!");
            Console.WriteLine($"{itemCount} items added in {dt.TotalTime/1000:D0} s ({dt.TotalTime/itemCount:N1} ms each)");
            Console.WriteLine();
            Console.WriteLine("Detailed summary with processing times can be found in Debug Output window.");

            Console.ReadKey();
        }

        private static MovieContainer ParseFromFile()
        {
            var baseDir = Directory.GetParent(Environment.CurrentDirectory).Parent?.FullName ?? string.Empty;
            var zipFilePath = Path.Combine(baseDir, "Resources", ZipFileName);
            using (var zipStorer = ZipStorer.Open(zipFilePath, FileAccess.Read))
            {
                var zipDir = zipStorer.ReadCentralDir();
                foreach (var zipEntry in zipDir)
                {
                    if (Path.GetFileName(zipEntry.FilenameInZip) == JsonFileName)
                    {
                        using (var ms = new MemoryStream())
                        {
                            zipStorer.ExtractFile(zipEntry, ms);
                            var json = Encoding.UTF8.GetString(ms.ToArray());
                            return JsonConvert.DeserializeObject<MovieContainer>(json);
                        }
                    }
                }
                return null;
            }
        }
    }
}