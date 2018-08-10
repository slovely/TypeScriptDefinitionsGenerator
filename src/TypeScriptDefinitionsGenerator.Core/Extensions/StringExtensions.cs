using System.Linq;

namespace TypeScriptDefinitionsGenerator.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string value)
        {
            return value.Substring(0, 1).ToLower() + value.Substring(1);
        }

        public static string[] GetTopLevelNamespaces(this string typeScriptType)
        {
            var startIndex = typeScriptType.IndexOf("<") + 1;
            var count = typeScriptType.Length;
            if (startIndex > 0) count = typeScriptType.LastIndexOf(">") - startIndex;
            typeScriptType = typeScriptType.Substring(startIndex, count);

            var parts = typeScriptType.Split(",");
            return parts
                .Where(p => p.Split(".").Length > 1)
                .Select(p => p.Split(".")[0])
                .Select(p => p.Trim())
                .ToArray();
        }

    }
}