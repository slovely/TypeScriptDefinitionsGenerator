using System.Collections.Generic;

namespace TypeScriptDefinitionsGenerator.Common
{
    internal class ServiceStackRouteInfo
    {
        public string Verb { get; }
        public string Path { get; }
        public string Url { get; }
        public List<string> RouteParameters { get; }

        public ServiceStackRouteInfo(string verb, string path, string url, List<string> routeParameters)
        {
            Verb = verb.ToUpperInvariant();
            Path = path;
            Url = url;
            RouteParameters = routeParameters;
        }
    }
}