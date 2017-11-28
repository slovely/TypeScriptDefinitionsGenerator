using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using TypeScriptDefinitionsGenerator.Core.Extensions;

namespace TypeScriptDefinitionsGenerator.Core
{
    public class WebApiUrlGenerator
    {
        public string GetUrl(MethodInfo method)
        {
            var url = "";
            var routeParameters = new List<string>();
            var actionParameters = GetActionParameters(method);
            var routeAttr = method.GetCustomAttributes().FirstOrDefault(a => a.GetType().FullName == "System.Web.Http.RouteAttribute");
            if (routeAttr != null)
            {
                url = GenerateUrlFromRoute((RouteAttribute) routeAttr, out routeParameters, actionParameters);
            }
            else
            {
                routeParameters.Add("id");
                var idParam = actionParameters.FirstOrDefault(a => a.Name == "id");
                if (idParam != null) idParam.RouteProperty = true;
                url = GenerateStandardControllerActionUrl(method);
            }

            var queryString = GetQueryStringParameters(actionParameters);
            if (string.IsNullOrEmpty(queryString)) 
                return url;

            return url + " + \"" + queryString + "\"";
        }

        private static string GenerateStandardControllerActionUrl(MethodInfo method)
        {
            var controllerName = GetControllerName(method);
            var actionName = GetActionName(method);

            var hasIdParameter = method.GetParameters().Any(p => p.Name == "id");

            return $"\"api/{controllerName}/{actionName}" + (hasIdParameter ? "/\" + id" : "\"");
        }

        private static string GenerateUrlFromRoute(RouteAttribute routeAttr, out List<string> routeParameters, List<ActionParameterInfo> actionParameters)
        {
            var routeParametersList = new List<string>();
            routeParameters = routeParametersList;
            var result = routeAttr.Template;
            var matches = Regex.Matches(result, "[{].+?[}]");
            if (matches.Count == 0)
                return "\"" + result + "\"";

            while (Regex.IsMatch(result, "(.)[{](.+?)[}](.*)"))
            {
                result = Regex.Replace(result, "(.)[{](.+?)[}](.*)", m =>
                {
                    var value = m.Groups[2].Value;
                    routeParametersList.Add(value);
                    var actionParam = actionParameters.FirstOrDefault(a => a.OriginalName == value);
                    if (actionParam != null) actionParam.RouteProperty = true;
                    return m.Groups[1].Value + "\" + " + value + " + \"" + m.Groups[3].Value;
                });
            }

            return "\"" + result + "\"";
        }

        private static string GetControllerName(MethodInfo method)
        {
            return method.DeclaringType.Name.Replace("Controller", "");
        }

        private static string GetActionName(MethodInfo method)
        {
            var actionNameAttribute = method.GetCustomAttributes(false)
                .FirstOrDefault(a => a.GetType().FullName == "System.Web.Http.ActionNameAttribute");
            if (actionNameAttribute != null)
            {
                return ((ActionNameAttribute) actionNameAttribute).Name.ToCamelCase();
            }
            return method.Name.ToCamelCase();
        }
        
        private static string GetQueryStringParameters(List<ActionParameterInfo> actionParameters)
        {
            var result = string.Join("&", actionParameters.Where(a => a.FromUri && !a.RouteProperty).Select(a => a.Name + "=\" + " + a.Name + " + \""));
            if (result != "") result = "?" + result;
            return result;
        }
        
        private static List<ActionParameterInfo> GetActionParameters(MethodInfo action)
        {
            var result = new List<ActionParameterInfo>();
            var parameters = action.GetParameters();
            foreach (var parameterInfo in parameters)
            {
                var param = new ActionParameterInfo();
                param.Name = param.OriginalName = parameterInfo.Name;
                param.Type = TypeConverter.GetTypeScriptName(parameterInfo.ParameterType);

                var bindAttribute = parameterInfo.GetCustomAttributes<BindAttribute>().FirstOrDefault();
                if (bindAttribute != null)
                {
                    param.Name = bindAttribute.Prefix ?? param.Name;
                }
                var fromBody = parameterInfo.GetCustomAttributes<FromBodyAttribute>().FirstOrDefault();
                // Parameters are from the URL unless specified by a [FromBody] attribute.
                param.FromUri = fromBody == null;

                param.Name = param.Name.ToCamelCase();
                result.Add(param);
            }

            return result;
        }
    }
}