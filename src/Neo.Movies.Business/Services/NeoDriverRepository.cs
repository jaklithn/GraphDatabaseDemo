using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Neo.Movies.Business.Entities;
using Neo.Movies.Business.Extensions;
using Neo4j.Driver.V1;

namespace Neo.Movies.Business.Services
{
    /// <summary>
    /// This is the officially C# driver supported by Neo4j.
    /// It does not add a very lot of functionality over the direct API.
    /// On the other hand it is understandable and is always in sync with the Neo features.
    /// It is built on the Bolt protocol which ensures maximal speed and security.
    /// After evaluation this is the driver I recommend for most users.
    /// </summary>
    public class NeoDriverRepository : IDisposable
    {
        private const int BatchSizeDelete = 10000;
        private const int BatchSizeInsert = 100;
        private readonly IDriver _driver;

        public NeoDriverRepository()
        {
            var url = ConfigurationManager.AppSettings["urlBolt"];
            var user = ConfigurationManager.AppSettings["user"];
            var password = ConfigurationManager.AppSettings["password"];
            _driver = GraphDatabase.Driver(url, AuthTokens.Basic(user, password));
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        /// <summary>
        /// Retrieve generic INode records by specifying Cypher query with appropriate parameters.
        /// </summary>
        /// <param name="statement">Cypher Match statement</param>
        /// <param name="returnKey">The key name used in the statement to return values</param>
        /// <param name="parameters">Optional parameter values to be injected in statement</param>
        /// <returns></returns>
        public IReadOnlyList<INode> GetNodes(string statement, string returnKey, IDictionary<string, object> parameters = null)
        {
            using (var session = _driver.Session())
            {
                var result = session.Run(statement, parameters);
                return result.Select(record => record[returnKey].As<INode>()).ToList();
            }
        }

        /// <summary>
        /// Retrieve anonymous type records by specifying Cypher query with appropriate parameters.
        /// </summary>
        /// <param name="statement">Cypher Match statement</param>
        /// <param name="parameters">Optional parameter values to be injected in statement</param>
        /// <returns>Set of property key/value pairs</returns>
        public IReadOnlyList<Dictionary<string, object>> GetNodes(string statement, IDictionary<string, object> parameters = null)
        {
            using (var session = _driver.Session())
            {
                var records = new List<Dictionary<string, object>>();
                var result = session.Run(statement, parameters);
                foreach (var record in result)
                {
                    var properties = record.Keys.ToDictionary(key => key, key => record.Values[key]);
                    records.Add(properties);
                }
                return records;
            }
        }

        /// <summary>
        /// Example of how to retrieve objects from graph database.
        /// This variant will load all records af a specific type.
        /// </summary>
        /// <typeparam name="T">Expected type</typeparam>
        /// <param name="nodeLabel">Type label used in database if not default type name</param>
        public IReadOnlyList<T> GetAllObjects<T>(string nodeLabel = null)
        {
            nodeLabel = nodeLabel ?? typeof(T).Name;
            var statement = $"MATCH (n:{nodeLabel}) RETURN n";
            const string returnKey = "n";
            return GetNodes(statement, returnKey, null).Select(n => n.AsObject<T>()).ToList();
        }

        /// <summary>
        /// Execute custom pass through query
        /// </summary>
        /// <param name="statement">Cypher Match statement</param>
        /// <param name="parameters">Optional parameter values to be injected in statement</param>
        public IResultSummary Execute(string statement, IDictionary<string, object> parameters = null)
        {
            using (var session = _driver.Session())
            {
                var result = session.Run(statement, parameters);
                return result.Summary;
            }
        }

        /// <summary>
        /// This approach will work fine for smaller databases of limited size, like in demo.
        /// For production databases is is recommended to wipe the file system, as it is much faster and always ensures a complete cleanup.
        /// </summary>
        public void ClearDatabase()
        {
            using (var session = _driver.Session())
            {
                // 1) Delete all nodes and relations
                int deletedNodes;
                do
                {
                    var statement = $"MATCH (n) WITH n LIMIT {BatchSizeDelete} DETACH DELETE n";
                    var result = session.Run(statement);
                    deletedNodes = result.Summary.Counters.NodesDeleted;
                    Console.WriteLine($"Deleted: {deletedNodes} nodes, {result.Summary.Counters.RelationshipsDeleted} relations, {result.Summary.Counters.IndexesRemoved} indexes");
                } while (deletedNodes == BatchSizeDelete);

                // 2) Delete all detected indexes
                var indexResult = session.Run("CALL db.indexes()");
                using (var enumerator = indexResult.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var record = enumerator.Current;
                        if (record != null)
                        {
                            // Example: DROP INDEX ON: Person(personId)
                            var statement = $"DROP {record.Values["description"]} ";
                            var result = session.Run(statement);
                            if (result.Summary.Counters.IndexesRemoved == 1)
                            {
                                Console.WriteLine($"Successfully deleted {statement}");
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// By encapsulating many statements in an explicit transaction, the total execution time is greatly improved!
        /// In a typical example it was measured to be ~14 times faster.
        /// </summary>
        /// <param name="objects">Objects of any type</param>
        /// <param name="label">Custom label to be used. If omitted object type is used.</param>
        public int AddNodes(IEnumerable<object> objects, string label = null)
        {
            var addedCount = 0;
            using (var session = _driver.Session())
            {
                var transaction = session.BeginTransaction();
                var i = 0;
                foreach (var obj in objects)
                {
                    i += 1;
                    addedCount += transaction.AddNode(obj, label, i);

                    if (i % BatchSizeInsert == 0)
                    {
                        // Restart with a new transaction regularly to avoid too big batches
                        transaction.Success();
                        transaction.Dispose();
                        transaction = session.BeginTransaction();
                    }
                }
                transaction.Success();
                transaction.Dispose();
            }
            return addedCount;
        }

        /// <summary>
        /// Add single object as node.
        /// </summary>
        /// <param name="obj">Object of any type</param>
        /// <param name="label">Custom label to be used. If omitted object type is used.</param>
        public int AddNode(object obj, string label = null)
        {
            return AddNodes(new[] { obj }, label);
        }

        public int AddRelations<T>(IEnumerable<T> relations, MappingConfig mappingConfig)
        {
            var addedCount = 0;
            using (var session = _driver.Session())
            {
                var transaction = session.BeginTransaction();
                var i = 0;
                foreach (var relation in relations)
                {
                    i += 1;
                    addedCount += transaction.AddRelation(relation, mappingConfig, i);

                    if (i % BatchSizeInsert == 0)
                    {
                        // Restart with a new transaction regularly to avoid too big batches
                        transaction.Success();
                        transaction.Dispose();
                        transaction = session.BeginTransaction();
                    }
                }
                transaction.Success();
                transaction.Dispose();
            }
            return addedCount;
        }

        public void AddIndex(string typeName, string propertyName)
        {
            using (var session = _driver.Session())
            {
                session.AddIndex(typeName, propertyName);
            }
        }
    }
}
