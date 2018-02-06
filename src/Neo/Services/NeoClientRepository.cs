using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Neo4jClient;
using Newtonsoft.Json.Serialization;
using Utility.Extensions;


namespace Neo.Services
{
    /// <summary>
    /// This is one of the more popular community developed Neo4j clients.
    /// This one adds a .Net like interfaces over the internal APIs.
    /// It looks nice, but you really need to know your Cypher syntax anyhow, so in the end it will not be a big advantage.
    /// Another disadvantage is that it usually is a bit behind the Neo development.
    /// </summary>
public abstract class NeoClientRepository : IDisposable
{
    protected readonly GraphClient Client;

    protected NeoClientRepository()
    {
        var url = ConfigurationManager.AppSettings["urlHttp"];
        var user = ConfigurationManager.AppSettings["user"];
        var password = ConfigurationManager.AppSettings["password"];

        Client = new GraphClient(new Uri(url), user, password) { JsonContractResolver = new CamelCasePropertyNamesContractResolver() };
        Client.Connect();
    }

        public void Dispose()
        {
            Client?.Dispose();
        }

        /// <summary>
        /// This approach will work fine for smaller databases of limited size, like in demo.
        /// For production databases is is recommended to wipe the file system, as it is much faster and always ensures a complete cleanup.
        /// </summary>
        public void ClearDatabase()
        {
            // 1) Delete all nodes and relations
            // MATCH (n) DETACH DELETE n
            Client.Cypher.Match("(n)").DetachDelete("n").ExecuteWithoutResults();
            Console.WriteLine("Deleted All nodes and relations");

            // 2) Delete possible indexes
            // TODO: GetIndexes is not working ...
            var indexes = Client.GetIndexes(IndexFor.Node);
            foreach (var indexMetaData in indexes)
            {
                Client.DeleteIndex(indexMetaData.Value.Template, IndexFor.Node);
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
            var query = Client.Cypher
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
            var query = Client.Cypher
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
