using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public string GenerateUrlFromRoute(string path, Type requestDto, bool includeQueryStringParams, out List<string> routeParameters)
        {
            var routeParametersList = new List<string>();
            routeParameters = routeParametersList;
            var result = path;
            var matches = Regex.Matches(result, "[{].+?[}]");
            if (matches.Count == 0)
            {
            }
            else
            {
                while (Regex.IsMatch(result, "(.)[{](.+?)[}](.*)"))
                {
                    result = Regex.Replace(result, "(.)[{](.+?)[}](.*)", m =>
                    {
                        var value = m.Groups[2].Value.ToCamelCase();
                        routeParametersList.Add(value);
                        return m.Groups[1].Value + "\" + " + value + " + \"" + m.Groups[3].Value;
                    });
                }
            }
            var queryString = includeQueryStringParams ? GetQueryStringParameters(requestDto, routeParameters) : null;
            if (string.IsNullOrEmpty(queryString))
                return "\"api" + result + "\"";

            return "\"api" + result + queryString + "\"";
        }

        private string GetQueryStringParameters(Type requestDto, List<string> routeParameters)
        {
            var parameters = requestDto.GetProperties()
                .Where(p => p.GetCustomAttributes().Any(attr => attr.GetType().FullName == "ServiceStack.ApiMemberAttribute"))
                .ToList();
            var result = string.Join("&", parameters.Where(a =>
            {
                var paramName = a.Name.ToCamelCase();
                return !routeParameters.Contains(paramName);
            }).Select(a =>
            {
                var name = a.Name.ToCamelCase();
                routeParameters.Add(name);
                return $"\" + ({name} ? \"{name}=\" + {name} : \"\") + \"";
            }));
            if (result != "") result = "?" + result;
            return result;
        }

        public List<ActionParameterInfo> GetActionParameters(Type request, ServiceStackRouteInfo routeInfo)
        {
            var result = new List<ActionParameterInfo>();
            foreach (var parameter in routeInfo.RouteParameters)
            {
                var param = new ActionParameterInfo();
                var property = request.GetProperty(parameter) ?? request.GetProperty(parameter.ToPascalCase());
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