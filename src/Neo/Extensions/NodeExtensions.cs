using System;
using System.Linq;
using Neo4j.Driver.V1;
using Utility.Extensions;


namespace Neo.Extensions
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
                    var value = ParseValue<T>(nodeValue, propertyType);
                    property.SetValue(obj, value);
                }
            }

            return obj;
        }

        private static object ParseValue<T>(object nodeValue, Type propertyType)
        {
            if (nodeValue == null)
            {
                return null;
            }
            if (propertyType.IsArray)
            {
                if (nodeValue is string strNodeValue)
                {
                    var arr = strNodeValue.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    switch (propertyType.Name)
                    {
                        case "String[]":
                            return arr;
                        // TODO: Handle other types if necessary ...
                        default:
                            if (arr.Length > 0)
                            {
                                var itemType = arr.GetValue(0).GetType();
                                return arr.Cast<object>().Select((t, i) => Convert.ChangeType(arr.GetValue(i), itemType)).ToList();
                            }
                            break;
                    }
                }
            }
            return Convert.ChangeType(nodeValue, propertyType);
        }
    }
}
