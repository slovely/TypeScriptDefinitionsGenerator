using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TypeScriptDefinitionsGenerator.Common.Extensions;

namespace TypeScriptDefinitionsGenerator.Common.UrlGenerators
{
    internal class ServiceStackHelper
    {
        public Type GetResponseTypeForRequest(Type request)
        {
            var interfaces = request.GetInterfaces().Where(i => i.FullName.StartsWith("ServiceStack.IReturn`1") && i.IsGenericType).ToList();
            if (interfaces.Any())
            {
                return interfaces.First().GetGenericArguments()[0];
            }
            return null;
        }

        public string GenerateUrlFromRoute(string path, out List<string> routeParameters)
        {
            var routeParametersList = new List<string>();
            routeParameters = routeParametersList;
            var result = path;
            var matches = Regex.Matches(result, "[{].+?[}]");
            if (matches.Count == 0)
                return "\"" + result + "\"";

            while (Regex.IsMatch(result, "(.)[{](.+?)[}](.*)"))
            {
                result = Regex.Replace(result, "(.)[{](.+?)[}](.*)", m =>
                {
                    var value = m.Groups[2].Value.ToCamelCase();
                    routeParametersList.Add(value);
                    return m.Groups[1].Value + "\" + " + value + " + \"" + m.Groups[3].Value;
                });
            }

            return "\"api" + result + "\"";
        }

        public List<ActionParameterInfo> GetActionParameters(Type request, ServiceStackRouteInfo routeInfo)
        {
            var result = new List<ActionParameterInfo>();
            foreach (var parameter in routeInfo.RouteParameters)
            {
                var param = new ActionParameterInfo();
                var property = request.GetProperty(parameter);
                param.Name = property?.Name ?? parameter;
                param.Type = property == null ? "any" : TypeConverter.GetTypeScriptName(property.PropertyType);
                param.FromUri = true;

                param.Name = (property?.Name ?? parameter).ToCamelCase();
                result.Add(param);
            }

            return result;
        }
    }
}