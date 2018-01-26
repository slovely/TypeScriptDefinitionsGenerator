using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TypeScriptDefinitionsGenerator.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition() == typeof(Nullable<>);
            return false;
        }

        public static bool IsGenericTask(this Type type)
        {
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition() == typeof(Task<>);
            return false;
        }
        public static bool IsTask(this Type type)
        {
            return type == typeof(Task);
        }

        public static bool IsIDictionary(this Type type)
        {
            return type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        public static Type GetUnderlyingNullableType(this Type type)
        {
            return type.GetGenericArguments().Single();
        }

        public static Type GetUnderlyingTaskType(this Type type)
        {
            return type.GetGenericArguments().Single();
        }

        public static bool IsActionResult(this Type type)
        {
            return typeof(IActionResult).IsAssignableFrom(type)
                   || typeof(Task<IActionResult>).IsAssignableFrom(type)
                   || typeof(Task<ActionResult>).IsAssignableFrom(type);
        }
    }
}
