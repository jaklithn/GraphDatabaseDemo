using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Neo.Movies.Business.Entities;
using Utility.Extensions;
using Neo4jClient;
using Newtonsoft.Json.Serialization;

namespace Neo.Movies.Business.Services
{
    /// <summary>
    /// This is one of the more popular community developed Neo4j clients.
    /// This one adds a .Net like interfaces over the internal APIs.
    /// It looks nice, but you really need to know your Cypher syntax anyhow, so in the end it will not be a big advantage.
    /// One distinct disadvantage is that it is a bit behind the Neo development.
    /// Currently it lacks support for the Bolt protocol and has no intention to add it.
    /// </summary>
    public class NeoClientRepository : IDisposable
    {
        private readonly GraphClient _client;

        public NeoClientRepository()
        {
            var url = ConfigurationManager.AppSettings["urlHttp"];
            var user = ConfigurationManager.AppSettings["user"];
            var password = ConfigurationManager.AppSettings["password"];

            _client = new GraphClient(new Uri(url), user, password) { JsonContractResolver = new CamelCasePropertyNamesContractResolver() };
            _client.Connect();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public Movie GetMovie(string imdbId)
        {
            var query = _client.Cypher
                .Match("(m:Movie)")
                .Where((Movie m) => m.ImdbId == imdbId)
                .Return<Movie>("m");
            return query.Results.FirstOrDefault();
        }

        /// <summary>
        /// This approach will work fine for smaller databases of limited size, like in demo.
        /// For production databases is is recommended to wipe the file system, as it is much faster and always ensures a complete cleanup.
        /// </summary>
        public void ClearDatabase()
        {
            // 1) Delete all nodes and relations
            // MATCH (n) DETACH DELETE n
            _client.Cypher.Match("(n)").DetachDelete("n").ExecuteWithoutResults();
            Console.WriteLine("Deleted All nodes and relations");

            // 2) Delete possible indexes
            // TODO: GetIndexes is not working ...
            var indexes = _client.GetIndexes(IndexFor.Node);
            foreach (var indexMetaData in indexes)
            {
                _client.DeleteIndex(indexMetaData.Value.Template, IndexFor.Node);
            }
        }

        /// <summary>
        /// Add object as node in Neo4j database.
        /// All public properties will automatically be added as properties of the node.
        /// </summary>
        /// <param name="obj">Generic POCO object</param>
        /// <param name="label">Specify type name to be used. Skip if you are satisfied with object type name.</param>
        public void AddNode<T>(T obj, string label = null)
        {
            label = label ?? obj.GetType().Name;
            var parameters = GetProperties(obj);
            var valuePairs = string.Join(", ", parameters.Select(p => $"{p.Key}: {{{p.Key}}}"));
            var query = _client.Cypher
                .Create($"(x:{label} {{{valuePairs}}})")
                .WithParams(parameters)
                .Return<T>("x");
            var result = query.Results.FirstOrDefault();
            if (result != null)
            {
                Console.WriteLine("Created 1 Node");
            }
        }

        public void AddRelation(string fromNodeName, string fromIdName, string toNodeName, string toIdName, string relationName, object relation, string relationFromIdName, string relationToIdName)
        {
            // MATCH(s:Person), (t:Person)
            // WHERE s.name = 'Source Node' AND t.name = 'Target Node'
            // CREATE(s) -[r:RELATIONTYPE]->(t)
            // RETURN r           
            var parameters = GetProperties(relation);
            var fromIdValue = parameters[relationFromIdName];
            var toIdValue = parameters[relationToIdName];
            var properties = parameters.Where(p => p.Key != relationFromIdName && p.Key != relationToIdName).ToDictionary(p => p.Key, p => p.Value);
            var valuePairs = string.Join(", ", properties.Select(p => $"{p.Key}: {{{p.Key}}}"));
            var query = _client.Cypher
                .Match($"(s:{fromNodeName})", $"(t:{toNodeName})")
                .Where($"s.{fromIdName}={fromIdValue}").AndWhere($"t.{toIdName}={toIdValue}")
                .Create($"(s)-[r:{relationName} {{{valuePairs}}}]->(t)")
                .WithParams(parameters)
                .Return<object>("r");
            var results = query.Results.Single();

            // TODO: How to parse resulting relation?
            // var relationship = query.Results.FirstOrDefault()["r"];
            // Console.WriteLine($"{result.Summary.Counters.RelationshipsCreated} {relationName} relations created with {result.Summary.Counters.PropertiesSet} properties");
        }


        private static Dictionary<string, object> GetProperties(object obj)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var property in obj.GetType().GetProperties())
            {
                var propertyName = property.Name;
                var value = property.GetValue(obj);

                var array = value as string[];
                if (array != null)
                {
                    value = string.Join(",", array);
                }
                if (value is DateTime)
                {
                    var dateTime = (DateTime)value;
                    value = dateTime.ToString("yyyy-MM-dd");
                }

                dictionary.Add(propertyName.ToCamelCase(), value);
            }
            return dictionary;
        }

    }
}
