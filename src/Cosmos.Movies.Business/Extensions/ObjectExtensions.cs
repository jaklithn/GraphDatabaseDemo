using System;
using System.Globalization;


namespace Cosmos.Movies.Business.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToValueString(this object value)
        {
            if (value == null)
            {
                return "null";
            }

            switch (value)
            {
                case string strValue:
                    strValue = strValue.Replace("'", @"\'");
                    return $"'{strValue}'";
                case DateTime dateValue:
                    return dateValue.ToString("yyyy-MM-dd");
                case double doubleValue:
                    // TODO: This should work ...
                    // return $"{doubleValue.ToString(CultureInfo.InvariantCulture)}d";
                    return $"'{value.ToString()}'";
                default:
                    return value.ToString();
            }
        }
    }
}