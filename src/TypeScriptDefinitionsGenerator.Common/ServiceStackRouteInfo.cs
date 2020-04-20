using System.Collections.Generic;

namespace TypeScriptDefinitionsGenerator.Common
{
    public class ServiceStackRouteInfo
    {
        public string Verb { get; }
        public string Path { get; }
        public string Url { get; }
        public string RawUrl { get; set; }
        public List<string> RouteParameters { get; }
        public List<ActionParameterInfo> ActionParameters { get; set; }
        public string ReturnTypeTypeScriptName { get; set; }

        public ServiceStackRouteInfo(string verb, string path, string url, string rawUrl, List<string> routeParameters)
        {
            Verb = verb.ToUpperInvariant();
            Path = path;
            Url = url;
            RawUrl = rawUrl;
            RouteParameters = routeParameters;
        }
    }
}