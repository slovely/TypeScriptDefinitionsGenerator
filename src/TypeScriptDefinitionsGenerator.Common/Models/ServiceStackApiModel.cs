using System.Collections.Generic;

namespace TypeScriptDefinitionsGenerator.Common.Models
{
    public class ServiceStackApiModel
    {
        public ServiceStackApiModel()
        {
            Requests = new List<ServiceStackRequestModel>();
            RequiredImports = new List<string>();
        }

        public IList<ServiceStackRequestModel> Requests { get; }
        public IList<string> RequiredImports { get; set; }
    }
    
    public class ServiceStackRequestModel
    {
        public ServiceStackRequestModel()
        {
            Routes = new List<ServiceStackRouteInfo>();
        }

        public IList<ServiceStackRouteInfo> Routes { get; }
        public string Name { get; set; }
        public string ReturnTypeTypeScriptName { get; set; }
    }

}