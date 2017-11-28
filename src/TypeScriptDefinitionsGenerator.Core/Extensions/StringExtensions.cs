namespace TypeScriptDefinitionsGenerator.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string value)
        {
            return value.Substring(0, 1).ToLower() + value.Substring(1);
        }
    }
}