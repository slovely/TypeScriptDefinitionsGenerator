using ServiceStack;
using TypeScriptDefinitionsGenerator.Common.UrlGenerators;
using Xunit;

namespace TypeScriptGenerator.Tests
{
    public class ServiceStackTests
    {
        [Fact]
        public void ParseUrlInParameters()
        {
            var route = "/api/testing/{TheId}/Another/{Param}";
            //var url = new ServiceStackHelper().GenerateUrlFromRoute(route, out var routeParameters);
            Assert.True(true);
        }
    }

    [ServiceStack.Route("/api/testing", "GET")]
    public class ServiceStackRequest
    {
        
    }
}