using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var iActionResultType = Type.GetType("Microsoft.AspNetCore.Mvc.IActionResult, Microsoft.AspNetCore.Mvc.Abstractions");
            if (iActionResultType == null) return false;
            var genericTaskType = typeof(Task<>);
            var iActionResultTypeTask = genericTaskType.MakeGenericType(Type.GetType("Microsoft.AspNetCore.Mvc.IActionResult, Microsoft.AspNetCore.Mvc.Abstractions"));
            var actionResultTypeTask = genericTaskType.MakeGenericType(Type.GetType("Microsoft.AspNetCore.Mvc.ActionResult, Microsoft.AspNetCore.Mvc.Core"));

            return iActionResultType.IsAssignableFrom(type)
                   || iActionResultTypeTask.IsAssignableFrom(type)
                   || actionResultTypeTask.IsAssignableFrom(type);
        }

        public static object GetPropertyValue(this object instance, string propertyName)
        {
            return instance.GetType().GetProperty(propertyName).GetValue(instance);
        }
    }
}
