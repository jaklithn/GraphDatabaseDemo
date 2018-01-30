using System;
using System.Collections.Generic;


namespace Utility.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Use reflection to parse object properties to a dictionary which is useful for Neo4j mappings.
        /// </summary>
        public static Dictionary<string, object> GetProperties(this object obj)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var property in obj.GetType().GetProperties())
            {
                var propertyName = property.Name;
                var value = property.GetValue(obj);

                if (value is string[] array)
                {
                    value = string.Join(",", array);
                }
                if (value is DateTime dateTime)
                {
                    value = dateTime.ToString("yyyy-MM-dd");
                }

                dictionary.Add(propertyName.ToCamelCase(), value);
            }
            return dictionary;
        }
    }
}
