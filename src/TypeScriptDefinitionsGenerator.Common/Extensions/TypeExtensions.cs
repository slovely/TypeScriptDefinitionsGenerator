using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TypeScriptDefinitionsGenerator.Common.Extensions
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
            var types = new List<Type> {type};
            while (type.BaseType != null)
            {
                types.Add(type.BaseType);
                type = type.BaseType;
                if (type.Name == "IActionResult") return true;
                // TODO: Need to check if it is a Task that returns IActionResult...
                //if (typeof(Task<>).)
            }

            return false;
        }
    }
}