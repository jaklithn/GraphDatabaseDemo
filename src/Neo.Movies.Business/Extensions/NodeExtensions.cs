using System;
using Neo4j.Driver.V1;
using Utility.Extensions;

namespace Neo.Movies.Business.Extensions
{
    public static class NodeExtensions
    {
        /// <summary>
        /// Use reflection to create typed C# object with properties mapped from incoming INode.
        /// </summary>
        public static T AsObject<T>(this INode node)
        {
            var type = typeof(T);
            var obj = Activator.CreateInstance<T>();

            foreach (var key in node.Properties.Keys)
            {
                var propertyKey = key.ToPascalCase();
                var property = type.GetProperty(propertyKey);
                if (property != null)
                {
                    var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    var nodeValue = node[key];
                    var value = nodeValue == null ? null : Convert.ChangeType(nodeValue, propertyType);
                    property.SetValue(obj, value);
                }
            }

            return obj;
        }
    }
}
