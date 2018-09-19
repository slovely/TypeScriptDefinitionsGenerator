using System;
using System.Linq;

namespace TypeScriptDefinitionsGenerator.Common.UrlGenerators
{
    public class ServiceStackHelper
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
    }
}