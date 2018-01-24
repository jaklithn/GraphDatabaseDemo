using System;
using System.Linq;
using Neo.Movies.Business.Entities;
using Neo4j.Driver.V1;
using Utility.Extensions;

namespace Neo.Movies.Business.Extensions
{
    public static class StatementRunnerExtensions
    {
        /// <summary>
        /// Add POCO object as node in Neo4j database.
        /// All public properties will automatically be added as properties of the node.
        /// Please note that it is NOT merely a concatenated string that is executed!
        /// To avoid injection attacks the parameters are properly inserted through a value array.
        /// </summary>
        /// <param name="statementRunner">Either a session or a transation to execute the statement</param>
        /// <param name="obj">Generic POCO object</param>
        /// <param name="label">Specify type name to be used. Skip if you are satisfied with object type name.</param>
        /// <param name="i">Optional counter if used from collection. Only used for logging.</param>
        public static int AddNode(this IStatementRunner statementRunner, object obj, string label, int? i = null)
        {
            // CREATE (:TypeName { propertyName1: propertyValue, propertyName2: propertyValue2 } )";
            label = label ?? obj.GetType().Name;
            var parameters = obj.GetProperties();
            var valuePairs = string.Join(", ", parameters.Select(p => $"{p.Key}: {{{p.Key}}}"));
            var statement = $"CREATE (x:{label} {{{valuePairs}}} ) RETURN x";
            var result = statementRunner.Run(statement, parameters);

            var node = (INode)result.Single().As<IRecord>()["x"];
            var nodeCounter = i.HasValue && i.Value > 1 ? $"({i})" : string.Empty;
            Console.WriteLine($"{string.Join(",", node.Labels)} node created with Id={node.Id} and {node.Properties.Count} properties {nodeCounter}");
            return result.Summary.Counters.NodesCreated;
        }

        /// <summary>
        /// Add relation between nodes.
        /// </summary>
        /// <typeparam name="T">Type used for relation record</typeparam>
        /// <param name="statementRunner">Either a session or a transation to execute the statement</param>
        /// <param name="relation">Object used as relation specifier</param>
        /// <param name="mappingConfig">Description on how to interpret record object</param>
        /// <param name="i">Optional counter if used from collection. Only used for logging.</param>
        public static int AddRelation<T>(this IStatementRunner statementRunner, T relation, MappingConfig mappingConfig, int? i = null)
        {
            // MATCH (f:FromNodeType), (t:ToNodeType)
            // WHERE f.FromPropertyName = 'FromPropertyValue' AND t.ToPropertyName = 'ToPropertyValue'
            // CREATE (f)-[r:RELATIONTYPE]->(t)
            // RETURN r           
            var parameters = relation.GetProperties();
            var fromPropertyName = nameof(IRelation.FromId).ToCamelCase();
            var toPropertyName = nameof(IRelation.ToId).ToCamelCase();
            var fromPropertyValue = parameters[fromPropertyName];
            var toPropertyValue = parameters[toPropertyName];
            var properties = parameters.Where(p => p.Key != fromPropertyName && p.Key != toPropertyName).ToDictionary(p => p.Key, p => p.Value);
            var valuePairs = string.Join(", ", properties.Select(p => $"{p.Key}: {{{p.Key}}}"));
            var statement =
                $"MATCH (f:{mappingConfig.FromNode}), (t:{mappingConfig.ToNode}) WHERE f.{mappingConfig.FromProperty}={fromPropertyValue} AND t.{mappingConfig.ToProperty}={toPropertyValue} CREATE (f)-[r:{mappingConfig.RelationName} {{{valuePairs}}}]->(t) RETURN r";
            var result = statementRunner.Run(statement, properties);

            var relationship = (IRelationship)result.Single().As<IRecord>()["r"];
            var relationCounter = i.HasValue ? $"({i})" : string.Empty;
            Console.WriteLine($"{relationship.Type} relation created with Id={relationship.Id} and {relationship.Properties.Count} properties {relationCounter}");
            return result.Summary.Counters.RelationshipsCreated;
        }

        /// <summary>
        /// Add simple index on type property.
        /// </summary>
        public static void AddIndex(this IStatementRunner statementRunner, string typeName, string propertyName)
        {
            propertyName = propertyName.ToCamelCase();
            var statement = $"CREATE INDEX ON: {typeName}({propertyName})";
            statementRunner.Run(statement);
        }
    }
}
