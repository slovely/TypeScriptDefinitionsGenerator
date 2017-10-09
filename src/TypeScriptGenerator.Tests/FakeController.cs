using System.Web.Http;

namespace TypeScriptGenerator.Tests
{
    public class FakeController : ApiController
    {
        public int Simple()
        {
            return 1;
        }

        [ActionName("AttrActionName")]
        public int DifferentActionName()
        {
            return 1;
        }

        public void ActionWithIdParameter(string id)
        {
        }

        [Route("api/custom")]
        public int SimpleCustomRoute()
        {
            return 1;
        }

        [Route("api/custom/{customerId}/orders/{orderId}")]
        public void CustomRouteWithParameters(string customerId, string orderId)
        {
        }

        [HttpGet]
        [Route("api/v1/flights/{flightIdentifier}/flightdetails")]
        public void GetFlightDetails(string flightIdentifier)
        {
        }
        
        public int ActionMethodWithBodyAndQueryString([FromBody] string body, int queryId)
        {
            return 38;
        }

        public string OverrideParamName([FromUri(Name = "x")] int number)
        {
            return number.ToString();
        }
    }
}