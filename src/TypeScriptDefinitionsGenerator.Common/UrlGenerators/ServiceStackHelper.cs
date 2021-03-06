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
        public bool ReturnsVoid(Type request)
        {
            var interfaces = request.GetInterfaces().Where(i => i.FullName == "ServiceStack.IReturnVoid").ToList();
            return interfaces.Any();
        }
        
        public Type GetResponseTypeForRequest(Type request)
        {
            var interfaces = request.GetInterfaces().Where(i => i.FullName.StartsWith("ServiceStack.IReturn`1") && i.IsGenericType).ToList();
            if (interfaces.Any())
            {
                return interfaces.First().GetGenericArguments()[0];
            }
            return null;
        }

        public string GenerateUrlFromRoute(string path, Type requestDto, bool includeQueryStringParams, bool supportMomentJs, out List<string> routeParameters)
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
            var queryString = includeQueryStringParams ? GetQueryStringParameters(requestDto, routeParameters, supportMomentJs) : null;
            if (string.IsNullOrEmpty(queryString))
                return "\"" + result.TrimStart('/') + "\"";

            return "\"" + result.TrimStart('/') + queryString + "\"";
        }

        private string GetQueryStringParameters(Type requestDto, List<string> routeParameters, bool supportMomentJs)
        {
            var parameters = requestDto.GetProperties()
                .Where(p => p.GetCustomAttributes().Any(attr => attr.GetType().FullName == "ServiceStack.ApiMemberAttribute"))
                .ToList();
            if (parameters.Any(x => !routeParameters.Contains(x.Name.ToCamelCase())))
            {
                return "?\" + Object.keys(querystring).filter(x => typeof querystring[x] === 'boolean' || typeof querystring[x] === 'number' ? true : querystring[x]).map(key => key + '=' + (querystring[key] instanceof Date" +
                       (supportMomentJs ? " || moment.isMoment(querystring[key])" : "") +
                       " ? querystring[key].toISOString() : querystring[key])).join('&') + \" ";
            }
            return "";
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
                if (property != null && (property.PropertyType.IsEnum || Nullable.GetUnderlyingType(property.PropertyType)?.IsEnum == true)) param.Type = "Enums." + param.Type;
                param.FromUri = true;
                param.ClrType = property?.PropertyType;

                param.Name = (property?.Name ?? parameter).ToCamelCase();
                result.Add(param);
            }

            return result;
        }

        public bool IsPropertyRequired(MemberInfo memberInfo)
        {
            var apiMemberAttr = memberInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "ServiceStack.ApiMemberAttribute");
            if (apiMemberAttr == null) return true;
            var isRequired = apiMemberAttr.NamedArguments.FirstOrDefault(n => n.MemberName == "IsRequired");
            return true.Equals(isRequired.TypedValue.Value);
        }
    }
}