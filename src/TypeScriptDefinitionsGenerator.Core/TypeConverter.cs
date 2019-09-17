using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using TypeScriptDefinitionsGenerator.Core.Extensions;

namespace TypeScriptDefinitionsGenerator.Core
{
    public class TypeConverter
    {
        private static readonly Dictionary<Type, string> _cache;

        static TypeConverter()
        {
            _cache = new Dictionary<Type, string>();
            // Integral types
            _cache.Add(typeof(object), "any");
            _cache.Add(typeof(bool), "boolean");
            _cache.Add(typeof(byte), "number");
            _cache.Add(typeof(sbyte), "number");
            _cache.Add(typeof(short), "number");
            _cache.Add(typeof(ushort), "number");
            _cache.Add(typeof(int), "number");
            _cache.Add(typeof(uint), "number");
            _cache.Add(typeof(long), "number");
            _cache.Add(typeof(ulong), "number");
            _cache.Add(typeof(float), "number");
            _cache.Add(typeof(double), "number");
            _cache.Add(typeof(decimal), "number");
            _cache.Add(typeof(string), "string");
            _cache.Add(typeof(char), "string");
            _cache.Add(typeof(DateTime), "string");
            _cache.Add(typeof(DateTimeOffset), "string");
            _cache.Add(typeof(byte[]), "string");
            _cache.Add(typeof(Type), "string");
            _cache.Add(typeof(Guid), "string");
            _cache.Add(typeof(Exception), "string");
            _cache.Add(typeof(void), "void");
        }

        public static string GetTypeScriptName(Type clrType)
        {
            string result;

            if (clrType.IsNullable())
            {
                clrType = clrType.GetUnderlyingNullableType();
            }
            if (clrType.IsGenericTask())
            {
                clrType = clrType.GetUnderlyingTaskType();
            }
            if (clrType.IsTask())
            {
                return "void";
            }
            if (_cache.TryGetValue(clrType, out result))
            {
                return result;
            }
            // If (I)ActionResult, then we can't know what the type is, so use any.
            if (clrType.IsActionResult() || typeof(HttpResponseMessage).IsAssignableFrom(clrType))
            {
                return "any";
            }

            // Dictionaries -- these should come before IEnumerables, because they also implement IEnumerable
            if (clrType.IsIDictionary())
            {
                return "IDictionary<" + GetTypeScriptName(clrType.GetGenericArguments()[1]) + ">";
            }
            if (clrType.IsArray)
            {
                return "Array<" + GetTypeScriptName(clrType.GetElementType()) + ">";
            }
            if (typeof(IEnumerable).IsAssignableFrom(clrType))
            {
                if (clrType.IsGenericType)
                {
                    return "Array<" + GetTypeScriptName(clrType.GetGenericArguments()[0]) + ">";
                }
                return "Array<any>";
            }
            if (clrType.IsEnum)
            {
                return clrType.FullName;
            }
            if (clrType.IsClass || clrType.IsInterface)
            {
                var name = clrType.FullName;

                if (clrType.IsGenericType)
                {
                    name = clrType.FullName.Remove(clrType.FullName.IndexOf('`')) + "<";
                    var count = 0;
                    foreach (var genericArgument in clrType.GetGenericArguments())
                    {
                        if (count++ != 0) name += ", ";
                        name += GetTypeScriptName(genericArgument);
                    }
                    name += ">";
                }
                return name;
            }

            Console.WriteLine("WARNING: Unknown conversion for type: " + clrType.FullName);
            return "any";
        }


    }
}