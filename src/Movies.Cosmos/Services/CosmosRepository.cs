using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Movies.Cosmos.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Graphs;
using Microsoft.Azure.Graphs.Elements;
using Movies.Entities;
using Newtonsoft.Json;
using Utility.Entities;
using Utility.Extensions;


namespace Movies.Cosmos.Services
{
    public class CosmosRepository 
    {
        private readonly DocumentClient _documentClient;
        private const string DatabaseId = "graphdb";
        private const string DemoCollectionId = "demo";
        private const string MovieCollectionId = "movies";

        public CosmosRepository()
        {
            var endPoint = ConfigurationManager.AppSettings["endPoint"];
            var authenticationKey = ConfigurationManager.AppSettings["authenticationKey"];
            var connectionPolicy = new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp };

            _documentClient = new DocumentClient(new Uri(endPoint), authenticationKey, connectionPolicy);
        }


        public void Dispose()
        {
            _documentClient?.Dispose();
        }


        public async Task LoadMovies(MovieContainer container)
        {
            // Create a collection
            var databaseUri = UriFactory.CreateDatabaseUri(DatabaseId);
            var movieCollection = await _documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseUri, new DocumentCollection { Id = MovieCollectionId }, new RequestOptions { OfferThroughput = 1000 });

            // Clean collection
            Execute(movieCollection, "g.V().drop()").Wait();

            // Add Vertexes
            var i = 0;
            foreach (var movie in container.Movies)
            {
                i += 1;
                await AddVertex(movieCollection, movie, "tmdbId", i);
            }
            foreach (var person in container.Persons)
            {
                i += 1;
                await AddVertex(movieCollection, person, "tmdbId", i);
            }

            // Add Edges
            i = 0;
            foreach (var actorRelation in container.ActorRelations)
            {
                i += 1;
                await AddRelation(movieCollection, actorRelation, "actedIn", i);
            }
            foreach (var directorRelation in container.DirectorRelations)
            {
                i += 1;
                await AddRelation(movieCollection, directorRelation, "directed", i);
            }
            foreach (var producerRelations in container.ProducerRelations)
            {
                i += 1;
                await AddRelation(movieCollection, producerRelations, "produced", i);
            }
            foreach (var writerRelation in container.WriterRelations)
            {
                i += 1;
                await AddRelation(movieCollection, writerRelation, "wrote", i);
            }
        }

        private async Task AddVertex<T>(DocumentCollection collection, T obj, string idKey, int? i = null)
        {
            var typeName = typeof(T).Name;
            var properties = obj.GetProperties();
            var expression = $"g.addV('{typeName.ToCamelCase()}').property('id', '{properties[idKey]}')";
            foreach (var property in properties.Where(p => p.Key != idKey))
            {
                expression += $".property('{property.Key}', {property.Value.ToValueString()})";
            }
            await Execute(collection, expression);
            var vertexCounter = i.HasValue && i.Value > 1 ? $"({i})" : string.Empty;
            Console.WriteLine($"{typeName} vertex created with id={properties[idKey]} and {properties.Count} properties {vertexCounter}");
        }

        private async Task AddRelation(DocumentCollection collection, IRelation relation, string relationKey, int? i = null)
        {
            var expression = $"g.V('{relation.FromId}').AddE('{relationKey}')";
            // TODO: It seems Gremlin accepts properties that we add to the Edge but they are not persisted ...
            //var properties = relation.GetProperties();
            //foreach (var property in properties.Where(p => p.Key != "fromId" && p.Key != "toId"))
            //{
            //    expression += $".property('{property.Key}', {property.Value.ToValueString()})";
            //}
            expression += $".to(g.V('{relation.ToId}'))";
            await Execute(collection, expression);
            var edgeCounter = i.HasValue ? $"({i})" : string.Empty;
            Console.WriteLine($"{relation.GetType().Name} relation created {edgeCounter}");
        }

        private async Task Execute(DocumentCollection collection, string gremlinExpression)
        {
            // The CreateGremlinQuery method extensions allow you to execute Gremlin queries and iterate results asychronously
            var query = _documentClient.CreateGremlinQuery<dynamic>(collection, gremlinExpression);
            while (query.HasMoreResults)
            {
                foreach (var result in await query.ExecuteNextAsync())
                {
                    Console.WriteLine($"\t {JsonConvert.SerializeObject(result)}");
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Demo code taken from https://docs.microsoft.com/en-us/azure/cosmos-db/create-graph-dotnet and https://github.com/Azure-Samples/azure-cosmos-db-graph-dotnet-getting-started
        /// </summary>
        public async Task RunDemo()
        {
            var database = await _documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseId });
            var demoCollection = await _documentClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DatabaseId), new DocumentCollection { Id = DemoCollectionId }, new RequestOptions { OfferThroughput = 1000 });

            // Azure Cosmos DB supports the Gremlin API for working with Graphs. Gremlin is a functional programming language composed of steps.
            // Here, we run a series of Gremlin queries to show how you can add vertices, edges, modify properties, perform queries and traversals
            // For additional details, see https://aka.ms/gremlin for the complete list of supported Gremlin operators
            var gremlinQueries = new Dictionary<string, string>
            {
                { "Cleanup",        "g.V().drop()" },
                { "AddVertex 1",    "g.addV('person').property('id', 'thomas').property('firstName', 'Thomas').property('age', 44)" },
                { "AddVertex 2",    "g.addV('person').property('id', 'mary').property('firstName', 'Mary').property('lastName', 'Andersen').property('age', 39)" },
                { "AddVertex 3",    "g.addV('person').property('id', 'ben').property('firstName', 'Ben').property('lastName', 'Miller')" },
                { "AddVertex 4",    "g.addV('person').property('id', 'robin').property('firstName', 'Robin').property('lastName', 'Wakefield')" },
                { "AddEdge 1",      "g.V('thomas').addE('knows').to(g.V('mary'))" },
                { "AddEdge 2",      "g.V('thomas').addE('knows').to(g.V('ben'))" },
                { "AddEdge 3",      "g.V('ben').addE('knows').to(g.V('robin'))" },
                { "UpdateVertex",   "g.V('thomas').property('age', 44)" },
                { "CountVertices",  "g.V().count()" },
                { "Filter Range",   "g.V().hasLabel('person').has('age', gt(40))" },
                { "Project",        "g.V().hasLabel('person').values('firstName')" },
                { "Sort",           "g.V().hasLabel('person').order().by('firstName', decr)" },
                { "Traverse",       "g.V('thomas').out('knows').hasLabel('person')" },
                { "Traverse 2x",    "g.V('thomas').out('knows').hasLabel('person').out('knows').hasLabel('person')" },
                { "Loop",           "g.V('thomas').repeat(out()).until(has('id', 'robin')).path()" },
                { "DropEdge",       "g.V('thomas').outE('knows').where(inV().has('id', 'mary')).drop()" },
                { "CountEdges",     "g.E().count()" },
                { "DropVertex",     "g.V('thomas').drop()" },
            };

            foreach (KeyValuePair<string, string> gremlinQuery in gremlinQueries)
            {
                Console.WriteLine($"Running {gremlinQuery.Key}: {gremlinQuery.Value}");

                // The CreateGremlinQuery method extensions allow you to execute Gremlin queries and iterate results asychronously
                IDocumentQuery<dynamic> query = _documentClient.CreateGremlinQuery<dynamic>(demoCollection, gremlinQuery.Value);
                while (query.HasMoreResults)
                {
                    foreach (dynamic result in await query.ExecuteNextAsync())
                    {
                        Console.WriteLine($"\t {JsonConvert.SerializeObject(result)}");
                    }
                }

                Console.WriteLine();
            }

            // Data is returned in GraphSON format, which can be deserialized into a strongly-typed vertex, edge or property class
            // The following snippet shows how to do this
            var gremlin = gremlinQueries["AddVertex 1"];
            Console.WriteLine($"Running Add Vertex with deserialization: {gremlin}");

            IDocumentQuery<Vertex> insertVertex = _documentClient.CreateGremlinQuery<Vertex>(demoCollection, gremlinQueries["AddVertex 1"]);
            while (insertVertex.HasMoreResults)
            {
                foreach (Vertex vertex in await insertVertex.ExecuteNextAsync<Vertex>())
                {
                    // Since Gremlin is designed for multi-valued properties, the format returns an array. Here we just read
                    // the first value
                    string name = (string)vertex.GetVertexProperties("firstName").First().Value;
                    Console.WriteLine($"\t Id:{vertex.Id}, Name: {name}");
                }
            }

            Console.WriteLine();

            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadLine();
        }

    }
}