using System.Web.Http;

namespace SampleWebsite.Models.ApiControllers
{
    public class PersonController : ApiController
    {
        [HttpPost]
        public Person GetPerson()
        {
            return new Person { Name = "Fred Flintstone", Gender = Gender.Male };
        }
        
        [HttpGet]
        [Route("api/custom/{personid}/load")]
        [Route("api/controller/{personid1}/load2")]
        public Person GetPersonCustom(int personid)
        {
            return new Person { Name = "Joe Bloggs:" + personid, Gender = Gender.Male };
        }

        public Person Create([FromBody] object person)
        {
            return null;
        }

        public int ActionMethodWithBodyAndQueryString([FromBody] string body, int queryId)
        {
            return 38;
        }
    }
}