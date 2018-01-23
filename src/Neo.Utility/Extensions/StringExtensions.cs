namespace Utility.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsSpecified(this string value)
        {
            return !value.IsNullOrEmpty();
        }

        public static string ToCamelCase(this string value)
        {
            return value.IsSpecified() ? char.ToLowerInvariant(value[0]) + value.Substring(1) : value;
        }

        public static string ToPascalCase(this string value)
        {
            return value.IsSpecified() ? char.ToUpperInvariant(value[0]) + value.Substring(1) : value;
        }
    }
}